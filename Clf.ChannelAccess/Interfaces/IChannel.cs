//
// IChannel.cs
//

namespace Clf.ChannelAccess
{

  //
  //      'client' program                            Epics IOC on network
  //   +----------------------+     network      +--------------------------------+
  //   | Channel(xx:one_long) | <--------------> |       PV : xx:one_long         |
  //   +----------------------+      comms       +--------------------------------+
  //                                              Has a 'value' (.VAL) 
  //                                              - int, string, float, double[] ...
  //    Several clients can be                    Also other 'auxiliary' fields
  //    accessing the same PV                     - .DESC, .STAT/.SEVR, .HIHI, .LOLO ...
  //
  // Two modes :
  //
  //   1. Connect with a subscription to 'monitor' callbacks.
  //      Here we create a long-lived 'IChannel' object that keeps in sync
  //      with a field of a PV, receiving updates when the remote value changes.
  //
  //   2. Briefly connect, get/put value, then disconnect.
  //      This is done via methods on the 'ChannelsHub' class,
  //      eg 'await Helpers.GetValueAsync("mvPv.VAL")'
  //
  // The interfaces described here represent the PV's value as an untyped 'object',
  // which will be of whatever type is being published by the remote PV. 
  // 

  //
  // Creating a 'channel' to a PV and connecting to it requires network comms operations
  // which will block our thread for a significant period, typically 100mS if everything
  // goes well.
  //
  #if SUPPORT_STATE_CHANGE_EVENTS
  // Once we've created a Channel, we can supply a function to handle the channel's
  // 'StateHasChanged' event. This event provides the mechanism by which we're informed
  // of interesting changes in the state our our Channel :
  //  - Are we connected or not ?
  //  - What type of data value is the PV providing ?
  //  - What 'value' has the PV published ?
  //
  // On a sunny day, we'll successfully connect to the PV and will shortly afterwards
  // be informed (A) that we're connected, and subsequently (B) what is the current value.
  //
  // First our 'statusChanged' action will be invoked to tell us
  // that we're connected, then it'll be invoked again to tell us the Value.
  // If we access the 'Value' property, it will tell us the same value
  // that was reported in the 'statusChanged' action.
  //
  // If the server providing the PV isn't running, we won't get a response
  // to our connection request and we'll have no idea what the PV's value might be.
  // 
  // We can interact with the PV by querying various properties, and we can submit
  // requests to change the value. Change requests might or might not be honoured by the PV,
  // and it's also possible that a change we asked for might get immediately trumped by a
  // change submitted by someone else. Whenever the PV's value actually changes, our Channel
  // gets notified and our 'statusChanged' action will get invoked telling us about the 
  // latest value (which might or might not be the value we recently asked for).
  // 
  // Our connection to the remote PV might get interrupted at any time, and when that happens
  // our 'statusChanged' action will be invoked, to tell us that we're now disconnected.
  //
  // A rainy-day scenario is also likely. It's possible that the PV we're referring to
  // won't be active at the time we try to connect, and that our 'Connect' attempt will
  // fail with a timeout error. All is not lost however as the PV might nevertheless
  // come alive at a future time.
  //
  #endif  

  //
  // This interface represents the methods that are relevant to client code.
  //
  // The implementation is handled by a concrete 'RemoteChannel' class,
  // which implements 'IChannel' and contains a lot of internal mechanisms
  // that we don't want to expose to the outside world. The 'RemoteChannel' class
  // is marked as 'internal', so it isn't accessible to code outside of the
  // Clf.ChannelAccess assembly.
  //
  //

  // TODO : REVIEW THE XML COMMENTS !!!

  /// <summary>
  /// This interface encapsulates the publicly accessible properties and methods of a Channel.
  /// <br/><br/>
  /// Additional methods are available as Extension Methods.
  /// </summary>

  public interface IChannel : System.IDisposable
  {

    /// <summary>
    /// Factory method that returns an existing instance of a Channel, if one has already been
    /// created, or creates a new channel. When you're finished with the Channel, you must remember to
    /// invoke 'Dispose', otherwise a warning message will be written to the system log and you will 
    /// incur a fine of 20p.
    /// <code>
    ///   // Create a channel, or return an existing instance if one is available.
    ///   var myChannel = Hub.GetOrCreateChannel("myPv") ;
    ///   // Typically we'll hook into the channel's 'state-has-changed' event
    ///   // so that we'll be informed about interesting events, eg Value changes.
    ///   myChannel.StateChanged += MyStateChangedHandler ;
    ///   ...
    ///   // Mustn't forget to deregister our event handler !!
    ///   myChannel.StateChanged -= MyStateChangedHandler ;
    ///   myChannel.Dispose() ;
    /// </code>
    /// Consider using the 'using' keyword to ensure that 'Dispose' will be invoked 
    /// when the 'myChannel' reference goes out of scope.
    /// <br/><br/>
    /// If a Channel to the specified PV didn't already exist, a new instance of a Channel
    /// will have been created. The API deliberately doesn't provide a way for you to query
    /// whether the Channel already existed, because that's not something that your code
    /// should have any dependency on.
    /// <br/><br/>
    /// The IChannel that you get back might be already in a 'connected' state, in which case 
    /// you can access the current Value immediately. However you can't rely on that - depending on
    /// various factors such as network load, it might take several seconds for the channel 
    /// to 'connect'. If the IOC isn't running then you'll *never* get into a Connected state.
    /// <br/><br/>
    /// Note that the usual way to handle a channel's lifetime is via a <see cref="ChannelAccess.ChannelsHandler"/>, where you
    /// install all your IChannel references into the handler and it takes care of invoking Dispose.
    /// </summary>
    
    /// <summary>
    /// Reports the name (as a <see cref="ChannelAccess.ChannelName"/>) of the Channel.
    /// </summary>
    
    ChannelName ChannelName { get ; }

    /// <summary>
    /// Reports the ValueAccessMode that has been configured for the Channel. 
    /// This determines what additional information is provided along with the Value.
    /// </summary>

    ValueAccessMode ValueAccessMode { get ; }

    /// <summary>
    /// If the ValueAccessMode is 'GetValueAndServerTimeStamp', the server may have
    /// sent a TimeStamp along with the Value.
    /// </summary>
    
    System.DateTime? TimeStampFromServer { get ; }

    // You can *always* retrieve the 'current state', even when
    // the instance has just been created and we haven't yet
    // received a response to our 'connect' request.

    /// <summary>
    /// Returns an immutable snapshot of the current 'state' of the Channel, as a <see cref="ChannelStatesSnapshot"/>. 
    /// You can drill into the ChannelStatesSnapshot's properties to find out whether the channel is connected,
    /// what the current Value is, and so on ; and also find out what its 'previous' state was, and get
    /// a summary of the change.
    /// </summary>

    ChannelStatesSnapshot Snapshot ( ) ;

    //
    // Whenever the 'state' of the PV changes, this 'action' gets fired.
    //
    // The action gets fired whenever anything 'interesting' happens :
    //
    //   - Connection is established
    //   - Connection is lost
    //   - Value of the PV becomes known (following an initial connect)
    //   - Value of the PV changes
    //

    /// <summary>
    /// The action gets fired whenever anything 'interesting' happens, such as :
    ///   <list type="bullet">
    ///     <item>
    ///       <description>Connection is established</description>
    ///     </item>
    ///     <item>
    ///       <description>Connection is lost</description>
    ///     </item>
    ///     <item>
    ///       <description>Value of the PV becomes known (following an initial connect)</description>
    ///     </item>
    ///     <item>
    ///       <description>Value of the PV changes</description>
    ///     </item>
    ///   </list>
    ///   <code>
    ///     - Connection is established
    ///     - Connection is lost
    ///     - Value of the PV becomes known (following an initial connect)
    ///     - Value of the PV changes
    ///   </code>
    /// </summary>
    #if SUPPORT_STATE_CHANGE_EVENTS
    static bool StateChangedEventIsSupported => true ;
    [System.Obsolete("Use a ChannelsHandler to define how events are handled")]
    event System.Action<StateChange,ChannelState>? StateChanged ;
    #else
    // static bool StateChangedEventIsSupported => false ;
    // [System.Obsolete("Use a ChannelsHandler to define how events are handled")]
    // event System.Action<StateChange,ChannelState>? StateChanged 
    // {
    //   add    => throw new System.NotSupportedException("The 'StateChanged' event is no longer supported") ;
    //   remove => throw new System.NotSupportedException("The 'StateChanged' event is no longer supported") ;
    // }
    #endif

    /// <summary>
    /// This property provides metadata describing the Field referred to by the ChannelName, 
    /// ie its data type, number of array elements and so on. It is available once the Channel 
    /// has successfully connected ; prior to that a 'null' value is reported.
    /// <br/><br/>
    /// Note that because the 'FieldInfo' type is an immutable record,
    /// when we access the FieldInfo property of a channel we don't have
    /// to worry about race conditions if we access its component fields
    /// such as the DbFieldDescriptor.
    /// </summary>

    FieldInfo? FieldInfo { get ; }

    //
    // In some circumstances it's useful to be able to 'await' a Task
    // that completes when either
    //    (A) we've been able to connect to the PV and acquire the Value,
    //        in which case the task returns 'true',
    // or (B) a 'comms timeout' has expired without us having acquired the Value,
    //        in which case the task returns 'false'.
    //
    // An alternative would be to just wait for a timeout period after creating the Channel,
    // and when the timeout expires, query the channel state to see whether we've
    // successfully connected :
    // 
    //   IChannel myChannel = ...
    //   await Task.Delay(timoutInMillseconds:2000) ; // Wait for two seconds
    //   var status = myChannel.GetCurrentStatus() ;
    //   if ( status.IsConnected )
    //   { ... etc ...
    //
    // However that simple technique would mean that we'd always be waiting
    // for the whole timeout period to expire, even if the Value became available earlier.
    // 
    // To avoid waiting for the entire timeout period, we could hook into the 'StateChanged' event
    // and cancel the 'await' via a CancellationToken when we're told that the connection has been
    // established and the Value has been acquired. That code starts to become fairly complicated though
    // and it's easier to work with this 'HasConnectedAsync' method that handles the whole process.
    //

    /// <summary>
    /// This function provides a mechanism whereby we can wait for a little while 
    /// in order to give the remote PV a chance to respond to the 'connection request'
    /// that will have been sent to the IOC when our Channel was created.
    /// <br/><br/>
    /// The 'await' returns true when we've successfully connected and have acquired the PV's value, or false
    /// if the timeout period specified in <see cref="Settings.CommsTimeoutPeriodRequested"/> expired before the value was obtained.
    /// <br/><br/>
    /// <code>
    ///   // This 'await' completes when either the remote PV responds, or the default timeout has expired.
    ///   bool hasConnectedWithinDefaultTimeoutPeriod = await myChannel.HasConnectedAndAcquiredValueAsync() ;
    ///   if ( hasConnectedWithinDefaultTimeoutPeriod )
    ///   {
    ///     // All good, the remote PV has responded to our connection request
    ///     // and we can now find out the 'fieldInfo' for the channel,
    ///     // and also the Value of the PV.
    ///     var state = myChannel.GetCurrentState() ;
    ///     // Note that because we performed an 'await' that didn't block the current thread,
    ///     // if we've installed a 'StateHasChanged' event handler then that will have fired
    ///     // once as soon as the connection was established, and a second time when the
    ///     // value was acquired.
    ///   }
    ///   else
    ///   {
    ///     // We waited a reasonable time (as defined in Settings.TimeoutPeriod)
    ///     // but up to that point there was no response from the PV. It's possible however
    ///     // that the PV might respond at some future time, in which case our 'StateHasChanged' 
    ///     // event handler will get fired in the usual way.
    ///   }
    /// </code>
    /// </summary>

    System.Threading.Tasks.Task<bool> HasConnectedAndAcquiredValueAsync ( ) ;
    
    #if true

    /// <summary>
    /// This function provides a mechanism whereby we can wait for a little while 
    /// in order to give the remote PV a chance to respond to the 'connection request'
    /// that will have been sent to the IOC when our Channel was created.
    /// <br/><br/>
    /// The 'await' returns true as soon as we've successfully connected and have received
    /// the 'FieldInfo'. At this point however we won't necessarily have acquired the Value of the PV.
    /// <br/><br/>
    /// <b>Maybe not necessary, as clients of IChannel will usually be interested in the Value. However there 
    /// might be cases where the client just wants to write a value to a Channel, without caring about the current#
    /// value - in which case this API *would* be useful.</b>
    /// </summary>

    System.Threading.Tasks.Task<bool> HasConnectedAsync ( ) ;

    #endif

    /// <summary>
    /// Write a new value, in a 'fire-and-forget' mode where we don't wait for any kind of
    /// acknowledgement from the PV.
    /// <br/><br/>
    /// If the proposed change was accepted, and the new value is different 
    /// from the previous value, our 'StateChanged' event handler will fire
    /// to let us know that the value has changed. 
    /// <br/><br/>
    /// Note that it's also possible that the value we've
    /// just written will have been rejected by the PV, in which case we'll never receive a
    /// change notification. And it's also possible that some other agent has written a different
    /// value that's taken precedence over ours, in which case when we do receive a change
    /// notification it won't mention the value we ourselves submitted.
    /// <br/><br/>
    /// In some instances a 'ValueChanged' will be fired regardless of whether the
    /// new value that's just been accepted by the PV is identical to the previous value.
    /// This depends on how the remote PV has been coded. For example a 'waveform' record
    /// will send us an update message even when the value we've written is the same,
    /// but an 'mbbi' record will only send us an update message if the new value is different.
    /// <br/><br/>
    /// Currently, we should wait until the channel is Connected before calling 'PutValue'. However,
    /// in principle we could allow PutValue to be called even before that channel had become connected,
    /// saving the value-to-be-written in a temporary variable and performing a PutValue when the Connection
    /// is established. See 'Settings.AllowPutValueEvenBeforeConnectionHasBeenEstablished'.
    /// </summary>

    void PutValue ( object valueToWrite ) ;

    /// <summary>
    /// Write a new value ... waiting for the server to confirm that the value
    /// has been accepted, and that all consequent actions have completed.
    /// <br/><br/>
    /// Returns 'true' as soon as we know that the value has been acccepted, 
    /// or 'false' if either (A) the nominal timeout expires or (B) the server 
    /// doesn't accept the new value. 
    /// <br/><br/>
    /// This API does not throw an exception unless something really unexpected happens,
    /// such as an internal error or a corruption due to a multithreading glitch.
    /// </summary>

    //
    // Strongly-typed values would be beneficial !
    // With helper methods to convert types eg string / enum / integer
    //

    System.Threading.Tasks.Task<PutValueResult> PutValueAsync ( object valueToWrite ) ;

    /// <summary>
    /// Write a new value ... waiting not only (A) for the server to confirm 
    /// that the value has been accepted, and that all consequent actions have completed, 
    /// but also (B) for a value-update message from the server. That message will have
    /// triggered a state-change telling us the updated value.
    /// <br/><br/>
    /// Returns 'true' as soon as we know that the value has been acccepted
    /// and that a message has been received notifying us of the new current value.
    /// Returns 'false' if either (A) the nominal timeout expires 
    /// or (B) the server doesn't accept the new value. 
    /// <br/><br/>
    /// Note that this API does NOT throw an exception if a timeout occurs - instead,
    /// it returns an error value.
    /// </summary>

    System.Threading.Tasks.Task<PutValueResult> PutValueAckAsync ( object valueToWrite ) ;

    /// <summary>
    /// Submit a request to the server, asking it to report the Value.
    /// <br/><br/>
    /// This is only necessary when we haven't registered a handler for the StateChanged event. 
    /// If we've registered a handler, we'll be immediately notified of any changes to the PV's value, 
    /// so making an explicit query will just return the same value that we could have found out 
    /// by looking at the current state via 'GetCurrentState()'.
    /// </summary>

    System.Threading.Tasks.Task<GetValueResult> GetValueAsync ( ) ;

    // Also considered ...
    // System.Threading.Tasks.Task<ValueInfo?> GetValueAsync ( ) ; 

    #if SUPPORT_VALUE_CHANGE_THROTTLING
      /// <summary>
      /// In some cases a PV will publish ValueChanged updates at a rate 
      /// that's unreasonably rapid, eg a camera might send us new images every 5mS.
      /// In that case a UI will be swamped with messages that it can't deal with in time. 
      /// <br/><br/>
      /// By setting this property to a non-null value, eg representing 100mS, we can 
      /// 'throttle' the incoming updates so that they are published no more frequently
      /// that the specified period. Updates that we receive until that time has expired
      /// will be quietly discarded. This is not ideal, as we don't get to see the
      /// most recently sent value.
      /// <br/><br/>
      /// An alternative 'throttling' strategy would be to wait for a specified period
      /// following the reception of any update, during which time further updates might arrive,
      /// and when that timeout expires, publish the most recently acquired value. That
      /// has the advantage of telling us the most recently sent value, but that value
      /// isn't published immediately - only after a delay.
      /// </summary>
      System.TimeSpan? MinimumTimeBetweenPublishedValueUpdates { get ; set ; }
    #endif

  }

}

