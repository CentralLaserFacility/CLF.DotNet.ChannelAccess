//
// ChannelBase_state_updates.cs
//

using Clf.Common.ExtensionMethods ;
using static CommunityToolkit.Mvvm.Messaging.IMessengerExtensions ;

namespace Clf.ChannelAccess
{

  partial class ChannelBase
  {

    protected void SetNewState ( 
      System.Func<ChannelState,ChannelState> stateUpdateFunc,
      StateChange                            stateChange 
    ) {
      // This function will usually be called on a 'foreign' thread,
      // ie one that isn't the thread that was used to create the Channel.
      // Actually that's fine ; we can safely overwrite the 'm_currentStateDescriptor'
      // field, as writing a reference variable is an atomic operation.
      // Immediately after setting the new value, we call 'NotifyClients_ChannelStateHasChanged'
      // which will inform the Channel's clients that a change has occurred.
      // The event will be 'posted' to the client's thread, if this is
      // supported by the current Synchronisation Context ; otherwise, 
      // the event will be raised on the current 'foreign' thread.
      var oldCurrentState = m_currentStateSnapshot.CurrentState ;
      var newCurrentState = stateUpdateFunc(
        oldCurrentState
      ) with { 
        SequenceNumber = oldCurrentState.SequenceNumber + 1
      } ;
      m_currentStateSnapshot = new ChannelStatesSnapshot(
        CurrentState  : newCurrentState,
        PreviousState : oldCurrentState,
        StateChange   : stateChange with { Channel = this } 
      ) ;
      NotifyClients_ChannelStateHasChanged() ;
    }

    protected void SetNewState_OnInitialConnectSucceeded ( FieldInfo newlyAcquiredFieldInfo )
    {
      SetNewState(
        stateUpdateFunc : oldState => oldState with {
          ConnectionStatus = new ChannelConnectionStatus(true,"Initial connect succeeded"),
          FieldInfo        = newlyAcquiredFieldInfo
        },
        new StateChange.ConnectionEstablished(
          newlyAcquiredFieldInfo.VerifiedAsNonNullInstance()
        )
      ) ;
    }

    protected void SetNewState_OnConnectionStatusChanged ( bool isNowConnected )
    {
      bool wasPreviouslyConnected = m_currentStateSnapshot.CurrentState.ConnectionAndValidityStatus.ConnectionStatus.IsConnected ;
      if ( isNowConnected != wasPreviouslyConnected )
      {
        SetNewState(
          stateUpdateFunc : oldState => oldState with {
            ConnectionStatus = new ChannelConnectionStatus(
              isNowConnected,
              isNowConnected
              ? "Channel connection restored"
              : "Channel connection dropped"
            )
            // ConnectionAndValidityStatus = oldState.ConnectionAndValidityStatus with { 
            //   ConnectionStatus = new ChannelIsConnected(isNowConnected) 
            // }
          },
          isNowConnected
          ? new StateChange.ConnectionRestored()
          : new StateChange.ConnectionLost()
        ) ;
      }
      else
      {
        ChannelsRegistry.SendMessageToSystemLog(
          Clf.Common.LogMessageLevel.WarningMessage,
          "Connection status weirdness !!"
        ) ;
      }
    }

    protected void SetNewState_OnValueAcquired ( ValueInfo incomingValue )
    {
      SetNewState(
        stateUpdateFunc : oldState => oldState with {
          ValueInfo = incomingValue
        },
        new StateChange.ValueAcquired(incomingValue)
      ) ;
    }

    protected void SetNewState_OnValueChanged ( ValueInfo incomingValue )
    {
      SetNewState(
        stateUpdateFunc : oldState => oldState with {
          ValueInfo = incomingValue
        },
        new StateChange.ValueChangeNotified(incomingValue)
      ) ;
    }

    protected void SetNewState_OnValidityChanged ( ChannelValidityStatus validityStatus )
    {
      SetNewState(
        stateUpdateFunc : oldState => oldState with {
          ValidityStatus = validityStatus
        },
        new StateChange.ConnectionValidityChanged(validityStatus)
      ) ;
    }

    internal void DeclareChannelInvalid ( string whyNotValid )
    {
      ChannelValidityStatus validityStatus = new(false,whyNotValid) ;
      SetNewState_OnValidityChanged(
        validityStatus
      ) ;
    }

    private void NotifyClients_ChannelStateHasChanged ( ) 
    {
      ChannelsRegistry.HandleChannelStateChangeNotification(
        m_currentStateSnapshot
      ) ;
      #if SUPPORT_STATE_CHANGE_EVENTS
      NotifyClients_StateHasChanged_UsingStateChangedEvent() ;
      #endif
      NotifyClients_StateHasChanged_UsingMessenger() ;
    }

    private void NotifyClients_StateHasChanged_UsingMessenger ( )
    {
      // Create a message to broadcast to all repicients.

      // Each recipient is expected to invoke 'AcknowledgeReception'
      // on the message, so that we know someone was interested.

      StateChangedMessage messageToBroadcast = (
        m_currentStateSnapshot.StateChange switch {
          StateChange.ConnectionStatusChanged 
            connectionStatusChanged => new ConnectionStatusChangedMessage(
              Channel      : this,
              IsConnected  : connectionStatusChanged.IsConnected,
              ChannelState : this.m_currentStateSnapshot.CurrentState
            ),
          StateChange.ValueChanged 
            valueChanged => new ValueChangedMessage(
              Channel      : this,
              ValueInfo    : valueChanged.ValueInfo,
              ChannelState : this.m_currentStateSnapshot.CurrentState
            ),
          _ => throw m_currentStateSnapshot.StateChange.AsUnexpectedValueException()
        } 
      ) ;

      this.RaiseInterestingEventNotification(
        new ProgressNotification.SendingMessengerMessage(
          messageToBroadcast.GetType().Name
        )
      ) ;

      try
      {

        // The ChannelsRegistry will have captured the Synchronisation Context associated
        // with the app. Depending on what kind of App it is, the Synchronisation Context
        // might be null (in which case our only option is to 'invoke' the delegate
        // on the same thread that this method is running on), or if it's not null then
        // we can Post a message to the 'main' thread.

        ChannelsRegistry.PostOrInvoke(
          sendOrPostCallbackDelegate : static ( object? parameterPassedToDelegate ) => {
            var messageToBroadcast = (StateChangedMessage) parameterPassedToDelegate! ;
            Settings.Messenger.Send<StateChangedMessage>(messageToBroadcast) ;
            // int nActiveRecipients = messageToBroadcast.NumberOfAcknowledgements ;
          },
          stateParameterToPassToDelegate : messageToBroadcast
        ) ;

        // NOTE : IN a previous (buggy!) version of that code, we were passing 'this'
        // as the parameter to the delegate, and building the StateChangedMessage inside
        // the body of the delegate by looking at 'this.m_currentStateSnapshot' (same code
        // as shown above). Problem : the delegate gets invoked via the SynchronisationContext
        // some time in the future, and by then a further state changed might have occurred
        // which means that we were sometimes accessing an erroneous 'messageToBroadcast'.

      }
      catch ( System.Exception x )
      {
        // Hmm, should raise a warning ?
        // But this needs to guarantee to not throw !!!
        Hub.NotifyExceptionCaught(this,x) ;
      }
    }

  }

}