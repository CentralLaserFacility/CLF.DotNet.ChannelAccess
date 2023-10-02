//
// Notification.cs
//
// LOGGING
//

using Clf.ChannelAccess.ExtensionMethods ;

namespace Clf.ChannelAccess
{

  // TODO_XML_DOCS

  public abstract record Notification ( string? AdditionalInfo ) : ChannelNotificationBase(AdditionalInfo) ;

  // Progress notifications

  // Hmm, could configure the 'additionalInfo' using an internal method ???

  public abstract record ProgressNotification ( string? AdditionalInfo = null  ) : Notification(AdditionalInfo) 
  {

    public record ApiResponseReceived ( ) : ProgressNotification() ;

    public record ActionNotification   ( string? ActionName ) : ProgressNotification(ActionName) ;

    public record CallbackNotification ( string? ActionName ) : ProgressNotification(ActionName) ;

    public record ApiCallCompleted     ( string? ApiName    ) : ProgressNotification(ApiName) ;

    public record WaitingForEvent      ( string? EventName  ) : ProgressNotification(EventName) ;

    public record WaitCompleted        ( string? EventName   ) : ProgressNotification(EventName) ;

    public record SendingMessengerMessage ( string StateChangedMessageType ) : ProgressNotification(StateChangedMessageType) ;

  }

  public abstract record ChannelLifetimeNotification ( string? additionalInfo = null ) : Notification(additionalInfo)
  {

    public record InstanceCreated ( IChannel channel ) : Notification(channel.ToString()) ;

    public record InstanceCloneCreated ( IChannel channel ) : Notification(channel.ToString()) ;

    public record InstanceCloneDisposed ( IChannel channel ) : Notification(channel.ToString()) ;

    public record InstanceFullyDisposed ( IChannel channel ) : Notification(channel.ToString()) ;

    public record InstanceHasAlreadyBeenDisposed ( IChannel channel ) : Notification(channel.ToString()) ;

  }

  public abstract record CommsNotification ( string? AdditionalInfo = null ) : Notification(AdditionalInfo)
  {

    public record ConnectionEstablished : CommsNotification ;

    public record ConnectionEstablished_ButNoAccess : CommsNotification ;

    public record ConnectionLost        : CommsNotification ;

    public record ConnectionRestored    : CommsNotification ;

    public abstract record ValueChangeNotification ( string Value ) : CommsNotification(Value) ;

    public record ValueAcquired ( string Value ) : ValueChangeNotification(Value) ;

    public record ValueAcquired_AfterSuspiciouslyLongDelay ( string Value ) : ValueChangeNotification(Value) ;

    public record ValueChangeNotified ( string Value ) : ValueChangeNotification(Value) ;

    public record ValueQueryCompleted ( string Value ) : ValueChangeNotification(Value) ;

    public record TimeoutExpired ( string? EventName = null ) : CommsNotification(EventName) ;

    public record WriteSucceeded ( string Value ) : CommsNotification(Value) ;

    public record WriteFailed ( string Value ) : CommsNotification(Value) ;

  }

  public abstract record AnomalyNotification ( string? AnomalyInfo = null ) 
  : Notification(AnomalyInfo) 
  {

    public record HaveReconnectedToChannelOnDifferentServer : AnomalyNotification ;

    public record CleanupWasCalledByFinaliser : AnomalyNotification ;

    public record UsageWarningNotification ( string WarningInfo         ) : AnomalyNotification(WarningInfo) ;

    public record UnexpectedException      ( System.Exception Exception ) : AnomalyNotification(Exception.Message) ;

    public record ApiCallFailed            ( string WhyFailed           ) : AnomalyNotification(WhyFailed) ;

    public record EventHasAlreadyBeenSet   ( string EventName           ) : AnomalyNotification(EventName) ;

    public record UnexpectedCondition      ( string WhatHappened        ) : AnomalyNotification(WhatHappened) ;

    public record UsageWarning             ( string UsageWarningInfo    ) : AnomalyNotification(UsageWarningInfo) ;

    public record UsageError               ( string UsageErrorInfo      ) : AnomalyNotification(UsageErrorInfo) ;

    // // Another way to provide the 'additional info' ...
    // 
    // public record ApiCallFailedEx : AnomalyNotification 
    // {
    //   ApiCallFailedEx ( string whyFailed )
    //   {
    //     // This has to be made writable by the derived class ...
    //     base.m_additionalInfoEx = $"WhyFailed : {whyFailed}" ;
    //   }
    // }
    // 
    // // Another way to provide the 'additional info' ...
    // // AHA ! Nice, because we get to name the property !!!
    // 
    // public record ApiCallFailedEx2 ( string WhyFailed ) : AnomalyNotification 
    // {
    //   // This would be an override of a base class virtual function ...
    //   public string? AdditionalInfoEx => $"WhyFailed : {WhyFailed}" ;
    // }

    public record UnexpectedApiCall ( 
      [System.Runtime.CompilerServices.CallerMemberName] string? ApiCallInfo = null  
    ) : AnomalyNotification(ApiCallInfo) ;

    public record SomeStateChangedHandlersWereNotDeregistered ( int HowMany ) 
    : AnomalyNotification(
      $"How many : {HowMany}"
    ) ;

  }

}

