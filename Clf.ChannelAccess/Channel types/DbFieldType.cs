//
// DbFieldType.cs
//

namespace Clf.ChannelAccess
{

  //
  // These are the 'primitive types' that we deal with.
  //
  // A PV 'field' can have a value that's a single instance, or an array.
  //
  // It's only the VAL field that can return an array !!
  // Other fields can only return a scalar value,
  // ie an array with a single element. 
  //

  /// <summary>
  /// Type of a PV's value : string, short, float, double, enum etc.
  /// </summary>

  public enum DbFieldType : short {
    DBF_STRING_s39 = LowLevelApi.ApiConstants.DBF_STRING, // 0 ; Up to 39 characters
    DBF_SHORT_i16  = LowLevelApi.ApiConstants.DBF_SHORT,  // 1 ; 16 bit integer
    DBF_FLOAT_f32  = LowLevelApi.ApiConstants.DBF_FLOAT,  // 2 ; Floating point single precision
    DBF_ENUM_i16   = LowLevelApi.ApiConstants.DBF_ENUM,   // 3 ; 16 bit integer, 0..15 max
    DBF_CHAR_byte  = LowLevelApi.ApiConstants.DBF_CHAR,   // 4 ; Char == Byte
    DBF_LONG_i32   = LowLevelApi.ApiConstants.DBF_LONG,   // 5 ; Long == 32 bit integer
    DBF_DOUBLE_f64 = LowLevelApi.ApiConstants.DBF_DOUBLE  // 6 ; Floating point double precision 
  }

  public static partial class Helpers
  {

    // MOVED TO InternalHelpers ...
    // public static System.ApplicationException AsUnexpectedValueException ( 
    //   this DbFieldType fieldType 
    // ) => new System.ApplicationException(
    //   $"Unexpected field-type {fieldType}"
    // ) ;

    // It's useful to have 'methods' associated with the DbFieldType,
    // but maybe we should consider defining it as a record
    // rather than as an enum ??

    public static unsafe int ElementSizeInBytes ( 
      this DbFieldType fieldType 
    ) => fieldType switch {
      DbFieldType.DBF_STRING_s39 => sizeof(LowLevelApi.ByteArray_40),
      DbFieldType.DBF_SHORT_i16  => sizeof(short),
      DbFieldType.DBF_FLOAT_f32  => sizeof(float),
      DbFieldType.DBF_ENUM_i16   => sizeof(short),
      DbFieldType.DBF_CHAR_byte  => sizeof(byte),
      DbFieldType.DBF_LONG_i32   => sizeof(int),
      DbFieldType.DBF_DOUBLE_f64 => sizeof(double),
      _                          => throw fieldType.AsUnexpectedEnumValueException()
    } ;

  }

}
