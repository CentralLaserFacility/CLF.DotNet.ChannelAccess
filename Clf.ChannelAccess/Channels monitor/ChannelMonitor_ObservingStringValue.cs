//
// ChannelMonitor_ObservingStringValue.cs
//

namespace Clf.ChannelAccess
{

  public class ChannelMonitor_ObservingStringValue : ChannelMonitorBase 
  {

    public static System.Action<string,string?> HandleValueChanged_WritingToConsole = (
      (name,newValue) => System.Console.WriteLine(
        $"PV '{name}' changed to {newValue?.ToString()??"null"} on thread #{System.Environment.CurrentManagedThreadId}"
      )
    ) ;

    public ChannelMonitor_ObservingStringValue ( 
      string                        channelName, 
      System.Action<string,string?> valueChanged,
      System.Action<string,bool>?   connectionStatusChanged = null
    ) :
    base(channelName)
    { 
      RemoteValueChanged      = valueChanged ;
      ConnectionStatusChanged = connectionStatusChanged ;
    }

    // string? Value { get ; set ; }

    public override System.Type ValueType => typeof(string) ;

    internal System.Action<string,string?>? RemoteValueChanged { get ; set ; }

    internal override void OnRemoteValueChanged ( object? incomingValue )
    {
      if ( incomingValue is not null )
      {
        incomingValue = incomingValue.ToString() ;
      }
      RemoteValueChanged?.Invoke(
        ChannelName,
        incomingValue as string
      ) ;
    }

  }

}

