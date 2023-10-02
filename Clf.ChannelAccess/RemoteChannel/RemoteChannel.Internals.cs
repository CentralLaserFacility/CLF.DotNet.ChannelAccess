//
// RemoteChannel_internals.cs
//

using FluentAssertions ;
using Clf.Common.ExtensionMethods ;
using static Clf.ChannelAccess.LowLevelApi.EcaCodeExtensionMethods ;
using Clf.ChannelAccess.ExtensionMethods ;
using System.Diagnostics.CodeAnalysis ;
using System.Threading.Tasks ;

namespace Clf.ChannelAccess
{

  partial class RemoteChannel
  {

    public bool LowLevelHandleIs ( System.IntPtr handle )
    => (
       m_channelHandle.Value != System.IntPtr.Zero
    && m_channelHandle.Value == handle
    ) ;

  }

}