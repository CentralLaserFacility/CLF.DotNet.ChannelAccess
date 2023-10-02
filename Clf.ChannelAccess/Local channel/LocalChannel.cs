//
// LocalChannel.cs
//

using System.Collections.Generic ;
using System.Threading.Tasks ;
using Clf.Common.ExtensionMethods ;
using Clf.ChannelAccess.ExtensionMethods ;
using System.Linq ;

using FluentAssertions ;

namespace Clf.ChannelAccess
{

  public class LocalChannel : ChannelBase
  {

    public DbFieldDescriptor DbFieldDescriptor => ChannelDescriptor.DbFieldDescriptor ;

    public readonly ChannelDescriptor ChannelDescriptor ;

    public LocalChannel ( 
      ChannelDescriptor channelDescriptor,
      bool              initiallyConnected = true
    ) :
    base(
      new(
        ValidatedChannelName : channelDescriptor.ChannelName.Validated(),
        ValueAccessMode : (
          ValueAccessMode.DBR_CTRL_RequestValueAndAuxiliaryInfo
          // Aha, best to default to using the same mode as a Remote channel,
          // otherwise if we do a GetOrCreate() it will create a new Remote Channel !!
          // ValueAccessMode.DBR_RequestValueAndNothingElse
        )
      )
    ) { 
      ChannelsRegistry.RegisterChannel(this) ;
      ChannelDescriptor = channelDescriptor ;

      if ( initiallyConnected )
      {
        SetConnectionStatus(true) ;
      }
    }

    public override FieldInfo? FieldInfo => m_currentStateSnapshot.CurrentState.FieldInfo ;

    public override Task<bool> HasConnectedAndAcquiredValueAsync ( )
    {
      return Task.FromResult(
         this.IsConnected()
      && this.ValueOrNull() != null
      ) ;
    }

    public override Task<bool> HasConnectedAsync ( )
    {
      return Task.FromResult(
        FieldInfo != null
      ) ;
    }

    public override void PutValue ( object valueToWrite )
    {
      ValueInfo valueInfo = new(
        this,
        valueToWrite,
        FieldInfo!
      ) ;
      if ( this.HasConnectedAndAcquiredValue() )
      {
        SetNewState_OnValueChanged(valueInfo) ;
      }
      else
      {
        SetNewState_OnValueAcquired(valueInfo) ;
      }
    }

    public override Task<PutValueResult> PutValueAsync ( object valueToWrite )
    {
      PutValue(valueToWrite) ;
      return Task.FromResult(PutValueResult.Success) ;
    }

    public override Task<PutValueResult> PutValueAckAsync ( object valueToWrite )
    {
      PutValue(valueToWrite) ;
      return Task.FromResult(PutValueResult.Success) ;
    }

    public override Task<GetValueResult> GetValueAsync ( )
    {
      var currentState = m_currentStateSnapshot.CurrentState ;
      return Task.FromResult(
        currentState.IsConnected
        ? new GetValueResult(
            currentState.ValueInfo!
          )
        : new GetValueResult(
            currentState.ChannelHasConnected
            ? WhyGetValueFailed.TimeoutOnThisQuery
            : WhyGetValueFailed.ChannelWasNeverConnected
          )
      ) ;
    }

    public void SetConnectionStatus ( bool newStatus_isConnected )
    {
      bool wasConnected = m_currentStateSnapshot.CurrentState.IsConnected ;
      if ( wasConnected == newStatus_isConnected )
      {
        // Setting to the same value !!!
        // Just ignore this ... or maybe log a warning ...
        return ;
      }
      if ( newStatus_isConnected )
      {
        if ( this.HasConnectedAndReportedItsFieldInfo() )
        {
          SetNewState_OnConnectionStatusChanged(true) ;
        }
        else
        {
          SetNewState_OnInitialConnectSucceeded(
            new FieldInfo(
              ValidatedChannelName,
              "simulated",
              DbFieldDescriptor
            )
          ) ;
          if ( ChannelDescriptor.InitialValueAsString != null )
          {
            this.TryPutValueParsedFromString(ChannelDescriptor.InitialValueAsString) ;
          }
        }
        m_currentStateSnapshot.CurrentState.IsConnected.Should().BeTrue() ;
      }
      else
      {
        SetNewState_OnConnectionStatusChanged(false) ;
        m_currentStateSnapshot.CurrentState.IsConnected.Should().BeFalse() ;
      }
    }

    public override bool IsSubscribedToValueChangeCallbacks => true ;

    public override bool IsActuallySubscribedToValueChangeCallbacks => true ;

    public override void EnsureIsSubscribedToValueChangeCallbacks ( )
    { }

    public override void SubscribeToValueChangeCallbacks ( )
    { }

  }

}

