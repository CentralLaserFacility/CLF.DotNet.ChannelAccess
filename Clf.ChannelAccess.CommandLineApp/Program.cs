//
// Program.cs
//

using System.Threading.Tasks ;

namespace Clf_ChannelAccess_CommandLineApp
{

  public static class Program
  {

    //
    // The 'args' passed in are not currently used,
    // but we could potentially parse the args list
    // as a one-off command
    // eg
    //
    //   > cacli g xx:i16
    //
    // Note that we access any passed-in arguments via an API call,
    // using 'System.Environment.GetCommandLineArgs()',
    // rather than forwarding them from the args provided here.
    //

    public static async Task Main ( string[] args_notUsed )
    {
      Clf.ChannelAccess.Settings.WhichDllsToUse = (
        Clf.ChannelAccess.WhichDllsToUse.ClfDebugDlls 
        // Clf.ChannelAccess.WhichDllsToUse.DaresburyReleaseDlls 
      ) ;
      await Clf.ChannelAccess.CommandLineInterpreter_UsingSystemConsole.CreateInstanceAndExecuteAsync() ;
      // Clf.ChannelAccess.CommandLineInterpreter_UsingSystemConsole.CreateInstanceAndExecute(args) ;
    }

  }

}
