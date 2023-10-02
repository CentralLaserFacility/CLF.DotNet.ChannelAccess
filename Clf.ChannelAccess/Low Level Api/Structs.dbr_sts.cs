//
// Structs.dbr_sts.cs
//

using System ;
using System.Runtime.InteropServices ;

namespace Clf.ChannelAccess.LowLevelApi
{

  //
  // These must match the 'C' definitions in cadef.h
  //

  //
  // DBR_ values with Status structures.
  // Just in case we want to access the Status and Severity
  // at the same time as the Value (maybe useful for the VAL field).
  //

  [StructLayout(LayoutKind.Sequential)]
  internal readonly struct dbr_sts
  {
    public readonly Int16 status ;
    public readonly Int16 severity ;
  }

  [StructLayout(LayoutKind.Sequential)]
  internal readonly struct dbr_sts_char_i8
  {
    public readonly Int16 status ;
    public readonly Int16 severity ;
    public readonly Int16 RISC_pad0 ; // Does this assume x86 endian ??
    public readonly Byte  RISC_pad1 ;
    public readonly Byte  value ;
  } ;

  [StructLayout(LayoutKind.Sequential)]
  internal readonly struct dbr_sts_int_i16
  {
    public readonly Int16 status ;
    public readonly Int16 severity ;
    public readonly Int16 RISC_pad ; 
    public readonly Int16 value ;
  } ;

  [StructLayout(LayoutKind.Sequential)]
  internal readonly struct dbr_sts_long_i32
  {
    public readonly System.Int16 status ;
    public readonly System.Int16 severity ;
    public readonly Int32        value ;
  } ;

  [StructLayout(LayoutKind.Sequential)]
  internal readonly struct dbr_sts_float_f32
  {
    public readonly Int16 status ;
    public readonly Int16 severity ;
    public readonly float value ;
  } ;

  [StructLayout(LayoutKind.Sequential)]
  internal readonly struct dbr_sts_double_f64
  {
    public readonly Int16  status ;
    public readonly Int16  severity ;
    public readonly Int16  RISC_pad ;
    public readonly double value ;
  } ;

  [StructLayout(LayoutKind.Sequential)]
  internal readonly struct dbr_sts_string
  {
    public readonly Int16          status ;
    public readonly Int16          severity ;
    public readonly ByteArray_40xN value ;
  } ;

  [StructLayout(LayoutKind.Sequential)]
  internal readonly struct dbr_sts_enum
  {
    public readonly Int16 status ;
    public readonly Int16 severity ;
    public readonly Int16 RISC_pad ;
    public readonly Int16 value ;
  } ;

}
