//
// ChannelMonitor_ObservingChannelValueAsObject.cs
//

namespace Clf.ChannelAccess
{

  // TINE !!

  public class ChannelMonitor_ObservingChannelValueAsObject : ChannelMonitorBase 
  {

    public static System.Action<string,object?> HandleValueChanged_WritingToConsole = (
      (name,newValue) => System.Console.WriteLine(
        $"PV '{name}' changed to {newValue?.ToString()??"null"} on thread #{System.Environment.CurrentManagedThreadId}"
      )
    ) ;

    public ChannelMonitor_ObservingChannelValueAsObject ( 
      string                        channelName, 
      System.Action<string,object?> valueChanged,
      System.Action<string,bool>?   connectionStatusChanged = null
    ) :
    base(channelName)
    { 
      RemoteValueChanged      = valueChanged ;
      ConnectionStatusChanged = connectionStatusChanged ;
    }

    public override System.Type ValueType => typeof(object) ;

    internal System.Action<string,object?>? RemoteValueChanged { get ; set ; }

    internal override void OnRemoteValueChanged ( object? incomingValue )
    {
      RemoteValueChanged?.Invoke(
        ChannelName,
        incomingValue
      ) ;
      // Used during initial testing ...
      // if ( incomingValue is not null )
      // {
      //   incomingValue = incomingValue.ToString() ;
      // }
      // RemoteValueChanged?.Invoke(
      //   ChannelName,
      //   incomingValue as string
      // ) ;
    }

  }

}

