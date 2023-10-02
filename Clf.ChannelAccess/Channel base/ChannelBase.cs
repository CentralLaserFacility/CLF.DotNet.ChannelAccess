//
// ChannelBase.cs
//

// using Clf.Common.ExtensionMethods ;
// using Clf.ChannelAccess.ExtensionMethods ;
// using System.Diagnostics.CodeAnalysis ;
// using System.Linq ;

using System.Threading.Tasks ;

namespace Clf.ChannelAccess
{
  
  public abstract partial class ChannelBase : System.IDisposable, IChannel
  {

    public System.DateTime CreationTimeStamp { get ; } = System.DateTime.Now ;

    public ChannelName ChannelName => ValidatedChannelName as ChannelName ; // .ShortName_OmittingVAL ;

    public ValueAccessMode ValueAccessMode => ValidatedChannelNameAndAccessMode.ValueAccessMode ;

    internal ValidatedChannelNameAndAccessMode ValidatedChannelNameAndAccessMode { get ; }

    internal ValidatedChannelName ValidatedChannelName => ValidatedChannelNameAndAccessMode.ValidatedChannelName ;

    public System.DateTime? TimeStampFromServer { get ; protected set ; }
    
    public int ChannelIdentifier { get ; }
 
    //
    // The 'state' of the channel evolves as events arrive from the
    // underlying Channel Access DLL. These events are generally raised
    // on a worker thread owned by the DLL. Client code using a Channel
    // should expect the channel's State, as reported by a call to 
    // the GetChannelStateSnapshot() function, to change at any time.
    //

    protected ChannelStatesSnapshot m_currentStateSnapshot = null! ;

    public ChannelStatesSnapshot Snapshot ( ) => m_currentStateSnapshot ;

    internal ChannelBase ( 
      ValidatedChannelNameAndAccessMode channelNameAndAccessMode
    ) {
      ChannelIdentifier = ChannelsRegistry.AllocateNextAvailableChannelIdentifier() ;
      ValidatedChannelNameAndAccessMode = channelNameAndAccessMode ;
      ChannelState channelState = new(
        ChannelName                 : this.ChannelName,
        ValueAccessMode             : this.ValueAccessMode,
        SequenceNumber              : 0,
        ConnectionStatus            : new(false,"Connect has not been attempted"),
        ValidityStatus              : new(true,"Assumed OK"), 
        FieldInfo                   : null,
        ValueInfo                   : null
      ) ;
      ChannelStatesSnapshot currentState = new(
        CurrentState  : channelState,
        PreviousState : null,
        StateChange   : new StateChange.ChannelCreated()
      ) ;
      m_currentStateSnapshot = new(
        channelState,
        null,
        currentState.StateChange
      ) ;
      RaiseInterestingEventNotification(
        new ChannelLifetimeNotification.InstanceCreated(this) 
      ) ;
      if ( ! ValidatedChannelNameAndAccessMode.IsValid() )
      {
        ValidatedChannelNameAndAccessMode = new(
          channelNameAndAccessMode.ValidatedChannelName,
          ValueAccessMode.DBR_RequestValueAndNothingElse
        ) ;
        RaiseInterestingEventNotification(
          new AnomalyNotification.UsageError("Unsupported access mode") 
        ) ;
      }
    }

    protected virtual void DoDisposeActions ( )
    {
    }

    // LOGGING

    public void RaiseInterestingEventNotification ( Notification notification ) 
    {
      if ( notification is AnomalyNotification.UsageError usageError )
      {
        // Force the Channel into a 'not valid' state !!!
        DeclareChannelInvalid(
          $"Usage error : {usageError.UsageErrorInfo}"
        ) ;
      }
      try
      {
        var notificationToSend = notification with { Channel = this } ;
        ChannelsRegistry.PostOrInvoke(
          sendOrPostCallbackDelegate: static state => {
            Hub.HandleNotification(
              (Notification) state!
            ) ;
          },
          stateParameterToPassToDelegate : notificationToSend
        ) ;
      }
      catch ( System.Exception x )
      {
        // Hmm, should raise a warning !
        // But this needs to guarantee to not throw !!!
        Hub.NotifyExceptionCaught(this,x) ;
      }
    }

    public abstract FieldInfo? FieldInfo { get ; }

    public abstract Task<bool> HasConnectedAndAcquiredValueAsync ( ) ;
    
    public abstract Task<bool> HasConnectedAsync ( ) ;
    
    public abstract void PutValue ( object valueToWrite ) ;
    
    public abstract Task<PutValueResult> PutValueAsync ( object valueToWrite ) ;
    
    public abstract Task<PutValueResult> PutValueAckAsync ( object valueToWrite ) ;
    
    public abstract Task<GetValueResult> GetValueAsync ( ) ;

    // A LOCAL CHANNEL IS EFFECTIVELY 'ALWAYS SUBSCRIBED' !!!

    public abstract bool IsSubscribedToValueChangeCallbacks { get ; }

    public abstract bool IsActuallySubscribedToValueChangeCallbacks { get ; }

    // Only relevant for a 'Remote' channel 

    public abstract void EnsureIsSubscribedToValueChangeCallbacks ( ) ;

    // Only used by a 'Remote' channel 

    public abstract void SubscribeToValueChangeCallbacks ( ) ;

    #if SUPPORT_VALUE_CHANGE_THROTTLING
    public System.TimeSpan? MinimumTimeBetweenPublishedValueUpdates { get ; set ; }
    #endif

  }

}