//
// ChannelValidityStatus.cs
//

using System.Diagnostics.CodeAnalysis;

namespace Clf.ChannelAccess
{

  public record ChannelValidityStatus ( 
    bool   IsValid, 
    string Explanation 
  ) {

    public static implicit operator bool ( ChannelValidityStatus isValid )
    => isValid.IsValid ;

    public bool IsInvalid ( [NotNullWhen(true)] out string? whyNotValid )
    {
      whyNotValid = (
        IsValid
        ? null
        : Explanation
      ) ;
      return (
        whyNotValid is null
        ? false // Null, so We're not invalid
        : true  // We are invalid !
      ) ;
    }

    public override string ToString ( ) 
    {
      string result = (
        IsValid 
        ? $"valid"
        : $"not valid"
      ) ;
      if ( ! string.IsNullOrEmpty(Explanation) )
      {
        result += $" : {Explanation}" ;
      }
      return result ;
    }

  }

}
