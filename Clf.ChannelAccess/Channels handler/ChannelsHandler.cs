//
// ChannelsHandler.cs
//

using System.Collections.Generic;
using static CommunityToolkit.Mvvm.Messaging.IMessengerExtensions;
using Clf.Common.ExtensionMethods;
using Clf.ChannelAccess.ExtensionMethods;
using System.Threading.Tasks;
using System.Linq;


namespace Clf.ChannelAccess
{

  //
  // ??? SHOULD IMPLEMENT SOME TESTS THAT CHECK
  // THAT DISPOSE IS ALWAYS CALLED THE RIGHT NUMBER OF TIMES.
  //

  //
  // ?? IF WE WERE TO PROVIDE AN ADDITIONAL REQUIRED PARAMETER WHEN CREATING A CHANNELS HANDLER,
  // REPRESENTING THE 'CREATOR' OBJECT, AND ALSO REQUIRE THAT OBJECT TO BE PASSED IN WHEN IT 'CLOSES'
  // THE CHANNEL ... WE COULD VERIFY THAT THE CHANNEL HAS BEEN CLOSED ('DISPOSED') JUST ONCE,
  // BY THE SAME OBJECT THAT CREATED IT. ???
  //

  /// <summary>
  /// A ChannelsHandler takes responsibility for Channels that are installed into it via
  /// the 'InstallChannel' and 'InstallHandlers' methods. That is, responsibility for calling 'Dispose'
  /// on those IChannel instances is transfered to the ChannelsHandler ; invoking 'Dispose' on the
  /// ChannelsHandler has the effect of invoking 'Dispose' on all its installed Channels.
  /// </summary>
  
  public sealed class ChannelsHandler 
  : CommunityToolkit.Mvvm.Messaging.IRecipient<Clf.ChannelAccess.StateChangedMessage>
  , System.IDisposable
  {

    #if SUPPORT_STATE_CHANGE_EVENTS
    public static bool RegisterWithChannelEvents = (
      false
      // true 
    ) ;
    #endif

    /// <summary>
    /// Raise Synthetic Event As Soon As Channel Has Been Created.
    /// </summary>
    public bool AutoRaiseSyntheticEvent { get ; }

    private System.Action<string> m_unhandledExceptionHandler = null ;

    /// <summary>
    /// 
    /// 
    /// </summary>
    /// <param name="unhandledExceptionHandler"></param>
    /// <param name="autoRaiseSyntheticEvent">
    /// Raise a 'synthetic' event as soon as the channel has been created, even before the connection has been established.
    /// </param>
    public ChannelsHandler(
      System.Action<string>? unhandledExceptionHandler = null,     
      bool                   autoRaiseSyntheticEvent   = true
    ) {
      // Register with the 'Messenger' so that this instance
      // will receive notifications whenever someone publishes a 'StateChangedMessage'.
      Settings.Messenger.Register<Clf.ChannelAccess.StateChangedMessage>(
        this
      ) ;
      AutoRaiseSyntheticEvent = autoRaiseSyntheticEvent;
      m_unhandledExceptionHandler = unhandledExceptionHandler ?? Hub.HandleWarningMessage ;
    }

    // RENAME => m_activeChannelWrappersDictionary ???

    private Dictionary<ChannelWrapper,ActiveChannelDescriptor> m_activeChannelsDictionary = new() ;
      
    // RENAME => AllInstalledChannelsWrappers ???

    internal IEnumerable<ChannelWrapper> AllInstalledChannels => m_activeChannelsDictionary.Keys ;

    // internal ChannelBase GetWrappedChannel ( 
    //   ChannelName      channelName, 
    //   ValueAccessMode? valueAccessMode 
    // ) => AllInstalledChannels.Select(
    //   channelWrapper => channelWrapper.WrappedChannel
    //   ).Single(
    //     channel => (
    //        channel.ChannelName     == channelName
    //     && channel.ValueAccessMode == ( valueAccessMode ?? channelName.Validated().DefaultValueAccessMode() )
    //   )
    // ) ;

    internal ChannelWrapper GetChannelWrapper ( 
      ChannelName      channelName, 
      ValueAccessMode? valueAccessMode 
    ) => AllInstalledChannels.Single(
      channel => (
          channel.ChannelName    == channelName
      && channel.ValueAccessMode == ( valueAccessMode ?? channelName.Validated().DefaultValueAccessMode() )
      )
    ) ;

    public bool TryGetInstalledChannel ( 
      ChannelName      channelName,
      out IChannel?    channel, 
      ValueAccessMode? valueAccessMode = null
    ) {
      channel = AllInstalledChannels.SingleOrDefault(
        channel => (
           channel.ChannelName     == channelName
        && channel.ValueAccessMode == ( valueAccessMode ?? channelName.Validated().DefaultValueAccessMode() )
        )
      ) ;
      return channel is not null ;
    }

    public void Dispose ( )
    {
      Settings.Messenger.Unregister<Clf.ChannelAccess.StateChangedMessage>(
        this
      ) ;
      AllInstalledChannels.ForEachItem(
        channel => channel.Dispose() 
      ) ;
      m_activeChannelsDictionary.Clear() ;
    }

    // NOTE : we *do* allow multiple calls that specify the same IChannel instance.

    internal void InstallChannelAndEventHandlers (
      IChannel                               channel,
      System.Action<bool,ChannelState>?      connectionStatusChangedHandler,
      System.Action<ValueInfo,ChannelState>? valueChangedHandler
    ) {
      var channel_asChannelWrapper = (ChannelWrapper) channel ;
      #if SUPPORT_STATE_CHANGE_EVENTS
      channel.AsChannelBase().EnsureIsSubscribedToValueChangeCallbacks() ;
      #endif
      // Hmm, gotcha !!! 
      // If our IChannel is a ChannelWrapper we'll be receiving
      // incoming messages from the wrapped channel rather than 
      // the wrapper, so we use that wrapped channel as
      // the key into the dictionary ...
      if ( ! m_activeChannelsDictionary.ContainsKey(channel_asChannelWrapper) )
      {
        m_activeChannelsDictionary.Add(
          channel_asChannelWrapper,
          new ActiveChannelDescriptor(
            channel_asChannelWrapper,
            m_unhandledExceptionHandler
          )
        ) ;
      }
      m_activeChannelsDictionary[channel_asChannelWrapper].AddHandlers(
        connectionStatusChangedHandler,
        valueChangedHandler
      ) ;
      #if SUPPORT_STATE_CHANGE_EVENTS
      if ( RegisterWithChannelEvents )
      {
        RegisterWithChannelEvent(channel) ;
      }
      #endif
      if ( AutoRaiseSyntheticEvent )
      {
        RaiseSyntheticEventsOnChannel(channel) ;
      }
    }

    /// <summary>
    /// Install the specified Channel into the ChannelsHandler. Responsibility for calling 'Dispose'
    /// on that IChannel instance is transfered to the ChannelsHandler. So it will no longer be necessary 
    /// for the client code that created the IChannel instance to invoke 'Dispose' on the IChannel. 
    /// The 'Dispose' on that channel will be invoked when 'Dispose' is called
    /// on the ChannelsHandler into which Channels have been installed.
    /// </summary>

    public void InstallChannel (
      IChannel channel
    ) {
      InstallChannelAndEventHandlers(
        channel                        : channel,
        connectionStatusChangedHandler : null,
        valueChangedHandler            : null
      ) ;
    }

    /// <summary>
    /// Install the specified Channel into the ChannelsHandler, together with handlers
    /// that will respond to 'ConnectionStatusChanged' and 'ValueChanged' notifications. 
    /// <br/> <br/>
    /// Responsibility for calling 'Dispose'
    /// on the IChannel instance is transfered to the ChannelsHandler. So it will no longer be necessary 
    /// for the client code that created the IChannel instance to invoke 'Dispose' on the IChannel. 
    /// The 'Dispose' on that channel will be invoked when 'Dispose' is called
    /// on the ChannelsHandler into which Channels have been installed.
    /// </summary>

    public void InstallChannel (
      IChannel                              channel,
      System.Action<bool,ChannelState>      connectionStatusChangedHandler,
      System.Action<ValueInfo,ChannelState> valueChangedHandler
    ) {
      InstallChannelAndEventHandlers(
        channel,
        connectionStatusChangedHandler,
        valueChangedHandler
      ) ;
    }

    /// <summary>
    /// Install the specified Channel into the ChannelsHandler, together with a handler
    /// that will respond to 'ConnectionStatusChanged' notifications. 
    /// <br/> <br/>
    /// Responsibility for calling 'Dispose'
    /// on the IChannel instance is transfered to the ChannelsHandler. So it will no longer be necessary 
    /// for the client code that created the IChannel instance to invoke 'Dispose' on the IChannel. 
    /// The 'Dispose' on that channel will be invoked when 'Dispose' is called
    /// on the ChannelsHandler into which Channels have been installed.
    /// </summary>

    public void InstallChannel (
      IChannel                         channel,
      System.Action<bool,ChannelState> connectionStatusChangedHandler
    ) {
      InstallChannelAndEventHandlers(
        channel,
        connectionStatusChangedHandler,
        null
      ) ;
    }

    /// <summary>
    /// Install the specified Channel into the ChannelsHandler, together with a handler
    /// that will respond to 'ValueChanged' notifications. 
    /// <br/> <br/>
    /// Responsibility for calling 'Dispose'
    /// on the IChannel instance is transfered to the ChannelsHandler. So it will no longer be necessary 
    /// for the client code that created the IChannel instance to invoke 'Dispose' on the IChannel. 
    /// The 'Dispose' on that channel will be invoked when 'Dispose' is called
    /// on the ChannelsHandler into which Channels have been installed.
    /// </summary>

    public void InstallChannel (
      IChannel                              channel,
      System.Action<ValueInfo,ChannelState> valueChangedHandler
    ) {
      InstallChannelAndEventHandlers(
        channel,
        null,
        valueChangedHandler
      ) ;
    }

    public void RemoveChannelAndDispose ( ChannelName channelName, ValueAccessMode? valueAccessMode = null )
    {
      var channelWrapperToRemove = GetChannelWrapper(channelName,valueAccessMode) ;
      m_activeChannelsDictionary.Remove(channelWrapperToRemove) ;
      channelWrapperToRemove.Dispose() ;
    }

    public void RemoveAndDisposeAllChannels ( )
    {
      foreach ( var channelWrapperToRemove in m_activeChannelsDictionary.Keys )
      {
        m_activeChannelsDictionary.Remove(channelWrapperToRemove) ;
        channelWrapperToRemove.Dispose() ;
      }
    }

    #if SUPPORT_STATE_CHANGE_EVENTS
    private bool m_raiseSyntheticStateChangedEventFromMessage_inProgress = false ;
    #endif

    //
    // The incoming message comes from a 'concrete' channel, ie a ChannelBase.
    //

    private void HandleIncomingMessage ( Clf.ChannelAccess.StateChangedMessage message )
    {
      IEnumerable<ChannelWrapper> activeChannelWrappers = m_activeChannelsDictionary.Keys ;
      foreach ( 
        var targetWrapper in activeChannelWrappers.Where(
          wrapper => wrapper.WrappedChannel == (ChannelBase) message.Channel
        )
      ) { 
        // RENAME => activeChannelDescriptor
        ActiveChannelDescriptor handlers = m_activeChannelsDictionary[targetWrapper] ;
        if ( message is Clf.ChannelAccess.ConnectionStatusChangedMessage connectionStatusChangedMessage )
        {
          handlers.InvokeConnectionStatusChangeHandlers(
            connectionStatusChangedMessage.IsConnected,
            message.ChannelState
          ) ;
        }
        else if ( message is Clf.ChannelAccess.ValueChangedMessage valueChangedMessage )
        {
          handlers.InvokeValueChangeHandlers(
            valueChangedMessage.ValueInfo,
            message.ChannelState
          ) ;
        }
        #if SUPPORT_STATE_CHANGE_EVENTS
        if ( 
           RegisterWithChannelEvents
        && ! m_raiseSyntheticStateChangedEventFromMessage_inProgress )
        {
          message.Channel.AsChannelBase().RaiseSyntheticStateChangedEventFromMessage(message) ;
        }
        #endif
      }
    }

    //
    // Query the status of the channel, and then
    // raise the 'ConnectionStatusChanged' and 'ValueChanged' events
    // on that channel, as if a message had arrived from the IOC.
    //

    // TODO_XML_DOCS

    public void RaiseSyntheticEventsOnChannel ( IChannel channel )
    {
      var channel_asChannelWrapper = (ChannelWrapper) channel ;
      ChannelStatesSnapshot channelStatesSnapshot = channel.Snapshot() ;
      var handlers = m_activeChannelsDictionary[
        channel_asChannelWrapper
      ] ;
      handlers.InvokeConnectionStatusChangeHandlers(
        channelStatesSnapshot.CurrentState.ConnectionStatus.IsConnected,
        channelStatesSnapshot.CurrentState
      ) ;
      ValueInfo? valueInfo = channelStatesSnapshot.CurrentState.ValueInfo ;
      if ( valueInfo != null )
      {
        handlers.InvokeValueChangeHandlers(
          valueInfo,
          channelStatesSnapshot.CurrentState
        ) ;
      }
    }

    // TODO_XML_DOCS

    public void RaiseConnectionChangedEventOnChannel ( IChannel channel )
    {
      var channel_asChannelWrapper = (ChannelWrapper) channel ;
      ChannelStatesSnapshot channelStatesSnapshot = channel.Snapshot() ;
      var handlers = m_activeChannelsDictionary[channel_asChannelWrapper] ;
      handlers.InvokeConnectionStatusChangeHandlers(
        channelStatesSnapshot.CurrentState.ConnectionStatus.IsConnected,
        channelStatesSnapshot.CurrentState
      ) ;
    }

    // TODO_XML_DOCS

    // Hmm, we want to raise synthetic events on a channel that we've installed,
    // and by definition that's going to be an IChannelWrapper that we obtained
    // via Hub.GetOrCreate().

    public void RaiseValueChangedEventOnChannel ( IChannel channel )
    {
      var channel_asChannelWrapper = (ChannelWrapper) channel ;
      ChannelStatesSnapshot channelStatesSnapshot = channel.Snapshot() ;
      var handlers = m_activeChannelsDictionary[channel_asChannelWrapper] ;
      ValueInfo? valueInfo = channelStatesSnapshot.CurrentState.ValueInfo ;
      if ( valueInfo != null )
      {
        handlers.InvokeValueChangeHandlers(
          valueInfo,
          channelStatesSnapshot.CurrentState
        ) ;
      }
    }

    /// <summary>
    /// Raise Synthetic Events On All Installed Channels.
    /// For each Channel that has been installed, query the current state of the Channel
    /// and invoke the installed event handlers based on the current ConnectionStatus
    /// and the current ValueInfo (if known).
    /// </summary>

    public void RaiseSyntheticEvents ( )
    {      
      #if SUPPORT_STATE_CHANGE_EVENTS
        m_raiseSyntheticStateChangedEventFromMessage_inProgress = true ;
      #endif
      AllInstalledChannels.ForEachItem(
        channel => {
          RaiseSyntheticEventsOnChannel(channel) ;
        }
      ) ;
      #if SUPPORT_STATE_CHANGE_EVENTS
        m_raiseSyntheticStateChangedEventFromMessage_inProgress = false ;
      #endif
    }

    public bool? AllChannelsConnectedSuccessfully { get ; private set ; }

    /// <summary>
    /// Wait for a little while (as defined by <see cref="Clf.ChannelAccess.Settings.CommsTimeoutPeriodRequested"/>)
    /// to give an opportunity for all the installed Channels to connect and report their current Value.
    /// </summary>
    /// <returns>True if all channels successfully connected and reported their current Values, 
    /// before the Timeout Period expired.</returns>

    public async Task<bool> TryFinishInitialisationAsync ( System.Action<IChannel>? channelFailedToConnectWithinTimeoutPeriod = null )
    {
      // Here we wait a while for all our channels to report that they've
      // successfully connected. Once that's happened, we can query any
      // of their values etc and also set up event handlers that will 
      // tell us about subsequent changes.
      // This 'AllChannelsConnectedSuccessfully' flag could just be a local variable,
      // but it's useful for client code to know whether or not all the channels connected ...
      AllChannelsConnectedSuccessfully = await AllInstalledChannels.WaitForAllChannelsToConnectAndAcquireValues() ;
      if ( AllChannelsConnectedSuccessfully is true )
      {
        // Now that we're sure that all channels have connected,
        // we know that any channel may interact with
        // any other channel (eg to get its current value) without
        // any risk that the value might not yet be available.
        // m_channelEventsHandler
        return true ;
      }
      else
      {
        // Hmm, not all our channels were available !!
        // This is bad, and how we deal with this depends 
        // on the particular situation : which channels failed,
        // which channels were critical, and so on.
        // One option would be to just keep trying,
        // ie jump back to do the 'await' again ... ???
        AllInstalledChannels.ForEachChannelThatFailedToConnectAndAcquireValue(
          channel => {
            // Log a warning message ???
            channelFailedToConnectWithinTimeoutPeriod?.Invoke(channel) ;
          }
        ) ;
        return false ;
      }
    }

    #if SUPPORT_STATE_CHANGE_EVENTS
    private void RegisterWithChannelEvent ( IChannel channel )
    {
      channel.StateChanged += Channel_StateChanged ;
    }
    private void DeregisterFromChannelEvent ( IChannel channel )
    {
      channel.StateChanged -= Channel_StateChanged ;
    }
    #endif

    //
    // This function gets invoked when a channels 'StateChanged' event is raised.
    // According to whether it describes a Connection change or a Value change event,
    // we fake it to look as if a message has been published via the Messenger.
    //

    #if SUPPORT_STATE_CHANGE_EVENTS
    private void Channel_StateChanged ( StateChange stateChange, ChannelState channelState  )
    {
      IChannel channel = stateChange.Channel ;
      StateChangedMessage message = null! ;
      if ( stateChange.DescribesConnectionStatusChange( out bool? isConnected ) )
      {
        // System.Console.WriteLine(
        //   $"{channelState.Channel.ChannelName} is now {(isConnected.Value?"CONNNECTED":"DISCONNECTED")}"
        // ) ;
        message = new ConnectionStatusChangedMessage(
          channel,
          isConnected.Value,
          channelState
        ) ;
      }
      else if ( stateChange.DescribesValueChange( out Clf.ChannelAccess.ValueInfo? valueInfo ) )
      {
        // System.Console.WriteLine(
        //   $"{
        //     channelState.Channel.ChannelName
        //   } value is {
        //     // currentState.Value_AsString()
        //     valueInfo.Value_AsString()
        //   }"
        // ) ;
        message = new ValueChangedMessage(
          channel,
          valueInfo,
          channelState
        ) ;
      }
      if ( message != null )
      {
        bool handled = HandleMessage(message) ;
      }
    }
    private void InvokeStateChangedEventHandler_OnConnectionStatusChanged ( 
      System.Action<StateChange,ChannelState> stateChangedHandler,
      IChannel                                channel, 
      bool                                    isConnected,
      ChannelState                            state
    ) {
      stateChangedHandler.Invoke(
        new StateChange.ConnectionStatusChanged(isConnected),
        state
      ) ;
    }
    private void InvokeStateChangedEventHandler_OnValueInfoChanged ( 
      System.Action<StateChange,ChannelState> stateChangedHandler,
      IChannel                                channel, 
      ValueInfo                               valueInfo,
      ChannelState                            state 
    ) {
      stateChangedHandler.Invoke(
        new StateChange.ValueChanged(valueInfo,IsInitialAcquisition:false),
        state
      ) ;
    }
    #endif


    void CommunityToolkit.Mvvm.Messaging.IRecipient<StateChangedMessage>.Receive ( StateChangedMessage message )
    {
      // message.AcknowledgeReception() ;
      {
        try
        {
          HandleIncomingMessage(message) ;
        }
        catch ( System.Exception x )
        {
          // Log the exception ??
          x.ToString(); //TODO: Handle exception in Log... suppressing warning
        }
      }
    }
    
    private static void UsageExamples (  ) 
    {
      var channelsHandler = new ChannelsHandler() ;
      IChannel myChannel = Clf.ChannelAccess.Hub.GetOrCreateChannel("xxx") ;
      channelsHandler.InstallChannel(
        myChannel,
        (isConnected,state) => { string name = myChannel.ChannelName ; }, 
        (valueInfo,state)   => { /* etc */ }
      ) ;
      // No need to 'dispose' myChannel, as the Dispose will be performed
      // when we invoke 'Dispose' on the ChannelsHandler.
      channelsHandler.Dispose() ;
    }

  }

}
