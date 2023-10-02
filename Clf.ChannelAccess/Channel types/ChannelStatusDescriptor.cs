//
// ChannelStatusDescriptor.cs
//

using System.Collections.Generic ;

namespace Clf.ChannelAccess
{

  public record ChannelStatusDescriptor ( 
    ChannelConnectionStatus ConnectionStatus, 
    ChannelValidityStatus   ValidityStatus
  ) {

    public bool IsConnectedAndValid => ConnectionStatus && ValidityStatus ;

    public IEnumerable<string> ErrorInfoLines
    {
      get
      {
        if ( ConnectionStatus.Explanation != null )
        {
          yield return ConnectionStatus.ToString() ;
        }
        if ( ValidityStatus.Explanation != null )
        {
          yield return ValidityStatus.ToString() ;
        }
      }
    }

    public static implicit operator bool ( ChannelStatusDescriptor isConnectedAndValid )
    => isConnectedAndValid.IsConnectedAndValid ;

    public override string ToString ( ) 
    => (
      $"{ConnectionStatus};{ValidityStatus}"
    ) ;

  }

}
