//
// ViewModel_UsingChannelsHandler.cs
//

using static CommunityToolkit.Mvvm.Messaging.IMessengerExtensions ;
using FluentAssertions ;
using Clf.ChannelAccess.ExtensionMethods ;
using System.Threading.Tasks ;

namespace Clf_ChannelAccess_UsageExamples 
{

  public class ViewModel_UsingChannelsHandler_01 : System.IDisposable
  {

    private Clf.ChannelAccess.ChannelsHandler m_channelsHandler ;

    public bool? AllChannelsConnectedSuccessfully => m_channelsHandler.AllChannelsConnectedSuccessfully ;

    // Returns true when all channels have initialised,
    // ie have connected and acquired their values ...

    public async Task<bool> TryFinishInitialisationAsync ( )
    {
      return await m_channelsHandler.TryFinishInitialisationAsync() ;
    }

    public void Dispose ( )
    {
      m_channelsHandler.Dispose() ;
    }

    // Public read-only properties reporting values acquired from our Channels.
    // Note that if we've 'awaited' all our channels before making this
    // view-model instance available, we can guarantee that 'Value()' will
    // never return null.

    // public double SomePropertyBeingReported => (double) m_channel_A.Value()! ;

    public double SomeOtherPropertyBeingReported => ( (double?) m_channel_B.Value() ) ?? 999.0 ;

    //
    // Private variables representing our channels.
    //

    private Clf.ChannelAccess.IChannel m_channel_A ;

    private Clf.ChannelAccess.IChannel m_channel_B ;

    private Clf.ChannelAccess.IChannel m_channel_C ;

    public ViewModel_UsingChannelsHandler_01 ( )
    {

      //
      // Here we're creating the 'channelEventsHandler' which will
      // own all the channels. Note that since we configure it with
      // an argument of 'false', no events will be raised until
      // the final call to 'RaiseSyntheticEventsOnAllRegisteredChannels'.
      // 

      m_channelsHandler = new(
        exceptionMessage => { 
          /* log an unhandled exception raised in an event handler */ 
        },
        autoRaiseSyntheticEvent : false
      ) ;      

      m_channelsHandler.InstallChannel(
        m_channel_A = Clf.ChannelAccess.Hub.GetOrCreateChannel("aaa"),
        connectionStatusChangedHandler : (isConnected,ChannelState_ExtensionMethods) => Channel_A_ConnectionStatusChanged(isConnected),
        valueChangedHandler            : (valueInfo,state)   => Channel_A_ValueChanged(valueInfo)
      ) ;

      m_channelsHandler.InstallChannel(
        m_channel_B = Clf.ChannelAccess.Hub.GetOrCreateChannel("bbb"),
        // connectionStatusChangedHandler : null, // No harm in specifying this, but not necessary
        valueChangedHandler               : (valueInfo,state) => { }
      ) ;

      m_channelsHandler.InstallChannel(
        m_channel_C = Clf.ChannelAccess.Hub.GetOrCreateChannel("ccc"),
        connectionStatusChangedHandler : (isConnected,state) => { }
        // valueChangedHandler         : null // No harm in specifying this, but not necessary
      ) ;

      // Now that all the channels have been created :
      //  - Raise events on each channel, based on the current states
      //  - Enable the handling of incoming events
      m_channelsHandler.RaiseSyntheticEvents() ;
    }

    private void Channel_A_ConnectionStatusChanged ( bool connected )
    {
    }

    private void Channel_A_ValueChanged ( Clf.ChannelAccess.ValueInfo valueInfo )
    {
      // Let's suppose that we need to respond in a way that involves 'B' ...
      // These query methods just tell us whether the Values are available,
      // and if they aren't, we don't wait ; waiting would require an async call.
      if ( m_channel_B.HasConnectedAndAcquiredValue() ) 
      {
        // We know that the Values are available !!!
        int a = (int) m_channel_A.Value()! ; 
        int b = (int) m_channel_B.Value()! ; 
        m_channel_C.PutValue(
          a + b
        ) ;
        // return true ; ???
      }
      else
      {
        // Hmm, tricky !!!
        // The best we could do is retry when both A and B become available ...
        // perhaps by adding a reference to this method to a queue ???
        // And then, re-invoking when any of the channels change ???
        // BUT THAT GETS VERY COMPLEX ; BEST ROUTE WOULD BE TO DISALLOW
        // ANY PUT-VALUE OPERATIONS INSIDE A VALUE-CHANGED HANDLER !!!
        // SO THAT 'PUT-VALUE' WOULD ONLY BE CALLED FROM A UI EVENT
        // return false ; ???
      }
    }

    // [Theory]
    // [InlineData(true)]
    // [InlineData(false)]
    public static void Test_01 ( bool sendValueChangedMessages )
    {
      // NOTE THAT WHILE THE WEAK REFERENCES APPROACH DOES AVOID THE PROBLEM OF
      // THE CONTINUED EXISTENCE OF 'EVENT RECIPIENTS' CAUSING A MEMORY LEAK,
      // MESSAGES WILL STILL BE DELIVERED TO THOSE RECIPIENTS UNTIL SUCH TIME
      // AS THE GARBAGE COLLECTOR RECLAIMS THEM - WHICH MIGHT BE NEVER.
      var viewModel = new ViewModel_UsingChannelsHandler_01() ;
      Clf.ChannelAccess.ChannelBase channel = null! ;
      Clf.ChannelAccess.StateChangedMessage message = (
        sendValueChangedMessages
        ? new Clf.ChannelAccess.ValueChangedMessage(channel,null!,null!) 
        : new Clf.ChannelAccess.ConnectionStatusChangedMessage(channel,true,null!) 
      ) ;
      Clf.ChannelAccess.Settings.Messenger.Send(
        message       
      ) ;
      Clf.ChannelAccess.Settings.Messenger.Send(
        message 
      ) ;
      // message.NumberOfAcknowledgements.Should().Be(2) ;
      viewModel.Dispose() ;
      // Once we've invoked Dispose, our ViewModel will have re-registered
      // its interest in those messages, so sending it again should not
      // result in its 'NumberOfAcknowledgements' being incremented ...
      Clf.ChannelAccess.Settings.Messenger.Send(
        message 
      ) ;
      // message.NumberOfAcknowledgements.Should().Be(2) ;
    }

    // [Theory]
    // [InlineData(true)]
    // [InlineData(false)]
    public static void Test_02 ( bool sendValueChangedMessages )
    {
      var viewModel_A = new ViewModel_UsingChannelsHandler_01() ;
      var viewModel_B = new ViewModel_UsingChannelsHandler_01() ;
      Clf.ChannelAccess.ChannelBase channel = null! ;
      Clf.ChannelAccess.StateChangedMessage message = (
        sendValueChangedMessages
        ? new Clf.ChannelAccess.ValueChangedMessage(channel,null!,null!) 
        : new Clf.ChannelAccess.ConnectionStatusChangedMessage(channel,true,null!) 
      ) ;
      Clf.ChannelAccess.Settings.Messenger.Send(
        message       
      ) ;
      Clf.ChannelAccess.Settings.Messenger.Send(
        message 
      ) ;
      // message.NumberOfAcknowledgements.Should().Be(4) ;
      viewModel_A.Dispose() ;
      // Once we've invoked Dispose, our ViewModel_A will have de-registered
      // its interest in those messages, so sending it again should result 
      // in its 'NumberOfAcknowledgements' being incremented by just ONE,
      // as only 'viewModel_B' will handle the message.
      Clf.ChannelAccess.Settings.Messenger.Send(
        message 
      ) ;
      // message.NumberOfAcknowledgements.Should().Be(5) ;
      viewModel_B.Dispose() ;
      // Once we've invoked Dispose on ViewModel_B it will have de-registered
      // its interest in those messages, so sending it again should NOT result 
      // in our message's 'NumberOfAcknowledgements' being incremented.
      Clf.ChannelAccess.Settings.Messenger.Send(
        message 
      ) ;
      // message.NumberOfAcknowledgements.Should().Be(5) ;
    }

  }

}
