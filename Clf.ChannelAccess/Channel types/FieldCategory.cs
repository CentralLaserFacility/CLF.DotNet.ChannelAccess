//
// FieldCategory.cs
//

namespace Clf.ChannelAccess
{

  //
  // We're sometimes interested in knowing
  // the 'category' of a Field :
  //
  //   Is it a VAL field ?
  //     If so, is it an ENUM field ??
  //

  public record FieldCategory ( ) ;

  public record ValField ( ) : FieldCategory() ;

  public record EnumValField ( ) : ValField() ;

  public record OtherField ( ) : FieldCategory() ;

}

