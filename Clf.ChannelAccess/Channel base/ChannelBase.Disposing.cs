//
// Channel_disposing.cs
//

using FluentAssertions;
// using Clf.Common.ExtensionMethods ;
// using static Clf.ChannelAccess.LowLevelApi.EcaCodeExtensionMethods ;
// using System.Diagnostics.CodeAnalysis ;
// using System.Threading.Tasks ;

namespace Clf.ChannelAccess
{

  // Hmm, since this scheme using 'IDisposable' has surprising behaviour,
  // maybe we shoud avoid 'Dispose' and use 'Close' instead ???

  // YES, THIS IS FUNDAMENTALLY A PROBLEMATIC DESIGN, WHICH GOES AGAINST
  // THE SPIRIT OF 'IDispose' EVEN IF IT KINDOF WORKS.
  //
  // BETTER DESIGN :
  //
  // ConcreteChannel : IChannel, IDisposable
  //   Created by the ChannelsHub.
  //   Maintains a count of how many ChannelClient instances 
  //   are referring to it.
  // ChannelClient : IChannel, IDisposable
  //   Created by the ChannelsHub.
  //   Has a reference to the ConcreteChannel.
  //   Forwards all method calls to its Concrete Channel.
  //   Dispose : decrements the count maintained by the Concrete Channel.
  //

  //
  // AHA ... whilst the 'clone' scheme is useful in ViewModels,
  // it kindof gets in the way for applications such as Machine Safety ...
  //

  partial class ChannelBase
  {

    // *** NEED TO EXPLICITLY TEST THIS CLONED REFERENCES SCHEME !!!!

    private bool m_instanceHasActuallyBeenDisposed ;

    //
    // In some cases we want to allow several ChannelWrapper instances
    // to be created, all referring to the same underlying Channel.
    //
    // That's the default behaviour provided by ChannelsHub.GetOrCreateChannel(),
    // which is used in ViewModel code where we might have several IChannel references
    // in different ViewModel instances, and invoke Dispose() on each of them ; last one
    // to leave turns the lights out.
    //
    // ?????????????????????????????????????????????????????????????????????
    // In MachineSafety however, we might be be 'getting' an instance multiple times but
    // won't necessarily have an opportunity to invoke Dispose. So in that case we'll
    // use ChannelsHub.GetOrCreateSingleInstanceOfChannel(), which either creates
    // the Channel or returns the existing instance *without* making it a clone.
    // Hmm, in Machine Safety we should always use a ChannelsMonitor that creates
    // a single collection of relevant Channels on startup ...
    //

    private int m_nReferencesToThisInstance = 1 ; // Aha !! Starts at 1 not zero !!!

    private bool m_cloneReferencesArePermitted = true ;

    internal bool CloneReferencesArePermitted => m_cloneReferencesArePermitted ;

    internal void DisableCloneReferences ( ) 
    {
      m_nReferencesToThisInstance.Should().Be(1) ;
      m_cloneReferencesArePermitted = false ;
    }

    internal int HowManyClonedReferencesExist => m_nReferencesToThisInstance - 1 ;

    internal ChannelBase WithReferenceCountIncremented ( )
    {
      CloneReferencesArePermitted.Should().BeTrue() ;
      m_nReferencesToThisInstance++ ;
      RaiseInterestingEventNotification(
        new ChannelLifetimeNotification.InstanceCloneCreated(this) 
      ) ;
      return this ;
    }

    public void Dispose ( )
    {
      // We're disposing this instance, so decrement its reference count.
      // If the updated reference count has reached zero, it means that
      // this was the last active instance so we can turn the lights out. 
      if ( m_instanceHasActuallyBeenDisposed )
      {
        // Dispose has been called too many times ...
        RaiseInterestingEventNotification(
          new ChannelLifetimeNotification.InstanceHasAlreadyBeenDisposed(this) 
        ) ;
        return ;
      }
      if ( 
        System.Threading.Interlocked.Decrement(
          ref this.m_nReferencesToThisInstance 
        ) == 0 
      ) {
        m_nReferencesToThisInstance.Should().Be(0) ;
        RaiseInterestingEventNotification(
          new ChannelLifetimeNotification.InstanceFullyDisposed(this) 
        ) ;
        DoDisposeActions() ;
        #if SUPPORT_STATE_CHANGE_EVENTS
        if ( HowManyStateChangedEventHandlersCurrentlyAttached != 0 )
        {
          RaiseInterestingEventNotification(
            new AnomalyNotification.SomeStateChangedHandlersWereNotDeregistered(
              HowManyStateChangedEventHandlersCurrentlyAttached
            )
          ) ;
        }
        #endif
        m_instanceHasActuallyBeenDisposed = true ;
      }
      else
      { 
        RaiseInterestingEventNotification(
          new ChannelLifetimeNotification.InstanceCloneDisposed(this) 
        ) ;
      }
    }

  }

}