//
// ValidatedChannelName.cs
//

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
  /// If the raw string can't be parsed into a valid identifier, an exception will be thrown.
  /// <br/><br/>
  /// You can also construct a <see cref="ChannelName"/> from a <see cref="ChannelAccess.BaseName"/> and 
  /// a <see cref="ChannelAccess.FieldName"/>. This is useful if you need to work with several PV's
  /// that differ just in the 'field' name, eg 'MyPv.HIHI' and 'MyPv.LOLO'. 
  /// </summary>

  public record class ValidatedChannelName : ChannelName // TODO : Maybe inheritance isn't such a good idea here ??
  {

    public static ValidatedChannelName FromUnvalidatedName ( ChannelName name ) => new ValidatedChannelName(name) ;

    public string BaseName { get ; }

    public string FieldName { get ; }

    public string ShortName_OmittingVAL => (
      IdentifiesValField
      ? BaseName
      : $"{BaseName}.{FieldName}" 
    ) ;

    public string FullName => $"{BaseName}.{FieldName}" ;

    public bool IdentifiesValField => FieldName == "VAL" ;

    public bool IdentifiesOtherField => ! IdentifiesValField ;

    public static implicit operator ValidatedChannelName ( 
      string identifier 
    ) => new ValidatedChannelName(identifier) ;

    public static implicit operator string ( ValidatedChannelName channelName )
    => channelName.ShortName_OmittingVAL ;

    public static bool ShowNameInFullWithVAL = true ;

    public override string ToString ( ) => (
      ShowNameInFullWithVAL
      ? FullName
      : ShortName_OmittingVAL 
    ) ;

    public ValidatedChannelName ( ChannelName name ) : base(name)
    { 
      if ( 
        Helpers.IsValidChannelName( 
          name, 
          out string? baseName,
          out string? fieldName,
          out var whatsWrong 
        ) 
      ) {
        BaseName  = baseName ;
        FieldName = fieldName ;
      }
      else
      {
        throw new UsageErrorException(
          $"Not a valid channel-identifier : '{Name}' ; {whatsWrong} "
        ) ;
      }
    }

    // Necessary because a ValidatedChannelName is-a ChannelName ...

    internal ValidatedChannelName ( 
      ChannelName name, 
      string      baseName, 
      string      fieldName 
    ) 
    : base(name)
    { 
      BaseName  = baseName ;
      FieldName = fieldName ;
    }

  }

}
