//
// Structs.dbr_time.cs
//

using System ;
using System.Runtime.InteropServices ;

namespace Clf.ChannelAccess.LowLevelApi
{

  // [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
  // internal struct dbr_time
  // {
  //   public Int16          status ;
  //   public Int16          severity ;
  //   public EpicsTimeStamp stamp ;
  // }

  [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
  internal readonly struct dbr_time_enum
  {
    public static unsafe int HowManyBytesRequiredForElementsCountOf ( int nElements )
    {
      return (
        sizeof(dbr_time_enum) 
      + sizeof(short) * ( nElements - 1 )
      ) ;
    }
    public System.DateTime? TimeStampFromServer => InternalHelpers.ConvertEpicsTimeStamp(stamp) ;
    // ----------------------------------------
    public readonly short          status ;
    public readonly short          severity ;
    public readonly EpicsTimeStamp stamp ;
    public readonly short          RISC_pad ;
    public readonly short          value ;
  } ;

  [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
  internal readonly struct dbr_time_string_s40
  {
    public static unsafe int HowManyBytesRequiredForElementsCountOf ( int nElements )
    {
      return (
        sizeof(dbr_time_string_s40) 
      + sizeof(ByteArray_40) * ( nElements - 1 )
      ) ;
    }
    public System.DateTime? TimeStampFromServer => InternalHelpers.ConvertEpicsTimeStamp(stamp) ;
    // ------------------------------------------------------------
    public readonly DBR_StatusAndSeverity statusAndSeverity ;
    public readonly EpicsTimeStamp        stamp ;
    public readonly ByteArray_40          value ;
  } ;

  [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
  internal readonly struct dbr_time_byte_i8
  {
    public static unsafe int HowManyBytesRequiredForElementsCountOf ( int nElements )
    {
      return (
        sizeof(dbr_time_byte_i8) 
      + sizeof(byte) * ( nElements - 1 )
      ) ;
    }
    public System.DateTime? TimeStampFromServer => InternalHelpers.ConvertEpicsTimeStamp(stamp) ;
    // ------------------------------------------------------------
    public readonly DBR_StatusAndSeverity statusAndSeverity ;
    public readonly EpicsTimeStamp        stamp ;
    public readonly short                 RISC_pad0 ; // Does this assume x86 endian ??
    public readonly byte                  RISC_pad1 ;
    public readonly byte                  value ;
  } ;

  [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
  internal readonly struct dbr_time_int_i16
  {
    public static unsafe int HowManyBytesRequiredForElementsCountOf ( int nElements )
    {
      return (
        sizeof(dbr_time_int_i16) 
      + sizeof(short) * ( nElements - 1 )
      ) ;
    }
    public System.DateTime? TimeStampFromServer => InternalHelpers.ConvertEpicsTimeStamp(stamp) ;
    // ------------------------------------------------------------
    public readonly DBR_StatusAndSeverity statusAndSeverity ;
    public readonly EpicsTimeStamp        stamp ;
    public readonly short                 RISC_pad ; // RISC alignment
    public readonly short                 value ;
  } ;

  [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
  internal readonly struct dbr_time_long_i32
  {
    public static unsafe int HowManyBytesRequiredForElementsCountOf ( int nElements )
    {
      return (
        sizeof(dbr_time_long_i32) 
      + sizeof(int) * ( nElements - 1 )
      ) ;
    }
    public System.DateTime? TimeStampFromServer => InternalHelpers.ConvertEpicsTimeStamp(stamp) ;
    // ------------------------------------------------------------
    public readonly DBR_StatusAndSeverity statusAndSeverity ;
    public readonly EpicsTimeStamp        stamp ;
    public readonly int                   value ;
  } ;

  [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
  internal readonly struct dbr_time_float_f32
  {
    public static unsafe int HowManyBytesRequiredForElementsCountOf ( int nElements )
    {
      return (
        sizeof(dbr_time_float_f32) 
      + sizeof(float) * ( nElements - 1 )
      ) ;
    }
    public System.DateTime? TimeStampFromServer => InternalHelpers.ConvertEpicsTimeStamp(stamp) ;
    // ------------------------------------------------------------
    public readonly DBR_StatusAndSeverity statusAndSeverity ;
    public readonly EpicsTimeStamp        stamp ;
    public readonly float                 value ;
  } ;

  [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
  internal readonly struct dbr_time_double_f64
  {
    public static unsafe int HowManyBytesRequiredForElementsCountOf ( int nElements )
    {
      return (
        sizeof(dbr_time_double_f64) 
      + sizeof(double) * ( nElements - 1 )
      ) ;
    }
    public System.DateTime? TimeStampFromServer => InternalHelpers.ConvertEpicsTimeStamp(stamp) ;
    // ------------------------------------------------------------
    public readonly DBR_StatusAndSeverity statusAndSeverity ;
    public readonly EpicsTimeStamp        stamp ;
    public readonly short                 RISC_pad ;
    public readonly double                value ;
  } ;

}
