//
// Hub_get_and_put_channel_values.cs
//

using Clf.ChannelAccess.ExtensionMethods;
using System.Threading.Tasks;

namespace Clf.ChannelAccess
{

  public static partial class Hub
  {

    //
    // These helpers provides methods for interacting with a PV via simple method calls
    // which don't require you to create a Channel and hook into its StateChanged event.
    // This is convenient when you just want to query a PV's data type, or read or 
    // write a Value.
    //
    // They are all 'async', because for any interaction with a Channel
    // you need to wait for the connection to be established.
    //

    //
    // The timeout is defined via the Settings and is the same for every call.
    // 
    // See https://stefansch.medium.com/another-set-of-common-async-task-mistakes-and-how-to-avoid-them-23a6af5b1ce8
    //

    //
    // Perform a one-off query of a field's value, a one-off write, and so on.
    //

    internal static async Task<FieldInfo?> GetFieldInfoAsync ( 
      ChannelName      channelName, 
      ValueAccessMode? valueAccessMode = null 
    ) {
      using ( 
        var channel = Hub.GetOrCreateChannel(
          channelName
        ) 
      ) {
        // Hmm, we're ignoring the 'connected' result, that will be false if
        // the operation timed out ; and we'll return 'null' for the field info.
        // ??? Should we be returning a 'GetFieldInfoResult', which defines
        // a 'Success' field and so on ???
        bool connected = await channel.HasConnectedAndAcquiredValueAsync().ConfigureAwait(false) ;
        return channel.FieldInfo() ;
      }
    }

    internal static async Task<object?> GetValueOrNullAsync ( 
      ChannelName      channelName, 
      ValueAccessMode? valueAccessMode = null 
    ) {
      using ( 
        var channel = Hub.GetOrCreateChannel(
          channelName,
          valueAccessMode
        ) 
      ) {
        bool connected = await channel.HasConnectedAndAcquiredValueAsync().ConfigureAwait(false) ;
        return channel.ValueOrNull() ;
      }
    }

    public static async Task<GetValueResult> GetValueInfoAsync ( 
      ChannelName      channelName, 
      ValueAccessMode? valueAccessMode = null 
    ) {
      using ( 
        var channel = Hub.GetOrCreateChannel(
          channelName,
          valueAccessMode
        ) 
      ) {
        bool connected = await channel.HasConnectedAndAcquiredValueAsync().ConfigureAwait(false) ;
        return (
          connected
          ? new GetValueResult(channel.ValueInfoOrNull())
          : new GetValueResult(WhyGetValueFailed.TimeoutOnThisQuery)
        ) ; 
      }
    }

    public static async Task<string> GetValueAsStringAsync ( 
      ChannelName                      channelName,      
      ValueAccessMode?                 valueAccessMode                 = null, 
      WhichValueInfoElementsToInclude? whichValueInfoElementsToInclude = null
    ) {
      using ( 
        var channel = Hub.GetOrCreateChannel(
          channelName,
          valueAccessMode
        ) 
      ) {
        bool connected = await channel.HasConnectedAndAcquiredValueAsync().ConfigureAwait(false) ;
        return channel.Value_AsDisplayString(whichValueInfoElementsToInclude) ;
      }
    }

    public static async Task<PutValueResult> PutValueAsync ( 
      ChannelName      channelName,
      object           valueToWrite,
      ValueAccessMode? valueAccessMode = null 
    ) {
      using ( 
        var channel = Hub.GetOrCreateChannel(
          channelName,
          valueAccessMode
        ) 
      ) {
        bool connected = await channel.HasConnectedAsync().ConfigureAwait(false) ;
        if ( ! connected )
        {
          return PutValueResult.Timeout ;
        }
        return await channel.PutValueAsync(
          valueToWrite
        ).ConfigureAwait(false) ;
      }
    }

    public static async Task<PutValueResult> PutValueAsync ( 
      ChannelName      channelName,
      ValueAccessMode  valueAccessMode,
      object           valueToWrite
    ) {
      using ( 
        var channel = Hub.GetOrCreateChannel(
          channelName,
          valueAccessMode
        ) 
      ) {
        bool connected = await channel.HasConnectedAsync().ConfigureAwait(false) ;
        if ( ! connected )
        {
          return PutValueResult.Timeout ;
        }
        return await channel.PutValueAsync(
          valueToWrite
        ).ConfigureAwait(false) ;
      }
    }

    public static async Task<PutValueResult> PutValueAckAsync ( 
      ChannelName      channelName,
      object           valueToWrite,
      ValueAccessMode? valueAccessMode = null 
    ) {
      using ( 
        var channel = Hub.GetOrCreateChannel(
          channelName,
          valueAccessMode
        ) 
      ) {
        bool connected = await channel.HasConnectedAsync().ConfigureAwait(false) ;
        if ( ! connected )
        {
          return PutValueResult.Timeout ;
        }
        return await channel.PutValueAckAsync(
          valueToWrite
        ).ConfigureAwait(false) ;
      }
    }

    /// <summary>
    /// PutValue Parsed From String Async.
    /// </summary>
    /// <param name="channelName"></param>
    /// <param name="stringValueToParse"></param>
    /// <returns></returns>
    public static async Task<PutValueResult> PutValueFromStringAsync ( 
      ChannelName      channelName,
      string           stringValueToParse,
      ValueAccessMode? valueAccessMode = null
    ) {
      // We could check for an empty string even before we attempt to connect ?
      // Hmm, no, because we might be writing to a value of type 'string',
      // and that would be perfectly fine.
      using ( 
        var channel = Hub.GetOrCreateChannel(
          channelName,
          valueAccessMode
        ) 
      ) {
        // Connect first so that we can discover the native type
        // of the PV's value, and know what type to convert to ...
        bool connected = await channel.HasConnectedAndAcquiredValueAsync().ConfigureAwait(false) ;
        if ( ! connected )
        {
          return PutValueResult.Timeout ;
        }
        // No that we've definitely connected, the 'FieldInfo' is known
        // so we can convert our string to the expected data type.
        if ( 
          channel.FieldInfo!.DbFieldDescriptor.TryParseValue(
            stringValueToParse,
            out var valueToWrite
          )
        ) {
          return await channel.PutValueAsync(
            valueToWrite
          ).ConfigureAwait(false) ;
        }
        else
        {
          return PutValueResult.InvalidValueSupplied ;
        }
      }
    }

    //
    // Fire and forget !
    //
    // This has been removed from the Hub API because it doesn't work !
    // We're not waiting for the Channel to connect, so PutValue will fail.
    // To make this work we'll need to wait for the Connect, and that means
    // declaring the method as 'async' ; but we already have a 'PutValueAsync'
    // that does exactly what we need ! Oops.
    //
    // public static void PutValue ( 
    //   ChannelName channelName,
    //   object      valueToWrite
    // ) {
    //   ConnectAndWriteValue() ;
    //   void ConnectAndWriteValue ( )
    //   {
    //     using ( 
    //       var channel = Hub.GetOrCreateChannel(
    //         channelName
    //       ) 
    //     ) {
    //       channel.PutValue(
    //         valueToWrite
    //       ) ;
    //     }
    //   }
    // }
    //

  }

}
