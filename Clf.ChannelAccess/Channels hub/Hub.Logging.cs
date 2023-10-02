//
// Hub_event_logging.cs
//
// LOGGING
//

using System.Collections.Generic ;
using System.Linq ;
using System.Runtime.InteropServices ;
using FluentAssertions ;
using static Clf.ChannelAccess.Helpers ;
using Clf.ChannelAccess.ExtensionMethods ;
using Clf.ChannelAccess.LowLevelApi.ExtensionMethods ;
using Clf.Common.ExtensionMethods ;
using static Clf.ChannelAccess.LowLevelApi.EcaCodeExtensionMethods ;
using System.Diagnostics.CodeAnalysis ;
using static CommunityToolkit.Mvvm.Messaging.IMessengerExtensions ;

// This is IMHO cleaner than having 'using Microsoft.Extensions' 
// because it forces us to fully qualify API calls to the Logger
using static Microsoft.Extensions.Logging.LoggerExtensions ;

namespace Clf.ChannelAccess
{

  // TODO : Move the static fields to an EventLog that we access via an 'Instance' property !!!
  // TODO : change 'InterestingEvent' to 'Notification' in the API

  partial class Hub
  {

    // Hmm, what's the best way of instantiating this Logger ??
    // Ideally this needs to be done in the static constructor ...
    // 1. Somehow access a global instance of the app's ServiceCollection ?
    // 2. Have the app call a function to install a Logger into the Hub ...
    // private static Microsoft.Extensions.DependencyInjection.ServiceCollection ServiceCollection ;

    // https://docs.microsoft.com/en-us/archive/msdn-magazine/2016/april/essential-net-logging-with-net-core
    // The recommendation is to have an individual ILogger instance for each instance of a class.

    // Hmm, the static Hub methods are convenient by ultimately evil !!!
    // Thread safety etc.

    public static Microsoft.Extensions.Logging.ILogger<IChannel>? Logger; // Might not get initialised !!

    private static System.Collections.Concurrent.ConcurrentQueue<Notification> m_notificationsQueue = new() ;

    /// <summary>
    /// Interesting Event Occurred.
    /// </summary>
    public static event System.Action<Notification>? OnInterestingEvent ;

    public static IEnumerable<Notification> AllInterestingEvents => m_notificationsQueue ;

    public static string ChannelNamePattern = "." ;

    public static IEnumerable<Notification> InterestingEventsFilteredByPattern 
    => m_notificationsQueue.Where(
      notification => notification.Channel.ChannelName.Name.Contains(ChannelNamePattern) 
    ) ;

    public static bool EnableLoggingOfAllInterestingEvents { get ; set ; } = (
      #if DEBUG
        true 
      #else
        false
      #endif
    ) ;

    internal static void HandleNotification ( Notification notification )
    {
      notification.Channel.Should().NotBeNull() ;
      if ( EnableLoggingOfAllInterestingEvents )
      {
        m_notificationsQueue.Enqueue(notification) ;
      }
      OnInterestingEvent?.Invoke(notification) ;
      // TODO : we need to map our notifications
      // onto various 'Levels' ... also, how do we make use of
      // the structured info supported by SeriLog ??
      // Can a 'sink' get access to the type of our Notification ???
      // Eg can we publish it as JSON with appropriate fields ??
      // So that we can filter on messages pertaining to a particular Channel.
      // Logger?.LogInformation(
      //   notification.ToString()
      // ) ;
      Logger?.Log(
        Microsoft.Extensions.Logging.LogLevel.Information,
        notification.ToString()
      ) ;
      // Log(this ILogger logger, LogLevel logLevel, string? message, params object?[] args)
      // Log(this ILogger logger, LogLevel logLevel, EventId eventId, string? message, params object?[] args)
      // Log(this ILogger logger, LogLevel logLevel, Exception? exception, string? message, params object?[] args)
      // Log(this ILogger logger, LogLevel logLevel, EventId eventId, Exception? exception, string? message, params object?[] args)
      // And can we make use of the 'scope' ???
      // public static IDisposable BeginScope(
      //   this ILogger logger,
      //   string messageFormat,
      //   params object?[] args
      // )
    }

    internal static void NotifyExceptionCaught ( IChannel channel, System.Exception x )
    {
      HandleNotification(
        new AnomalyNotification.UnexpectedException(x) with { Channel = channel }
      ) ;
    }

    private static void WriteLineToConsoleAndDebug ( string messageLine )
    {
      System.Console.WriteLine(
        messageLine
      ) ;
      System.Diagnostics.Debug.WriteLine(
        messageLine
      ) ;
    }

    // Handle 'warning' messages that have been raised, eg in a ChannelsHandler.

    private static System.Collections.Concurrent.ConcurrentQueue<string> m_warningMessages = new() ;

    /// <summary>
    /// Warning Message Was raised.
    /// </summary>

    public static event System.Action<string>? OnWarningMessage 
    = (message) => WriteLineToConsoleAndDebug(
      $"ChannelAccess : {message}"
    ) ;

    public static IEnumerable<string> AllWarningMessages => m_warningMessages ;

    public static void HandleWarningMessage ( string warningMessage )
    {
      m_warningMessages.Enqueue(warningMessage) ;
      OnWarningMessage?.Invoke(warningMessage) ;
    }

    // Handle 'informational' messages that have been raised, eg when the DLL Path has been assigned.

    private static System.Collections.Concurrent.ConcurrentQueue<string> m_informationalMessages = new() ;

    /// <summary>
    /// Informational Message Was raised.
    /// </summary>

    public static event System.Action<string>? OnInformationalMessage 
    = (message) => WriteLineToConsoleAndDebug(
      $"ChannelAccess : {message}"
    ) ;

    public static IEnumerable<string> AllInformationalMessages => m_informationalMessages ;

    public static void HandleInformationalMessage ( string informationalMessage )
    {
      m_informationalMessages.Enqueue(informationalMessage) ;
      OnInformationalMessage?.Invoke(informationalMessage) ;
    }

  }

}
