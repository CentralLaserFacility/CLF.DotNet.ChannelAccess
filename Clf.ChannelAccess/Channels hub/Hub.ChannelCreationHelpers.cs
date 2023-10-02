//
// Hub_channel_creation_helpers.cs
//

using System.Collections.Generic ;
using System.Linq ;
using System.Runtime.InteropServices ;
using FluentAssertions ;
using static Clf.ChannelAccess.Helpers ;
using Clf.ChannelAccess.ExtensionMethods ;
using Clf.ChannelAccess.LowLevelApi.ExtensionMethods ;
using Clf.Common.ExtensionMethods ;
using static Clf.ChannelAccess.LowLevelApi.EcaCodeExtensionMethods ;
using System.Diagnostics.CodeAnalysis ;
using static CommunityToolkit.Mvvm.Messaging.IMessengerExtensions ;

namespace Clf.ChannelAccess
{

  partial class Hub
  {

    private static IChannel CreateChannelInstance ( 
      ChannelName      channelName,       
      ValueAccessMode? valueAccessMode = null
    ) {
      ChannelsRegistry.HasRegisteredChannel(
        channelName,
        valueAccessMode ?? channelName.Validated().DefaultValueAccessMode()
      ).Should().BeFalse() ;
      IChannel channel = new RemoteChannel(
        channelName,
        valueAccessMode ?? channelName.Validated().DefaultValueAccessMode()
      ) ;
      return channel ;
    }

    //
    // This is the one and only point at which we create a Channel !!
    //

    private static IChannel GetOrCreateChannel_ReturningInvalidChannelOnException ( 
      string                channelName, 
      System.Func<IChannel> createChannel 
    ) {
      try
      {
        return new ChannelWrapper(
          createChannel().AsChannelBase()
        ) ;
      }
      catch ( System.Exception x ) 
      { // TODO: Add this to the log
        return new InvalidChannel(
          channelName,
          x.Message
        ) ;
      }
    }

  }

}
