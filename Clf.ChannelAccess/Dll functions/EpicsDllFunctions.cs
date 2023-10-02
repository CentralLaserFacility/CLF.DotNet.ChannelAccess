//
// EpicsDllFunctions.cs
//

namespace Clf.ChannelAccess
{

  public static class EpicsDllFunctions
  {

    public static void EnsureAvailable ( )
    { 
      Clf.ChannelAccess.LowLevelApi.DllFunctions.EnsureDllFunctionsAvailable() ;
    }

  }

}
