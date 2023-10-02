//
// ChannelNotificationBase.cs
//

using Clf.ChannelAccess.ExtensionMethods ;

namespace Clf.ChannelAccess
{

  //
  // Channel Notifications are sent to the Hub, for logging.
  //
  // These events are stateful as they record the Channel and the TimeStamp.
  //

  // TODO_XML_DOCS

  public abstract record ChannelNotificationBase 
  {

    //
    // Aha ! We want to make the Notification immutable,
    // as far as clients are concerned ; best way to do this 
    // is to use 'init' but make it internal, so that client code
    // is prevented from creating a clone using 'with' syntax.
    //
    // The Channel property is set via a 'with' statement
    // in RaiseInterestingEventNotification, and SetNewState ...
    //

    public IChannel Channel { get ; internal init ; } = null! ;

    //
    // Various 'string' properties are provided in the concrete record
    // definitions, as named properties. Their values are passed down
    // to this base class via the constructors, so that they can be
    // shown via the 'ToString()' method.
    //
    // The alternative would be to provide a virtual function
    // that would be implemented in each concrete record ?
    // That might be cleaner actually ...
    //

    private readonly string? m_additionalInfo ;

    protected ChannelNotificationBase ( string? additionalInfo = null )
    {
      // The 'Channel' property is *always* assigned
      // by the time the notification is published.
      m_additionalInfo = additionalInfo ;
    }

    public System.DateTime TimeStamp { get ; } = System.DateTime.Now ;

    public double TimeStamp_InSecondsAfterChannelCreation => (
      TimeStamp - Channel.AsChannelBase().CreationTimeStamp
    ).TotalSeconds ; 

    //
    // Aha, in C# 10 we can do this !!!
    //
    // https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-10#record-types-can-seal-tostring
    // Sealing the ToString method prevents the compiler from synthesizing a 'ToString' method for any derived record types.
    // A sealed ToString ensures all derived record types use the ToString method defined in a common base record type.
    //

    public sealed override string ToString ( )
    {
      // https://www.csharp-examples.net/string-format-double/
      return (
        Channel.ChannelNameWithValueAccessModeAndChannelIdentifier()
      + " at \u0394t = " // delta
      + TimeStamp_InSecondsAfterChannelCreation.ToString("000.00000") // "F3" // "000.###"
      + " : "
      + this.GetType().Name 
      + (
          string.IsNullOrEmpty(m_additionalInfo)
          ? ""
          : $" {m_additionalInfo}"
        )
      ) ;
    }

  }


}
