//
// RemoteChannel_private_fields.cs
//

namespace Clf.ChannelAccess
{

  partial class RemoteChannel
  {

    // Events that we wait for

    private Clf.Common.AsyncManualResetEvent m_initialConnectionEvent    = new() ;

    private Clf.Common.AsyncManualResetEvent m_valueAcquiredEvent        = new() ;

    private Clf.Common.AsyncManualResetEvent m_valueQueryCompletedEvent  = new() ;

    private Clf.Common.AsyncManualResetEvent m_valueUpdateReceived       = new() ;

    private Clf.Common.AsyncManualResetEvent<bool> m_writeCompletedEvent = new() ;

    // Handles for interaction with the DLL functions

    private readonly LowLevelApi.ChannelHandle m_channelHandle = new() ;

    private LowLevelApi.SubscriptionHandle m_subscriptionHandle = new() ;

  }

}