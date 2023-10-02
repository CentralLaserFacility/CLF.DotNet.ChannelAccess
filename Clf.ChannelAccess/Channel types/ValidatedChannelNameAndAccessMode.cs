//
// ChannelNameAndAccessMode.cs
//

namespace Clf.ChannelAccess
{

  // Rename => ValidatedChannelNameAndAccessMode

  internal record ValidatedChannelNameAndAccessMode ( 
    ValidatedChannelName ValidatedChannelName, 
    ValueAccessMode      ValueAccessMode 
  ) {

    // public static implicit operator ChannelNameAndAccessMode ( string channelName )
    // => new ChannelNameAndAccessMode(channelName) ;
    // 
    // public static implicit operator ChannelNameAndAccessMode ( ChannelName channelName )
    // => new ChannelNameAndAccessMode(channelName) ;

    public override string ToString ( ) 
    => InternalHelpers.GetChannelNameAndAccessModeAsString(
      ValidatedChannelName,
      ValueAccessMode
    ) ;

    // If we're accessing a VAL field, all the ValueAccessMode options are permitted.
    // For other fields, we should only be accessing the Value itself.

    public bool IsValid ( out string? whyNotValid ) 
    {
      whyNotValid = ( 
        ValidatedChannelName.IdentifiesValField
        ? null
        : (
            ValueAccessMode == ValueAccessMode.DBR_RequestValueAndNothingElse
            ? null
            : $"ValueAccessMode of {ValueAccessMode} is only permitted for a 'VAL' field"
          )
      ) ;
      return (
        whyNotValid is null
        ? true
        : false
      ) ;
    }

    public bool IsValid ( ) 
    => IsValid( 
      out var _ 
    ) ;

  }

}
