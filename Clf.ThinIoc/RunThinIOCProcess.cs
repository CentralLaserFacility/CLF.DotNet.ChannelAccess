using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Clf.ThinIoc
{
  public static class RunThinIOCProcess
  {
    public static System.Diagnostics.Process? ThinIocProcess;
    /// <summary>
    /// Starts a new ThinIOC process. 
    /// Its your responsibilty to dispose it at higher level.
    /// </summary>
    /// <param name="exePath"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static async Task RunSingleThinIOCProcess_AndWait10s(string exePath, string args)
    {
      var processStartInfo = new System.Diagnostics.ProcessStartInfo(exePath, args);
      ThinIocProcess = System.Diagnostics.Process.Start(processStartInfo);
      await Task.Delay(10000);
    }
    /// <summary>
    /// Times out after 20s max
    /// </summary>
    /// <returns></returns>
    public static async Task WaitUntilNoOtherProcessesAreHangingAround()
    {
      int count = 20;
      Process[] process = System.Diagnostics.Process.GetProcessesByName("runThinIOC");
      while (process.Length > 0 && count > 0)
      {
        await Task.Delay(1000);
        count--;
      }

      //foreach (var p in process) 
      //{
      //  try
      //  {
      //    p.Kill();
      //    p.Dispose();
      //  }
      //  catch(Exception ex) { 
      //    // we dont need to do anything if the process is already dead at this point
      //  }
      //  
      //}
    }
  }
}
