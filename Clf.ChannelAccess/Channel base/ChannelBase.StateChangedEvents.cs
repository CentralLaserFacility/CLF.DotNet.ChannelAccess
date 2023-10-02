//
// Channel.cs
//

using FluentAssertions ;
using Clf.Common.ExtensionMethods ;
using static Clf.ChannelAccess.LowLevelApi.EcaCodeExtensionMethods ;
using Clf.ChannelAccess.ExtensionMethods ;
using System.Diagnostics.CodeAnalysis ;
using System.Linq ;

using static CommunityToolkit.Mvvm.Messaging.IMessengerExtensions ;

namespace Clf.ChannelAccess
{

  partial class ChannelBase
  {

    #if SUPPORT_STATE_CHANGE_EVENTS

    //
    // ***************************
    //
    // As an alternative to using an 'event' for the State change, we can instead broadcast
    // a message using the MVVM Toolkit 'WeakReferenceMessenger' ? That removes the requirement
    // to -= the client code's message handler, which is tricky to do consistently.
    // There will be resource leaks if the -= isn't done exactly right.
    // 
    // Other advantages of Messenger : (1) the sender doesn't have to be concerned about
    // the possibility of the receiver throwing an exception, (2) the message can be sent
    // asynchronously, so the sending thread doesn't get blocked while the receiver
    // is dealing with the message (eg repainting a graph of waveform data).
    //
    // Disadvantage is that it can make debugging harder. 
    //

    private event System.Action<StateChange,ChannelState>? m_stateChangedEvent ;

    private object m_stateChangedEvent_syncLock = new() ;

    // Interesting possibility : as soon as a handler has been attached,
    // that handler could be IMMEDIATELY invoked, with parameters
    // that represent the most recent change and the current state.

    public virtual void OnStateChangedHandlerAdded ( )
    { }

    public virtual void OnLastStateChangedHandlerRemoved ( )
    { }

    public event System.Action<StateChange,ChannelState>? StateChanged 
    {
      add 
      {
        // Explicit lock is required here, because although the compiler generated implementation
        // of 'add' is thread safe (and without locks!) we need to protect more than just the event.
        // HMM - DOES THIS LEAVE US EXPOSED TO A DEADLOCK ??? THE CALL TO 'SubscribeToValueChangeCallbacks'
        // INVOKES METHODS ON THE HUB ... BUT ONLY TO CALL 'RaiseInterestingEventNotification' 
        // SO THAT'S PROBABLY OK ...
        lock ( m_stateChangedEvent_syncLock )
        {
          int nRegisteredEventsBeforeAdd = HowManyStateChangedEventHandlersCurrentlyAttached ;
          OnStateChangedHandlerAdded() ;
          m_stateChangedEvent += value ;
          int nRegisteredEventsAfterAdd = HowManyStateChangedEventHandlersCurrentlyAttached ;
          nRegisteredEventsAfterAdd.Should().Be(
            nRegisteredEventsBeforeAdd + 1
          ) ;
        }
      }
      remove 
      {
        lock ( m_stateChangedEvent_syncLock )
        {
          int nRegisteredEventsBeforeRemove = HowManyStateChangedEventHandlersCurrentlyAttached ;
          nRegisteredEventsBeforeRemove.Should().BeGreaterThanOrEqualTo(1) ;
          m_stateChangedEvent -= value ;
          int nRegisteredEventsAfterRemove = HowManyStateChangedEventHandlersCurrentlyAttached ;
          nRegisteredEventsAfterRemove.Should().Be(
            nRegisteredEventsBeforeRemove - 1
          ) ;
          if ( nRegisteredEventsAfterRemove == 0 )
          {
            OnLastStateChangedHandlerRemoved() ;
          }
        }
      }
    }

    // This helper invokes each delegate on m_stateChangedEvent's InvocationList
    // taking care to catch any exception that might be thrown !!

    private void InvokeStateChangedEvent ( )
    {
      var delegatesToInvoke = m_stateChangedEvent?.GetInvocationList() ?? new System.Delegate[0] ;
      foreach ( System.Delegate delegateToInvoke in delegatesToInvoke )
      {
        try
        {
          delegateToInvoke.DynamicInvoke(
            m_currentStateSnapshot.StateChange,
            m_currentStateSnapshot.CurrentState
          ) ;
        }
        catch ( System.Reflection.TargetInvocationException x )
        {
          // ????????????
          RaiseInterestingEventNotification(
            new AnomalyNotification.UnexpectedException(x.InnerException!) 
          ) ;
        }
        catch ( System.Exception x )
        {
          RaiseInterestingEventNotification(
            new AnomalyNotification.UnexpectedException(x) 
          ) ;
        }
      }
    }

    public void RaiseSyntheticStateChangedEventFromMessage ( StateChangedMessage message )
    {
      // Fake !!!
      StateChange? stateChange = null ;
      if ( message is ConnectionStatusChangedMessage connectionStatusChangedMessage )
      {
        stateChange = new StateChange.ConnectionStatusChanged(
          connectionStatusChangedMessage.IsConnected
        ) ;
      }
      else if ( message is ValueChangedMessage valueChangedMessage )
      {
        stateChange = new StateChange.ValueChanged(
          valueChangedMessage.ValueInfo,
          IsInitialAcquisition : true // ????????????
        ) ;
      }
      var delegatesToInvoke = m_stateChangedEvent?.GetInvocationList() ?? new System.Delegate[0] ;
      foreach ( System.Delegate delegateToInvoke in delegatesToInvoke )
      {
        try
        {
          delegateToInvoke.DynamicInvoke(
            stateChange!,
            m_currentStateSnapshot.CurrentState
          ) ;
        }
        catch ( System.Reflection.TargetInvocationException x )
        {
          // ????????????
          RaiseInterestingEventNotification(
            new AnomalyNotification.UnexpectedException(x.InnerException!) 
          ) ;
        }
        catch ( System.Exception x )
        {
          RaiseInterestingEventNotification(
            new AnomalyNotification.UnexpectedException(x) 
          ) ;
        }
      }
    }

    // Aha, this could solve the problem of getting a leak
    // if clients forget to unsubscribe ???

    private void DetachAllStateChangedEventHandlers ( )
    {
      // https://stackoverflow.com/questions/447821/how-do-i-unsubscribe-all-handlers-from-an-event-for-a-particular-class-in-c
      System.Delegate[]? delegatesToRemove = m_stateChangedEvent?.GetInvocationList() ;
      if ( 
        delegatesToRemove != null 
      && delegatesToRemove.Any()
      ) {
        foreach ( System.Delegate delegateToRemove in delegatesToRemove )
        {
          m_stateChangedEvent -= (System.Action<StateChange,ChannelState>) delegateToRemove ;
        }
      }
    }

    public int HowManyStateChangedEventHandlersCurrentlyAttached
    => m_stateChangedEvent?.GetInvocationList().Length ?? 0 ;

    private void NotifyClients_StateHasChanged_UsingStateChangedEvent ( )
    {
      try
      {
        // Best to 'Send' via a 'PostOrInvoke' which will
        // make use of the Synchronisation Context if available ...
        ChannelsRegistry.PostOrInvoke(
          sendOrPostCallbackDelegate : static ( object? parameterPassedToDelegate ) => {
            var thisChannel = (ChannelBase) parameterPassedToDelegate! ;
            thisChannel.InvokeStateChangedEvent() ;
          },
          stateParameterToPassToDelegate : this
        ) ;
      }
      catch ( System.Exception x )
      {
        // Hmm, should raise a warning ?
        // But this needs to guarantee to not throw !!!
        Hub.NotifyExceptionCaught(x) ;
      }
    }

    // Invoke the specified 'stateChangedHandler' function
    // passing in the most recent change, and the current state.

    public void InvokeStateChangedHandler ( 
      System.Action<StateChange,ChannelState> stateChangedHandler
    ) {
      stateChangedHandler(
        m_currentStateSnapshot.StateChange,
        m_currentStateSnapshot.CurrentState
      ) ;
    }

    public static void InvokeStateChangedHandler (
      ChannelStatesSnapshot                   currentStateSnapshot,
      System.Action<StateChange,ChannelState> stateChangedHandler
    ) {
      stateChangedHandler(
        currentStateSnapshot.StateChange,
        currentStateSnapshot.CurrentState
      ) ;
    }

    public static void InvokeStateChangedHandler (
      IChannel                                channel,
      System.Action<StateChange,ChannelState> stateChangedHandler
    ) {
      var currentStateDescriptor = channel.Snapshot() ;
      stateChangedHandler(
        currentStateDescriptor.StateChange,
        currentStateDescriptor.CurrentState
      ) ;
    }

    #endif

  }

}