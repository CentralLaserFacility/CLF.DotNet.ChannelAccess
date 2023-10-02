//
// ChannelsRegistry_logging.cs
//

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Clf.ChannelAccess.ExtensionMethods;
using Clf.Common.ExtensionMethods;
using System.Diagnostics.CodeAnalysis;

namespace Clf.ChannelAccess
{

  partial class ChannelsRegistry 
  {

    public static System.Action<Clf.Common.LogMessageLevel,string>? HandleMessageToSystemLog ;

    public static void SendMessageToSystemLog ( Clf.Common.LogMessageLevel level, string messageLine )
    {
      HandleMessageToSystemLog?.Invoke(level,messageLine) ;
      #if DEBUG
        // Make sure that in DEBUG mode, we always see the message
        if ( HandleMessageToSystemLog is null )
        {
          // TODO: Serilog
          System.Diagnostics.Debug.WriteLine(
            $"{level} : {messageLine}"
          ) ;
        }
      #endif
    }

    public static void HandleChannelStateChangeNotification ( ChannelStatesSnapshot channelStateDescriptor )
    {
      // Hmm, currently this does nothing, but we might use it
      // to log all the State Change Notifications ???
    }

  }

}
