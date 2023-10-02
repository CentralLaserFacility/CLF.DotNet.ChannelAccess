//
// ThinIocProcess.cs
//

using System.Collections.Generic;
using Clf.Common.ExtensionMethods;
using System.Linq;
using System.Threading.Tasks;

namespace Clf.ChannelAccess
{

  // Hmm, having the constructor start the process isn't such a good plan ... ???

  public class ThinIocProcess : System.IDisposable
  {

    // This is the 'friendly name' we need to specify in 'GetProcessesByName'
    // when we want to discover all the running instances of the process

    public const string ProcessName = "Clf.ThinIoc.Server" ;

    // TODO : Hmm, this always gets us to the DEBUG build ...

    public static string FullPathToExe => ( 
      Clf.Common.PathUtilities.RootDirectoryHoldingDotNetGithubRepos
    + $@"DotNet.ChannelAccess\Clf.ThinIoc.Server\bin\Debug\net7.0-windows\{ProcessName}.exe" 
    ) ;

    public static System.Diagnostics.Process[] CurrentlyRunningProcesses 
    => System.Diagnostics.Process.GetProcessesByName("Clf.ThinIoc.Server") ;

    public static bool IsRunning ( string title )
    {
      return CurrentlyRunningProcesses.Where(
        process => process.MainWindowTitle == title
      ).Any() ;
    }

    public static System.TimeSpan TimeToWaitForAppStartup_Default = System.TimeSpan.FromSeconds(6.0) ;

    public static async Task EnsureIsRunningAsync ( 
      string           dbFile,
      System.TimeSpan? timeToWaitForAppStartup = null
    ) {
      if ( IsRunning(dbFile) )
      {
        System.Diagnostics.Debug.WriteLine(
          $"ThinIoc process seems to be running, with dbFile '{dbFile}'"
        ) ;
      }
      else
      {
        System.Diagnostics.Debug.WriteLine(
          $"ThinIoc process with dbFile '{dbFile}' is not running"
        ) ;
        System.Diagnostics.Debug.WriteLine(
          $"Starting ThinIoc process with dbFile '{dbFile}' ..."
        ) ;
        new ThinIocProcess(dbFile) ;
        timeToWaitForAppStartup ??= TimeToWaitForAppStartup_Default ;
        await Task.Delay(
          // timeToWaitForAppStartup ?? TimeToWaitForAppStartup_Default
          timeToWaitForAppStartup.Value
        ) ;
        System.Diagnostics.Debug.WriteLine(
          $"Have waited for ThinIoc to start : {timeToWaitForAppStartup.Value.Seconds} secs"
        ) ;
      }
    }

    // This is a nasty hack, but it can be
    // called from a constructor - which is convenient ...

    public static void EnsureIsRunning_WithSleepToAllowTimeForAppStartup ( 
      string           dbFile,
      System.TimeSpan? timeToSleepWaitingForAppStartup = null
    ) {
      if ( ! IsRunning(dbFile) )
      {
        new ThinIocProcess(dbFile) ;
        System.Threading.Thread.Sleep(
          timeToSleepWaitingForAppStartup ?? TimeToWaitForAppStartup_Default
        ) ;
      }
    }

    public static void StopRunning ( string dbFile )
    {
      if ( IsRunning(dbFile) )
      {
        CurrentlyRunningProcesses.Where(
          process => process.MainWindowTitle == dbFile
        ).SingleOrDefault()?.CloseMainWindow() ;
      }
    }

    private System.Diagnostics.Process? m_thinIocProcess ;

    public static IEnumerable<System.Diagnostics.Process> AllRunningInstances 
    => System.Diagnostics.Process.GetProcessesByName(ProcessName) ;

    public static void CloseAllRunningInstances ( )
    {
      AllRunningInstances.ForEachItem(
        process => process.CloseMainWindow()
      ) ;
    }

    //
    // Hmm, it seems that in order to be able to run the EXE
    // using 'Process.Start', you need to have the .Net Desktop Runtime installed
    //  https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.1-windows-x64-installer
    // ... even if the SDK is already installed ...
    //

    public ThinIocProcess ( string dbFileNameOrRecordsFileName )
    {
      string commandLineArguments = (
        (
          dbFileNameOrRecordsFileName.EndsWith(".db")
          ? $"/dbFile={dbFileNameOrRecordsFileName}"
          : $"/recordFile={dbFileNameOrRecordsFileName}"
        )
      + " /autoStart"
      ) ;
      m_thinIocProcess = System.Diagnostics.Process.Start(
        new System.Diagnostics.ProcessStartInfo(
          FullPathToExe,
          commandLineArguments
        )
      ) ;
      if ( m_thinIocProcess is null )
      {
        throw new ChannelAccess.UsageErrorException(
          $"Failed to start {FullPathToExe} {commandLineArguments}"
        ) ;
      }
      else
      {
        string title = m_thinIocProcess.MainWindowTitle ;
      }
    }

    public void Dispose ( )
    {
      m_thinIocProcess?.CloseMainWindow() ;
    }

  }

}

