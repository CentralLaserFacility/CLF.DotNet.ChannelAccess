//
// SubscriptionHandle.cs
//

using Clf.Common.ExtensionMethods ;

namespace Clf.ChannelAccess.LowLevelApi
{

  internal record SubscriptionHandle ( System.IntPtr Value )
  {

    public SubscriptionHandle ( ) : 
    this(
      System.IntPtr.Zero
    ) { 
    }

    public bool IsValidHandle => ! IsNull ;

    public bool IsNull => Value == System.IntPtr.Zero ;

    public static implicit operator System.IntPtr ( SubscriptionHandle subscription )
    => subscription.Value.VerifiedAsNonNullPointer() ;

    public static implicit operator SubscriptionHandle ( System.IntPtr pSubscription )
    => new SubscriptionHandle(pSubscription) ;

  }

}
