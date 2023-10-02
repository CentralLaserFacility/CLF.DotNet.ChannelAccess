//
// InvalidChannel.cs
//

using System.Collections.Generic ;
using System.Threading.Tasks ;
using Clf.Common.ExtensionMethods ;
using Clf.ChannelAccess.ExtensionMethods ;
using System.Linq ;

using FluentAssertions ;
using System;

namespace Clf.ChannelAccess
{

  //
  // If client code specifies an improper name when trying to create a channel,
  // such as a PV name that is empty, we don't want to throw an exception,
  // but we'll nevertheless return an instance of an IChannel.
  //
  // The channel we return will have the name as supplied, but will have an Invalid state.
  // Any API calls made will be rejected, silently.
  //
  // A log message will be written on every API call.
  //

  internal class InvalidChannel : IChannel
  {

    public ChannelName ChannelName { get ; }

    // If an explicit ValueAccessMode was not specified in the GetOrCreate(), we will have
    // selected the Mode depending on whether or not the channel name was '.VAL'.
    // However if the channel name was not successfully parsed, we won't have been able
    // to make that choice. So let's 'fake' a value that says we just requested the VAL field.

    public ValueAccessMode ValueAccessMode => ValueAccessMode.DBR_RequestValueAndNothingElse ;

    public InvalidChannel ( 
      ChannelName     channelName,
      string          whyNotValid
    ) { 
      ChannelName = channelName ;
      m_channelStatesSnapshot = new ChannelStatesSnapshot(
        new ChannelState(
          ChannelName      : this.ChannelName,
          ValueAccessMode  : this.ValueAccessMode,
          SequenceNumber   : 0,
          ConnectionStatus : new ChannelConnectionStatus(false,whyNotValid),
          ValidityStatus   : new ChannelValidityStatus(false,whyNotValid),
          FieldInfo        : null,
          ValueInfo        : null
        ),
        null,
        new StateChange.ChannelCreatedAsInvalid()
      ) ;
      // We don't register this channel,
      // because it doesn't have a valid name !!!
      // ChannelsRegistry.RegisterChannel(this) ;
    }

    public DateTime? TimeStampFromServer => null ;

    private readonly ChannelStatesSnapshot m_channelStatesSnapshot ;

    public ChannelStatesSnapshot Snapshot ( ) => m_channelStatesSnapshot ;

    public FieldInfo? FieldInfo => null ;

    public Task<bool> HasConnectedAndAcquiredValueAsync ( )
    {
      return Task.FromResult(false) ;
    }

    public Task<bool> HasConnectedAsync ( )
    {
      return Task.FromResult(false) ;
    }

    public void PutValue ( object valueToWrite )
    {
      // Fire-and-forget : raise a warning in the log
    }

    public Task<PutValueResult> PutValueAsync ( object valueToWrite )
    {
      return Task.FromResult(PutValueResult.RejectedByServer) ;
    }

    public Task<PutValueResult> PutValueAckAsync ( object valueToWrite )
    {
      return Task.FromResult(PutValueResult.RejectedByServer) ;
    }

    public Task<GetValueResult> GetValueAsync ( )
    {
      return Task.FromResult(
        new GetValueResult(
          WhyGetValueFailed.ChannelWasNeverConnected
        )
      ) ;
    }

    public void Dispose ( )
    {
    }

  }

}

