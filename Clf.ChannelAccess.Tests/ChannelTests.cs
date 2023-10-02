//
// ChannelTests.cs
//

using Xunit ;
using FluentAssertions ;
using static FluentAssertions.FluentActions ;
using System.Threading.Tasks ;
using Clf.ChannelAccess.ExtensionMethods ;
using Clf.Common.ExtensionMethods ;
using System.Collections.Generic;

// using static Clf.ChannelAccess.Hub ;

namespace Clf_ChannelAccess_Tests
{

  //
  // Because all our tests are in this single class,
  // they will *not* be run in parallel by XUnit,
  // and that's good - because running them in parallel
  // would seriously mess things up !
  //
  // If tests are also packaged in another class,
  // it'll probably be be necessary to prevent them
  // from running in parallel :
  //
  //   [assembly:Xunit.CollectionBehavior(Xunit.CollectionBehavior.CollectionPerAssembly)]
  //
  // ... as explained in
  // https://xunit.net/docs/running-tests-in-parallel
  //

  public class ChannelTests : System.IDisposable
  {

    private bool UseLocalChannels = (
      false
      // true 
    ) ;

    private static Xunit.Abstractions.ITestOutputHelper? g_output ;

    // private static readonly Clf.ChannelAccess.IChannelsHub g_channelsHub 
    //  = Clf.ChannelAccess.ChannelsHub.Instance ;

    public static Clf.ChannelAccess.ChannelDescriptorsList RecordDescriptorsList ;

    public static string ChannelTests_DbFilePath = "C:\\tmp\\ChannelTests.db" ;

    static ChannelTests ( )
    {

      Clf.ChannelAccess.Settings.WhichDllsToUse = (
        // Clf.ChannelAccess.WhichDllsToUse.DaresburyReleaseDlls 
        Clf.ChannelAccess.WhichDllsToUse.ClfDebugDlls 
      ) ;

      // Clf.ChannelAccess.Settings.WhichDllsToUse = Clf.ChannelAccess.LowLevelApi.WhichDllsToUse.ClfDebugDlls ;

      RecordDescriptorsList = Clf.ChannelAccess.ChannelDescriptorsList.Create(

        // Scalar values
        "x:i16|i16|0"       // First test ...
      , "x:byte|byte|0"
      , "x:i32|i32|0"
      , "x:f32|f32|0"
      , "x:f64|f64|0"
      , "x:s39|s39|0"

      // Enum value - because of that nasty quirk,
      // we have to provide an initial value of 1 !!!
      , "x:enum|enum:a,b|1"

      // Short array values
      , "x:s39_4|s39[4]|0,..."   // The '...' means 'repeat the value to fill the array'
      , "x:byte_4|byte[4]|0,..."
      , "x:i16_4|i16[4]|0,..."
      , "x:i32_4|i32[4]|0,..."
      , "x:f32_4|f32[4]|0,..."
      , "x:f64_4|f64[4]|0,..."

      // Long array values
      , "x:s39_100|s39[100]|0,..."
      , "x:byte_4000|byte[4000]|0,..."
      , "x:byte_16000|byte[16000]|0,..."
      // , "x:byte_16383|byte[16383]|0,..." FAILS !!!
      , "x:i16_4000|i16[4000]|0,..."
      , "x:i32_4000|i32[4000]|0,..."
      , "x:f32_4000|f32[4000]|0,..."
      , "x:f64_2000|f64[2000]|0,..."

      // String values (limits)
      , "x:s39_empty|s39|''"
      , "x:s39_4_empty|s39[4]|,a,,..."
      // , "x:s39_4_empty|s39[4]|'',a,'',..."
      , "x:s39_37|s39|1111111111222222222233333333331234567"
      // , "x:s39_38|s39|11111111112222222222333333333312345678" // THIS WILL FAIL !!!
      ) ;
      RecordDescriptorsList.WriteToFile(
        ChannelTests_DbFilePath
      ) ;
    }

    public ChannelTests ( Xunit.Abstractions.ITestOutputHelper? testOutputHelper )
    {
      g_output = testOutputHelper ;
      g_output?.WriteLine(
        $"Test starting at {System.DateTime.Now.ToString("h:mm:ss.fff")}"
      ) ;
      g_output?.WriteLine(
        $"ChannelTests instance created"
      ) ;
      g_output?.WriteLine(
        $"SynchronizationContextSupportsResumingOnCallingThread : {
          Clf.ChannelAccess.ChannelsRegistry.SynchronizationContextSupportsResumingOnCallingThread
        }"
      ) ;

      Clf.ChannelAccess.ChannelsRegistry.HandleMessageToSystemLog = HandleSystemLogMessage ; 

      Clf.ChannelAccess.Hub.OnInterestingEvent += LogInterestingEvent ;
      if ( UseLocalChannels )
      {
        Clf.ChannelAccess.Hub.CreateLocalChannels(
          RecordDescriptorsList
        ) ;
        g_output?.WriteLine(
          $"Running tests using LOCAL CHANNELS"
        ) ;
      }
      else
      {
        g_output?.WriteLine(
          $"Running tests using REMOTE CHANNELS (ThinIoc)"
        ) ;
      }
    }

    private async Task EnsureServerIsRunning ( )
    {
      if ( UseLocalChannels )
      {
        return ;
      }
      await Clf.ChannelAccess.ThinIocProcess.EnsureIsRunningAsync(
        ChannelTests_DbFilePath
      ) ;
    }

    private void LogInterestingEvent ( Clf.ChannelAccess.Notification interestingEvent )
    {
      g_output?.WriteLine(
        $"{interestingEvent}"
      ) ;
    }

    private void HandleSystemLogMessage ( Clf.Common.LogMessageLevel level, string message )
    {
      g_output?.WriteLine(
        $"{level} : {message}"
      ) ;
    }

    public void Dispose ( )
    {
      // Might be unnecessary - the tests run fine if this is omitted ...
      Clf.ChannelAccess.ChannelsRegistry.DeregisterAllChannels() ;
      Clf.ChannelAccess.Hub.OnInterestingEvent -= LogInterestingEvent ;
      g_output?.WriteLine(
        $"Test disposed at {System.DateTime.Now:h:mm:ss.fff}"
      ) ;
    }

    public sealed class OneTestRunningVerifier : System.IDisposable
    {
      private static bool g_testIsRunning = false ;
      private string? m_nameOfTestMethod ;
      public OneTestRunningVerifier ( [System.Runtime.CompilerServices.CallerMemberName] string? nameOfTestMethod = null )
      {
        m_nameOfTestMethod = nameOfTestMethod ;
        g_output?.WriteLine(
          $"Starting test '{nameOfTestMethod}'"
        ) ;
        g_testIsRunning.Should().BeFalse() ;
        g_testIsRunning = true ;
      }
      public void Dispose ( )
      {
        g_output?.WriteLine(
          $"Finished test : '{m_nameOfTestMethod}'"
        ) ;
        Clf.ChannelAccess.ChannelsRegistry.DisplayRegisteredChannels(
          line => g_output!.WriteLine(line)
        ) ;
        g_testIsRunning = false ;
      }
    }

    const string ChannelNamePrefix = "xxx:" ;

    [Fact]
    public async Task EnumTestSucceeded ( )
    {
      await EnsureServerIsRunning() ;
      // https://fluentassertions.com/exceptions/
      // await Invoking(
      //   async () => {
      //     await RunTest_C_ENUM(
      //       Clf.ChannelAccess.RecordDescriptor.FromEncodedString("x:enum|enum:a,b|0")
      //     ) ;
      //   }
      // ).Should().ThrowAsync<System.Exception>() ;
      await RunTest_C(
        Clf.ChannelAccess.ChannelDescriptor.FromEncodedString("x:enum|enum:a,b|1")
      ) ;
      await RunTest_C_ENUM(
        Clf.ChannelAccess.ChannelDescriptor.FromEncodedString("x:enum|enum:a,b|0")
      ) ;
      await RunTest_C_ENUM(
        Clf.ChannelAccess.ChannelDescriptor.FromEncodedString("x:enum|enum:a,b|1")
      ) ;
    }

    [Fact]
    public async Task FirstTestSucceed ( )
    {
      await EnsureServerIsRunning() ;
      await RunTest_C(
        RecordDescriptorsList[0]
      ) ;
    }

    [Fact]
    public async Task AllTestsSucceed ( )
    {
      await EnsureServerIsRunning() ;
      foreach ( var recordDescriptor in RecordDescriptorsList )
      {
        await RunTest_C(
          recordDescriptor
        ) ;
      }
    }

    private async Task RunTest_C ( 
      Clf.ChannelAccess.ChannelDescriptor channelDescriptor
    ) {
      channelDescriptor.DbFieldDescriptor.TryParseValue(
        channelDescriptor.InitialValueAsString.VerifiedAsNonNullInstance(),
        out object? initialValue
      ).Should().BeTrue() ;
      channelDescriptor.ChannelName.IsValid(
        out var validatedChannelName,
        out string? whyNotValid
      ).Should().BeTrue() ;
      validatedChannelName.Should().NotBeNull() ;
      whyNotValid.Should().BeNull() ;
      // Aha, because 'Assert' is decorated with [DoesNotReturnIf(false)],
      // the compiler knows that 'initialValue' is definitely non-null
      // from here onwards ...
      System.Diagnostics.Debug.Assert(initialValue is not null) ;
      object? valueMostRecentlyWritten = null ;
      using ( var channelsHandler = new Clf.ChannelAccess.ChannelsHandler() )
      {
        List<bool> connectionEventsReceived = new() ;
        List<object> valueUpdateEventsReceived = new() ;
        bool? channelIsConnected_fromMostRecentEventMessage = null ;
        object? channelValue_fromMostRecentEventMessage = null ;
        // Create our Channel
        var channel = Clf.ChannelAccess.Hub.GetOrCreateChannel(channelDescriptor.ChannelName) ;
        Clf.ChannelAccess.ChannelsRegistry.HasRegisteredChannel(
          channelDescriptor.ChannelName
        ).Should().BeTrue() ;
        // Install the channel and wire up event handlers
        channelsHandler.InstallChannel(
          channel,
          (isConnected,state) => {
            g_output?.WriteLine($"MESSAGE : IsConnected => {isConnected}") ;
            connectionEventsReceived.Add(isConnected) ;
            channelIsConnected_fromMostRecentEventMessage = isConnected ;
          },
          (valueInfo,state) => {
            g_output?.WriteLine($"MESSAGE : Value => {valueInfo.Value_AsDisplayString()}") ;
            valueUpdateEventsReceived.Add(valueInfo.Value) ;
            channelValue_fromMostRecentEventMessage = valueInfo.Value ;
          }
        ) ;
        // Install the same channel for a second time ...
        channelsHandler.InstallChannel(
          Clf.ChannelAccess.Hub.GetOrCreateChannel(channelDescriptor.ChannelName),
          (isConnected,state) => {
            g_output?.WriteLine($"MESSAGE 2 : IsConnected => {isConnected}") ;
            connectionEventsReceived.Add(isConnected) ;
            channelIsConnected_fromMostRecentEventMessage = isConnected ;
          },
          (valueInfo,state) => {
            g_output?.WriteLine($"MESSAGE 2 : Value => {valueInfo.Value_AsDisplayString()}") ;
            valueUpdateEventsReceived.Add(valueInfo.Value) ;
            channelValue_fromMostRecentEventMessage = valueInfo.Value ;
          }
        ) ;
        // Our 'channel' has now either been created, if it didn't already exist,
        // or we've been given a clone of an already-existing channel.
        // Either way, it should at this point be in a state where it's going to
        // be receiving messages when interesting events occur. However since we won't 
        // have been able to actually 'subscribe' until the connection was successful,
        // a call to 'subscribe' might not have been performed yet. ??????????????? REVIEW THIS !!!!!!!!!!!
        channel.AsChannelBase().IsSubscribedToValueChangeCallbacks.Should().BeTrue() ;
        // Now we'll wait (A) for the channel to connect, and (B) for the initial value
        // to be acquired. If the channel already existed, this method will return immediately.
        bool connected = await channel.HasConnectedAndAcquiredValueAsync() ;
        connected.Should().BeTrue(channelDescriptor.ChannelName) ;
        channelIsConnected_fromMostRecentEventMessage.Should().BeTrue() ; 
        // Now that we know we're connected, there should be an active subscription in place.
        // THIS CHECK COULD BE MOVED INTO 'HasConnectedAndAcquiredValueAsync'  
        channel.AsChannelBase().IsActuallySubscribedToValueChangeCallbacks.Should().BeTrue() ;
        // Write an initial value and wait for confirmation.
        var putInitialValueResult = await channel.PutValueAckAsync(initialValue) ;
        putInitialValueResult.Should().Be(Clf.ChannelAccess.PutValueResult.Success,channelDescriptor.ChannelName) ;
        channelValue_fromMostRecentEventMessage.Should().BeEquivalentTo(initialValue) ;
        valueMostRecentlyWritten = initialValue ;
        // Write a different value and wait for confirmation.
        channelValue_fromMostRecentEventMessage = null ;
        var incrementedValue = Clf.ChannelAccess.Helpers.CreateIncrementedValue(initialValue) ;
        var putIncrementedValueResult = await channel.PutValueAckAsync(incrementedValue) ;
        putIncrementedValueResult.Should().Be(Clf.ChannelAccess.PutValueResult.Success,channelDescriptor.ChannelName) ;
        channelValue_fromMostRecentEventMessage.Should().BeEquivalentTo(incrementedValue) ;
        valueMostRecentlyWritten = incrementedValue ;
        if ( channel.FieldInfo!.DbFieldDescriptor.DbFieldType == Clf.ChannelAccess.DbFieldType.DBF_ENUM_i16 )
        {
          return ; // ???????????????????
        }
        // Write that same 'different value' a 2nd time and wait for confirmation.
        channelValue_fromMostRecentEventMessage = null ;
        putIncrementedValueResult = await channel.PutValueAckAsync(incrementedValue) ;
        putIncrementedValueResult.Should().Be(Clf.ChannelAccess.PutValueResult.Success,channelDescriptor.ChannelName) ;
        channelValue_fromMostRecentEventMessage.Should().BeEquivalentTo(incrementedValue) ;
        // Restore the original value and wait for confirmation.
        channelValue_fromMostRecentEventMessage = null ;
        var restoredValueResult = await channel.PutValueAckAsync(initialValue) ;
        restoredValueResult.Should().Be(Clf.ChannelAccess.PutValueResult.Success,channelDescriptor.ChannelName) ;
        channelValue_fromMostRecentEventMessage.Should().BeEquivalentTo(initialValue) ;
        valueMostRecentlyWritten = initialValue ;
        // Increment the value using the new 'TryPutModifiedValue' API ...
        channelValue_fromMostRecentEventMessage = null ;
        var putModifiedValueResult = await channel.TryPutModifiedValueAckAsync<object>(
          x => Clf.ChannelAccess.Helpers.CreateIncrementedValue(initialValue)
        ) ;
        channelValue_fromMostRecentEventMessage.Should().BeEquivalentTo(incrementedValue) ;
        valueMostRecentlyWritten = incrementedValue ;
      }
      // At this point there should be no existing references to our Channel
      Clf.ChannelAccess.ChannelsRegistry.HasRegisteredChannel(
        channelDescriptor.ChannelName
      ).Should().BeFalse() ;
      // Now use a 'Hub' API to query a channel ...
      {
        Clf.ChannelAccess.GetValueResult getValueResult = await Clf.ChannelAccess.Hub.GetValueInfoAsync(channelDescriptor.ChannelName) ;
        getValueResult.IsSuccess.Should().BeTrue() ;
        getValueResult.ValueInfo.Should().NotBeNull() ;
        getValueResult.ValueInfo.Value.Should().BeEquivalentTo(valueMostRecentlyWritten) ;
      }
      // At this point there should be no existing references to our Channel
      Clf.ChannelAccess.ChannelsRegistry.HasRegisteredChannel(
        channelDescriptor.ChannelName
      ).Should().BeFalse() ;
    }

    private async Task RunTest_C_ENUM ( 
      Clf.ChannelAccess.ChannelDescriptor channelDescriptor
    ) {
      channelDescriptor.DbFieldDescriptor.TryParseValue(
        channelDescriptor.InitialValueAsString.VerifiedAsNonNullInstance(),
        out object? initialValue
      ).Should().BeTrue() ;
      // Aha, because 'Assert' is decorated with [DoesNotReturnIf(false)],
      // the compiler knows that 'initialValue' is definitely non-null
      // from here onwards ...
      System.Diagnostics.Debug.Assert(initialValue is not null) ;
      using ( var channelsHandler = new Clf.ChannelAccess.ChannelsHandler() )
      {
        List<bool> connectionEventsReceived = new() ;
        List<object> valueUpdateEventsReceived = new() ;
        bool? channelIsConnected_fromMostRecentEventMessage = null ;
        object? channelValue_fromMostRecentEventMessage = null ;
        var channel = Clf.ChannelAccess.Hub.GetOrCreateChannel(channelDescriptor.ChannelName) ;
        channelsHandler.InstallChannel(
          channel,
          (isConnected,state) => { 
            g_output?.WriteLine($"MESSAGE : IsConnected => {isConnected}") ;
            connectionEventsReceived.Add(isConnected) ;
            channelIsConnected_fromMostRecentEventMessage = isConnected ;
          },
          (valueInfo,state) => { 
            g_output?.WriteLine($"MESSAGE : Value => {valueInfo.Value_AsDisplayString()}") ;
            valueUpdateEventsReceived.Add(valueInfo.Value) ;
            channelValue_fromMostRecentEventMessage = valueInfo.Value ;
          }
        ) ;
        // Our 'channel' has now been either created, if it didn't already exist,
        // or we've been given a clone of an already-existing channel.
        // Either way, it should at this point be in a state where it's going to
        // be receiving messages when interesting events occur. However since we won't 
        // have been able to actually 'subscribe' until the connection was successful,
        // a call to 'subscribe' might not have been performed yet.
        channel.AsChannelBase().IsSubscribedToValueChangeCallbacks.Should().BeTrue() ;
        // Now we'll wait (A) for the channel to connect, and (B) for the initial value
        // to be acquired. If the channel already existed, this method will return immediately.
        bool connected = await channel.HasConnectedAndAcquiredValueAsync() ;
        connected.Should().BeTrue(channelDescriptor.ChannelName) ;
        channelIsConnected_fromMostRecentEventMessage.Should().BeTrue() ; 
        // Now that we know we're connected, there should be an active subscription in place.
        channel.AsChannelBase().IsActuallySubscribedToValueChangeCallbacks.Should().BeTrue() ;
        // Write an initial value and wait for confirmation.
        var putInitialValueResult = await channel.PutValueAckAsync(initialValue) ;
        g_output?.WriteLine($"putInitialValueResult is {putInitialValueResult}") ;
        //// putInitialValueResult.Should().Be(Clf.ChannelAccess.PutValueResult.Success,recordDescriptor.PvName) ;
        //// channelValue_fromMostRecentEventMessage.Should().BeEquivalentTo(initialValue) ;
        // Write a different value and wait for confirmation.
        var incrementedValue = Clf.ChannelAccess.Helpers.CreateIncrementedValue(initialValue) ;
        var putIncrementedValueResult = await channel.PutValueAckAsync(incrementedValue) ;
        g_output?.WriteLine($"putIncrementedValueResult is {putIncrementedValueResult}") ;
        //// putIncrementedValueResult.Should().Be(Clf.ChannelAccess.PutValueResult.Success,recordDescriptor.PvName) ;
        //// channelValue_fromMostRecentEventMessage.Should().BeEquivalentTo(incrementedValue) ;
      }
    }

    // TODO SOON :
    //   Strings of more than 39 characters
    //   Arrays of zero length => 'ECA_MESSAGE_BADCOUNT'
    //   Large arrays of data
    //   Enums with longer names and up to 16 options
    // [Theory]
    // [InlineData("")]
    // public async Task RunTest_ExpectedToFail ( string recordDescriptor_encoded ) 
    // {
    //   // var recordDescriptor = Clf.ChannelAccess.RecordDescriptor.FromEncodedString(recordDescriptor_encoded) ;
    // }

  }

}
