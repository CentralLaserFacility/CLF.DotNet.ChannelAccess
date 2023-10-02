//
// ChannelsMonitor.cs
//

using System.Collections.Generic ;
using Clf.Common.ExtensionMethods ;

namespace Clf.ChannelAccess
{

  public class ChannelsMonitor : System.IDisposable
  {

    private Dictionary<string,ChannelMonitorBase> m_channelsBeingAccessed = new() ;

    private Clf.ChannelAccess.ChannelsHandler m_channelsHandler = new() ;

    // We need a custom delegate definition here.
    // A System.Func<> can't specify 'out' parameters,
    // whereas that *is* permitted in a custom delegate.

    public delegate bool TypeConversionFunc ( 
      object      incomingValue,
      System.Type desiredType,
      out object? convertedValue
    ) ;

    // ??????? EXPERIMENT !!!!!!!!!!!!!
    // public delegate bool TypeConversionFunc<T> ( 
    //   object      incomingValue,
    //   System.Type desiredType,
    //   out T?      convertedValue
    // ) ;

    public TypeConversionFunc CanPerformNumericTypeConversion = CanPerformNumericTypeConversion_DefaultImplementation ;

    public System.Action<string>? OnTypeMismatchError ;

    public ChannelsMonitor ( params ChannelMonitorBase[] channelsToAcess )
    {
      channelsToAcess.ForEachItem(
        channel => AddChannel(channel)
      ) ;
    }

    public void AddChannels ( params ChannelMonitorBase[] channelsToAcess )
    {
      channelsToAcess.ForEachItem(
        channel => AddChannel(channel)
      ) ;
    }

    internal static bool CanPerformNumericTypeConversion_NO ( 
      object      incomingValue,
      System.Type desiredType,
      out object? convertedValue
    ) {
      convertedValue = null ;
      return false ;
    }

    // Hmm, this would be better as a class instance,
    // where we could dispatch to a virtual method for each desired type ??

    internal static bool CanPerformNumericTypeConversion_DefaultImplementation ( 
      object      incomingValue,
      System.Type desiredType,
      out object? convertedValue
    ) {
      // We perform conversions to the desired Type
      // if the provided value is 'compatible'.
      // These rules are as needed for Machine Safety.
      convertedValue = null ;
      if ( desiredType == typeof(bool) )
      {
        if ( incomingValue is int newIntValue ) 
        {
          // If the incoming value is an int and we're expecting bool,
          // treat 0/1 as false/true and anything else as null ...
          convertedValue = newIntValue switch {
            0 => false,
            1 => true,
            _ => (bool?) null
          } ;
        }
        else if ( incomingValue is short newShortValue ) 
        {
          // If the incoming value is an int and we're expecting bool,
          // treat 0/1 as false/true and anything else as null ...
          convertedValue = newShortValue switch {
            0 => false,
            1 => true,
            _ => (bool?) null
          } ;
        }
        else if ( incomingValue is string newStringValue ) 
        {
          // If the incoming value is a string ...
          newStringValue = newStringValue.ToLower() ;
          if ( 
              newStringValue.Contains("on") 
          // || newStringValue.Contains("yes") 
          ) {
            convertedValue = true ;
          }
          else if ( 
              newStringValue.Contains("off") 
          // || newStringValue.Contains("no") 
          ) {
            convertedValue = false ;
          }
          else if ( newStringValue.Contains("null") )
          {
            convertedValue = null ;
          }
          else
          {
            convertedValue = newStringValue switch {
              "false" => false,
              "f"     => false,
              "0"     => false,
              "no"    => false, // ??
              "out"   => false, // ??
              "true"  => true,
              "t"     => true,
              "1"     => true,
              "yes"   => true, // ??
              "in"    => true, // ??
              _       => null
            } ;
            // bool? ReturnNull_LoggingStringValue ( string s )
            // {
            //   System.Console.WriteLine(
            //     $"**** Incoming string value '{s}' not recognised as boolean ; PV is '{channelName}'"
            //   ) ;
            //   return null ;
            // }
          }
        }
        else if ( incomingValue is double newDoubleValue ) 
        {
          // If the incoming value is a double and we're expecting bool,
          // treat 0 as false and anything else as true ...
          convertedValue = (
            newDoubleValue != 0.0 
          ) ;
        }
        else if ( incomingValue is float newFloatValue ) 
        {
          // If the incoming value is a double and we're expecting bool,
          // treat 0 as false and anything else as true ...
          convertedValue = (
            newFloatValue != 0.0f 
          ) ;
        }
      }
      else if ( desiredType == typeof(double) )
      {
        if ( incomingValue is float newFloatValue )
        {
          convertedValue = (double) newFloatValue ;
        }
        else if ( incomingValue is string newStringValue )
        {
          convertedValue = (
            newStringValue.CanParseAs<double>( out double doubleValue )
            ? doubleValue
            : null
          ) ;
        }
      }
      return (
        convertedValue != null 
      ) ;
    }

    // Nice but ...
    // public MonitoredChannelEx AddNewChannel<TValue> ( 
    //   string                        channelName, 
    //   System.Action<string,TValue?> valueChangedAction 
    // ) {
    //   System.Type valueType = typeof(TValue) ;
    //   return default(TValue) switch
    //   {
    //     int => new MonitoredChannelEx_Numeric<int>(channelName,valueChangedAction),
    //     string => new MonitoredChannelEx_String(channelName,valueChangedAction),
    //     _ => throw new System.NotImplementedException()
    //   } ;
    //   // return null! ;
    // }

    // NOT TESTED, HENCE KEPT PRIVATE ! But could be useful ...
    private ChannelMonitor_ObservingNumericValue<TValue> AddNewChannel_ProvidingNumericValue<TValue> ( 
      string                        channelName, 
      System.Action<string,TValue?> valueChangedAction,
      System.Action<string,bool>?   connectionStatusChangedAction = null
    ) 
    where TValue : struct
    {
      var channel = new ChannelMonitor_ObservingNumericValue<TValue>(
        channelName,
        valueChangedAction,
        connectionStatusChangedAction
      ) ;
      AddChannel(channel) ;
      return channel ;
    }

    // NOT TESTED, HENCE KEPT PRIVATE ! But could be useful ...
    private ChannelMonitor_ObservingStringValue AddNewChannel_ProvidingStringValue  ( 
      string                        channelName, 
      System.Action<string,string?> valueChangedAction, 
      System.Action<string,bool>?   connectionStatusChangedAction = null
    ) {
      var channel = new ChannelMonitor_ObservingStringValue(
        channelName,
        valueChangedAction,
        connectionStatusChangedAction
      ) ;
      AddChannel(channel) ;
      return channel ;
    }

    public void AddChannel ( ChannelMonitorBase monitoredChannel )
    {
      try
      {
        monitoredChannel.Channel = Clf.ChannelAccess.Hub.GetOrCreateChannel(monitoredChannel.ChannelName) ;
        m_channelsHandler.InstallChannel(
          monitoredChannel.Channel,
          (isConnected,channelState) => {
            monitoredChannel.InvokeConnectionStatusChangedAction(isConnected) ;
            if ( isConnected is false)
            {
              monitoredChannel.OnRemoteValueChanged(
                null
              ) ;
            }
          },
          (valueInfo,channelState) => {
            monitoredChannel.OnRemoteValueChanged(
              valueInfo.Value
            ) ;
          }
        ) ;
        monitoredChannel.ChannelsMonitor = this ;
        m_channelsBeingAccessed.Add(
          monitoredChannel.ChannelName,
          monitoredChannel
        ) ;
      }
      catch ( System.Exception )
      {
        throw ;
      }
    }

    public void Reset ( )
    {
      m_channelsBeingAccessed.Clear() ;
      m_channelsHandler.RemoveAndDisposeAllChannels() ;
      // m_channelsHandler.Dispose() ;
      // m_channelsHandler = new() ;
    }

    public void Dispose ( )
    {
      m_channelsHandler.Dispose() ;
    }

  }

}









