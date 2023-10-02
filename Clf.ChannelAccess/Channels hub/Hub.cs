//
// Hub.cs
//

using System.Runtime.InteropServices;
using static Clf.ChannelAccess.Helpers;
using Clf.ChannelAccess.LowLevelApi.ExtensionMethods;
using static Clf.ChannelAccess.LowLevelApi.EcaCodeExtensionMethods;
using static CommunityToolkit.Mvvm.Messaging.IMessengerExtensions;
using System.Collections.Generic;

namespace Clf.ChannelAccess
{

  public static partial class Hub
  {

    public static IEnumerable<IChannel> GetRegisteredChannelsSnapshot ( ) 
    => ChannelsRegistry.GetRegisteredChannelsSnapshot() ;


    /// <summary>
    /// Safely dispose all registered channels
    /// </summary>
    public static void DisposeAllChannels()
    {
      ChannelsRegistry.DeregisterAllChannels();
    }
  }

}
