//
// LocalChannelTests.cs
//

using Xunit ;
using FluentAssertions ;
using System.Threading.Tasks ;
using Clf.ChannelAccess.ExtensionMethods ;
using System.Linq ;

using static CommunityToolkit.Mvvm.Messaging.IMessengerExtensions ;

namespace Clf_ChannelAccess_Tests
{

  public class LocalChannelTests : System.IDisposable
  {

    private static Xunit.Abstractions.ITestOutputHelper? g_output ;

    // private static readonly Clf.ChannelAccess.ChannelsRegistry g_channelsHub 
    //  = Clf.ChannelAccess.ChannelsRegistry.Instance ;

    public LocalChannelTests ( Xunit.Abstractions.ITestOutputHelper? testOutputHelper )
    {
      Clf.ChannelAccess.Settings.WhichDllsToUse = (
        // Clf.ChannelAccess.WhichDllsToUse.DaresburyReleaseDlls 
        Clf.ChannelAccess.WhichDllsToUse.ClfDebugDlls 
      ) ;

      g_output = testOutputHelper ;
      g_output?.WriteLine(
        $"Test starting at {System.DateTime.Now.ToString("h:mm:ss.fff")}"
      ) ;
      g_output?.WriteLine(
        $"ChannelTests instance created"
      ) ;
      Clf.ChannelAccess.Hub.OnInterestingEvent += LogInterestingEvent ;
    }

    private void LogInterestingEvent ( Clf.ChannelAccess.Notification interestingEvent )
    {
      g_output?.WriteLine(
        $"{interestingEvent}"
      ) ;
    }

    public void Dispose ( )
    {
      // Might be unnecessary - tests run if this is omitted ...
      Clf.ChannelAccess.ChannelsRegistry.DeregisterAllChannels() ;
      Clf.ChannelAccess.Hub.OnInterestingEvent -= LogInterestingEvent ;
      g_output?.WriteLine(
        $"Test disposed at {System.DateTime.Now.ToString("h:mm:ss.fff")}"
      ) ;
    }

    public sealed class OneTestRunningVerifier : System.IDisposable
    {
      private static bool g_testIsRunning = false ;
      private string? m_functionName ;
      public OneTestRunningVerifier ( [System.Runtime.CompilerServices.CallerMemberName] string? functionName = null )
      {
        m_functionName = functionName ;
        g_output?.WriteLine(
          $"Starting test : '{functionName}'"
        ) ;
        g_testIsRunning.Should().BeFalse() ;
        Clf.ChannelAccess.ChannelsRegistry.GetRegisteredChannelsSnapshot().Any().Should().BeFalse() ;
        g_testIsRunning = true ;
      }
      public void Dispose ( )
      {
        g_output?.WriteLine(
          $"Finished test : '{m_functionName}'"
        ) ;
        Clf.ChannelAccess.ChannelsRegistry.DisplayRegisteredChannels(
          line => g_output!.WriteLine(line)
        ) ;
        Clf.ChannelAccess.ChannelsRegistry.DeregisterAllChannels() ;
        g_testIsRunning = false ;
      }
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true  )]
    public async Task can_read_int_value_using_local_channel ( bool initiallyConnected ) 
    {
      using var _ = new OneTestRunningVerifier() ;

      object valueToWriteAndVerify = 123 ;
      // Clf.ChannelAccess.ChannelName channelName 
      string channelName = "myInt" ;

      CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register<Clf.ChannelAccess.StateChangedMessage>(
        this,
        (sender,message) => {
          g_output!.WriteLine($"Messenger message recieved ! {message}") ;
        }
      ) ;

      {
        var localChannel = Clf.ChannelAccess.Hub.CreateLocalChannel(
          Clf.ChannelAccess.ChannelDescriptor.FromEncodedString($"{channelName}|i32"),
          initiallyConnected : initiallyConnected
          // channelsHub => new Clf.ChannelAccess.LocalChannel(
          //   channelsHub,
          //   Clf.ChannelAccess.RecordDescriptor.FromEncodedString(
          //     $"{channelName}|i32"
          //   ),
          //   initiallyConnected : initiallyConnected
          // )
        ) ;
        if ( ! initiallyConnected )
        {
          localChannel.AsLocalChannel().SetConnectionStatus(true) ;
        }
      }

      using Clf.ChannelAccess.IChannel? channel = Clf.ChannelAccess.Hub.GetOrCreateChannel(channelName) ;

      channel.AsLocalChannel().Should().BeOfType<Clf.ChannelAccess.LocalChannel>() ;

      await Clf.ChannelAccess.Hub.PutValueAsync(
        channelName,
        valueToWriteAndVerify
      ) ;

      bool connected = await channel.HasConnectedAndAcquiredValueAsync() ;
      channel.ValueOrNull().Should().BeEquivalentTo(valueToWriteAndVerify) ;

      // Exercise the 'parse-from-string' API
      var putValueResult = await Clf.ChannelAccess.Hub.PutValueFromStringAsync(
        channelName,
        valueToWriteAndVerify.ToString()!
      ) ;
      var valueReadBack = await Clf.ChannelAccess.Hub.GetValueOrNullAsync(channelName) ;

      valueReadBack.Should().BeEquivalentTo(valueToWriteAndVerify) ; 

    }

    [Theory]
    // [InlineData( false )]
    [InlineData( true  )]
    public async Task can_read_and_write_double_value_using_local_channel ( bool initiallyConnected ) 
    {
      using var _ = new OneTestRunningVerifier() ;

      object valueToWriteAndVerify = 123.0 ;
      // Clf.ChannelAccess.ChannelName channelName 
      string channelName = "myF64" ;

      CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register<Clf.ChannelAccess.StateChangedMessage>(
        this,
        (sender,message) => {
          g_output!.WriteLine($"Messenger message recieved ! {message}") ;
        }
      ) ;

      var recordDescriptor = Clf.ChannelAccess.ChannelDescriptor.FromEncodedString(
        $"{channelName}|f64|0.123"
      ) ;

      {
        var localChannel = Clf.ChannelAccess.Hub.CreateLocalChannel(
          recordDescriptor,
          initiallyConnected : initiallyConnected
        ) ;
        if ( ! initiallyConnected )
        {
          (
            (Clf.ChannelAccess.LocalChannel) localChannel 
          ).SetConnectionStatus(true) ;
        }
      }

      using var channelsHandler = new Clf.ChannelAccess.ChannelsHandler() ;

      bool?                        channel_isConnected = null ;
      Clf.ChannelAccess.ValueInfo? channel_valueInfo   = null ;

      var channel = Clf.ChannelAccess.Hub.GetOrCreateChannel(recordDescriptor.ChannelName) ;
      channelsHandler.InstallChannel(
        channel,
        (isConnected,state) => { 
          channel_isConnected = isConnected ;
        },
        (valueInfo,state) => { 
          channel_valueInfo = valueInfo ;
        }
      ) ;

      bool connected = await channel.HasConnectedAndAcquiredValueAsync() ;
      await Clf.ChannelAccess.Hub.PutValueAckAsync(
        channelName,
        valueToWriteAndVerify
      ) ;

      // channel.Value().Should().BeEquivalentTo(valueToWriteAndVerify) ;

      // Exercise the 'parse-from-string' API
      var putValueResult = await Clf.ChannelAccess.Hub.PutValueFromStringAsync(
        channelName,
        valueToWriteAndVerify.ToString()!
      ) ;
      var valueReadBack = await Clf.ChannelAccess.Hub.GetValueOrNullAsync(channelName) ;

      valueReadBack.Should().BeEquivalentTo(valueToWriteAndVerify) ; 

    }

    [Theory]
    // [InlineData( false )]
    [InlineData( true, "f64|0.123"  )]
    [InlineData( true, "enum:aa,bb,cc|0"  )]
    [InlineData( true, "enum:aa,bb,cc|1"  )]
    [InlineData( true, "enum:aa,bb,cc|2"  )]
    public async Task can_read_and_write_value_using_local_channel ( bool initiallyConnected, string typeAndInitialValue ) 
    {
      using var _ = new OneTestRunningVerifier() ;

      // Clf.ChannelAccess.ChannelName channelName 
      string channelName = "myChannel" ;

      CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register<Clf.ChannelAccess.StateChangedMessage>(
        this,
        (sender,message) => {
          g_output!.WriteLine($"Messenger message recieved ! {message}") ;
        }
      ) ;

      var recordDescriptor = Clf.ChannelAccess.ChannelDescriptor.FromEncodedString(
        $"{channelName}|{typeAndInitialValue}"
      ) ;

      object valueToWriteAndVerify = null! ;

      {
        var localChannel = Clf.ChannelAccess.Hub.CreateLocalChannel(
          recordDescriptor,
          initiallyConnected : initiallyConnected
        ) ;
        if ( ! initiallyConnected )
        {
          (
            (Clf.ChannelAccess.LocalChannel) localChannel 
          ).SetConnectionStatus(true) ;
        }
        valueToWriteAndVerify = localChannel.Value()! ;
      }

      using var channelsHandler = new Clf.ChannelAccess.ChannelsHandler() ;

      bool?                        channel_isConnected = null ;
      Clf.ChannelAccess.ValueInfo? channel_valueInfo   = null ;

      var channel = Clf.ChannelAccess.Hub.GetOrCreateChannel(recordDescriptor.ChannelName) ;

      // Doing a 'GetOrCreate()' asking for the same channel name,
      // should get us the same underlying IChannel instance
      var channel2 = Clf.ChannelAccess.Hub.GetOrCreateChannel(recordDescriptor.ChannelName) ;
      channel2.AsLocalChannel().Should().Be(channel.AsLocalChannel()) ;

      channelsHandler.InstallChannel(
        channel,
        (isConnected,state) => { 
          channel_isConnected = isConnected ;
        },
        (valueInfo,state) => { 
          channel_valueInfo = valueInfo ;
        }
      ) ;

      bool connected = await channel.HasConnectedAndAcquiredValueAsync() ;
      await Clf.ChannelAccess.Hub.PutValueAckAsync(
        channelName,
        valueToWriteAndVerify
      ) ;

      // channel.Value().Should().BeEquivalentTo(valueToWriteAndVerify) ;

      // Exercise the 'parse-from-string' API
      var putValueResult = await Clf.ChannelAccess.Hub.PutValueFromStringAsync(
        channelName,
        valueToWriteAndVerify.ToString()!
      ) ;
      var valueReadBack = await Clf.ChannelAccess.Hub.GetValueOrNullAsync(channelName) ;

      valueReadBack.Should().BeEquivalentTo(valueToWriteAndVerify) ; 

    }

  }

}
