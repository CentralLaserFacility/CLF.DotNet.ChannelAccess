//
// ChannelMonitorBase.cs
//

namespace Clf.ChannelAccess
{

  //
  // A 'ChannelMonitor' sets up a subscription to a Channel,
  // and raises an event (via 'OnRemoteValueChanged') when either
  // (A) the Value changes, or (B) the Value becomes unavailable,
  // in which case the value is reported as null.
  //
  // The 'channel' will typically provide a Value of type bool/int/string etc,
  // and we define appropriate subclasses that convert that into the data type
  // required by the client eg Machine Safety.
  //
  //  ChannelMonitorBase
  //    ChannelMonitor_ObservingNumericValue<bool>
  //    ChannelMonitor_ObservingNumericValue<int>
  //    ChannelMonitor_ObservingStringValue
  //

  public abstract class ChannelMonitorBase
  {

    public string ChannelName { get ; }

    public ChannelsMonitor ChannelsMonitor { get ; internal set ; } = null! ;

    protected ChannelMonitorBase ( string channelName )
    {
      ChannelName = channelName ;
    }

    //
    // This is an 'internal' method which interprets an incoming value
    // of type 'object' and raises a strongly typed 'change' event
    // in the subclass.
    //
    // Provides 'null' when the Channel is not available ie disconnected.
    //

    internal abstract void OnRemoteValueChanged ( object? newValue ) ;

    //
    // This action gets invoked when the ChannelsMonitor is notified
    // that a PV's connection status has changed.
    //

    internal System.Action<string,bool>? ConnectionStatusChanged { get ; set ; }

    internal void InvokeConnectionStatusChangedAction ( bool isConnected )
    {
      ConnectionStatusChanged?.Invoke(ChannelName,isConnected) ;
    }

    public abstract System.Type ValueType { get ; }

    internal Clf.ChannelAccess.IChannel? Channel { get ; set ; } 

  }

}

