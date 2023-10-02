//
// RemoteChannel.cs
//

using FluentAssertions ;
using Clf.Common.ExtensionMethods ;
using static Clf.ChannelAccess.LowLevelApi.EcaCodeExtensionMethods ;
using System.Diagnostics.CodeAnalysis ;
using System.Linq ;

namespace Clf.ChannelAccess
{

  //
  // This is our slightly-higher-level wrapper around the DLL functions,
  // representing a channel that we're possibly connected to.
  //
  // We might be connecting to the VAL field, or to an 'other' field that isn't 'VAL'.
  //
  // Once we've created the channel and successfully connected
  // we'll know the Field Type, the element count and so on.
  //
  // We won't necessarily have obtained a Value from the channel,
  // but we will have installed callbacks that will inform us
  //  (A) when an updated value is transmitted,
  //  (B) when the connection/disconnection status changes.
  //

  //
  // Notes :
  //
  //   Callbacks always go to a static delegate pointing to a method on the ChannelsHub,
  //   and then get redirected to the appropriate 'Channel' instance.
  //   Thus we avoid the horrible possibility that the callback delegate, or the object
  //   associated with that delegate, might have been garbage collected, unbeknown to the
  //   C runtime. That would be a real possibility if the callback were to be set up
  //   as a method on a 'Channel' instance.
  //
  //   ECA result codes can return various levels of result :
  //   - Success, all fine
  //   - Expected error, such as a timeout or disconnect
  //   - Unexpected error due to a programming error on the client
  //   - Unexpected error due to a failure in the comms or in the server
  //
  //   ????????????????????????????????
  //   We'll deal with 'expected' errors by passing a function in the API's,
  //   which will be invoked if an error occurs. That's better than returning
  //   an error indication as a function result, as that can too easily be ignored.
  //   If the function call expects an error handler function, it's harder 
  //   for client code to omit dealing with that situation.
  //

  internal sealed partial class RemoteChannel : ChannelBase
  {

    //
    // Creating a 'channel' and connecting to it requires network comms operations
    // which will block our thread for a significant period.
    //

    //
    // On a sunny day, we'll successfully connect to the PV and will be informed of
    // the current value. First our 'statusChanged' action will be invoked to tell us
    // that we're connected, then it'll be invoked again to tell us the Value. Those
    // actions get fired *before* the 'await' completes and returns the 'Channel' variable
    // that we'll subsequently use to communicate with the PV. If we access the
    // 'Value' property, it will tell us the same value that was
    // reported in the 'statusChanged' action.
    //
    // We can interact with the PV by querying various properties, and we can submit
    // requests to change the value. Change requests might or might not be honoured by the PV,
    // and it's also possible that a change we asked for might get immediately trumped by a
    // change submitted by someone else. Whenever the PV's value actually changes, our Channel
    // gets notified and our 'statusChanged' action will get invoked telling us about the 
    // latest value (which might or might not be the value we recently asked for).
    // 
    // Our connection to the remote PV might get interrupted at any time, and when that happens
    // our 'statusChanged' action will be invoked - firstly to tell us that we're now disconnected,
    // then a second time to tell us that the Value is null.
    //
    // A rainy-day scenario is also likely. It's possible that the PV we're referring to
    // won't be active at the time we try to connect, and that our 'Connect' attempt will
    // fail with a timeout error. All is not lost however as the PV might nevertheless
    // come alive at a future time. So in this case, the 'statusChanged' action gets invoked
    // firstly to tell us that we're disconnected, and secondly to tell us that the 'Value'
    // is currently 'null' ie not known. Following the 'await', we'll have a valid 'Channel'
    // variable as before, but its 'Value' property will be 'null'.
    //

    //
    // NOTE THAT WE DON'T PROVIDE A WAY OF SETTING UP EVENT HANDLERS THAT MIGHT FIRE
    // BEFORE THE CONNECTION HAS BEEN ESTABLISHED !!! THIS IS ACTUALLY A GOOD THING !!!
    //

    public override FieldInfo? FieldInfo => m_currentStateSnapshot.CurrentState.FieldInfo ;

    public override string ToString ( ) 
    #if DEBUG
    => $"{ChannelName}#{ChannelIdentifier}" ;
    #else
    => $"{ChannelName}" ;
    #endif

    // --------------------------

    #if SUPPORT_STATE_CHANGE_EVENTS
    public override void OnStateChangedHandlerAdded ( )
    {
      if ( ! IsSubscribedToValueChangeCallbacks )
      {
        SubscribeToValueChangeCallbacks() ;
      }
    }
    #endif

    #if SUPPORT_STATE_CHANGE_EVENTS
    public override void OnLastStateChangedHandlerRemoved ( )
    {
      // Now that no subscribers are listening to our event,
      // we can revoke the subscription so that the server
      // will no longer bother to send us 'monitor' updates.
      if ( IsActuallySubscribedToValueChangeCallbacks )
      {
        // NOTE : WE'RE CALLING THIS WITHIN OUR LOCK - IS THAT OK ???
        // MIGHT BE SAFER TO DO THIS OUTSIDE THE LOCK ??? 
        // BUT POTENTIALLY THAT LEAVES US WITH A RACE CONDITION
        // ClearSubscription() ; // NO !!! Rely on the 'Dispose()' to do this ...
      }
      else
      {
        // Hmm, this is an unusual situation. When the first client subscribed
        // to our event, with '+=', we called 'SubscribeToValueChangeCallbacks'
        // so surely we should be 'subscribed' at this point ? Not necessarily though,
        // because if the PV hadn't successfully connected, we won't have had an
        // opportunity to actually Subscribe ; our 'SubscribeToValueChangeCallbacks'
        // will merely have set a flag to indicate that when the connection does happen,
        // we should subscribe automatically.
      }
    }
    #endif

    protected override void DoDisposeActions ( )
    {
      Disconnect() ;
    }


  }

}