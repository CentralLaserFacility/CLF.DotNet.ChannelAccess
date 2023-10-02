//
// EcaSeverity.cs
//

namespace Clf.ChannelAccess.LowLevelApi
{

  internal enum EcaSeverity {
    Warning          = 0,  // Unsuccessful
    Success          = 1,  // Successful
    RecoverableError = 2,
    Info             = 3,  // Successful
    FatalError       = 4   // Non recoverable
  }

}
