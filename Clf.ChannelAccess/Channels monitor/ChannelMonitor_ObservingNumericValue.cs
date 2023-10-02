//
// ChannelMonitor_ObservingNumericValue.cs
//

namespace Clf.ChannelAccess
{

  // A  channel monitor that provides a numeric value, eg bool or int

  public class ChannelMonitor_ObservingNumericValue<TNumericValue> : ChannelMonitorBase 
  where TNumericValue : struct
  {

    public static System.Action<string,TNumericValue?> HandleValueChanged_WritingToConsole = (
      (name,newValue) => System.Console.WriteLine(
        $"PV '{name}' changed to {newValue?.ToString()??"null"} on thread #{System.Environment.CurrentManagedThreadId}"
      )
    ) ;

    public ChannelMonitor_ObservingNumericValue ( 
      string                               channelName, 
      System.Action<string,TNumericValue?> valueChanged,
      System.Action<string,bool>?          connectionStatusChanged = null
    ) :
    base(channelName)
    { 
      RemoteValueChanged      = valueChanged ;
      ConnectionStatusChanged = connectionStatusChanged ;
    }

    public override System.Type ValueType => typeof(TNumericValue) ;

    internal System.Action<string,TNumericValue?>? RemoteValueChanged { get ; set ; }

    internal override void OnRemoteValueChanged ( object? incomingValue )
    {
      if ( incomingValue != null )
      {
        System.Type incomingValueType = incomingValue.GetType() ;
        if ( incomingValueType != ValueType )
        {
          if ( 
            ChannelsMonitor.CanPerformNumericTypeConversion(
              incomingValue,
              ValueType,
              out object? convertedValue
            ) 
          ) {
            RemoteValueChanged?.Invoke(
              ChannelName,
              convertedValue as TNumericValue?
            ) ;            
          }
          else
          {
            // Log a message, conversion failed ...
            ChannelsMonitor.OnTypeMismatchError?.Invoke(
              $"Failed to convert {ChannelName} value to {ValueType}"
            ) ;
            RemoteValueChanged?.Invoke(
              ChannelName,
              null
            ) ;
          }
          return ;
        }
      }
      RemoteValueChanged?.Invoke(
        ChannelName,
        incomingValue as TNumericValue?
      ) ;
    }

  }

}

