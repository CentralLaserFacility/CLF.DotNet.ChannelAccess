//
// RemoteChannel_subscription.cs
//

using FluentAssertions ;
using Clf.Common.ExtensionMethods ;
using Clf.ChannelAccess.ExtensionMethods ;
using static Clf.ChannelAccess.LowLevelApi.EcaCodeExtensionMethods ;
using System.Diagnostics.CodeAnalysis ;

namespace Clf.ChannelAccess
{

  partial class RemoteChannel
  {

    //
    // Hmm, even if we've invoked 'SubscribeToValueChangeCallbacks'
    // we won't necessarily become 'subscribed' immediately, that can
    // only happen once we've successfully connected. So we report 'true'
    // if either (A) we're actually subscribed, or (B) it's out intention
    // to subscribe as soon as the connection succeeds.
    //

    public override bool IsSubscribedToValueChangeCallbacks => (
       m_subscriptionHandle.IsValidHandle
    || m_shouldSubscribeToValueChangeCallbacksWhenConnectSucceeds
    ) ;

    public override bool IsActuallySubscribedToValueChangeCallbacks => (
       m_subscriptionHandle.IsValidHandle
    ) ;

    public override void EnsureIsSubscribedToValueChangeCallbacks ( )
    {
      if ( ! IsSubscribedToValueChangeCallbacks ) 
      {
        SubscribeToValueChangeCallbacks() ;
      }
    }

    private bool m_shouldSubscribeToValueChangeCallbacksWhenConnectSucceeds = false ;

    public override void SubscribeToValueChangeCallbacks ( )
    {

      if ( IsSubscribedToValueChangeCallbacks )
      {
        // We're already subscribed !!!
        RaiseInterestingEventNotification(
          new AnomalyNotification.UnexpectedApiCall()
        ) ;
        return ;
      }

      bool isCurrentlyConnected = this.IsConnected() ;
      bool hasBeenConnected = this.HasConnectedAndReportedItsFieldInfo() ;
      if ( ! isCurrentlyConnected ) // CHECK THIS !!!
      {
        // Oops, we haven't yet had a response to our Connection request,
        // so we can't yet invoke 'ca_create_subscription' as we 
        // don't know the 'dbrType' to ask for. Let's just set this flag
        // so that when (or if!) the connection completes, and we have the
        // FieldInfo that's necessary to determine the dbrType, 
        // we'll invoke this function again.
        m_shouldSubscribeToValueChangeCallbacksWhenConnectSucceeds = true ;
        return ;
      }

      ActuallySubscribeToValueChangeCallbacks() ;

    }

    private void ActuallySubscribeToValueChangeCallbacks ( )
    {

      FieldInfo.Should().NotBeNull() ;

      bool shouldFetchAuxiliaryInfo = FieldInfo!.FieldCategory switch {
        EnumValField => (
                          Settings.ShouldFetchEnumInfoOnEveryEnumFieldAccess
                          ? true                                          // Always fetch 'enum' info
                          : FieldInfo.DbFieldDescriptor.EnumNames is null // Only the first time
                        ),
        ValField     => true,
        OtherField   => false,
        _            => throw FieldInfo.FieldCategory.AsUnexpectedValueException()
      } ;

      int dbFieldTypeCode = (int) FieldInfo!.DbFieldDescriptor.DbFieldType ;
      LowLevelApi.DbRecordRequestType dbrType = (LowLevelApi.DbRecordRequestType) (
        shouldFetchAuxiliaryInfo
        ? dbFieldTypeCode + (int) LowLevelApi.DbRecordRequestType.DBR_CTRL_ // Hack ... 28 !!!
        : dbFieldTypeCode
      ) ;

      // When we create the subscription we configure it to send *all*
      // the available elements. If the server has been set up with an
      // array of very large capacity, it might send up that whole array
      // every time even if we're not interested in all of them.
      // However, note that the 'ValueUpdateNotificationEventArgs' that holds
      // the sent data has a field that tells us the number of elements,
      // so depending on how the server is coded, it might not be sending us
      // an unnecessarily large number of elements if that's not necessary.

      int nElementsToSendInEachMonitorMessage = FieldInfo.DbFieldDescriptor.ElementsCountOnServer ;

      m_subscriptionHandle = LowLevelApi.DllFunctions.ca_create_subscription(
        channel              : m_channelHandle,
        dbrType              : dbrType,
        count                : nElementsToSendInEachMonitorMessage,
        whichFieldsToMonitor : WhichFieldsToMonitor.MonitorAllFields,
        valueUpdateCallback  : DllCallbackHandlers.MonitorEventCallbackHandler,
        userArg              : this.ChannelIdentifier
      ) ;
      m_subscriptionHandle.IsValidHandle.Should().BeTrue() ;
      RaiseInterestingEventNotification(
        new ProgressNotification.ApiCallCompleted("ca_create_subscription")
      ) ;
      LowLevelApi.DllFunctions.ca_flush_io() ;
    }

    private void ClearSubscription ( )
    {
      IsActuallySubscribedToValueChangeCallbacks.Should().BeTrue() ;
      LowLevelApi.DllFunctions.ca_clear_subscription(m_subscriptionHandle) ;
      RaiseInterestingEventNotification(
        new ProgressNotification.ApiCallCompleted("ca_clear_subscription")
      ) ;
      m_subscriptionHandle = new LowLevelApi.SubscriptionHandle() ;
      IsActuallySubscribedToValueChangeCallbacks.Should().BeFalse() ;
    }

  }

}