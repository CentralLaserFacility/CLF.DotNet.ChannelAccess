//
// ChannelsMonitor_UsageExample.cs
//

namespace Clf.ChannelAccess
{

  public class ChannelsMonitor_UsageExample
  {

    public ChannelsMonitor_UsageExample ( System.Action<string> handleChannelChange )
    {

      // Once we've installed a 'MonitoredChannel' into a ChannelsMonitor,
      // the Monitor will listen for changes in that channel's Value.
      // If the channel becomes disconnected, a 'null' value is reported.

      var channelsMonitor = new ChannelsMonitor(
        new ChannelMonitor_ObservingNumericValue<bool>( 
          "myPvName",  
          (name,newValue) => handleChannelChange(
            $"PV named '{name}' value changed to {newValue} on thread #{System.Environment.CurrentManagedThreadId}"
          ),
          (name,isConnected) => handleChannelChange(
            $"PV named '{name}' connection status changed to {isConnected} on thread #{System.Environment.CurrentManagedThreadId}"
          )
        ) 
      ) ;

    }
    
  }

}

