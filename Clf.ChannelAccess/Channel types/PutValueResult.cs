//
// PutValueResult.cs
//

namespace Clf.ChannelAccess
{

  // Represents the result from a call to PutValueAsync or PutValueAckAsync.

  // We may want to refactor this to be a 'class', as we have for GetValueResult,
  // which will be able to report more rich information about what failed ...

  // TODO_XML_DOCS

  public enum PutValueResult {
    Success,
    RejectedByServer,
    Timeout,
    InvalidValueSupplied,
    InvalidChannel, // Channel is in an Invalid state
    DisposedChannel
  } ;

  //
  // Alternative scheme ... ???
  //
  // public record PutValueResult
  // {
  //   public readonly static PutValueResult Succcess = new() ;
  //   public readonly static PutValueResult Rejected = new() ;
  //   public readonly static PutValueResult Timeout  = new() ;
  //   private PutValueResult ( )
  //   { }
  //   public static new bool operator == ( PutValueResult a, PutValueResult b )
  //   {
  //     return ReferenceEquals(a,b) ;
  //   }
  //   public static new bool operator != ( PutValueResult a, PutValueResult b )
  //   {
  //     return !ReferenceEquals(a,b) ;
  //   }
  //   public static implicit operator bool ( PutValueResult result )
  //   {
  //     return result == Succcess ;
  //   }
  // }
  //

}
