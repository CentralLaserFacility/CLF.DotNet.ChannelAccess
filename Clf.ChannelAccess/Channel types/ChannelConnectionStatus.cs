//
// ChannelConnectionStatus.cs
//

namespace Clf.ChannelAccess
{

  // Alternative to the 'bool' representing 'IsConnected' ...

  public record ChannelConnectionStatus ( 
    bool   IsConnected, 
    string Explanation 
  ) {

    // public static implicit operator ChannelIsConnected ( bool isConnected )
    // => new ChannelIsConnected(isConnected) ;

    public static implicit operator bool ( ChannelConnectionStatus isConnected )
    => isConnected.IsConnected ;

    public override string ToString ( ) 
    {
      string result = (
        IsConnected 
        ? $"connected"
        : $"not-connected"
      ) ;
      if ( ! string.IsNullOrEmpty(Explanation) )
      {
        result += $" : {Explanation}" ;
      }
      return result ;
    }

    public static void UsageExample ( )
    {
      ChannelConnectionStatus x = new(false,"Feeling poorly") ;
      bool z = x ; // Seamlessly convert to bool
      // Usage in a ChannelsHandler would look like this ...
      System.Action<ChannelConnectionStatus,ChannelState> connectionStatusChangedHandler = (
        (isConnected,state) => {
          if ( isConnected )
          {
            // ...
          }
          else
          {
            string whyNotConnected = isConnected.Explanation! ;
          }
        }
      ) ;

    }

  }

}
