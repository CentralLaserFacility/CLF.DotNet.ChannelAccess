//
// RemoteChannel_connection.cs
//

using FluentAssertions ;
using Clf.Common.ExtensionMethods ;
using static Clf.ChannelAccess.LowLevelApi.EcaCodeExtensionMethods ;
using Clf.ChannelAccess.ExtensionMethods ;
using System.Diagnostics.CodeAnalysis ;
using System.Threading.Tasks ;

namespace Clf.ChannelAccess
{

  partial class RemoteChannel
  {

    public bool ChannelHasBeenCreated => m_channelHandle.IsValidHandle ;

    private void CreateChannel ( )
    {
      ChannelHasBeenCreated.Should().BeFalse() ;

      // When we call 'create_channel', the connection callback
      // associated with the Hub will eventually be invoked, and will call
      // 'NotifyConnectionStatusChanged' on this instance. At that point
      // we'll query the channel to find out the ChannelInfo.
      // NOTE : This 'ca_create_channel' call always succeeds,
      // even if the serving IOC is not running. We'd only get a 'timeout' error
      // in a subsequent call to 'ca_pend_io', if we were to try to access the server
      // with a blocking API call. However 'ca_pend_io' doesn't play nicely
      // with 'async' - we get occasional weird clashes, so it's best avoided.

      //
      // Hmm, in the VS2022 build, the callback gets invoked *before* 
      // this function returns, and even before 'ca_create_channel'
      // gets a chance to set 'm_channelHandle' ...
      //

      LowLevelApi.DllFunctions.ca_create_channel(
        m_channelHandle,
        channelName        : ChannelName.Name,
        connectionCallback : DllCallbackHandlers.ConnectionEventCallbackHandler,
        tagValue           : this.ChannelIdentifier
      ) ;

      m_channelHandle.IsValidHandle.Should().BeTrue() ;

      RaiseInterestingEventNotification(
        new ProgressNotification.ApiCallCompleted("ca_create_channel")
      ) ;

      // Is this necessary ? Doesn't seem to do any harm,
      // and a 'flush' *is* necessary for other functions
      // that work with a callback ... but it works fine without.
      // ChannelAccessApi.DllFunctions.ca_flush_io() ;

      LowLevelApi.DllFunctions.ca_puser(m_channelHandle).Should().Be(this.ChannelIdentifier) ;
      
      // When the PV is published by Lewis, we never get a connection callback !!!

      // LowLevelApi.DllFunctions.ca_pend_event(1.0) ; // LEWIS_HACK ... has no effect ...
      // LowLevelApi.DllFunctions.ca_flush_io() ; // LEWIS_HACK
      // LowLevelApi.DllFunctions.ca_pend_io(1.0) ; // LEWIS_HACK

    }

    private void Disconnect ( )
    {
      RaiseInterestingEventNotification(
        new ProgressNotification.ActionNotification("Disconnecting")
      ) ;
      if ( IsActuallySubscribedToValueChangeCallbacks )
      {
        ClearSubscription() ;
      }
      ClearChannel() ;
      ChannelsRegistry.DeRegisterChannel(this) ; 
    }

    private void ClearChannel ( )
    {
      if ( m_channelHandle.IsValidHandle )
      {
        LowLevelApi.DllFunctions.ca_clear_channel(
          m_channelHandle
        ) ;
        m_channelHandle.IsNull.Should().BeTrue() ;
        RaiseInterestingEventNotification(
          new ProgressNotification.ApiCallCompleted("ca_clear_channel")
        ) ;
        // Not be necessary ? But doesn't do any harm ...
        // ChannelAccessApi.DllFunctions.ca_flush_io() ;
      }
    }

    public async override System.Threading.Tasks.Task<bool> HasConnectedAsync ( ) 
    {

      var timeToWaitBeforeReturningFalse = Settings.CommsTimeoutPeriodInEffect ;

      if ( m_channelHandle.IsNull )
      {
        CreateChannel() ;
      }

      bool hasConnected ;

      try
      {

        //
        // Having invoked 'CreateChannel' we now wait for a Connection callback.
        // Until that arrives, we have no idea about the Field type of the channel, so
        // we're not in a position to create a subscription that will eventually tell us
        // about the value.
        //

        //
        // In principle we could 'return' at this point, and rely on the Hub to invoke
        // event-handlers when interesting things happen ; such as a connection having been
        // established (at which point we can create a subscription) and subsequently
        // a change notification arriving.
        //
        // If we were to return now ... the client can potentially access properties of the Channel,
        // and will initially find them all unknown, but that's fine. The client could issue
        // a request to submit a change, and actually that would also be fine ; we'd save the proposed
        // new value, and would send it over to the remote PV once a connection has been established.
        // In the meantime we'd just report that the request is outstanding, and that confirmation
        // has not yet been received.
        //

        RaiseInterestingEventNotification(
          new ProgressNotification.WaitingForEvent("initialConnection")
        ) ;

        //
        // The connection event handler might or might not have already fired.
        // If it hasn't fired, we wait here until it does. If the timeout expires,
        // an exception is thrown.
        //

        await m_initialConnectionEvent.Task.WaitAsync(
          timeToWaitBeforeReturningFalse
        ).ConfigureAwait(false) ;

        // We didn't get an exception, so no timeout, all's good ...

        RaiseInterestingEventNotification(
          new ProgressNotification.WaitCompleted("initialConnection")
        ) ;

        m_initialConnectionEvent.IsSet.Should().BeTrue() ;
        FieldInfo.Should().NotBeNull() ;

        hasConnected = true ;

      }
      catch ( System.Threading.Tasks.TaskCanceledException )
      {
        // An 'await' was terminated with a timeout ... ????????????????????????????????????
        RaiseInterestingEventNotification(
          new CommsNotification.TimeoutExpired("initialConnection (cancelled)")
        ) ;
        hasConnected = false ;
      }
      catch ( System.TimeoutException )
      {
        // An 'await' was terminated with a timeout ...
        RaiseInterestingEventNotification(
          new CommsNotification.TimeoutExpired("initialConnection")
        ) ;
        hasConnected = false ;
      }
      catch ( System.Exception x )
      {
        RaiseInterestingEventNotification(
          new AnomalyNotification.UnexpectedException(x)
        ) ;
        hasConnected = false ;
      }

      return hasConnected ;
    }

    public async override System.Threading.Tasks.Task<bool> HasConnectedAndAcquiredValueAsync ( ) 
    {

      var timeToWaitBeforeReturningFalse = Settings.CommsTimeoutPeriodInEffect ;

      if ( await HasConnectedAsync() is false )
      {
        // We waited the entire time, but didn't connect
        return false ;
      }

      // So, we've successfully connected, and we now know the PV's
      // data type and so on. Next we need to acquire the value.

      bool hasAcquiredValue ;
      string? whatWeAreAwaiting = null ;
      try
      {
        if ( IsSubscribedToValueChangeCallbacks ) 
        {
          whatWeAreAwaiting = "valueAcquired" ;
          RaiseInterestingEventNotification(
            new ProgressNotification.WaitingForEvent(whatWeAreAwaiting)
          ) ;
          // By the time this 'await' returns, the value that was
          // returned in the monitor message will have been set
          // as the most recent result ...
          await m_valueAcquiredEvent.Task.WaitAsync(
            timeToWaitBeforeReturningFalse
          ).ConfigureAwait(false) ;
          m_valueAcquiredEvent.Task.IsCompletedSuccessfully.Should().BeTrue() ;
          RaiseInterestingEventNotification(
            new ProgressNotification.WaitCompleted(whatWeAreAwaiting)
          ) ;
        }
        else
        {
          whatWeAreAwaiting = "GetValueAsync" ;
          // We'll just perform a one-off query
          RaiseInterestingEventNotification(
            new ProgressNotification.WaitingForEvent(whatWeAreAwaiting)
          ) ;
          await GetValueAsync().ConfigureAwait(false) ;
          RaiseInterestingEventNotification(
            new ProgressNotification.WaitCompleted(whatWeAreAwaiting)
          ) ;
        }
        hasAcquiredValue = true ;
      }
      catch ( System.Threading.Tasks.TaskCanceledException x )
      {
        x.ToString(); //TODO: Handle exception in Log... suppressing warning
        // Our 'await' was terminated with a timeout ...
        // NOTE : COMPILER THINKS THAT 'whatWeAreAwaiting'
        // COULD STILL BE NULL AT THIS POINT ???
        RaiseInterestingEventNotification(
          new CommsNotification.TimeoutExpired(whatWeAreAwaiting)
        ) ;
        hasAcquiredValue = false ;
      }
      catch ( System.Exception x )
      {
        RaiseInterestingEventNotification(
          new AnomalyNotification.UnexpectedException(x)
        ) ;
        hasAcquiredValue = false ;
      }
      return hasAcquiredValue ;
    }

  }

}