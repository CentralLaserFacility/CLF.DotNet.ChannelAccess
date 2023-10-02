//
// RemoteChannel_constructor.cs
//

using FluentAssertions ;
using Clf.Common.ExtensionMethods ;
using static Clf.ChannelAccess.LowLevelApi.EcaCodeExtensionMethods ;
using System.Diagnostics.CodeAnalysis ;
using Clf.ChannelAccess.ExtensionMethods ;

namespace Clf.ChannelAccess
{

  partial class RemoteChannel
  {

    internal RemoteChannel (
      ChannelName     channelName,
      ValueAccessMode valueAccessMode
    ) :
    base(
      new ValidatedChannelNameAndAccessMode(
        channelName.Validated(),
        valueAccessMode
      )
    ) {

      ChannelsRegistry.RegisterChannel(this) ;

      CreateChannel() ;

    }

  }

}