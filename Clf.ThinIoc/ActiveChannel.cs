//
// ActiveChannel.cs
// 

using Clf.ChannelAccess.ExtensionMethods ;

namespace Clf.ThinIoc
{

  internal class ActiveChannel
  {

    public readonly Clf.ChannelAccess.ChannelDescriptor RecordDescriptor ;

    private readonly Clf.ChannelAccess.IChannel m_channel ;

    public System.Action<bool>?                        ConnectionStatusChanged = null ;

    public System.Action<Clf.ChannelAccess.ValueInfo>? CurrentValueChanged = null ;

    private string? m_mostRecentlyWrittenValue = null ;

    public ActiveChannel (
      Clf.ChannelAccess.ChannelDescriptor         recordDescriptor,
      System.Action<bool>?                        connectionStatusChanged = null,
      System.Action<Clf.ChannelAccess.ValueInfo>? currentValueChanged     = null
    ) {
      RecordDescriptor        = recordDescriptor ; 
      ConnectionStatusChanged = connectionStatusChanged ;
      CurrentValueChanged     = currentValueChanged ;
      m_channel = Clf.ChannelAccess.Hub.GetOrCreateChannel(
        RecordDescriptor.ChannelName
      ) ;
      #if SUPPORT_STATE_CHANGE_EVENTS
      m_channel.StateChanged += Channel_StateChanged ;
      #else
        #warning "NEED TO REGISTER WITH MESSENGER EVENTS !!!" ;
        throw new System.NotImplementedException(
          $"NEED TO REGISTER WITH MESSENGER EVENTS !!!"
        ) ;
      #endif
    }

    public string CurrentValue => m_channel.Value_AsDisplayString() ;

    private void Channel_StateChanged ( 
      Clf.ChannelAccess.StateChange  change, 
      Clf.ChannelAccess.ChannelState currentState 
    ) {
      if ( change.DescribesConnectionStatusChange( out bool? isConnected ) )
      {
        ConnectionStatusChanged?.Invoke(isConnected.Value) ;
        if ( 
           m_mostRecentlyWrittenValue == null 
        && RecordDescriptor.InitialValueAsString != null  
        ) {
          // m_channel.PutValue(RecordDescriptor.InitialValue) ;
        }
      }
      else if ( change.DescribesValueChange( out Clf.ChannelAccess.ValueInfo? valueInfo ) )
      {
        CurrentValueChanged?.Invoke(
          valueInfo
        ) ;
      }
    }

    public void WriteValueAsString ( string value )
    {
    }

  }

}