//
// Channel_Example_01.cs
//

using System.Threading.Tasks;

namespace Clf_ChannelAccess_UsageExamples
{

  public static class Channel_Example_01
  {

    public static async Task Run ( )
    {

      using Clf.ChannelAccess.IChannel myChannel = Clf.ChannelAccess.Hub.GetOrCreateChannel(
        channelName : "xx:one_long"
      ) ;
      // if ( Clf.ChannelAccess.IChannel.StateChangedEventIsSupported )
      // {
      //   myChannel.StateChanged += (change,newState) => {
      //     System.Console.WriteLine(
      //       change.ToString()
      //     ) ;
      //   } ;
      // }
      // else
      {
        throw new System.ApplicationException("StateChange event is not supported") ;
      }
      
      bool hasConnected = await myChannel.HasConnectedAndAcquiredValueAsync() ;
     
      await myChannel.PutValueAsync(123) ;

    }

  }

}
