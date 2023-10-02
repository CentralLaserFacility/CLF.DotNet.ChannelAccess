//
// StateChangedMessages.cs
//

namespace Clf.ChannelAccess
{
  public record StateChangedMessage ( ChannelBase Channel, ChannelState ChannelState ) 
  {
    // // Useful for testing, so that we can verify messages were handled ...
    // private int m_numberOfAcknowledgements = 0 ;
    // public int NumberOfAcknowledgements => m_numberOfAcknowledgements ;
    // // Safest to use InterlockedIncrement here
    // // in case messages are handled on different threads
    // public void AcknowledgeReception ( ) 
    // {
    //   System.Threading.Interlocked.Increment(
    //     ref m_numberOfAcknowledgements 
    //   ) ;
    // }
  } 

  public record ValueChangedMessage ( 
    ChannelBase  Channel, 
    ValueInfo    ValueInfo,
    ChannelState ChannelState
  ) : 
  StateChangedMessage(
    Channel,
    ChannelState
  ) {
    public override string ToString ( ) 
    => $"ValueChanged : {Channel.ChannelName} => {ValueInfo.Value_AsDisplayString()}" ;
  }

  public record ConnectionStatusChangedMessage ( 
    ChannelBase  Channel, 
    bool         IsConnected,
    ChannelState ChannelState 
  ) : 
  StateChangedMessage(
    Channel,
    ChannelState
  ) {
    public override string ToString ( ) 
    => $"ConnectionStatusChanged : {Channel.ChannelName} => {IsConnected}" ;
  }

}
