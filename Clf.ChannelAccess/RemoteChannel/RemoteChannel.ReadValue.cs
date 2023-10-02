//
// RemoteChannel_read_value.cs
//

using FluentAssertions ;
using Clf.Common.ExtensionMethods ;
using static Clf.ChannelAccess.LowLevelApi.EcaCodeExtensionMethods ;
using System.Diagnostics.CodeAnalysis ;
using System.Threading.Tasks ;

namespace Clf.ChannelAccess
{

  partial class RemoteChannel
  {

    // This is useful if we haven't subscribed to changes ...

    // This always uses the 'default' timeout as specified in the Settings.

    public async override Task<GetValueResult> GetValueAsync ( )
    {
      if ( m_currentStateSnapshot.CurrentState.ChannelHasConnected is false )
      {
        // Since we've not yet connected we can't initiate the read request,
        // so we'll wait until the connection has been established ...
        bool hasConnected = await HasConnectedAsync() ;
        if ( ! hasConnected )
        {
          return new GetValueResult(
            WhyGetValueFailed.ChannelWasNeverConnected
          ) ;
        }
      }
      if ( IsSubscribedToValueChangeCallbacks )
      {
        // Hmm, calling 'GetValueAsync' was not necessary
        // as this channel is getting value updates.
        // We'll just return the latest value we've been told about.
        RaiseInterestingEventNotification(
          new AnomalyNotification.UsageWarningNotification(
            "Unnecessary call to 'GetValueAsync' on a channel that has the Value available"
          )
        ) ;
        bool hasConnectedAndAcquiredValue = await HasConnectedAndAcquiredValueAsync() ;
        if ( ! hasConnectedAndAcquiredValue )
        {
          return new GetValueResult(
            WhyGetValueFailed.TimeoutOnThisQuery
          ) ;
        }
      }
      else
      {
        InitiateReadRequest(
          FieldInfo!.DbFieldDescriptor
        ) ;
        await m_valueQueryCompletedEvent.Task.ConfigureAwait(false) ;
      }
      return new GetValueResult(
        m_currentStateSnapshot.CurrentState.ValueInfo!
      ) ;
    }

    // The descriptor tells us the maximum number of elements 
    // that can be provided by the server ... we might be asking
    // for *all* of these, or just for the first few ???

    private void InitiateReadRequest(
      DbFieldDescriptor dbFieldDescriptor
    ) {  
      // We'll request a fetch of *all* the available elements
      int nElementsWanted = dbFieldDescriptor.ElementsCountOnServer ;
      // TODO : we should deal explcitly with some specific error codes ???
      //  ECA_BADCOUNT   - Requested count larger than native element count
      //  ECA_NORDACCESS - Read access denied
      //  ECA_DISCONN    - Channel is disconnected
      RaiseInterestingEventNotification(
        new ProgressNotification.ActionNotification("InitiateReadRequest")
      ) ;
      m_valueQueryCompletedEvent.Reset() ;
      LowLevelApi.DbRecordRequestDescriptor dbrDescriptor = new LowLevelApi.DbRecordRequestDescriptor(
        dbFieldType     : dbFieldDescriptor.DbFieldType,
        valueAccessMode : this.ValueAccessMode
      ) ;
      LowLevelApi.DllFunctions.ca_array_get_callback(
        channel             : m_channelHandle,
        type                : dbrDescriptor.DbrType,
        nElementsWanted     : nElementsWanted,
        valueUpdateCallBack : DllCallbackHandlers.ValueQueryEventCallbackHandler,
        userArg             : (System.IntPtr) ChannelIdentifier
      ) ;
      RaiseInterestingEventNotification(
        new ProgressNotification.ApiCallCompleted(
          $"ca_array_get_callback with DBR type {(int)dbrDescriptor.DbrType} = {dbrDescriptor.DbrType} ; nElementsWanted={nElementsWanted}"
        )
      ) ;
      // AHA ! Must call 'flush' otherwise the message isn't sent to the server
      // immediately. If we forget to call 'flush', the message *will* eventually
      // get sent, but not until the default timeout period of 30 secs has elapsed,
      // in which case the callback handler won't be invoked until that 30 secs has elapsed.
      // Forgetting to call 'flush' causes quite a lot of confusion !!!
      LowLevelApi.DllFunctions.ca_flush_io() ;
    }

  }

}