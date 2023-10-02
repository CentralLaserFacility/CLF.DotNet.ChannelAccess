//
// RemoteChannel_callbackHandling.cs
//

using FluentAssertions ;

using static Clf.ChannelAccess.LowLevelApi.DllFunctions ; // Brings the extension methods into scope ...

namespace Clf.ChannelAccess
{

  internal sealed partial class RemoteChannel
  {

    // All these methods are invoked from a callback function via the Hub,
    // on a worker thread that will have been created in the CA DLL.

    internal void HandleConnectionCallback ( System.IntPtr pChannel, bool connected )
    {

      m_channelHandle.SetAsNonNull(pChannel) ;

      RaiseInterestingEventNotification(
        new ProgressNotification.CallbackNotification($"ConnectionCallback connected={connected}")
      ) ;

      if ( m_initialConnectionEvent.IsNotSet )
      {
        // Our event is not set, so this is the initial connect !!
        connected.Should().BeTrue() ;

        if ( m_channelHandle.ca_field_type_reports_no_access() )
        {
          // The return from 'ca_field_type' reports DBF_NO_ACCESS,
          // which means (??) the channel is not actually available.
          m_initialConnectionEvent.TrySet() ;
          return ;
        }

        // Hmm, this might be useful to know ... however this API reports a junk value,
        // even now that we've waited until we've successfully connected to the channel ...

        // double beaconPeriod = LowLevelApi.DllFunctions.ca_beacon_period(m_channelHandle) ;

        // Now that we've successfully connected,
        // we can query the channel for the PV's data type etc
        // and install this metadata into the 'CurrentState'

        // We raise the 'notification' *before* issuing the state change
        // so that a debugging log will show the notification first,
        // followed by any messages outputted as a result of the state change

        RaiseInterestingEventNotification(
          new CommsNotification.ConnectionEstablished()
        ) ;

        SetNewState_OnInitialConnectSucceeded(
          newlyAcquiredFieldInfo : new FieldInfo(
            ChannelName   : ValidatedChannelName,
            HostIpAndPort : m_channelHandle.ca_host_name(),
            new DbFieldDescriptor(
              DbFieldType           : m_channelHandle.ca_field_type(),
              ElementsCountOnServer : m_channelHandle.ca_element_count(),
              IsWriteable           : m_channelHandle.ca_write_access()
            )
          )
        ) ;

        if ( m_shouldSubscribeToValueChangeCallbacksWhenConnectSucceeds )
        {
          ActuallySubscribeToValueChangeCallbacks() ;
        }

        // Someone might be waiting for this event,
        // so signal it ...

        m_initialConnectionEvent.TrySet() ;

      }
      else
      {
        // We're being notified of a 'subsequent' connect or disconnect,
        // ie not the first 'connect'
        RaiseInterestingEventNotification(
          connected
          ? new CommsNotification.ConnectionRestored()
          : new CommsNotification.ConnectionLost()
        ) ;
        SetNewState_OnConnectionStatusChanged(connected) ;        
      }
    }

    #if SUPPORT_VALUE_CHANGE_THROTTLING
    // To support 'throttling' of excessively rapid value updates.
    // For this 'remote channel', we record the time that the most recent
    // value-change update occurred. If the 'MinimumTimeBetweenPublishedValueUpdates' property
    // has been configured (eg to a non null value such as 100mS), then we'll
    // activate some logic in the DllCallbackHandler which will
    // discard any updates that arrive until that timeout has expired.
    public System.DateTime? TimeWhenMostRecentValueChangeNotified = null ;
    #endif

    public void HandleValueUpdateCallback ( ValueInfo decodedValue )
    {
      if ( m_valueAcquiredEvent.IsNotSet )
      {
        // A client may be waiting for this event to be set,
        // because acquiring the PV's value for the first time
        // is a significant happening ... the client can now proceed
        // to populate a UI with valid data, for example.
        RaiseInterestingEventNotification(
          new CommsNotification.ValueAcquired(
            decodedValue.Value_AsDisplayString()
          )
        ) ;
        double timeOfValueAcquisition_inSecondsAfterChannelCreation = (
          System.DateTime.Now - this.CreationTimeStamp
        ).TotalSeconds ; 
        if ( timeOfValueAcquisition_inSecondsAfterChannelCreation > Settings.LongestExpectedValueAcquisitionTimeInSecs )
        {
          RaiseInterestingEventNotification(
            new CommsNotification.ValueAcquired_AfterSuspiciouslyLongDelay(
              decodedValue.Value_AsDisplayString()
            )
          ) ;
        }
        SetNewState_OnValueAcquired(decodedValue) ;
        // Normally setting the event will succeed and return true.
        // However under some circumstances, we could find that the event
        // has already been set, unexpectedly, and calling 'Set' instead
        // of 'TrySet' will throw an exception. If this call returns
        // false, it's unusual but not serious.
        bool setEventSucceeded = m_valueAcquiredEvent.TrySet() ;
        if ( setEventSucceeded is false )
        {
          RaiseInterestingEventNotification(
            new AnomalyNotification.EventHasAlreadyBeenSet(
              "ValueAcquired"
            )
          ) ;
        }
      }
      else
      {
        // Once an initial callback event has been raised,
        // our 'm_valueAcquiredEvent' will have been
        // set. So this is a 'subsequent update', due to
        // our subscription, and we just inform the client.
        // No-one is 'waiting' on the event ... except that
        // if we've recently performed a write, we might be
        // waiting for an update, so let's set this event.
        // Setting it more than once is fine.

        // m_valueUpdateReceived.TrySet() ; // ???????? ORIGINALLY HERE ...

        RaiseInterestingEventNotification(
          new CommsNotification.ValueChangeNotified(
            decodedValue.Value_AsDisplayString()
          )
        ) ;
        SetNewState_OnValueChanged(decodedValue) ;

        m_valueUpdateReceived.TrySet() ; // ONLY SET ONCE THE CLIENT HAS BEEN INFORMED AND RESPONDED !!!

      }
    }

    internal void HandleValueQueryCallback ( ValueInfo decodedValue )
    {
      RaiseInterestingEventNotification(
        new CommsNotification.ValueQueryCompleted(
          decodedValue.Value_AsDisplayString()
        )
      ) ;
      SetNewState_OnValueChanged(decodedValue) ;
      // Normally setting the event will succeed and return true.
      // However under some circumstances, we find that the event
      // has already been set, unexpectedly, and calling 'Set' instead
      // of 'TrySet' will throw an exception. This could happen as a result
      // of several 'value queries' being issued ? If this call returns
      // false, it's unusual but not something to worry about.
      bool setEventSucceeded = m_valueQueryCompletedEvent.TrySet() ;
      if ( setEventSucceeded is false )
      {
        RaiseInterestingEventNotification(
          new AnomalyNotification.EventHasAlreadyBeenSet(
            "ValueQueryCompleted"
          )
        ) ;
      }
    }

    internal void HandleWriteCompletedCallback ( LowLevelApi.ValueUpdateNotificationEventArgs args )
    {
      try
      {
        //
        // Even though the callback provides a 'ValueUpdateNotificationEventArgs'
        // structure, the structure contains a 'pDbr' which is set to null, and 
        // also the 'dbrType' is set to a nonsense value of -1. In other words
        // we're not getting a confirmation of the actual value that has been written.
        // The only valid field is the 'ecaStatus', which will tell us if
        // there's been an error.
        //
        // object? valueConfirmed = Unsafe.CreateValueFromDbRecordStruct(
        //   args.pDbr,
        //   (DbFieldType) args.dbrType,
        //   args.nElements
        // ) ;
        // valueConfirmed.Should().NotBeNull() ;
        // valueConfirmed.Should().BeEquivalentTo(valueToWrite) ;
        // Value = valueConfirmed ;
        //
        args.pDbr.Should().Be(System.IntPtr.Zero) ;
        args.tagValue.Should().Be(ChannelIdentifier) ;
        if ( args.ecaStatus == LowLevelApi.ApiConstants.ECA_NORMAL )
        {
          // All we need to do here is flag our 'write completed' event,
          // so that the main thread will be able to continue past its 'await'.
          // Since we've got an active subscription, we'll shortly receive an
          // unsolicited notification telling us about the updated value.
          m_writeCompletedEvent.TrySet(true) ;
        }
        else
        {
          // Hmm, we need a mechanism to set an exception !!!
          m_writeCompletedEvent.TrySet(false) ;
        }
      }
      catch ( System.Exception x )
      {
        x.ToString(); //TODO: Handle exception in Log... suppressing warning
      }
    }

  }

}