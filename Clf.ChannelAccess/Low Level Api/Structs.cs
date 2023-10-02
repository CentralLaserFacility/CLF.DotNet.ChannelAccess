//
// .Structs.cs
//

using System ;
using System.Runtime.InteropServices ;

namespace Clf.ChannelAccess.LowLevelApi
{

  //
  // These must match the 'C' definitions in cadef.h
  //

  //
  // Note that you must use 'StructLayout' for all structs that you use with interop
  // otherwise the layout of the struct in memory is entirely up to the runtime, and
  // it could do such things as reorder the fields in order to pack them better, etc.
  //
  // Hmm, some docs imply that 'Sequential' is the default for structs,
  // but it's better to be on the safe side and mark them explicitly.
  //

  [StructLayout(LayoutKind.Sequential)]
  [LowLevelApi.ApiWrapper.OriginalStructType(typeof(StructsAsDefinedInC.connection_handler_args))]
  internal readonly struct ConnectionStatusChangedEventArgs 
  {
    public const int CA_OP_CONN_UP   = 6 ;
    public const int CA_OP_CONN_DOWN = 7 ;
    [LowLevelApi.ApiWrapper.OriginalName("chid")] public readonly IntPtr pChannel ;
    [LowLevelApi.ApiWrapper.OriginalName("op")]   public readonly Int32  connectionState ; // Either 'UP' or 'DOWN' ... 
  }

  //
  // Maybe rework these field declarations to specify the logical types,
  // eg DbFieldType, rather than the physical types. Then we can define
  // a Validate method that checks the validity of the structure,
  // and have it in one place (ie here) rather than duplicated in the
  // various places where this structure is used.
  //
  // Alternatively, define 'getter' properties reflecting the logical types ?
  //

  [StructLayout(LayoutKind.Sequential)]
  [LowLevelApi.ApiWrapper.OriginalStructType(typeof(StructsAsDefinedInC.event_handler_args))]
  internal struct ValueUpdateNotificationEventArgs 
  {
    [LowLevelApi.ApiWrapper.OriginalName( "usr"    )] public nint   tagValue  ; // User argument supplied with the request
    [LowLevelApi.ApiWrapper.OriginalName( "chid"   )] public IntPtr pChannel  ; // Channel ID to which this event pertains
    [LowLevelApi.ApiWrapper.OriginalName( "type"   )] public Int32  dbrType   ; // 'DBR_' type of the value returned ; or -1 !!!
    [LowLevelApi.ApiWrapper.OriginalName( "count"  )] public Int32  nElements ; // Element count returned ... may be zero ???
    [LowLevelApi.ApiWrapper.OriginalName( "dbr"    )] public IntPtr pDbr      ; // Pointer to DBR_ struct returned ; null if status is not ECA_NORMAL
    [LowLevelApi.ApiWrapper.OriginalName( "status" )] public Int32  ecaStatus ; // ECA_XXX status
  }

  [StructLayout(LayoutKind.Sequential)]
  [LowLevelApi.ApiWrapper.OriginalStructType(typeof(StructsAsDefinedInC.exception_handler_args))]
  internal readonly struct ExceptionHandlerEventArgs 
  {
    public readonly IntPtr usr    ; // User argument supplied when installed
    public readonly IntPtr chid   ; // Channel id (may be NULL)
    public readonly Int32  type   ; // Type requested
    public readonly Int32  count  ; // Count requested
    public readonly IntPtr addr   ; // User's address to write results of CA_OP_GET 
    public readonly Int32  stat   ; // Channel access ECA_XXXX status code
    public readonly Int32  op     ; // CA_OP_GET, CA_OP_PUT, ..., CA_OP_OTHER
    public readonly IntPtr ctx    ; // Character string containing context info
    public readonly IntPtr pFile  ; // Source file name (may be NULL)
    public readonly Int16  lineNo ; // Source file line number (may be zero)
    public string? Message => Marshal.PtrToStringAnsi(ctx) ;
    public string RequestInfo => $"Requested {count} elements of type {type}" ;
  }

  [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
  internal struct dbr_class_name // Not yet tried ...
  {
    // Hmm, might be better to just declare the first byte
    // and assume that a further 39 bytes follow that ?
    [MarshalAs(UnmanagedType.ByValTStr,SizeConst=ApiConstants.MAX_STRING_SIZE)]
    public String value ;
  } ;

  [StructLayout(LayoutKind.Sequential)]
  [LowLevelApi.ApiWrapper.OriginalStructType(typeof(StructsAsDefinedInC.epicsTimeStamp))]
  internal struct EpicsTimeStamp
  {
    [LowLevelApi.ApiWrapper.OriginalName( "secPastEpoch" )] public UInt32 secPastEpoch ; // Seconds since 00:00 Jan 1st,1990
    [LowLevelApi.ApiWrapper.OriginalName( "nsec"         )] public UInt32 nsec ;         // NanoSeconds within that second
  }

  // All 'dbr_xxx' structures start with these fields.
  // Even though struct definitions don't support Inheritance,
  // it's useful to declare this type as we can then define 
  // a pointer to a header, and subsequently cast it
  // to a 'derived' type.

  [StructLayout(LayoutKind.Sequential)]
  internal struct DBR_StatusAndSeverity
  {
    public Int16 status ;
    public Int16 severity ;
  }

  [StructLayout(LayoutKind.Sequential)]
  internal readonly struct DBR_Precision
  {
    public readonly Int16 precision ; 
    public readonly Int16 RISC_pad ;
  }

  [StructLayout(LayoutKind.Sequential)]
  internal readonly struct DBR_NumericValue<TValue>
  where TValue : unmanaged
  {
    public readonly TValue value ;
    public unsafe void GetValueElements ( 
      int          nElements,
      ref TValue[] destinationArray
    ) {
      destinationArray ??= new TValue[nElements] ;
      fixed ( 
        TValue * pFirstPayloadElement = &value,
        pFirstDestinationArrayElement = destinationArray
      ) {
        long nBytesToCopy = nElements * sizeof(TValue) ;
        System.Buffer.MemoryCopy(
          source                 : pFirstPayloadElement,
          destination            : pFirstDestinationArrayElement,
          destinationSizeInBytes : nBytesToCopy,
          sourceBytesToCopy      : nBytesToCopy
        ) ;
      }
    }
  } ;

  [StructLayout(LayoutKind.Sequential)]
  internal readonly struct DBR_ControlInfo<TValue>
  where TValue : unmanaged
  {
    public readonly TValue upper_disp_limit_HOPR ;
    public readonly TValue lower_disp_limit_LOPR ;
    public readonly TValue upper_alarm_limit_HIHI ;
    public readonly TValue upper_warning_limit_HIGH ;
    public readonly TValue lower_warning_limit_LOW ;
    public readonly TValue lower_alarm_limit_LOLO ;
    public readonly TValue upper_ctrl_limit_DRVH ;
    public readonly TValue lower_ctrl_limit_DRVL ;
  } ;

  [StructLayout(LayoutKind.Sequential)]
  internal unsafe readonly struct ByteArray 
  {
    private readonly byte FirstByte ;
    public unsafe string AsString ( int nBytesPotentiallyAvailable )
    {
      fixed ( byte * pFirstByte = &FirstByte )
      {
        return System.Text.Encoding.ASCII.GetString(
          pFirstByte,
          nBytesPotentiallyAvailable
        ).TrimEnd('\0') ;
      }
    }
  }

  [StructLayout(LayoutKind.Sequential,Size=8)]
  internal readonly struct ByteArray_8
  {
    // This works fine, but it's probably better
    // to convert the data to a string using
    // the 'Unsafe.GetBytesAsString' functions, where
    // we can use sizeof<T> ... ???
    public readonly byte FirstByte ;
    public unsafe string AsString ( )
    {
      fixed ( 
        byte * pFirstByte = &FirstByte 
      ) {
        return System.Text.Encoding.ASCII.GetString(
          pFirstByte,
          8
        ).TrimEnd('\0') ;
      }
    }
  }

  [StructLayout(LayoutKind.Sequential,Size=26)]
  internal readonly struct ByteArray_26 
  {
    public readonly byte FirstByte ;
  }

  //
  // Representation of the string values associated with an enum.
  // 16 values of 26 characters each, including the terminating nulls.
  //

  [StructLayout(LayoutKind.Sequential,Size=26*16)]
  internal readonly struct ByteArray_26x16 
  {
    public readonly byte FirstByte ;
    public unsafe string[] GetStringValues ( int nStrings = 16 )
    {
      string[] strings = new string[nStrings] ;
      fixed ( byte * pFirstByte = &FirstByte )
      {
        for ( int iString = 0 ; iString < nStrings ; iString++ )
        {
          strings[iString] = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(
            new System.IntPtr(
              pFirstByte
            + iString * 26
            )
          )! ; 
        }
      }
      return strings ;
    }
  }

  [StructLayout(LayoutKind.Sequential,Size=40)]
  internal readonly struct ByteArray_40 
  {
    public readonly byte FirstByte ;
    public unsafe ByteArray_40 ( string s )
    {
      // The struct has been declared as having a size of 40 bytes,
      // so we can safely write 40 bytes into memory starting at
      // the address of 'FirstByte'.
      FirstByte = 0 ;
      // Get an array of up to 40 bytes representing the string
      byte[] bytesRepresentingString = System.Text.Encoding.ASCII.GetBytes(
        s.Length > 39
        ? s.Substring(0,39)
        : s
      ) ;
      // Allocate an array of exactly 40 bytes, whose values will be
      // initialised to 0. Copy across however many bytes 
      // are necessary to represent the string, leaving the trailing bytes
      // set to zero.
      byte[] bytesToPopulateStruct = new byte[40] ;
      for ( int i = 0 ; i < bytesRepresentingString.Length ; i++ )
      {
        bytesToPopulateStruct[i] = bytesRepresentingString[i] ;
      }
      // Copy the bytes we're prepared, into this 'ByteArray_40' structure.
      fixed ( 
        byte * 
        pFirstSourceByte      = bytesToPopulateStruct,
        pFirstDestinationByte = &FirstByte
      ) {
        System.Buffer.MemoryCopy(
          source                 : pFirstSourceByte,
          destination            : pFirstDestinationByte,
          destinationSizeInBytes : 40,
          sourceBytesToCopy      : 40
        ) ;
      }
    }
    public unsafe string AsString ( )
    {
      fixed ( byte * pFirstByte = &FirstByte )
      {
        return Marshal.PtrToStringAnsi(
          (IntPtr) pFirstByte
        )! ;
      }
    }
  }

  [StructLayout(LayoutKind.Sequential,Size=1)]
  internal readonly struct ByteArray_40xN 
  {
    public readonly byte FirstByte ;
  }

}
