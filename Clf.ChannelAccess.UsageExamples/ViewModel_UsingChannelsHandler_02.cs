//
// ViewModel_UsingChannelsHandler_02.cs
//

using static CommunityToolkit.Mvvm.Messaging.IMessengerExtensions ;
using FluentAssertions ;
using Clf.ChannelAccess.ExtensionMethods ;
using System.Threading.Tasks ;

namespace Clf_ChannelAccess_UsageExamples 
{

  public class ViewModel_UsingChannelsHandler_02 : System.IDisposable
  {

    private Clf.ChannelAccess.ChannelsHandler m_channelEventsHandler ;

    public bool? AllChannelsConnectedSuccessfully => m_channelEventsHandler.AllChannelsConnectedSuccessfully ;

    // Returns true when all channels have initialised,
    // ie have connected and acquired their values ...

    public async Task<bool> TryFinishInitialisationAsync ( )
    {
      return await m_channelEventsHandler.TryFinishInitialisationAsync() ;
    }

    public void Dispose ( )
    {
      m_channelEventsHandler.Dispose() ;
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
    // Nominally these don't necessarily have to be declared as 'IChannel?'
    // because they are never null - they're created in the constructor.
    // 
    // However, when we've configured the ChannelEventsHandler to raise events
    // as soon as each channel has been instantiated, it's actually best practice
    // to declare them as 'IChannel?' - because there are situations where
    // an event handler would be invoked before all the channels have been created.
    //

    private Clf.ChannelAccess.IChannel? m_channel_A ;

    private Clf.ChannelAccess.IChannel? m_channel_B ;

    private Clf.ChannelAccess.IChannel? m_channel_C ;

    public ViewModel_UsingChannelsHandler_02 ( )
    {
      m_channelEventsHandler = new(
        autoRaiseSyntheticEvent : true
      ) ;
      m_channelEventsHandler.InstallChannel(
        m_channel_A = Clf.ChannelAccess.Hub.GetOrCreateChannel("aaa"),
        (isConnected,state) => Channel_A_ConnectionStatusChanged(isConnected),
        (valueInfo,state)   => Channel_A_ValueChanged(valueInfo)
      ) ;
      //
      m_channelEventsHandler.InstallChannel(
        m_channel_B = Clf.ChannelAccess.Hub.GetOrCreateChannel("bbb"),
        (isConnected,state) => { },
        (valueInfo,state)   => { }
      ) ;
      //
      m_channelEventsHandler.InstallChannel(
        m_channel_C = Clf.ChannelAccess.Hub.GetOrCreateChannel("ccc"),
        (isConnected,state) => { },
        (valueInfo,state)   => { }
      ) ;
    }

    private void Channel_A_ConnectionStatusChanged ( bool connected )
    {
    }

    private void Channel_A_ValueChanged ( Clf.ChannelAccess.ValueInfo valueInfo )
    {
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
      using var viewModel = new ViewModel_UsingChannelsHandler_02() ;
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
      using var viewModel_A = new ViewModel_UsingChannelsHandler_02() ;
      using var viewModel_B = new ViewModel_UsingChannelsHandler_02() ;
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
