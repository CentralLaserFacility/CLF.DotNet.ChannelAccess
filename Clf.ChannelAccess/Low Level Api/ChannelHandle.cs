//
// ChannelHandle.cs
//

using Clf.Common.ExtensionMethods ;

namespace Clf.ChannelAccess.LowLevelApi
{

  // This is deliberately mutable ...
  // Hmm, is that a good idea ???
  // Might be better to allow a null instance ?

  internal class ChannelHandle 
  {

    public ChannelHandle ( System.IntPtr? value = null )
    {
      Value = value ?? System.IntPtr.Zero ;
    }

    public System.IntPtr Value { get ; private set ; }

    public bool IsValidHandle => ! IsNull ;

    public bool IsNull => Value == System.IntPtr.Zero ;

    public void SetAsNonNull ( System.IntPtr value ) 
    => Value = value.VerifiedAsNonNullPointer() ;

    public void SetAsNull ( ) => Value = System.IntPtr.Zero ;

    // public static readonly ChannelHandle Zero = new(System.IntPtr.Zero) ;

    public static implicit operator System.IntPtr ( ChannelHandle channel )

    => channel.Value.VerifiedAsNonNullPointer() ;

    public static implicit operator ChannelHandle ( System.IntPtr pContext )
    => new ChannelHandle(pContext) ;

  }

}
