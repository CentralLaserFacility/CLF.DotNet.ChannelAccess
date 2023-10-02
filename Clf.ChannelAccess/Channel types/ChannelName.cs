//
// ChannelName.cs
//

using System.Diagnostics.CodeAnalysis ;

namespace Clf.ChannelAccess
{

  /// <summary>
  /// Represents an identifier that can be used to access a PV published by an IOC.
  /// <br/><br/>
  /// The identifer is considered to have two parts :
  /// <code>
  ///    TZ:Mot:P6K1.VAL
  ///    ########### ####
  ///    |           |
  ///    baseName    fieldName
  /// </code>
  /// If the 'field name' is omitted, 'VAL' is assumed.
  /// <br/><br/>
  /// Where a <see cref="ChannelName"/> is expected you can supply a string, and vice versa. 
  /// </summary>

  public record class ChannelName ( string Name ) 
  {

    public static implicit operator ChannelName ( 
      string identifier 
    ) => new ChannelName(identifier) ;

    public static implicit operator string ( ChannelName channelName )
    => channelName.Name ;

    public ValidatedChannelName AsValidatedName ( ) => new ValidatedChannelName(this) ;

    public bool IsValid ( 
      [NotNullWhen(true)]  out ValidatedChannelName? validatedChannelName,
      [NotNullWhen(false)] out string?               whyNotValid
    ) {
      return Name.IsValidChannelName( 
        out validatedChannelName,
        out whyNotValid
      ) ;
    }

    public bool IsValid ( [NotNullWhen(false)] out string? whyNotValid ) 
    => Name.IsValidChannelName( 
      out var _,
      out whyNotValid
    ) ;

    public bool IsValid ( ) 
    => Name.IsValidChannelName( 
      out var _,
      out var _
    ) ;

    public override string ToString ( ) => Name ;

  }

}
