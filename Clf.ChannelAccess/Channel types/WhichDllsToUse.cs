//
// WhichDllsToUse.cs
//

namespace Clf.ChannelAccess
{

  public enum WhichDllsToUse {
    DaresburyReleaseDlls,
    ClfDebugDlls,
    // ClfReleaseDlls, // NOT YET AVAILABLE !!!
    ClfDebugDlls_FromEpicsBuildInLocalRepo,
    ClfReleaseDlls_FromEpicsBuildInLocalRepo,
    Default = ClfDebugDlls
  }

}
