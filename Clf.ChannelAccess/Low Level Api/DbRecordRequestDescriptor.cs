//
// DbRecordRequestDescriptor.cs
//

namespace Clf.ChannelAccess.LowLevelApi ;
using Clf.ChannelAccess.LowLevelApi.ExtensionMethods ;

/// <summary>
/// Encapsulates everything we need to know in order to 
/// (A) make a synchronous request for a PV value, 
/// or (B) to set up a Subscription that will deliver values asynchronously.
/// <br></br>
/// Namely : the DBR type (eg DBR_CTRL_SHORT), 
/// </summary>

internal class DbRecordRequestDescriptor 
{

  // What kind of DBR_XXX request we'll be making.

  public DbRecordRequestType DbrType { get ; }

  // Type of the 'value' we'll acquire eg string, double, int etc.

  public DbFieldType DbFieldType { get ; }

  // The ValueAccessMode determines what kind of DBR_ request we'll make,
  // according to whether we want Auxiliary info, the Timestamp, etc.

  public ValueAccessMode ValueAccessMode { get ; }

  // If the 'DbRecordRequestType' pertains to a structure that
  // provides 'CTRL' information, we'll fetch it from the record.
  // THESE FIELDS NO LONGER NECESSARY AS WE ALWAYS 'switch' on the ValueAccessMode property ...
  // public bool ShouldFetchAuxiliaryInfo_CTRL => ValueAccessMode is ValueAccessMode.RequestValueAndAuxiliaryInfo ;
  // public bool ShouldFetchTimeStamp_TIME     => ValueAccessMode is ValueAccessMode.RequestValueAndAuxiliaryInfo ;

  public DbrRequestedInfoCategory DbrRequestedInfoCategory => DbrType.GetDbrRequestedInfoCategory() ;

  public DbRecordRequestDescriptor ( DbRecordRequestType dbrType )
  {
    DbrType = dbrType ;
    (
      this.DbFieldType,
      this.ValueAccessMode
    ) = dbrType.GetDbFieldTypeAndValueAccessMode() ;
  }

  public DbRecordRequestDescriptor ( DbFieldType dbFieldType, ValueAccessMode valueAccessMode )
  {
    DbFieldType = dbFieldType ;
    // By default, when we fetch the value of a '.VAL' field we also request
    // the 'auxiliary' information - namely the Alarm Status, the information about
    // upper and lower limits, the engineering units and so on.
    // Note that this information is not available for 'other' fields,
    // ie non-VAL fields.
    ValueAccessMode = valueAccessMode ;
    DbrType = (DbRecordRequestType) (
      ValueAccessMode switch {
        ValueAccessMode.DBR_CTRL_RequestValueAndAuxiliaryInfo   => (int) dbFieldType + ApiConstants.DBR_CTRL_offset_28,
        ValueAccessMode.DBR_TIME_RequestValueAndServerTimeStamp => (int) dbFieldType + ApiConstants.DBR_TIME_offset_14,
        ValueAccessMode.DBR_RequestValueAndNothingElse     => (int) dbFieldType + ApiConstants.DBR_offset_zero,
        _                                              => throw ValueAccessMode.AsUnexpectedEnumValueException()
      }
    ) ;
    // DbrType = (DbRecordRequestType) (
    //   (int) dbFieldType
    // + (
    //   ShouldFetchAuxiliaryInfo_CTRL
    //   ? 28 // MAGIC NUMBER ...
    //   : 0
    //   )
    // ) ;
  }

  // When we perform a synchronous 'read', it's necessary to allocate a
  // suitably sized buffer to hold the dbRecord structure that will be
  // populated as a result of the API call. The size of the buffer
  // depends on (A) the type of the struct we'll be receiving,
  // and on (B) the number of array elements that will be returned.
  // The first element gets written into the struct's 'value' field,
  // any other elements are written into subsequent memory locations.

  public int HowManyDbRecordBytesRequiredForArraySize ( int nValueArrayElements )
  => (
    this.ValueAccessMode switch {
      ValueAccessMode.DBR_CTRL_RequestValueAndAuxiliaryInfo => DbFieldType switch {
        DbFieldType.DBF_STRING_s39 => dbr_ctrl_string_s40 .HowManyBytesRequiredForElementsCountOf(nValueArrayElements),
        DbFieldType.DBF_SHORT_i16  => dbr_ctrl_int_i16    .HowManyBytesRequiredForElementsCountOf(nValueArrayElements),
        DbFieldType.DBF_FLOAT_f32  => dbr_ctrl_float_f32  .HowManyBytesRequiredForElementsCountOf(nValueArrayElements),
        DbFieldType.DBF_ENUM_i16   => dbr_ctrl_enum       .HowManyBytesRequiredForElementsCountOf(nValueArrayElements),
        DbFieldType.DBF_CHAR_byte  => dbr_ctrl_byte_i8    .HowManyBytesRequiredForElementsCountOf(nValueArrayElements),
        DbFieldType.DBF_LONG_i32   => dbr_ctrl_long_i32   .HowManyBytesRequiredForElementsCountOf(nValueArrayElements),
        DbFieldType.DBF_DOUBLE_f64 => dbr_ctrl_double_f64 .HowManyBytesRequiredForElementsCountOf(nValueArrayElements),
        _                          => throw DbFieldType.AsUnexpectedEnumValueException()
      },
      ValueAccessMode.DBR_TIME_RequestValueAndServerTimeStamp => DbFieldType switch {
        DbFieldType.DBF_STRING_s39 => dbr_time_string_s40 .HowManyBytesRequiredForElementsCountOf(nValueArrayElements),
        DbFieldType.DBF_SHORT_i16  => dbr_time_int_i16    .HowManyBytesRequiredForElementsCountOf(nValueArrayElements),
        DbFieldType.DBF_FLOAT_f32  => dbr_time_float_f32  .HowManyBytesRequiredForElementsCountOf(nValueArrayElements),
        DbFieldType.DBF_ENUM_i16   => dbr_time_enum       .HowManyBytesRequiredForElementsCountOf(nValueArrayElements),
        DbFieldType.DBF_CHAR_byte  => dbr_time_byte_i8    .HowManyBytesRequiredForElementsCountOf(nValueArrayElements),
        DbFieldType.DBF_LONG_i32   => dbr_time_long_i32   .HowManyBytesRequiredForElementsCountOf(nValueArrayElements),
        DbFieldType.DBF_DOUBLE_f64 => dbr_time_double_f64 .HowManyBytesRequiredForElementsCountOf(nValueArrayElements),
        _                          => throw DbFieldType.AsUnexpectedEnumValueException()
      },
      ValueAccessMode.DBR_RequestValueAndNothingElse => DbFieldType.ElementSizeInBytes() * nValueArrayElements,
      _ => throw ValueAccessMode.DBR_CTRL_RequestValueAndAuxiliaryInfo.AsUnexpectedEnumValueException()
    }
  ) ;

  // public int HowManyDbRecordBytesRequiredForArraySize_OLD ( int nValueArrayElements )
  // {
  //   return (
  //     ShouldFetchAuxiliaryInfo_CTRL
  //     ? DbFieldType switch {
  //     DbFieldType.DBF_STRING_s39 => dbr_ctrl_string_s40 .HowManyBytesRequiredForElementsCountOf(nValueArrayElements),
  //     DbFieldType.DBF_SHORT_i16  => dbr_ctrl_int_i16    .HowManyBytesRequiredForElementsCountOf(nValueArrayElements),
  //     DbFieldType.DBF_FLOAT_f32  => dbr_ctrl_float_f32  .HowManyBytesRequiredForElementsCountOf(nValueArrayElements),
  //     DbFieldType.DBF_ENUM_i16   => dbr_ctrl_enum       .HowManyBytesRequiredForElementsCountOf(nValueArrayElements),
  //     DbFieldType.DBF_CHAR_byte  => dbr_ctrl_byte_i8    .HowManyBytesRequiredForElementsCountOf(nValueArrayElements),
  //     DbFieldType.DBF_LONG_i32   => dbr_ctrl_long_i32   .HowManyBytesRequiredForElementsCountOf(nValueArrayElements),
  //     DbFieldType.DBF_DOUBLE_f64 => dbr_ctrl_double_f64 .HowManyBytesRequiredForElementsCountOf(nValueArrayElements),
  //     _                          => throw DbFieldType.AsUnexpectedValueException()
  //     }
  //     : DbFieldType.ElementSizeInBytes() * nValueArrayElements
  //   ) ;
  // }

}
