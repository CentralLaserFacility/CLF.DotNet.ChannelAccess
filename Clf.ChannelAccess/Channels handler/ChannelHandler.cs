//
// ChannelHandler.cs
//

using System.Threading.Tasks ;

namespace Clf.ChannelAccess
{

  // Handles a single Channel, in the same fashion as a ChannelsHandler.

  public sealed class ChannelHandler : System.IDisposable
  {

    private ChannelsHandler m_channelsHandler ;

    public void Dispose ( )
    {
      m_channelsHandler.Dispose() ;
    }

    public readonly IChannel Channel ;

    public ChannelHandler (
      IChannel                               channel,
      System.Action<bool,ChannelState>?      connectionChangedHandler  = null,
      System.Action<ValueInfo,ChannelState>? valueChangedHandler       = null,
      System.Action<string>?                 unhandledExceptionHandler = null
    ) {
      m_channelsHandler = new ChannelsHandler(
        unhandledExceptionHandler,
        autoRaiseSyntheticEvent : true
      ) ;
      m_channelsHandler.InstallChannelAndEventHandlers(
        Channel = channel,
        connectionChangedHandler,
        valueChangedHandler
      ) ;
    }

    public bool? ChannelConnectedSuccessfully => m_channelsHandler.AllChannelsConnectedSuccessfully ;

    public async Task<bool> TryFinishInitialisationAsync ( )
    {
      return await m_channelsHandler.TryFinishInitialisationAsync() ;
    }

  }

}
