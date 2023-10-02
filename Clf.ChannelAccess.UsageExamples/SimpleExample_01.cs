//
// Channel_Example_01.cs
//

namespace Clf_ChannelAccess_UsageExamples
{

  public static class SimpleExample_01
  {

    public static async System.Threading.Tasks.Task Run ( )
    {
      string result ;
      try
      {
        System.Console.WriteLine("Invoking Clf.ChannelAccess.Hub.GetValueAsStringAsync('xx:one_short')") ;
        result = await Clf.ChannelAccess.Hub.GetValueAsStringAsync("xx:one_short") ;
      }
      catch ( System.Exception x ) 
      {
        result = x.Message ;
      }
      System.Console.WriteLine(
        $"=> {result}"
      ) ;
    }

  }

}
