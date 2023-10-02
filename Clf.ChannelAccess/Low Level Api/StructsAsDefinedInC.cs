//
// StructsAsDefinedInC.cs
//

using System ;
using System.Runtime.InteropServices ;

namespace Clf.ChannelAccess.LowLevelApi
{

  //
  // These are the struct definitions as set up in the 'original'
  // wrapper code from Daresbury. Repeated here just so that we can
  // validate that the offsets to the various fields are identical
  // in the equivalent 'struct' definitions in the ApiWrapper.
  //

  internal static class StructsAsDefinedInC
  {

    public const int MAX_UNITS_SIZE		    =  8;
    public const int MAX_ENUM_STRING_SIZE = 26 ;
    public const int MAX_ENUM_STATES		  = 16 ;
    public const int MAX_STRING_SIZE		  = 40 ;
    public const int MAX_NAME_SIZE		    = 36 ;
    public const int MAX_DESC_SIZE		    = 24 ;

    [StructLayout(LayoutKind.Sequential)]
    public struct connection_handler_args
    {
      public const int CA_OP_CONN_UP   = 6 ;
      public const int CA_OP_CONN_DOWN = 7 ;
      public IntPtr chid ;
      public Int32  op ; 
      public connection_handler_args ( IntPtr chid, Int32 op )
      {
        this.chid = chid ;
        this.op   = op ;
      }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct event_handler_args
    {
      public IntPtr usr ;
      public IntPtr chid ;
      public Int32  type ; 
      public Int32  count ;
      public IntPtr dbr ;    
      public Int32  status ;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct exception_handler_args
    {
      IntPtr usr    ; // User argument supplied when installed
      IntPtr chid   ; // Channel id (may be NULL)
      Int32  type   ; // Type requested
      Int32  count  ; // Count requested
      IntPtr addr   ; // User's address to write results of CA_OP_GET 
      Int32  stat   ; // Channel access ECA_XXXX status code
      Int32  op     ; // CA_OP_GET, CA_OP_PUT, ..., CA_OP_OTHER
      IntPtr ctx    ; // Character string containing context info
      IntPtr pFile  ; // Source file name (may be NULL)
      Int16  lineNo ; // Source file line number (may be zero)
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct epicsTimeStamp
    {
      public UInt32 secPastEpoch ;
      public UInt32 nsec ;
    }

    // DBR_TIME_

    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
    public struct dbr_time_char_i8
    {
      public Int16          status ;
      public Int16          severity ;
      public epicsTimeStamp stamp ;
      public Int16          RISC_pad0 ; 
      public Byte           RISC_pad1 ;
      public Byte           value ;
    } ;

    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
    public struct dbr_time_int_i16
    {
      public Int16          status ;
      public Int16          severity ;
      public epicsTimeStamp stamp ;
      public Int16          RISC_pad ;
      public Int16          value ;
    } ;

    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
    public struct dbr_time_long_i32
    {
      public System.Int16   status ;
      public System.Int16   severity ;
      public epicsTimeStamp stamp ;
      public Int32          value ;
    } ;

    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
    public struct dbr_time_float_f32
    {
      public Int16          status ;
      public Int16          severity ;
      public epicsTimeStamp stamp ;
      public float          value ;
    } ;

    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
    public struct dbr_time_double_f64
    {
      public Int16          status ;
      public Int16          severity ;
      public epicsTimeStamp stamp ;
      public Int16          RISC_pad ;
      public double         value ;
    } ;

    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
    public struct dbr_time_string
    {
      public Int16          status ;
      public Int16          severity ;
      public epicsTimeStamp stamp ;
      [MarshalAs(UnmanagedType.ByValTStr,SizeConst=MAX_STRING_SIZE)]
      public String         value ;
    } ;

    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
    public struct dbr_time_enum
    {
      public Int16          status ;
      public Int16          severity ;
      public epicsTimeStamp stamp ;
      public Int16          RISC_pad ;
      public UInt16         value ;
    } ;

    // DBR_CTRL_

    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
    public struct dbr_ctrl_char_i8
    {
      public Int16 status ;
      public Int16 severity ;
      [MarshalAs(UnmanagedType.ByValTStr,SizeConst=MAX_UNITS_SIZE)]
      public String units ;
      public Byte upper_disp_limit ;
      public Byte lower_disp_limit ;
      public Byte upper_alarm_limit ;
      public Byte upper_warning_limit ;
      public Byte lower_warning_limit ;
      public Byte lower_alarm_limit ;
      public Byte upper_ctrl_limit ;
      public Byte lower_ctrl_limit ;
      public Byte RISC_pad ;
      public Byte value ;
    } ;

    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
    public struct dbr_ctrl_int_i16
    {
      public Int16 status ;
      public Int16 severity ;
      [MarshalAs(UnmanagedType.ByValTStr,SizeConst=MAX_UNITS_SIZE)]
      public String units ;
      public Int16 upper_disp_limit ;
      public Int16 lower_disp_limit ;
      public Int16 upper_alarm_limit ;
      public Int16 upper_warning_limit ;
      public Int16 lower_warning_limit ;
      public Int16 lower_alarm_limit ;
      public Int16 upper_ctrl_limit ;
      public Int16 lower_ctrl_limit ;
      public Int16 value ;
    } ;

    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
    public struct dbr_ctrl_long_i32
    {
      public Int16 status ;
      public Int16 severity ;
      [MarshalAs(UnmanagedType.ByValTStr,SizeConst=MAX_UNITS_SIZE)]
      public String units ;
      public Int32  upper_disp_limit ;
      public Int32  lower_disp_limit ;
      public Int32  upper_alarm_limit ;
      public Int32  upper_warning_limit ;
      public Int32  lower_warning_limit ;
      public Int32  lower_alarm_limit ;
      public Int32  upper_ctrl_limit ;
      public Int32  lower_ctrl_limit ;
      public Int32  value ;
    } ;

    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
    public struct dbr_ctrl_float_f32
    {
      public Int16 status ;
      public Int16 severity ;
      public Int16 precision ;
      public Int16 RISC_pad ;
      [MarshalAs(UnmanagedType.ByValTStr,SizeConst=MAX_UNITS_SIZE)]
      public String units ;
      public float upper_disp_limit ;
      public float lower_disp_limit ;
      public float upper_alarm_limit ;
      public float upper_warning_limit ;
      public float lower_warning_limit ;
      public float lower_alarm_limit ;
      public float upper_ctrl_limit ;
      public float lower_ctrl_limit ;
      public float value ;
    } ;

    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
    public struct dbr_ctrl_double_f64
    {
      public Int16 status ;
      public Int16 severity ;
      public Int16 precision ;
      public Int16 RISC_pad0 ;
      [MarshalAs(UnmanagedType.ByValTStr,SizeConst=MAX_UNITS_SIZE)]
      public String units ;
      public double upper_disp_limit ;
      public double lower_disp_limit ;
      public double upper_alarm_limit ;
      public double upper_warning_limit ;
      public double lower_warning_limit ;
      public double lower_alarm_limit ;
      public double upper_ctrl_limit ;
      public double lower_ctrl_limit ;
      public double value ;
    } ;

    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
    public struct dbr_ctrl_string
    {
      public Int16 status ;
      public Int16 severity ;
      [MarshalAs(UnmanagedType.ByValTStr,SizeConst=MAX_STRING_SIZE)]
      public String value ;
    } ;

    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
    public struct dbr_ctrl_enum
    {
      public Int16 status ;
      public Int16 severity ;
      public Int16 no_str ; // Number of 'state strings'
      [MarshalAs(UnmanagedType.ByValArray,SizeConst=MAX_ENUM_STATES*MAX_ENUM_STRING_SIZE)]
      public Char[] strs ;  // 16 'state strings' all of length 26 incl terminating null
      public UInt16 value ; // Current value (not necessarily 0..no_str-1)
    } ;

  }

}
