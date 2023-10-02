//
// ViewModel_UsingMessenger_01.cs
//

using System.Collections.Generic ;
using static CommunityToolkit.Mvvm.Messaging.IMessengerExtensions ;
using FluentAssertions ;
using Xunit ;
using Clf.ChannelAccess.ExtensionMethods ;
using System.Threading.Tasks;

namespace Clf.ChannelAccess.Experimental.Messenger_01
{

  public class ViewModel_UsingMessenger 
  : CommunityToolkit.Mvvm.Messaging.IRecipient<Clf.ChannelAccess.StateChangedMessage>
  {

    // Hmm, note that we can't call async methods here,
    // without decalaring this as 'async void' ...

    public void Receive ( StateChangedMessage message )
    {
      // Hmm, we can find a neater way of handling this
      // eg by installing the action handlers into a dictionary,
      // but you get the general idea ...
      if ( message is Clf.ChannelAccess.ValueChangedMessage valueChangedMessage )
      {
        if ( message.Channel == m_channel_A )
        {
          // AHA !!! COULD TELL THE MESSAGE THAT IT'S BEEN INTERESTING ...
          // THEN, IF THE SENDER FINDS THAT NO-ONE HANDLED IT,
          // IT MEANS THAT THE CHANNEL ISN'T BEING LISTENED TO ... 
          // message.HasBeenHandled(this) ;
          // message.RecipientsCount++ ;
          Channel_A_ValueChanged(valueChangedMessage.ValueInfo) ;
        }
        // else if ( message.Channel == m_channel_B )
        // {
        //   Channel_B_ValueChanged(valueChangedMessage.ValueInfo) ;
        // }
        // if ( message.Channel == m_channel_C )
        // {
        //   Channel_C_ValueChanged(valueChangedMessage.ValueInfo) ;
        // }
      }
      else if ( message is Clf.ChannelAccess.ConnectionStatusChangedMessage connectionStatusChangedMessage )
      {
        if ( message.Channel == m_channel_A )
        {
          Channel_A_ConnectionStatusChanged(connectionStatusChangedMessage.IsConnected) ;
        }
      }
    }

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

    private Clf.ChannelAccess.IChannel m_channel_B ;

    private Clf.ChannelAccess.IChannel m_channel_C ;

    public ViewModel_UsingMessenger ( )
    {
      //
      // Here we could set up a dictionary to handle events ...
      //
      // AddEventhandlersForChannel(
      //   m_channel_A,
      //   bool => { /* Code to handle connection-status-changed */ 
      //   valueInfo => { /* Code to handle value-changed */ }
      // ) ;
      // AddEventhandlersForChannel(
      //   m_channel_B,
      //   bool => { /* Code to handle connection-status-changed */ 
      //   valueInfo => { /* Code to handle value-changed */ }
      // ) ;
      //
      // In the constructor we can create all our IChannel instances.
      // Each instance will attempt to create a connection to a PV.
      // If that PV is available on the network, that channel will receive
      // a 'connected' message and shortly afterwards a 'value-changed' message
      // that tells us its current value (and data type etc).
      m_channel_A = Clf.ChannelAccess.Hub.GetOrCreateChannel("aaa") ;
      // Don't do this !!! Because when the event fires, it runs code that
      // expects the value of 'B' to be available. However, (A) that channel
      // might not yet have connected and acquired the Value, and also (B) 
      // if we're very unlucky and our thread gets reschedued at just the wrong moment,
      // the 'm_channel_C' reference might still be null ... 
      // m_channel_A.StateChanged += Channel_A_StateChanged ; // NO NO NO !!!
      m_channel_B = Clf.ChannelAccess.Hub.GetOrCreateChannel("bbb") ;
      m_channel_C = Clf.ChannelAccess.Hub.GetOrCreateChannel("ccc") ;
    }

    private void Channel_A_ConnectionStatusChanged ( bool connected )
    {
    }

    private void Channel_A_ValueChanged ( Clf.ChannelAccess.ValueInfo valueInfo )
    {
      // Let's suppose that we need to respond in a way that involves 'B' ...
      // These query methods just tell us whether the Values are available,
      // and if they aren't, we don't wait ; waiting would require an async call.
      if ( 
         m_channel_A.HasConnectedAndAcquiredValue()
      && m_channel_B.HasConnectedAndAcquiredValue() 
      ) {
        // We know that the Values are available !!!
        int a = (int) m_channel_A.ValueOrThrow()! ; 
        int b = (int) m_channel_B.ValueOrThrow()! ; 
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

    public bool AllChannelsConnectedSuccessfully { get ; private set ; }

    // Returns true when all channels have initialised,
    // ie have connected and acquired their values ...

    public async Task<bool> TryFinishInitialisationAsync ( )
    {
      // Here we wait a while for all our channels to report that they've
      // successfully connected. Once that's happened, we can query any
      // of their values etc and also set up event handlers that will 
      // tell us about subsequent changes.
      var allChannels = new[]{
        m_channel_A,
        m_channel_B, 
        m_channel_C
      } ;
      // This 'AllChannelsConnectedSuccessfully' flag could just be a local variable,
      // but it's useful for client code to know whether or not all the channels connected ...
      AllChannelsConnectedSuccessfully = await allChannels.WaitForAllChannelsToConnectAndAcquireValues() ;
      if ( AllChannelsConnectedSuccessfully )
      {
        // Now that we're sure that all channels have connected,
        // we know that any channel may interact with
        // any other channel (eg to get its current value) without
        // any risk that the value might not yet be available.
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
        allChannels.ForEachChannelThatFailedToConnectAndAcquireValue(
          channel => {
            // Log a warning message ???
          }
        ) ;
        return false ;
      }
    }

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

    private bool m_disposeHasBeenInvoked = false ;

    protected virtual void Dispose ( bool disposing )
    {
      if ( ! m_disposeHasBeenInvoked )
      {
        if ( disposing )
        {
          m_channel_A.Dispose() ;
          m_channel_B.Dispose() ;
          m_channel_C.Dispose() ;
        }
        m_disposeHasBeenInvoked = true ;
      }
    }

    public void Dispose ( )
    {
      Dispose(disposing:true) ;
      System.GC.SuppressFinalize(this) ;
    }

  }

  public class Test_ViewModel_UsingMessenger
  {

    // [Fact]
    public void Test_01 ( )
    {
      
    }

  }

}

