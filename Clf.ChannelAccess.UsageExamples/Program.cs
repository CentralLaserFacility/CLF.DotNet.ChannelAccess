//
// Program.cs
//

namespace Clf_ChannelAccess_UsageExamples
{

  class Program
  {

    static async System.Threading.Tasks.Task Main ( string[] args )
    {
    
      try
      {
        // await System.Threading.Tasks.Task.Delay(100) ;
        // await Channel_Example_01.Run() ;
        // Clf_ChannelAccess_UsageExamples.MessengerExample.Run() ;
        await Clf_ChannelAccess_UsageExamples.SimpleExample_01.Run() ;
      }
      catch ( System.Exception x )
      {
        System.Console.WriteLine(
          $"EXCEPTION : '{x.Message}'"
        ) ;
        System.Console.WriteLine(
          $"Waiting to exit ..."
        ) ;
        System.Console.ReadLine() ;
      }
      finally
      {
        System.Console.WriteLine(
          $"Waiting to exit ..."
        ) ;
        System.Console.ReadLine() ;
      }

    }

  }

}
