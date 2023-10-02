//
// StateChange.cs
//

using System.Diagnostics.CodeAnalysis ;
using Clf.ChannelAccess.ExtensionMethods ;

namespace Clf.ChannelAccess
{

  /// <summary>
  /// This notification describes the single thing that changed
  /// when we transitioned from the Previous state to the Current state.
  /// </summary>


  public abstract record StateChange : ChannelNotificationBase
  {

    public bool DescribesConnectionStatusChange ( ) => this is ConnectionStatusChanged ;

    public bool DescribesConnectionStatusChange ( 
      [NotNullWhen(true)] out bool? isConnected
    ) {
      if ( this is ConnectionStatusChanged connectionStatusChanged )
      {
        isConnected = connectionStatusChanged.IsConnected ;
        return true ;
      }
      else
      {
        isConnected = null ; 
        return false ;
      }
    }

    public bool DescribesConnectionStatusChange ( 
      [NotNullWhen(true)] out bool? isConnected, 
      [NotNullWhen(true)] out bool? isFirstSucessfulConnection 
    ) {
      if ( this is ConnectionStatusChanged connectionStatusChanged )
      {
        isConnected                = connectionStatusChanged.IsConnected ;
        isFirstSucessfulConnection = this is ConnectionEstablished ;
        return true ;
      }
      else
      {
        isConnected                = null ; 
        isFirstSucessfulConnection = null ; 
        return false ;
      }
    }

    public bool DescribesValueChange ( ) => this is ValueChanged ;

    public bool DescribesValueChange ( 
      [NotNullWhen(true)] out ValueInfo? newValue
    ) {
      if ( this is ValueChanged valueChange )
      {
        newValue = valueChange.ValueInfo ;
        return true ;
      }
      else
      {
        newValue = null ;
        return false ;
      }
    }
    
    public bool DescribesValueChange ( 
      [NotNullWhen(true)] out ValueInfo? newValue,
      [NotNullWhen(true)] out bool?      isInitialAcquisition
    ) {
      if ( this is ValueChanged valueChange )
      {
        newValue             = valueChange.ValueInfo ;
        isInitialAcquisition = this is StateChange.ValueAcquired ;
        return true ;
      }
      else
      {
        newValue             = null ;
        isInitialAcquisition = null ;
        return false ;
      }
    }
    
    // Subclasses ...

    public /*abstract*/ record ConnectionStatusChanged ( bool IsConnected ) : StateChange() ;

    public record ConnectionValidityChanged ( ChannelValidityStatus ValidityStatus ) : StateChange() ;

    public record ValueChanged ( ValueInfo ValueInfo, bool IsInitialAcquisition ) : StateChange() ;

    // Channel lifetime events

    //
    // This is the first 'state change' raised. 
    // The channel is not yet connected, in fact we haven't event attempted to connect.
    // So nothing is known about the PV, apart from its name ; all the properties are null,
    // we don't yet know the data type (FieldInfo) or the Value (ValueInfo).
    //
    public record ChannelCreated ( ) : StateChange() ;

    //
    // If the channel name has an invalid syntax, we'll remain in this state forever.
    //

    public record ChannelCreatedAsInvalid ( ) : StateChange() ;

    // This event is raised just prior to the Channel being destructed.
 
    public record ChannelDisconnecting ( ) : StateChange() ;

    // Connection events

    public record ConnectionEstablished ( FieldInfo FieldInfo ) : ConnectionStatusChanged(true) ;

    public record ConnectionLost        (                     ) : ConnectionStatusChanged(false) ;
                                                                                
    public record ConnectionRestored    (                     ) : ConnectionStatusChanged(true) ;

    // Value change events

    public record ValueAcquired       ( ValueInfo ValueInfo ) : ValueChanged(ValueInfo,true) ;

    public record ValueChangeNotified ( ValueInfo ValueInfo ) : ValueChanged(ValueInfo,false) ;

    // Warning events - hmm, handle these in the Hub !!!

    // public record ConnectionAttemptTimedOut ( System.TimeSpan TimeElapsed ) : ConnectionEvent(false) ;

    // public record ValueAcquisitionTimedOut ( System.TimeSpan TimeElapsed ) : ChangeDescriptor ; 

    // public record UnexpectedException ( System.Exception Exception ) : BadNewsEvent ;
    // 
    // public record FieldInfoHasChangedOnReconnect : BadNewsEvent ;

    public static void UsageDemo_A ( StateChange change )
    {
      switch ( change )
      {
      case ChannelCreated:
        System.Console.WriteLine(
          $"Channel {change.Channel.ChannelName} has been created, not yet connected so Value is unknown"
        ) ;
        break ;
      case ConnectionEstablished connectionEstablished:
        System.Console.WriteLine($"Connection has been established") ;
        System.Console.WriteLine($"Connected : {connectionEstablished.IsConnected}") ;
        System.Console.WriteLine($"Information about the PV is now known") ;
        connectionEstablished.FieldInfo.RenderAsStrings(
          System.Console.WriteLine
        ) ;
        break ;
      case ConnectionLost connectionLost:
        System.Console.WriteLine($"Connection has been lost") ;
        break ;
      case ConnectionRestored connectionRestored:
        System.Console.WriteLine($"Connection has been restored") ;
        break ;
      case ValueAcquired valueAcquired:
        System.Console.WriteLine(
          "Value has been acquired"
        ) ;
        valueAcquired.ValueInfo.RenderAsStrings(
          System.Console.WriteLine
        ) ;
        break ;
      case ValueChangeNotified valueChangeNotified:
        System.Console.WriteLine(
          "We've been notified that the Value has changed"
        ) ;
        valueChangeNotified.ValueInfo.RenderAsStrings(
          System.Console.WriteLine
        ) ;
        break ;
      default:
        System.Console.WriteLine(
          $"State changed : {change}"
        ) ;
        break ;
      }
    }

    public static void UsageDemo_B ( StateChange change )
    {
      switch ( change )
      {
      case ChannelCreated:
        System.Console.WriteLine(
          $"Channel {change.Channel.ChannelName} has been created, not yet connected so Value is unknown"
        ) ;
        break ;
      case ConnectionStatusChanged connectionStatusChanged:
        System.Console.WriteLine(
          connectionStatusChanged.IsConnected
          ? "Channel is connected"
          : "Channel is not connected"
        ) ;
        break ;
      case ValueChanged valueChanged:
        System.Console.WriteLine(
          "Value has changed"
        ) ;
        valueChanged.ValueInfo.RenderAsStrings(
          System.Console.WriteLine
        ) ;
        break ;
      }
    }

    public static void UsageDemo_C ( StateChange change )
    {
      if (
        change.DescribesValueChange(
          out var value,
          out var isInitialAcquisition
        )
      ) {
        // ...
      }
      else if (
        change.DescribesConnectionStatusChange(
          out var isNowConnected,
          out var isFirstSuccessfulConnection
        )
      ) {
        // ...
      }
    }

  }

}

