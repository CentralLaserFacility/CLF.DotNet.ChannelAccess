//
// IChannel_ExtensionMethods.cs
//

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Clf.Common.ExtensionMethods;

namespace Clf.ChannelAccess.ExtensionMethods
{

  public static class IChannelExtensions
  {

    public static string GetChannelNameAndAccessModeAsString ( this IChannel channel )
    {
      return channel.AsChannelBase().ValidatedChannelNameAndAccessMode.ToString() ;
    }

    public static string ChannelNameWithValueAccessModeAndChannelIdentifier ( this IChannel channel )
    {
      return $"{
        channel.GetChannelNameAndAccessModeAsString()
      }#{
        channel.AsChannelBase().ChannelIdentifier.ToString("D4")
      }" ;
    }

    //
    // These 'convenience' methods provide functions that
    // return useful information extracted from the current 'State' of the channel.
    //
    // Using these functions leaves you open to the possibility of 'race conditions'.
    // Because if you call several of these functions in a sequence, the state of the PV
    // might have changed between one call and the next. The channel you're working with
    // will have been informed of the state change and will have raised an event, but
    // that won't affect the code currently being executed.
    //
    // So for example this might fail :
    //
    //   if ( myChannel.IsConnected() )
    //   {
    //     myChannel.IsConnected().Should().BeTrue() ; // MIGHT NO LONGER BE TRUE !!!
    //
    // If we're unlucky, a new message might arrive just after we've returned from the
    // first call to 'IsConnected', and if that message tells us that the connection
    // has been dropped, the second call will unexpectedly return false.
    //
    // [
    //   That unlucky situation will ony occur if the incoming message modifies
    //   the state of the channel on a different thread - but that can't be avoided
    //   in some circumstances, eg in a Blazor Server app where the SychronisationContext
    //   is null and there's no mechanism for 'posting' an event to the UI thread.
    // ]

    //
    // The right way to behave is to query the current State, and take actions based on that.
    // Since the current 'StateSnapshot' is returned as an immutable object, we can guarantee
    // that the various elements of the State are always consistent even if the State Snapshot
    // is not the very latest one that's available.
    //
    //   var stateSnapshot = myChannel.GetCurrentState() ;
    //   if ( stateSnapshot.IsConnected() )
    //   {
    //     stateSnapshot.IsConnected().Should().BeTrue() ; // GUARANTEED !!!
    //
    // With that proviso, these functions are nevertheless quite useful.
    // They mimic the original API that was provided with the earlier version of 'Channel'.
    //

    // COMMENTS SHOULD MAKE THE DANGER EVIDENT ...

    public static bool IsConnected ( this IChannel channel )
    => channel.Snapshot().CurrentState.IsConnected ;

    public static bool IsValid ( this IChannel channel )
    => channel.Snapshot().CurrentState.IsValid ;

    public static bool IsInvalid ( this IChannel channel, [NotNullWhen(true)] out string? whyNotValid )
    => channel.Snapshot().CurrentState.IsInvalid(
      out whyNotValid
    ) ;

    public static bool IsConnectedAndValid ( this IChannel channel )
    => channel.Snapshot().CurrentState.ConnectionAndValidityStatus.IsConnectedAndValid ;

    public static bool HasConnectedAndReportedItsFieldInfo ( this IChannel channel )
    => channel.Snapshot().CurrentState.ChannelHasConnected ;

    public static bool HasConnectedAndAcquiredValue ( this IChannel channel )
    => channel.Snapshot().CurrentState.ValueInfo is not null ;

    // Short names ...

    public static ValueInfo? ValueInfo ( this IChannel channel )
    => channel.Snapshot().CurrentState.ValueInfo ;

    public static object? Value ( this IChannel channel )
    => channel.Snapshot().CurrentState.ValueInfo?.ValueAsObject ;

    // Equivalent functions with names that explicitly express
    // the possibility of a null value, or throwing if null.
    // Declared as internal because ...

    internal static ValueInfo? ValueInfoOrNull ( this IChannel channel )
    => channel.Snapshot().CurrentState.ValueInfo ;

    internal static ValueInfo ValueInfoOrThrow ( this IChannel channel )
    => channel.Snapshot().CurrentState.ValueInfo ?? throw new System.NullReferenceException("ValueInfo not available") ;

    internal static object? ValueOrNull ( this IChannel channel )
    => channel.Snapshot().CurrentState.ValueInfo?.ValueAsObject ;

    internal static object ValueOrThrow ( this IChannel channel )
    => channel.Snapshot().CurrentState.ValueInfo?.ValueAsObject ?? throw new System.NullReferenceException("Value not available") ;

    // Obtain the current value as a particular type T.
    // Returns 'false' if either (A) the value is not known,
    // or (B) the value is not of the specified type.

    public static bool TryGetCurrentValue<T> ( 
      this IChannel             channel, 
      [NotNullWhen(true)] out T value 
    ) {
      value = default! ;
      if ( channel.ValueOrNull() is T tmpValue )
      {
        value = tmpValue ;
        return true ;
      }
      else
      {
        return false ;
      }
    }

    // Obtain the current value as a particular type.
    // Returns 'false' if either (A) the value is not known,
    // or (B) the value is not of the specified type.

    public static bool TryGetCurrentValue ( 
      this IChannel                   channel, 
      System.Type                     type,
      [NotNullWhen(true)] out object? value 
    ) {
      value = channel.ValueOrNull() ;
      if ( value is null )
      {
        return false ;
      }
      if ( value.GetType() == type )
      {
        // The value is of the expected type
        return true ;
      }
      else
      {
        // The value is NOT of the expected type
        value = null ;
        return false ;
      }
    }

    // Attempt to obtain the current value as a particular type T.
    // If that succeeds, compute a modified value and use 'PutValue'
    // to write it to the channel, in fire-and-forget mode.

    public static bool TryPutModifiedValue<T> ( 
      this IChannel    channel, 
      System.Func<T,T> getModifiedValueFunc
    ) {
      if ( channel.TryGetCurrentValue( out T currentValue ) )
      {
        channel.PutValue(
          getModifiedValueFunc(currentValue)!
        ) ;
        return true ;
      }
      else
      {
        return false ;
      }
    }

    public static async Task<PutValueResult> TryPutModifiedValueAckAsync<T> ( 
      this IChannel    channel, 
      System.Func<T,T> getModifiedValueFunc
    ) {
      if ( channel.TryGetCurrentValue( out T currentValue ) )
      {
        return await channel.PutValueAckAsync(
          getModifiedValueFunc(currentValue)!
        ) ;
      }
      else
      {
        // Hmm, we need another option here !!!
        // Couldn't put modified value because current value wasn't available
        // BUT MAYBE CLIENT CODE ISN'T GOING TO CARE ABOUT THE REASON FOR FAILURE !!!
        // ON FAILURE, WE COULD JUST WRITE AN ENTRY INTO THE LOG ?
        // AND JUST HAVE A bool INSTEAD OF A PutValueResult !!!
        return PutValueResult.RejectedByServer ;
      }
    }

    // Value as a string produces a valid result even if the value is null 

    public static string Value_AsDisplayString ( 
      this IChannel                    channel, 
      WhichValueInfoElementsToInclude? whichValueInfoElementsToInclude = null
    ) => (
      channel?.Snapshot().CurrentState.Value_AsDisplayString(
        whichValueInfoElementsToInclude     
      ) ?? "null"
    ) ;

    // Retrieve other info from a Channel ...

    public static AlarmStatusAndSeverity? AlarmStatusAndSeverity ( this IChannel channel )
    => channel.Snapshot().CurrentState.ValueInfo?.AlarmStatusAndSeverity ;
    
    public static AuxiliaryInfo? AuxiliaryInfo ( this IChannel channel )
    => channel.Snapshot().CurrentState.ValueInfo?.AuxiliaryInfo ;
    
    public static System.DateTime? Value_TimeStamp ( this IChannel channel )
    => channel.Snapshot().CurrentState.ValueInfo?.LocalTimeStamp ;
    
    public static FieldInfo? FieldInfo ( this IChannel channel )
    => channel.Snapshot().CurrentState.FieldInfo ;

    // Cast from an IChannel to the concrete class that we know it represents.
    // These helpers work with IChannel instances that are either 'wrapped' or not.

    internal static RemoteChannel AsRemoteChannel ( this IChannel channel )
    => (
      channel is ChannelWrapper wrapper
      ? (RemoteChannel) wrapper.WrappedChannel
      : (RemoteChannel) channel 
    ) ;

    internal static LocalChannel AsLocalChannel ( this IChannel channel )
    => (
      channel is ChannelWrapper wrapper
      ? (LocalChannel) wrapper.WrappedChannel
      : (LocalChannel) channel 
    ) ;

    internal static ChannelBase AsChannelBase ( this IChannel channel )
    => (
      channel is ChannelWrapper wrapper
      ? (ChannelBase) wrapper.WrappedChannel
      : (ChannelBase) channel 
    ) ;

    internal static ChannelWrapper AsChannelWrapper ( this IChannel channel )
    => (
      (ChannelWrapper) channel
    ) ;

    // Helpers to wait for a bunch of channels to connect ...

    public static async Task<bool> WaitForAllChannelsToConnectAndAcquireValues ( 
      this IEnumerable<IChannel> channels
    ) {
      var tasks = channels.Select(
        channel => channel.HasConnectedAndAcquiredValueAsync()
      ).ToArray() ;
      // Here we only return from the 'await' when each and every channel
      // has been given an opportunity to connect and acquire its value.
      // If all the channels successfully connect, we return as soon as
      // the last channel has connected, with all the 'channelConnectedResults'
      // elements set to 'true'.
      // If even a single channel fails to connect within the nominal time,
      // we'll have waited for it and the corresponding entry in the
      // 'results' array will have been set to 'false'.
      bool[] channelConnectedResults = await Task.WhenAll(tasks) ;
      // We return 'true' if *all* the channels connected sucessfully.
      return channelConnectedResults.All(
        channelConnectedOrNot => channelConnectedOrNot is true
      ) ;
    }

    public static void ForEachChannelThatFailedToConnectAndAcquireValue ( 
      this IEnumerable<IChannel> channels,
      System.Action<IChannel>    action
    ) {
      channels.Where(
        channel => channel.Snapshot().CurrentState.ValueInfo is null
      ).ForEachItem(
        channel => action(channel)
      ) ;
    }

    public static async Task ForEachChannelThatFailsToConnectAndAcquireValue ( 
      this IEnumerable<IChannel> channels,
      System.Action<IChannel>    action
    ) {
      bool allConnectedSuccessfully = await channels.WaitForAllChannelsToConnectAndAcquireValues() ;
      channels.Where(
        channel => channel.Snapshot().CurrentState.ValueInfo is null
      ).ForEachItem(
        channel => action(channel)
      ) ;
    }

    //
    // Returns false if the value could not be written.
    // Possible reasons for failure ...
    //  - channel is null
    //  - channel has never been connected, so the 'FieldInfo' has not
    //    been reported and consequently we don't know the DbField type
    //    and will be unable to convert the string
    //

    public static bool TryPutValueParsedFromString ( this IChannel? channel, string valueAsString )
    {
      DbFieldDescriptor? fieldDescriptor = channel?.FieldInfo?.DbFieldDescriptor ;
      if ( 
         channel != null
      && fieldDescriptor != null 
      ) {
        object valueToWrite = Helpers.ConvertStringToChannelValue(
          valueAsString,
          fieldDescriptor
        ) ;
        channel.PutValue(valueToWrite) ;
        return true ;
      }
      return false ;
    }

    //
    // Returns false if the value could not be written.
    // Possible reasons for failure ...
    //  - channel is null
    //  - channel has never been connected, so the 'FieldInfo' has not
    //    been reported and consequently we don't know the DbField type
    //    and will be unable to convert the string
    //  - the value was rejected by the PV
    //  - the timeout elapsed ie we failed to recieve an acknowledgement
    //

    public static async Task<bool> TryPutValueParsedFromStringAckAsync ( this IChannel? channel, string valueAsString )
    {
      DbFieldDescriptor? fieldDescriptor = channel?.FieldInfo?.DbFieldDescriptor ;
      if ( 
         channel != null
      && fieldDescriptor != null 
      ) {
        object valueToWrite = Helpers.ConvertStringToChannelValue(
          valueAsString,
          fieldDescriptor
        ) ;
        return (
          await channel.PutValueAckAsync(valueToWrite)
        ) != PutValueResult.Success ;
      }
      return false ;
    }

  }

}

