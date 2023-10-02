//
// Structs.dbr_ctrl.cs
//

using System.Runtime.InteropServices ;

namespace Clf.ChannelAccess.LowLevelApi
{

  //
  // This 'dbr_ctrl_enum' structure provides the ONLY mechanism for
  // retrieving the 'state strings' pertaining to the ENUM.
  //

  [StructLayout(LayoutKind.Sequential)]
  internal readonly struct dbr_ctrl_enum
  {
    public static unsafe int HowManyBytesRequiredForElementsCountOf ( int nElements )
    {
      return (
        sizeof(dbr_ctrl_enum) 
      + sizeof(short) * ( nElements - 1 )
      ) ;
    }
    public readonly short           status ;
    public readonly short           severity ;
    public readonly short           nStrings ; 
    public readonly ByteArray_26x16 stringBytes_26x16 ;
    public readonly short           enumValue ; // Current value (not necessarily in the range 0..nStrings-1)
  } ;

  //
  // DBR_ values with Status, Graphic and Control Structures
  //

  //
  // Accessing these structs via a pointer works fine, provided we've
  // defined the struct in a way the lets the compiler see it as 'unmanaged'.
  // That means, not having any 'fields' in the struct that are reference types
  // such as 'string' or 'array'.
  //

  //
  // This 'dbr_ctrl_string' has the suffix '40' to indicate that
  // it holds a string up up to 40 characters - but that count includes
  // a terminating null. When we use the API's from C#, the maximum length
  // of a string is 39 characters, hence our use of 's39' in other places.
  //

  [StructLayout(LayoutKind.Sequential)]
  internal readonly struct dbr_ctrl_string_s40
  {
    public static unsafe int HowManyBytesRequiredForElementsCountOf ( int nElements )
    {
      return (
        sizeof(dbr_ctrl_string_s40) 
      + sizeof(ByteArray_40) * ( nElements - 1 )
      ) ;
    }
    public readonly DBR_StatusAndSeverity statusAndSeverity ;
    public readonly ByteArray_40          value ;
  }

  [StructLayout(LayoutKind.Sequential)]
  internal readonly struct dbr_ctrl_byte_i8
  {
    public static unsafe int HowManyBytesRequiredForElementsCountOf ( int nElements )
    {
      return (
        sizeof(dbr_ctrl_byte_i8) 
      + sizeof(byte) * ( nElements - 1 )
      ) ;
    }
    public readonly DBR_StatusAndSeverity statusAndSeverity ;
    public readonly ByteArray_8           units ;
    public readonly DBR_ControlInfo<byte> ctrlInfo ;
    public readonly byte                  RISC_pad ;
    public readonly byte                  value ;
  }

  [StructLayout(LayoutKind.Sequential)]
  internal readonly struct dbr_ctrl_int_i16
  {
    public static unsafe int HowManyBytesRequiredForElementsCountOf ( int nElements )
    {
      return (
        sizeof(dbr_ctrl_int_i16) 
      + sizeof(short) * ( nElements - 1 )
      ) ;
    }
    public readonly DBR_StatusAndSeverity  statusAndSeverity ;
    public readonly ByteArray_8            units ;
    public readonly DBR_ControlInfo<short> ctrlInfo ;
    public readonly short                  value ;
  }

  [StructLayout(LayoutKind.Sequential)]
  internal readonly struct dbr_ctrl_long_i32
  {
    public static unsafe int HowManyBytesRequiredForElementsCountOf ( int nElements )
    {
      return (
        sizeof(dbr_ctrl_long_i32) 
      + sizeof(int) * ( nElements - 1 )
      ) ;
    }
    public readonly DBR_StatusAndSeverity statusAndSeverity ;
    public readonly ByteArray_8           units ;
    public readonly DBR_ControlInfo<int>  ctrlInfo ;
    public readonly int                   value ;
  } ;

  [StructLayout(LayoutKind.Sequential)]
  internal readonly struct dbr_ctrl_float_f32
  {
    public static unsafe int HowManyBytesRequiredForElementsCountOf ( int nElements )
    {
      return (
        sizeof(dbr_ctrl_float_f32) 
      + sizeof(float) * ( nElements - 1 )
      ) ;
    }
    public readonly DBR_StatusAndSeverity  statusAndSeverity ;
    public readonly DBR_Precision          precision ;
    public readonly ByteArray_8            units ;
    public readonly DBR_ControlInfo<float> ctrlInfo ;
    public readonly float                  value ;
  } ;

  [StructLayout(LayoutKind.Sequential)]
  internal readonly struct dbr_ctrl_double_f64
  {
    public static unsafe int HowManyBytesRequiredForElementsCountOf ( int nElements )
    {
      return (
        sizeof(dbr_ctrl_double_f64) 
      + sizeof(double) * ( nElements - 1 )
      ) ;
    }
    public readonly DBR_StatusAndSeverity   statusAndSeverity ;
    public readonly DBR_Precision           precision ;
    public readonly ByteArray_8             units ;
    public readonly DBR_ControlInfo<double> ctrlInfo ;
    public readonly double                  value ;
  } ;

}
