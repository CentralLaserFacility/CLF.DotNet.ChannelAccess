//
// ThinIoc_DllFunctions.cs
// 

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices ;
using Clf.Common.ExtensionMethods ;
using System.Linq ;
using System.Collections.Generic;

//
// The 'thin_ioc' project builds the dll to x64\Debug_DLL.
// This dll (together with the necessary support dll's) has to be
// manually copied to the console app project directory, alongside Program.cs.
// These DLL's need to be configured as 'Copy to output directory'.
// so that they'll get copied to the same directory as our EXE.
//

// To enable single-stepping into the DLL code :
// - copy the .pdb file alongside the .dll
//   [ note that this doesn't have to be copied to the output directory ]
// - in the C# project properties, check 'enable native debugging'
// https://stackoverflow.com/questions/21996641/how-to-step-into-p-invoked-c-code
//

//
// To build and run this mess :
//   COMPILE thin-ioc.cpp
//   BUILD the thin_ioc project
//   Copy thin_ioc.* from x64/Debug_DLL to /TestThinIoc_ConsoleApp
//   Rebuild the TestThinIoc_ConsoleApp project, so that the DLL's get copied to the output directory
//   Run the TestThinIoc_ConsoleApp
//

//
// Default Marshaling for Arrays - .NET Framework | Microsoft Docs
// https://docs.microsoft.com/en-us/dotnet/framework/interop/default-marshaling-for-arrays
// Platform Invoke (P/Invoke) | Microsoft Docs
// https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke
//

//
// Exporting C Functions for Use in C or C++ Language Executables
// https://docs.microsoft.com/en-us/cpp/build/exporting-c-functions-for-use-in-c-or-cpp-language-executables?view=msvc-170
// 
// DLL Export Viewer
// https://www.nirsoft.net/utils/dll_export_viewer.html
// 
// Native DLL export functions viewer
// https://newbedev.com/is-there-any-native-dll-export-functions-viewer
//
// Exporting from a DLL Using __declspec(dllexport)
// https://docs.microsoft.com/en-us/cpp/build/exporting-from-a-dll-using-declspec-dllexport?view=msvc-170
//
// A Journey Through a P/Invoke Call
// https://www.jacksondunstan.com/articles/5120
//

//
// How to Debug a Release Build
// https://docs.microsoft.com/en-us/cpp/build/how-to-debug-a-release-build?view=msvc-170
//

//
// How can I specify a [DllImport] path at runtime ? *****************
// https://stackoverflow.com/questions/8836093/how-can-i-specify-a-dllimport-path-at-runtime
// It's the native Win32 DLL loading rules that govern things, regardless of whether you're using
// the handy managed wrappers (the P/Invoke marshaller just calls LoadLibrary). Those rules are enumerated
// in great detail here :
// https://docs.microsoft.com/en-gb/windows/win32/dlls/dynamic-link-library-search-order?redirectedfrom=MSDN
// The default search order will start looking in the directory from which your application was loaded.
// If you place the DLL there during the install, it will be found.
// If you need to force the application to look in a different directory for the DLL,
// you can modify the default search path using the SetDllDirectory function from Kernel32.
// https://docs.microsoft.com/en-gb/windows/win32/api/winbase/nf-winbase-setdlldirectorya
//
// Register PDB File Location in Visual Studio
//   Tools -> Options -> Debugging -> Symbols
//   Add path to PDB files
//

namespace Clf.ThinIoc
{
  
  public record class EpicsEnvironmentVariable ( string Name, string NominalValue ) 
  {
    public static EpicsEnvironmentVariable EPICS_CA_MAX_ARRAY_BYTES = new("EPICS_CA_MAX_ARRAY_BYTES" , "16384" ) ;
    public static EpicsEnvironmentVariable EPICS_CA_AUTO_ADDR_LIST  = new("EPICS_CA_AUTO_ADDR_LIST"  , "YES"   ) ; // NO !!
    public static EpicsEnvironmentVariable EPICS_CA_ADDR_LIST       = new("EPICS_CA_ADDR_LIST"       , ""      ) ; // IP address
  }
    
  // https://github.com/dls-controls/pythonSoftIOC

  public static class ThinIocDllFunctions
  {

    static ThinIocDllFunctions ( )
    {
      string pathToExeDirectory = System.IO.Path.GetDirectoryName(
        System.Reflection.Assembly.GetEntryAssembly()?.Location
      )! ;
      Clf.Common.Win32.SetDllDirectory(
        pathToExeDirectory + @"\_EpicsDlls\Clf_Debug"
      ) ;
    }

    //
    // We can really only invoke 'thin_ioc_start' once,
    // because it allocates lots of memory etc for the db's and so on,
    // which won't get released unless you make a complicated sequence
    // of API calls ; see 'base-3.15.9\src\ioc\db\dbUnitTest.c'
    //

    private const string THIN_IOC_DLL_NAME = "thin_ioc" ;

    private enum ApiCallResult : short {
      SUCCESS                   = 0,
      ALREADY_INITIALISED       = 1,
      FAILED_TO_LOAD_DBD_FILE   = 2,
      FAILED_TO_REGISTER_DRIVER = 3,
      NOT_INITIALISED           = 4,
      FAILED_TO_LOAD_DB_FILE    = 5,
      DBD_NOT_LOADED            = 6,
      IOC_START_FAILED          = 7
    }

    public enum RunningStatus : short {
      NotYetRun,
      RunAttemptFailed,
      IsRunning,
      HasRunAndIsNowStopped
    }

    private volatile static RunningStatus m_status = RunningStatus.NotYetRun ;

    public static RunningStatus Status 
    { 
      get => m_status ;
      private set => m_status = value ; 
    } 

    public const short DllVersionNumberExpected = 103 ;

    public static void VerifyDllVersion ( )
    {
      short actualVersionOfDll = thin_ioc_get_version() ;
      if ( actualVersionOfDll != DllVersionNumberExpected )
      {
        throw new ChannelAccess.UnexpectedConditionException(
          $"ThinIOC : expected DLL version {DllVersionNumberExpected} but found {actualVersionOfDll}"
        ) ;
      }
      [System.Runtime.InteropServices.DllImport(THIN_IOC_DLL_NAME)]
      static extern short thin_ioc_get_version ( ) ;
    }

    public static void VerifyNotYetRun ( )
    {
      if ( Status != RunningStatus.NotYetRun )
      {
        throw new ChannelAccess.UsageErrorException(
          $"ThinIOC can only run once : current status is {Status}"
        ) ;
      }
    }

    public static void SetMaxArrayLengthBytes ( int nBytes ) 
    {
      SetEnvironmentVariable(
        EpicsEnvironmentVariable.EPICS_CA_MAX_ARRAY_BYTES.Name,
        nBytes.ToString()
      ) ;
    }

    public static bool TryGetMaxArrayLengthBytes ( [NotNullWhen(true)] out int nBytes ) 
    {
      return int.TryParse(
        GetEnvironmentVariable(
          EpicsEnvironmentVariable.EPICS_CA_MAX_ARRAY_BYTES.Name
        ),
        out nBytes 
      ) ;
    }

    public static void SetEnvironmentVariable ( 
      string name,
      string value
    ) {
      VerifyDllVersion() ;
      thin_ioc_set_env(name,value) ;
      [System.Runtime.InteropServices.DllImport(THIN_IOC_DLL_NAME)]
      static extern void thin_ioc_set_env ( 
        [In] [MarshalAs(UnmanagedType.LPStr)] string name,
	      [In] [MarshalAs(UnmanagedType.LPStr)] string value
      ) ;
    }

    public static string GetEnvironmentVariable ( 
      string name
    ) {
      VerifyDllVersion() ;
      return Marshal.PtrToStringAnsi(
        thin_ioc_get_env(name)
      )! ;
      [System.Runtime.InteropServices.DllImport(THIN_IOC_DLL_NAME)]
      static extern System.IntPtr thin_ioc_get_env ( 
        [In] [MarshalAs(UnmanagedType.LPStr)] string name
      ) ;
    }

    public static int QueryHowManyDbdOptions ( )
    {
      return thin_ioc_how_many_dbd_options() ;
      [System.Runtime.InteropServices.DllImport(THIN_IOC_DLL_NAME)]
      static extern int thin_ioc_how_many_dbd_options ( ) ;
    }

    public static string QueryDbdOptionName ( int dbdOption )
    {
      var pDbdOptionName = thin_ioc_get_dbd_option_name(dbdOption) ;
      return (
        pDbdOptionName == System.IntPtr.Zero
        ? throw new System.ArgumentException()
        : Marshal.PtrToStringAnsi(pDbdOptionName)
      )! ;
      [System.Runtime.InteropServices.DllImport(THIN_IOC_DLL_NAME)]
      static extern System.IntPtr thin_ioc_get_dbd_option_name ( int dbdOption ) ;
    }

    public static System.Collections.Generic.IReadOnlyList<string> QueryAvailableDbdOptionNames ( )
    => Enumerable.Range(
      0,
      QueryHowManyDbdOptions()
    ).Select(
      i => QueryDbdOptionName(i)
    ).ToList().AsReadOnly() ;

    public static System.Collections.Generic.IReadOnlyList<string> AvailableDbdOptionNames => QueryAvailableDbdOptionNames() ;

    // At present we only support a single 'dbd' option ('softIoc')
	  // but the 'dbdOption' argument gives us the possibility of
	  // loading different definitions eg accommodating motor records
	  // or whatever.

    public static int? InitialisationDbdOption ;

    // The 'dbd' option configures ThinIoc to understand a particular set
    // of 'record' types, that will be able to be mentioned in 'db' files
    // that are loaded subsequently.

    public static void Initialise ( )
    {
      // TODO : this is necessary to ensure that the thin_ioc DLL is loaded,
      // but it would be better to bundle this into the ChannelAccess method ? 
      // HMM - THIS NEEDS TO BE DONE IN THE APP STARTUP CODE ... ????????????
      Clf.ChannelAccess.EpicsDllFunctions.EnsureAvailable() ;
      string pathToExeDirectory = System.IO.Path.GetDirectoryName(
        System.Reflection.Assembly.GetEntryAssembly()?.Location
      )! ;
      // Initialise(0) ;
      Initialise(
        pathToExeDirectory
        + @"\EpicsDlls_Debug\softIoc.dbd" // FIX_THIS ??????????????????
      ) ;
    }

    public static void Initialise ( int dbdOption )
    {
      ApiCallResult result = thin_ioc_initialise(dbdOption) ;
      switch ( result )
      {
      case ApiCallResult.SUCCESS:
        InitialisationDbdOption = dbdOption ;
        return ;
      default:
        throw new Clf.ChannelAccess.UsageErrorException(
          $"ThinIOC initialisation (dbdOption={dbdOption}) failed : {result}"
        ) ;
      }
      [System.Runtime.InteropServices.DllImport(THIN_IOC_DLL_NAME)]
      static extern ApiCallResult thin_ioc_initialise ( int dbdOption ) ;
    }

    public static void Initialise ( string pathToSoftIocDbdFile )
    {
      if ( ! System.IO.File.Exists(pathToSoftIocDbdFile) )
      {
        throw new ChannelAccess.UsageErrorException(
          $"DBD file not found : {pathToSoftIocDbdFile}"
        ) ;
      }
      ApiCallResult result = thin_ioc_initialise_with_dbd_path(pathToSoftIocDbdFile) ;
      switch ( result )
      {
      case ApiCallResult.SUCCESS:
        return ;
      default:
        throw new ChannelAccess.UsageErrorException(
          $"ThinIOC initialisation (dbdPath={pathToSoftIocDbdFile}) failed : {result}"
        ) ;
      }
      [System.Runtime.InteropServices.DllImport(THIN_IOC_DLL_NAME)]
      static extern ApiCallResult thin_ioc_initialise_with_dbd_path ( string dbdFile ) ;
    }

    public static void Initialise ( 
      string fullPathToDbdFile,
      string fullPathToDriverRegistrationDll
    ) {
      ApiCallResult result = thin_ioc_initialise_with_custom_dbd(
        fullPathToDbdFile,
        fullPathToDriverRegistrationDll
      ) ;
      switch ( result )
      {
      case ApiCallResult.SUCCESS:
        InitialisationDbdOption = -1 ;
        return ;
      default:
        throw new ChannelAccess.UnexpectedConditionException(
          $"ThinIOC initialisation failed : {result}"
        ) ;
      }
      [System.Runtime.InteropServices.DllImport(THIN_IOC_DLL_NAME)]
      static extern ApiCallResult thin_ioc_initialise_with_custom_dbd ( 
        [In] [MarshalAs(UnmanagedType.LPStr)] string dbdFile,
        [In] [MarshalAs(UnmanagedType.LPStr)] string driverRegistrationDll
      ) ;
    }

    public static void LoadDbFile ( 
      string         pathToDbFile, 
      string?        macros                     = null,
      System.Action? actionOnFailedToLoadDbFile = null
    ) {
      VerifyDllVersion() ;
      VerifyNotYetRun() ;
      // Hmm, this returns 'SUCCESS' even if the .db file contains errors,
      // eg if the PV name specified in the 'record()' definition is not valid.
      // Not much we can do about that though. All we can do is detect that the
      // PV is not running.
      ApiCallResult result = thin_ioc_load_db_file(pathToDbFile,macros) ;
      switch ( result )
      {
      case ApiCallResult.SUCCESS:
        return ;
      case ApiCallResult.FAILED_TO_LOAD_DB_FILE:
        if ( actionOnFailedToLoadDbFile is null )
        {
          // Hmm, if the load fails, maybe it would be helpful
          // to read the file contents and write a copy somewhere
          // so that it can be displayed ???
          throw new ChannelAccess.UsageErrorException(
            $"ThinIOC failed to load db file {pathToDbFile}"
          ) ;
        }
        else
        {
          // If an action has been supplied,
          // invoke that rather than throwing an exception
          actionOnFailedToLoadDbFile() ;
          break ;
        }
      default:
        throw new ChannelAccess.UnexpectedConditionException(
          $"ThinIOC API call failed : {result}"
        ) ;
      }
      [System.Runtime.InteropServices.DllImport(THIN_IOC_DLL_NAME)]
      static extern ApiCallResult thin_ioc_load_db_file ( 
        [In] [MarshalAs(UnmanagedType.LPStr)] string  pathToDbFile, 
        [In] [MarshalAs(UnmanagedType.LPStr)] string? macros = null
      ) ;
    }

    //
    // Hmm, we're effectively performing a 'Wait' on these events, so is this
    // susceptible to deadlocks ? Hopefully not, as although our 'wait' for the event
    // is blocking the calling thread, it's no different from the situation where
    // the calling thread is blocked due to a long-running computation ... ???
    //

    private static System.Threading.ManualResetEvent g_thinIocThreadHasStarted_event = new(false) ;
    private static System.Threading.ManualResetEvent g_stopThinIoc_event             = new(false) ;

    // When this returns without having thrown an exception,
    // our call to 'thin_ioc_start()' has succeeded,
    // and the internal threads that implement our db records
    // will hopefully have started. However we can't necessarily
    // guarantee that all the PV's are in a state where they can
    // respond to 'PutValue' requests ?
    //
    // It might be prudent to do a Task.Delay() after calling this,
    // to allow time for all the PV's to completely come alive ??
    //

    public static void StartThinIocOnNewBackgroundThread ( )
    {
      VerifyNotYetRun() ;
      // Here we initiate a long-lived activity on a thread-pool thread,
      // which terminates when the 'stop' event gets set. 
      // HMM, WOULD IT BE BETTER TO CREATE A THREAD EXPLICITLY ???
      // THAT WAY WE'D KNOW THE THREAD WOULD NOT BE RE-USED ???
      System.Threading.Tasks.Task task_ignored = System.Threading.Tasks.Task.Run(
        () => {
          ApiCallResult result = thin_ioc_start() ;
          if ( result is ApiCallResult.SUCCESS )
          {
            // The IOC will now be 'running' on our worker thread.
            // It will continue to run while that thread is alive.
            Status = RunningStatus.IsRunning ;
            g_thinIocThreadHasStarted_event.Set() ;
            g_stopThinIoc_event.WaitOne() ;
            thin_ioc_call_atExits() ;
            Status = RunningStatus.HasRunAndIsNowStopped ;
            return ;
          }
          else
          {
            Status = RunningStatus.RunAttemptFailed ;
            g_thinIocThreadHasStarted_event.Set() ;
            return ;
          }
        }
      ) ;
      // Wait here until our worker thread
      // tells us its call to 'thin_ioc_start()' has returned,
      // with a 'Status' result that indicates whether it succeeded or failed.
      // Here, 'waitSucceded' will be false if for some bizarre reason
      // our call to 'thin_ioc_start()' hasn't returned within a reasonable time.
      bool waitSucceeded = g_thinIocThreadHasStarted_event.WaitOne(
        millisecondsTimeout : 5000
      ) ;
      if ( ! waitSucceeded ) 
      {
        throw new ChannelAccess.UnexpectedConditionException(
          "ThinIOC : timed out waiting for 'thin_ioc_start()' to return"
        ) ;
      }
      if ( Status is RunningStatus.RunAttemptFailed )
      {
        throw new ChannelAccess.UnexpectedConditionException(
          "ThinIOC : failed to start IOC thread"
        ) ;
      }
      [System.Runtime.InteropServices.DllImport(THIN_IOC_DLL_NAME)]
      static extern ApiCallResult thin_ioc_start ( ) ;
      [System.Runtime.InteropServices.DllImport(THIN_IOC_DLL_NAME)]
      static extern void thin_ioc_call_atExits ( ) ;
    }

    // Start 'ThinIoc', and wait for a little while to allow some time
    // for the PV's to completely come alive and be able to respond to 
    // requests ...

    public static async System.Threading.Tasks.Task StartThinIocOnNewBackgroundThreadAsync ( System.TimeSpan waitTime )
    {
      StartThinIocOnNewBackgroundThread() ;
      await System.Threading.Tasks.Task.Delay(waitTime) ;
    }

    public static void RequestThinIocStop ( )
    {
      if ( Status is RunningStatus.IsRunning )
      {
        g_stopThinIoc_event.Set() ;
      }
      else
      {
        // Hmm, possible race condition here - and it doesn't
        // really matter if we request a Stop more than once ...
        // throw new System.ApplicationException(
        //   "ThinIOC : not currently running"
        // ) ;
      }
    }

    public static IReadOnlyList<string> GetChannelNames ( )
    {
      string commaSeparatedChannelNames = Marshal.PtrToStringAnsi(
        thin_ioc_get_pv_names() 
      )! ;
      return commaSeparatedChannelNames.Split(',') ;
      [System.Runtime.InteropServices.DllImport(THIN_IOC_DLL_NAME)]
      static extern nint thin_ioc_get_pv_names ( ) ;
    }

  }

}