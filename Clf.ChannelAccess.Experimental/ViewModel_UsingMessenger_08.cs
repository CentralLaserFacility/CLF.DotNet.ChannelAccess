//
// ViewModel_UsingMessenger_08.cs
//

using System.Collections.Generic ;
using static CommunityToolkit.Mvvm.Messaging.IMessengerExtensions ;
using FluentAssertions ;
using Xunit ;
using Clf.ChannelAccess.ExtensionMethods ;
using System.Threading.Tasks ;

namespace Clf.ChannelAccess.Experimental.Messenger_08
{

  public sealed class ChannelEventsHandler 
  : CommunityToolkit.Mvvm.Messaging.IRecipient<Clf.ChannelAccess.StateChangedMessage>
  , System.IDisposable
  {

    public ChannelEventsHandler ( )
    {
      // Register with the 'WeakReferenceMessenger' so that this instance
      // will receive notifications whenever someone publishes a 'StateChangedMessage'.
      CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register<Clf.ChannelAccess.StateChangedMessage>(
        this
      ) ;
    }

    public void Dispose ( )
    {
      CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Unregister<Clf.ChannelAccess.StateChangedMessage>(
        this
      ) ;
    }

    private bool InvokeEventHandlersWhenMessagesAreReceived { get ; set ; }

    private List<IChannel> m_channels = new() ;

    private Dictionary<IChannel,System.Action<bool>>      m_connectionEventsDictionary  = new() ;

    private Dictionary<IChannel,System.Action<ValueInfo>> m_valueChangeEventsDictionary = new() ;

    public void GetOrCreateChannel (
      string                   channelName,
      out IChannel             channel,
      System.Action<bool>      connectionChangedHandler,
      System.Action<ValueInfo> valueChangedHandler
    ) {
      channel = Clf.ChannelAccess.Hub.GetOrCreateChannel(channelName) ;
      AddChannelAndEventHandlers(
        channel,
        connectionChangedHandler,
        valueChangedHandler
      ) ;
      RaiseSyntheticEventsOnChannel(channel) ;
    }

    public void GetOrCreateChannel (
      string              channelName,
      out IChannel        channel,
      System.Action<bool> connectionChangedHandler
    ) {
      GetOrCreateChannel (
        channelName,
        out channel,
        connectionChangedHandler,
        valueChangedHandler : value => { }
      ) ;
    }

    public void GetOrCreateChannel (
      string                   channelName,
      out IChannel             channel,
      System.Action<ValueInfo> valueChangedHandler
    ) {
      GetOrCreateChannel (
        channelName,
        out channel,
        connectionChangedHandler : isConnected => { },
        valueChangedHandler
      ) ;
    }

    public void AddChannelAndEventHandlers (
      IChannel                 channel,
      System.Action<bool>      connectionChangedHandler,
      System.Action<ValueInfo> valueChangedHandler
    ) {
      m_channels.Add(channel) ;
      m_connectionEventsDictionary  .Add( channel, connectionChangedHandler ) ;
      m_valueChangeEventsDictionary .Add( channel, valueChangedHandler      ) ;
    }

    public IEnumerable<IChannel> RegisteredChannels => m_channels ;

    bool HandleMessage ( Clf.ChannelAccess.StateChangedMessage message )
    {
      if ( message is Clf.ChannelAccess.ConnectionStatusChangedMessage connectionStatusChangedMessage )
      {
        if ( m_connectionEventsDictionary.TryGetValue(message.Channel,out var handler) )
        {
          handler(connectionStatusChangedMessage.IsConnected) ;
          return true ;
        }
      }
      else if ( message is Clf.ChannelAccess.ValueChangedMessage valueChangedMessage )
      {
        if ( m_valueChangeEventsDictionary.TryGetValue(message.Channel,out var handler) )
        {
          handler(valueChangedMessage.ValueInfo) ;
          return true ;
        }
      }
      return false ;
    }

    private Queue<StateChangedMessage> m_queueHoldingDeferredIncomingMessages = new() ;

    void CommunityToolkit.Mvvm.Messaging.IRecipient<StateChangedMessage>.Receive ( StateChangedMessage message )
    {
      // message.AcknowledgeReception() ;
      if ( InvokeEventHandlersWhenMessagesAreReceived )
      {
        try
        {
          bool handled = HandleMessage(message) ;
        }
        catch ( System.Exception x )
        {
          // Log the exception ??
        }
      }
      else
      {
        m_queueHoldingDeferredIncomingMessages.Enqueue(message) ;
      }
    }

    //
    // Query the status of the channel, and then
    // raise the 'ConnectionStatusChanged' and 'ValueChanged' events
    // on that channel, as if a message had arrived from the IOC.
    //

    public void RaiseSyntheticEventsOnChannel ( IChannel channel )
    {
      ChannelStatesSnapshot status = channel.Snapshot() ;
      m_connectionEventsDictionary[channel](
        status.CurrentState.IsConnected
      ) ;
      ValueInfo? valueInfo = status.CurrentState.ValueInfo ;
      if ( valueInfo != null )
      {
        m_valueChangeEventsDictionary[channel](
          valueInfo
        ) ;
      }
    }

    public void RaiseSyntheticEventsOnAllRegisteredChannels ( )
    {
      m_channels.ForEach(
        channel => {
          RaiseSyntheticEventsOnChannel(channel) ;
        }
      ) ;
    }

    public bool? AllChannelsConnectedSuccessfully { get ; private set ; }

    public async Task<bool> TryFinishInitialisationAsync ( )
    {
      // Here we wait a while for all our channels to report that they've
      // successfully connected. Once that's happened, we can query any
      // of their values etc and also set up event handlers that will 
      // tell us about subsequent changes.
      // This 'AllChannelsConnectedSuccessfully' flag could just be a local variable,
      // but it's useful for client code to know whether or not all the channels connected ...
      AllChannelsConnectedSuccessfully = await RegisteredChannels.WaitForAllChannelsToConnectAndAcquireValues() ;
      if ( AllChannelsConnectedSuccessfully is true )
      {
        // Now that we're sure that all channels have connected,
        // we know that any channel may interact with
        // any other channel (eg to get its current value) without
        // any risk that the value might not yet be available.
        // m_channelEventsHandler
        return true ;
      }
      else
      {
        // Hmm, not all our channels were available !!
        // This is bad, and how we deal with this depends 
        // on the particular situation : which channels failed,
        // which channels were critical, and so on.
        // One option would be to just keep trying,
        // ie jump back to do the 'await' again ... ???
        RegisteredChannels.ForEachChannelThatFailedToConnectAndAcquireValue(
          channel => {
            // Log a warning message ???
          }
        ) ;
        return false ;
      }
    }

  }

  public class ViewModel_UsingMessenger_Base : System.IDisposable
  {

    private ChannelEventsHandler m_channelEventsHandler = new() ;

    protected void AddEventHandlersForChannel (
      IChannel                 channel,
      System.Action<bool>      connectionChangedHandler,
      System.Action<ValueInfo> valueChangedHandler
    ) {
      m_channelEventsHandler.AddChannelAndEventHandlers(
        channel,
        connectionChangedHandler,
        valueChangedHandler
      ) ;
    }

    // protected IChannel GetOrCreateChannel (
    //   string                   channelName,
    //   System.Action<bool>      connectionChangedHandler,
    //   System.Action<ValueInfo> valueChangedHandler
    // ) {
    //   IChannel channel = Clf.ChannelAccess.Hub.GetOrCreateChannel(channelName) ;
    //   m_channelEventsHandler.Add(
    //     channel,
    //     connectionChangedHandler,
    //     valueChangedHandler
    //   ) ;
    //   return channel ;
    // }

    protected void GetOrCreateChannel (
      string                   channelName,
      out IChannel             channel,
      System.Action<bool>      connectionChangedHandler,
      System.Action<ValueInfo> valueChangedHandler
    ) {
      m_channelEventsHandler.GetOrCreateChannel(
        channelName,
        out channel,
        connectionChangedHandler,
        valueChangedHandler
      ) ;
    }

    public bool? AllChannelsConnectedSuccessfully => m_channelEventsHandler.AllChannelsConnectedSuccessfully ;

    // Returns true when all channels have initialised,
    // ie have connected and acquired their values ...

    // We need to ensure that when the ViewModel ceases to exist,
    // 'Dispose' is called on all our Channels ... so that when
    // no ViewModel clients are making use of a particular channel,
    // we close down the Subscription and prevent the IOC from
    // sending us unnecessary updates ...
    //
    // HMM, WOULD THERE BE A BETTER WAY OF DOING THAT ?
    // PERHAPS BY DETECTING THAT THERE ARE NO LONGER ANY
    // ACTIVE RECIPIENTS FOR THE STATE-CHANGED MESSAGE
    // SENT BY PARTICULAR CHANNEL ???
    //
    // We could either (A) close down the subscription as soon as
    // we detect that no handlers have responded to a message,
    // or (B) we could wait for a few seconds' grace period
    // and close the subscription if no events get handled
    // during that time.
    //
    // This would require that anyone who creates a Channel
    // remembers to install a handler that responds to its messages,
    // otherwise if that was the only channel created, the Hub
    // would think that no-one was interested and would immediately
    // unsubscribe. We could detect the omission of a handler by
    // issuing a message immediately as a side effect of GetOrCreate,
    // and ensure that it does get handled by the new subscriber.
    //
    // Note : always-creating-a-subscription would
    // simplify the internals of ChannelAccess quite a bit.
    // Currently the only time we don't need a subscription
    // is when we create a channel for the purpose of doing
    // just a single 'GetValueAsync()'.
    //

    public async Task<bool> TryFinishInitialisationAsync ( )
    {
      return await m_channelEventsHandler.TryFinishInitialisationAsync() ;
      // // Here we wait a while for all our channels to report that they've
      // // successfully connected. Once that's happened, we can query any
      // // of their values etc and also set up event handlers that will 
      // // tell us about subsequent changes.
      // // This 'AllChannelsConnectedSuccessfully' flag could just be a local variable,
      // // but it's useful for client code to know whether or not all the channels connected ...
      // AllChannelsConnectedSuccessfully = await m_channelEventsHandler.RegisteredChannels.WaitForAllChannelsToConnectAndAcquireValues() ;
      // if ( AllChannelsConnectedSuccessfully is true )
      // {
      //   // Now that we're sure that all channels have connected,
      //   // we know that any channel may interact with
      //   // any other channel (eg to get its current value) without
      //   // any risk that the value might not yet be available.
      //   // m_channelEventsHandler
      //   return true ;
      // }
      // else
      // {
      //   // Hmm, not all our channels were available !!
      //   // This is bad, and how we deal with this depends 
      //   // on the particular situation : which channels failed,
      //   // which channels were critical, and so on.
      //   // One option would be to just keep trying,
      //   // ie jump back to do the 'await' again ... ???
      //   m_channelEventsHandler.RegisteredChannels.ForEachChannelThatFailedToConnectAndAcquireValue(
      //     channel => {
      //       // Log a warning message ???
      //     }
      //   ) ;
      //   return false ;
      // }
    }

    protected void RaiseInitialEventsOnAllRegisteredChannels ( )
    => m_channelEventsHandler.RaiseSyntheticEventsOnAllRegisteredChannels() ;

    protected ViewModel_UsingMessenger_Base ( )
    {
    }

    public void Dispose ( )
    {
      m_channelEventsHandler.Dispose() ;
    }

  }

  public class ViewModel_UsingMessenger : ViewModel_UsingMessenger_Base
  {

    // Public read-only properties reporting values acquired from our Channels.
    // Note that if we've 'awaited' all our channels before making this
    // view-model instance available, we can guarantee that 'Value()' will
    // never return null.

    // public double SomePropertyBeingReported => (double) m_channel_A.Value()! ;

    public double SomeOtherPropertyBeingReported => ( (double?) m_channel_B.ValueOrNull() ) ?? 999.0 ;

    // Private variables representing our channels.
    // Note that these don't have to be declared as 'IChannel?'
    // because they are never null - they're created in the constructor.

    private Clf.ChannelAccess.IChannel m_channel_A ;

    private Clf.ChannelAccess.IChannel? m_channel_B ;

    private Clf.ChannelAccess.IChannel m_channel_C ;

    public ViewModel_UsingMessenger ( )
    {
      //
      // In the constructor we can create all our IChannel instances.
      // Each instance will attempt to create a connection to a PV.
      // If that PV is available on the network, that channel will receive
      // a 'connected' message and shortly afterwards a 'value-changed' message
      // that tells us its current value (and data type etc).
      // Note however that these events will only arrive if the Channel instance
      // had to be created afresh ; if there was already a Channel instance,
      // which was already connected, then no events will be raised !!!
      //
      GetOrCreateChannel(
        "aaa",
        out m_channel_A,
        isConnected => Channel_A_ConnectionStatusChanged(isConnected),
        valueInfo   => Channel_A_ValueChanged(valueInfo)
      ) ;
      //
      // Don't do this !!! Because when the event fires, it runs code that
      // expects the value of 'B' to be available. However, (A) that channel
      // might not yet have connected and acquired the Value, and also (B) 
      // if we're very unlucky and our thread gets rescheduled at just the wrong moment,
      // the 'm_channel_C' reference might still be null ... 
      // m_channel_A.StateChanged += Channel_A_StateChanged ; // NO NO NO !!!
      //
      GetOrCreateChannel(
        "bbb",
        out m_channel_B,
        isConnected => { },
        valueInfo   => { }
      ) ;
      //
      GetOrCreateChannel(
        "ccc",
        out m_channel_C,
        isConnected => { },
        valueInfo   => { }
      ) ;
      RaiseInitialEventsOnAllRegisteredChannels() ;
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
        int a = (int) m_channel_A.ValueOrNull()! ; 
        int b = (int) m_channel_B.ValueOrNull()! ; 
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

    // private void Channel_B_StateChanged ( Clf.ChannelAccess.StateChange stateChange, Clf.ChannelAccess.ChannelState state )
    // {
    // }
    // 
    // private void Channel_C_StateChanged ( Clf.ChannelAccess.StateChange stateChange, Clf.ChannelAccess.ChannelState state )
    // {
    // }

  }

  public class Test_ViewModel_UsingMessenger
  {

    // [Theory]
    // [InlineData(true)]
    // [InlineData(false)]
    public void Test_01 ( bool sendValueChangedMessages )
    {
      // NOTE THAT WHILE THE WEAK REFERENCES APPROACH DOES AVOID THE PROBLEM OF
      // THE CONTINUED EXISTENCE OF 'EVENT RECIPIENTS' CAUSING A MEMORY LEAK,
      // MESSAGES WILL STILL BE DELIVERED TO THOSE RECIPIENTS UNTIL SUCH TIME
      // AS THE GARBAGE COLLECTOR RECLAIMS THEM.
      CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Reset() ;
      using var viewModel = new ViewModel_UsingMessenger() ;
      ChannelBase channel = null! ;
      StateChangedMessage message = (
        sendValueChangedMessages
        ? new Clf.ChannelAccess.ValueChangedMessage(channel,null!,null!) 
        : new Clf.ChannelAccess.ConnectionStatusChangedMessage(channel,true,null!) 
      ) ;
      CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(
        message       
      ) ;
      CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(
        message 
      ) ;
      // int nReceptionAcknowledgements = message.NumberOfAcknowledgements ;
      return ; // THE CHECKS ARE EXPECTED TO FAIL ... !!!
      // message.NumberOfAcknowledgements.Should().Be(2) ;
    }

  }

}

