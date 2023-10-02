//
// ContextHandle.cs
//

using Clf.Common.ExtensionMethods ;

namespace Clf.ChannelAccess.LowLevelApi
{

  // Hmm, could use SafeHandle here ??? Scary ...
  // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.safehandle?view=net-5.0#remarks

  internal record ContextHandle ( System.IntPtr Value ) 
  {

    public ContextHandle ( ) : 
    this(
      System.IntPtr.Zero
    ) { 
    }

    public bool IsValidHandle => ! IsNull ;

    public bool IsNull => Value == System.IntPtr.Zero ;

    public static implicit operator System.IntPtr ( ContextHandle channel )
    => channel.Value.VerifiedAsNonNullPointer() ;

    public static implicit operator ContextHandle ( System.IntPtr pChannel )
    => new ContextHandle(pChannel) ;

  }

}
