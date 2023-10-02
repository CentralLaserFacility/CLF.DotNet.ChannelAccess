//
// DllCallbackHandlers.cs
//

using Clf.ChannelAccess.ExtensionMethods ;
using Clf.ChannelAccess.LowLevelApi.ExtensionMethods ;
using FluentAssertions ;

namespace Clf.ChannelAccess
{

  internal static class DllCallbackHandlers
  {

    static DllCallbackHandlers ( )
    {
      //
      // Initialise the event handlers that we'll reference in the C API
      //
      ConnectionEventCallbackHandler     = HandleConnectionEventCallback ;
      MonitorEventCallbackHandler        = HandleMonitorEventCallback ;
      ValueQueryEventCallbackHandler     = HandleValueQueryResponseCallback ;
      WriteCompletedEventCallbackHandler = HandleWriteCompletedCallback ;
      ExceptionCallbackHandler           = HandleExceptionCallback ;
      PrintfCallbackHandler              = HandlePrintfCallback ;
    }

    // Invoked as a consequence of calling 'ca_create_channel'

    public static LowLevelApi.ConnectionCallback ConnectionEventCallbackHandler = (args) => { } ;

    // Invoked as a consequence of calling 'ca_create_subscription'

    public static LowLevelApi.ValueUpdateCallback MonitorEventCallbackHandler = (args) => { } ;

    // Invoked as a consequence of calling 'ca_array_get_callback'

    public static LowLevelApi.ValueUpdateCallback ValueQueryEventCallbackHandler = (args) => { } ;

    // Invoked as a consequence of calling 'ca_array_put_callback'

    public static LowLevelApi.ValueUpdateCallback WriteCompletedEventCallbackHandler = (args) => { } ;

    // Handler for exception messages

    public static LowLevelApi.ExceptionHandlerCallback ExceptionCallbackHandler = (args) => { } ;

    // Handler for 'printf' messages

    public static LowLevelApi.PrintfCallback PrintfCallbackHandler = (format,va_list) => { } ;

    // Aha, this *does* get invoked, with 'format' telling us that the CA reperater
    // has been unable to be contacted after 50 tries ...

    public static void HandlePrintfCallback ( string format, System.ArgIterator va_list )
    {
      // HandleExceptionMessageLine?.Invoke(
      //   $"Message : '{format}' with {va_list.GetRemainingCount()} args"
      // ) ;
    }

    public static void HandleExceptionCallback ( LowLevelApi.ExceptionHandlerEventArgs exceptionEventArgs )
    {
      // This method gets invoked on a worker thread !!!
      // string exceptionMessage = ChannelAccessApi.Unsafe.GetNullTerminatedBytesAsString(
      //   exceptionEventArgs.ctx
      // ) ;
      // IncomingMessages.Writer.TryWrite(
      //   new ExceptionMessage(
      //     null,
      //     System.DateTime.Now,
      //     exceptionMessage
      //   )
      // ).Should().BeTrue() ;
    }

    //
    // This gets invoked on a worker thread when a channel becomes connected or disconnected.
    //
    // We need to raise the event on the thread that the UI is running on, and that would be
    // straightforward in a UI app with a message loop. However we also want to run this
    // as a console app and with XUnit tests, and in that case raising an event on
    // the correct Context is problematic.
    //

    public static void HandleConnectionEventCallback ( LowLevelApi.ConnectionStatusChangedEventArgs connectionEventArgs )
    {
      // LowLevelApi.DllFunctions.VerifyCurrentThreadIsAttachedToClientContext() ;
      // The 'connectionEventArgs' doesn't let us supply a 'tag' value that we can use
      // to identify the channel - but it provides a physical pointer to the channel
      // structure as allocated by the 'CreateChannel' API..
      // Given that physical pointer we can look up the 'tag' data for the channel,
      // which holds the integer 'ChannelIdentifier' assigned to the channel,
      // and use that to look up the Channel instance we're being notified about.
      LowLevelApi.ChannelHandle channelHandle = connectionEventArgs.pChannel ;
      channelHandle.Should().NotBe(System.IntPtr.Zero) ;
      int channelIdentifier = LowLevelApi.DllFunctions.ca_puser(channelHandle) ;
      if ( 
        ChannelsRegistry.TryLookupRegisteredChannel(
          channelIdentifier,
          out var channel
        )
      ) {
        bool isConnected = connectionEventArgs.connectionState switch
        {
          LowLevelApi.ConnectionStatusChangedEventArgs.CA_OP_CONN_UP => true,
          LowLevelApi.ConnectionStatusChangedEventArgs.CA_OP_CONN_DOWN => false,
          _ => throw new UnexpectedConditionException(
                 $"Unexpected connection state {connectionEventArgs.connectionState}"
               )
        };
        if ( LowLevelApi.DllFunctions.ca_has_invalid_field_type(channelHandle) )
        {
          // Under some circumstances invalid field type is raised on channel disconnection
          // and that is okay (Example: IocStats support module channels).
          // Raise Interesting Event Notifications on both connect/disconnect states.
          // In Future maybe instead of using UnexpectedCondition, we can use appropriate condition ??
          if (isConnected)
          {
            channel.AsChannelBase().RaiseInterestingEventNotification(
              new ChannelAccess.AnomalyNotification.UnexpectedCondition("C API reported invalid field type on connection")
            );
            return;
          }
          else
          {
            // Under some circumstances 'ca_field_type' would try to interpret
            // a crazy value of -1 returned by the API (not documented).
            // Capture this connection event. Hmm, suppose we subsequently get further events
            // for this channel - maybe we should be setting a 'channel-not-valid' flag
            // and ignoring all future interactions on the channel ???
            channel.AsChannelBase().RaiseInterestingEventNotification(
              new ChannelAccess.AnomalyNotification.UnexpectedCondition("C API reported invalid field type on disconnect")
            );
          }
        }   
        channel.AsRemoteChannel().HandleConnectionCallback(
          connectionEventArgs.pChannel,
          isConnected
        ) ;
      }
      else
      {
        // Channel is no longer registered !!!
        ChannelAccess.Hub.HandleWarningMessage(
          $"C DLL invoked a connection callback on a channel that is no longer registered (id={channelIdentifier})"
        ) ;
      }
    }

    //
    // This gets invoked on a worker thread when ChannelAccess tells us
    // about a change in the Value of a PV Field, as a consequence of
    // our having subscribed to change notifications on that channel.
    //

    public static void HandleMonitorEventCallback ( LowLevelApi.ValueUpdateNotificationEventArgs args )
    {
      HandleValueUpdateNotificationEvent(
        args,
        (channel,pvValueInfo) => channel.HandleValueUpdateCallback(pvValueInfo) 
      ) ;
    }

    //
    // This gets invoked on a worker thread when ChannelAccess tells us the Value
    // of a PV Field, following a query that we initiated via 'ca_array_get_callback'.
    //

    public static void HandleValueQueryResponseCallback ( LowLevelApi.ValueUpdateNotificationEventArgs args )
    {
      HandleValueUpdateNotificationEvent(
        args,
        (channel,pvValueInfo) => channel.HandleValueQueryCallback(pvValueInfo) 
      ) ;
    }

    //
    // Here we handle messages pertining to 'value' updates,
    // arising either from a subscription or from a 'get' request
    //

    public static unsafe void HandleValueUpdateNotificationEvent ( 
      LowLevelApi.ValueUpdateNotificationEventArgs        args,
      System.Action<RemoteChannel,ValueInfo> handleDecodedResult
    ) {
      // This callback always gets invoked on a worker thread
      var thread = System.Environment.CurrentManagedThreadId ;
      LowLevelApi.DllFunctions.VerifyCurrentThreadIsAttachedToClientContext() ;
      // The message provides a low level pointer to a block of data.
      // We'll copy that into a C# 'array' of an appropriate type,
      // and submit a message to the incoming-events queue.
      if ( 
        ChannelsRegistry.TryLookupRegisteredChannel(
          (int) args.tagValue,
          out IChannel? channel
        )
      ) {
        ValueInfo? decodedValue = null ;
        try
        {
          RemoteChannel remoteChannel = channel.AsRemoteChannel() ;
          if ( args.ecaStatus == LowLevelApi.ApiConstants.ECA_NORMAL )
          {
            args.pDbr.Should().NotBe(System.IntPtr.Zero) ;
            LowLevelApi.DbRecordRequestType dbrType = (LowLevelApi.DbRecordRequestType) args.dbrType ;
            (DbFieldType fieldType, ValueAccessMode valueAccessMode) = dbrType.GetDbFieldTypeAndValueAccessMode() ;
            int channelIdentifier = LowLevelApi.DllFunctions.ca_puser(args.pChannel) ;
            channelIdentifier.Should().Be(remoteChannel.ChannelIdentifier) ;
            remoteChannel.LowLevelHandleIs(args.pChannel).Should().BeTrue() ;
            fieldType.Should().Be(channel.FieldInfo!.DbFieldDescriptor.DbFieldType) ;
            #if SUPPORT_VALUE_CHANGE_THROTTLING
              if ( remoteChannel.MinimumTimeBetweenPublishedValueUpdates.HasValue )
              {
                // For this channel, there's a possibility that updates might arrive at
                // an unreasonably rapid rate that we wouldn't be able to keep up with,
                // so let's implement a simple 'throttling' mechanism ...
                System.DateTime timeWhenThisUpdateArrived = System.DateTime.Now ;
                if ( remoteChannel.TimeWhenMostRecentValueChangeNotified.HasValue )
                {
                  System.TimeSpan timeSinceLastUpdateHandled = (
                    timeWhenThisUpdateArrived 
                  - remoteChannel.TimeWhenMostRecentValueChangeNotified.Value 
                  ) ;
                  if ( timeSinceLastUpdateHandled < remoteChannel.MinimumTimeBetweenPublishedValueUpdates.Value )
                  {
                    // This update has arrived very soon after the previous one,
                    // so let's discard it ...
                    return ;
                  }
                  else
                  {
                    // There's been a reasonable time between this most recent update
                    // and the previous one, so let's remember the arrival time and proceed
                    remoteChannel.TimeWhenMostRecentValueChangeNotified = timeWhenThisUpdateArrived ;
                  }
                }
                else
                {
                  // This is the first update we've received, so remember the arrival time and proceed
                  remoteChannel.TimeWhenMostRecentValueChangeNotified = timeWhenThisUpdateArrived ;
                }
              }
            #endif
            decodedValue = LowLevelApi.Unsafe.CreateValueInfoFromDbRecordStruct(
              remoteChannel,
              args.pDbr.ToPointer(),
              new LowLevelApi.DbRecordRequestDescriptor(dbrType),
              args.nElements,
              channel.FieldInfo!,
              out string[]? enumOptionNames
            ) ;
            // If we haven't already received 'enum' info, set it now.
            // WE DO THIS ONCE - BUT THE PV MIGHT HAVE CHANGED UNDER OUR FEET !!! Leave for now :)
            channel.FieldInfo!.DbFieldDescriptor.SetEnumNames(enumOptionNames) ;
          }
          else
          {
            args.pDbr.Should().Be(System.IntPtr.Zero) ;
          }
        }
        catch ( System.Exception x )
        {
          Hub.NotifyExceptionCaught(
            channel,
            x 
          ) ;
        }
        finally
        {
          if ( decodedValue != null )
          {
            handleDecodedResult(
              channel.AsRemoteChannel(),
              decodedValue
            ) ;
          }
        }
      }
      else
      {
        // Channel is no longer active, even though
        // we've received a message destined for it ...
      }
    }

    public static void HandleWriteCompletedCallback ( LowLevelApi.ValueUpdateNotificationEventArgs args )
    {
      // This callback always gets invoked on a worker thread
      if ( 
        ChannelsRegistry.TryLookupRegisteredChannel(
          (int)         args.tagValue,
          out IChannel? channel
        )
      ) {
        channel.AsRemoteChannel().HandleWriteCompletedCallback(args) ;
      }
    }

  }

}
