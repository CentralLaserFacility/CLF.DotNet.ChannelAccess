//
// ChannelsRegistry_static_fields.cs
//

using System.Collections.Generic ;
using System.Linq ;
using System.Threading.Tasks ;
using FluentAssertions ;

using Clf.Common.ExtensionMethods ;
using static Clf.ChannelAccess.LowLevelApi.EcaCodeExtensionMethods ;
using Clf.ChannelAccess.ExtensionMethods ;

namespace Clf.ChannelAccess
{

  partial class ChannelsRegistry 
  {

    public static string Version { get ; private set ; } = "??Version??" ;

    internal static LowLevelApi.ContextHandle ClientContextHandle { get ; private set ; } = new() ;

  }

}
