//
// Channel_Example_02.cs
//

using Clf.ChannelAccess.ExtensionMethods ;

using System.Threading.Tasks ;

namespace Clf_ChannelAccess_UsageExamples
{

  public static class Channel_Example_02
  {

    public static async Task Run ( )
    {

      var myChannel = Clf.ChannelAccess.Hub.GetOrCreateChannel(
        channelName : "xx:one_long"
      ) ;

      //////////////// if ( Clf.ChannelAccess.IChannel.StateChangedEventIsSupported )
      //////////////// {
      ////////////////   myChannel.StateChanged += Channel_StateChanged ;
      //////////////// }
      //////////////// else
      {
        throw new System.ApplicationException("StateChange event is not supported") ;
      }
      
      bool hasConnected = await myChannel.HasConnectedAndAcquiredValueAsync() ;
     
      myChannel.PutValue(123) ;

      ////////////////////// if ( Clf.ChannelAccess.IChannel.StateChangedEventIsSupported )
      ////////////////////// {
      //////////////////////   myChannel.StateChanged -= Channel_StateChanged ;
      ////////////////////// }

      myChannel.Dispose() ;

    }

    private static void Channel_StateChanged ( Clf.ChannelAccess.StateChange change, Clf.ChannelAccess.ChannelState currentState )
    {
      if ( change.DescribesConnectionStatusChange( out bool? isConnected ) )
      {
        System.Console.WriteLine(
          $"{currentState.ChannelName} is now {(isConnected.Value?"CONNNECTED":"DISCONNECTED")}"
        ) ;
      }
      else if ( change.DescribesValueChange( out Clf.ChannelAccess.ValueInfo? valueInfo ) )
      {
        System.Console.WriteLine(
          $"{
            currentState.ChannelName
          } value is {
            // currentState.Value_AsString()
            valueInfo.Value_AsDisplayString()
          }"
        ) ;
      }
    }

  }

}
