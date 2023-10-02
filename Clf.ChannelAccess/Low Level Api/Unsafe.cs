//
// Unsafe.cs
//

using System.Collections.Generic ;
using System.Diagnostics.CodeAnalysis ;
using System.Linq ;
using FluentAssertions ;
using Clf.Common.ExtensionMethods ;

namespace Clf.ChannelAccess.LowLevelApi
{

  internal static class Unsafe
  {

    public static unsafe string GetBytesAsString ( 
      void * pFirstByte, 
      int    nBytesAvailable 
    ) {
      return GetBytesAsString(
        (byte*) pFirstByte,
        nBytesAvailable
      ) ;
    }

    public static unsafe string GetBytesAsString ( 
      byte * pFirstByte, 
      int    nBytesAvailable 
    ) {
      return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(
        new System.IntPtr(pFirstByte)
      )! ; 
    }

    public static unsafe string GetNullTerminatedBytesAsString ( System.IntPtr pFirstByte )
    {
      // System.Runtime.InteropServices.Marshal.PtrToStringAnsi ???
      // Hmm, should tidy this up and make it less inefficient ...
      List<byte> bytes = new List<byte>(64) ;
      byte * pFirstByteNotYetRead = (byte*) pFirstByte ;
      while ( *pFirstByteNotYetRead != 0 )
      {
        bytes.Add(
          *pFirstByteNotYetRead++
        ) ;
      }
      return System.Text.Encoding.ASCII.GetString(
        bytes.ToArray()
      ) ;
    }

    //
    // This variant gets the i'th string
    // from an array of fixed-length strings
    // encoded in an array of bytes
    //
    //   +---------+---------+---------+---------+---
    //   | slot #0 | slot #1 | slot #2 | slot #3 | 
    //   +---------+---------+---------+---------+---
    //
    // Such as :
    //   for a 'units' string ( 1 x 8 )
    //   for 'enum' strings ( 16 x 26 )
    //   for values holding one or more strings ( N x 40 )
    //

    public static unsafe string GetBytesAsString ( 
      System.IntPtr pBufferBase, 
      int           nBytesAvailablePerString, 
      int           iSlotNumber,
      int           nSlotsExpectedToBeValid
    ) {
      iSlotNumber.Should().BeGreaterOrEqualTo(0) ;
      iSlotNumber.Should().BeLessThan(nSlotsExpectedToBeValid) ;
      return System.Text.Encoding.ASCII.GetString(
        (byte*) (
          pBufferBase 
        + nBytesAvailablePerString * iSlotNumber
        ),
        nBytesAvailablePerString
      ).TrimEnd('\0') ;
    }

    public static unsafe string GetBytesAsString<TByteStruct> ( 
      ref TByteStruct byteStruct, 
      int             nBytesAvailablePerString, 
      int             iSlotNumber,
      int             nSlotsExpectedToBeValid
    ) 
    where TByteStruct : unmanaged
    {
      // Hmm, could do some validation here according
      // to the type of the struct, or an Attribute
      // that we could query using reflection.
      // But actually the worst that can happen is that we
      // create a string containing non-valid characters.
      iSlotNumber.Should().BeGreaterOrEqualTo(0) ;
      iSlotNumber.Should().BeLessThan(nSlotsExpectedToBeValid) ;
      fixed ( void * pFirstByte = &byteStruct ) 
      {
        return System.Text.Encoding.ASCII.GetString(
          (byte*) (
            (System.IntPtr) pFirstByte 
          + nBytesAvailablePerString * iSlotNumber
          ),
          nBytesAvailablePerString
        ).TrimEnd('\0') ;

      }
    }

    // COULD SPLIT THIS UP !
    //   LOG THE DBR_ code as part of the Notification
    // RENAME ?? RetrieveValueAndOtherInfoFromDbRecordStruct ??

    public static unsafe void GetOptionalAuxiliaryInfoFromDbRecordStruct ( 
      void *                      pDbrStruct, 
      DbRecordRequestDescriptor   dbRecordDescriptor,
      out AlarmStatusAndSeverity? alarmStatusAndSeverity,
      out AuxiliaryInfo?          auxiliaryValues,
      out System.DateTime?        timeStampFromServer,
      out string[]?               enumOptionNames,
      out void *                  pFirstValueElement
    ) {
      alarmStatusAndSeverity = null ;
      auxiliaryValues        = null ;
      timeStampFromServer    = null ;
      enumOptionNames        = null ;
      if ( dbRecordDescriptor.ValueAccessMode == ValueAccessMode.DBR_CTRL_RequestValueAndAuxiliaryInfo )
      {  
        // All 'DBR_CTRL_' structures have these fields at the beginning
        var pStatusAndSeverity = (DBR_StatusAndSeverity*) pDbrStruct ;
        alarmStatusAndSeverity = new(
          AlarmStatus_STAT     : (AlarmStatus_STAT)   pStatusAndSeverity->status,
          AlarmSeverity_SEVR   : (AlarmSeverity_SEVR) pStatusAndSeverity->severity
        ) ;
        switch ( dbRecordDescriptor.DbFieldType )
        {
        case DbFieldType.DBF_STRING_s39:
          {
            var pCtrlStruct = (dbr_ctrl_string_s40*) pDbrStruct ;
            auxiliaryValues = null ;
            pFirstValueElement = &pCtrlStruct->value ;
          }
          break ;
        case DbFieldType.DBF_SHORT_i16: 
          {
            var pCtrlStruct = (dbr_ctrl_int_i16*) pDbrStruct ;
            auxiliaryValues = new AuxiliaryInfo(
              EGU  : pCtrlStruct->units.AsString(),
              HOPR : pCtrlStruct->ctrlInfo.upper_disp_limit_HOPR,
              LOPR : pCtrlStruct->ctrlInfo.lower_disp_limit_LOPR,
              HIHI : pCtrlStruct->ctrlInfo.upper_alarm_limit_HIHI,
              HIGH : pCtrlStruct->ctrlInfo.upper_warning_limit_HIGH,
              LOW  : pCtrlStruct->ctrlInfo.lower_warning_limit_LOW,
              LOLO : pCtrlStruct->ctrlInfo.lower_alarm_limit_LOLO,
              DRVH : pCtrlStruct->ctrlInfo.upper_ctrl_limit_DRVH,
              DRVL : pCtrlStruct->ctrlInfo.lower_ctrl_limit_DRVL
            ) ;
            pFirstValueElement = &pCtrlStruct->value ;
          }
          break ;
        case DbFieldType.DBF_FLOAT_f32: 
          {
            var pCtrlStruct = (dbr_ctrl_float_f32*) pDbrStruct ;
            auxiliaryValues = new AuxiliaryInfo(
              EGU  : pCtrlStruct->units.AsString(),
              PREC : pCtrlStruct->precision.precision,
              HOPR : pCtrlStruct->ctrlInfo.upper_disp_limit_HOPR,
              LOPR : pCtrlStruct->ctrlInfo.lower_disp_limit_LOPR,
              HIHI : pCtrlStruct->ctrlInfo.upper_alarm_limit_HIHI,
              HIGH : pCtrlStruct->ctrlInfo.upper_warning_limit_HIGH,
              LOW  : pCtrlStruct->ctrlInfo.lower_warning_limit_LOW,
              LOLO : pCtrlStruct->ctrlInfo.lower_alarm_limit_LOLO,
              DRVH : pCtrlStruct->ctrlInfo.upper_ctrl_limit_DRVH,
              DRVL : pCtrlStruct->ctrlInfo.lower_ctrl_limit_DRVL
            ) ;
            pFirstValueElement = &pCtrlStruct->value ;
          }
          break ;
        case DbFieldType.DBF_ENUM_i16:  
          {
            // This is how we acquire the option names relating to an 'ENUM' field
            var pCtrlStruct = (dbr_ctrl_enum*) pDbrStruct ;
            enumOptionNames   = pCtrlStruct->stringBytes_26x16.GetStringValues(pCtrlStruct->nStrings) ;
            auxiliaryValues = null ;
            pFirstValueElement = &pCtrlStruct->enumValue ;
          }
          break ;
        case DbFieldType.DBF_CHAR_byte: 
          {
            var pCtrlStruct = (dbr_ctrl_byte_i8*) pDbrStruct ;
            auxiliaryValues = new AuxiliaryInfo(
              EGU  : pCtrlStruct->units.AsString(),
              DRVL : pCtrlStruct->ctrlInfo.lower_ctrl_limit_DRVL
            ) ;
            pFirstValueElement = &pCtrlStruct->value ;
          }
          break ;
        case DbFieldType.DBF_LONG_i32:  
          {
            var pCtrlStruct = (dbr_ctrl_long_i32*) pDbrStruct ;
            auxiliaryValues = new AuxiliaryInfo(
              EGU  : pCtrlStruct->units.AsString(),
              HOPR : pCtrlStruct->ctrlInfo.upper_disp_limit_HOPR,
              LOPR : pCtrlStruct->ctrlInfo.lower_disp_limit_LOPR,
              HIHI : pCtrlStruct->ctrlInfo.upper_alarm_limit_HIHI,
              HIGH : pCtrlStruct->ctrlInfo.upper_warning_limit_HIGH,
              LOW  : pCtrlStruct->ctrlInfo.lower_warning_limit_LOW,
              LOLO : pCtrlStruct->ctrlInfo.lower_alarm_limit_LOLO,
              DRVH : pCtrlStruct->ctrlInfo.upper_ctrl_limit_DRVH,
              DRVL : pCtrlStruct->ctrlInfo.lower_ctrl_limit_DRVL
            ) ;
            pFirstValueElement = &pCtrlStruct->value ;
          }
          break ;
        case DbFieldType.DBF_DOUBLE_f64:
          {
            var pCtrlStruct = (dbr_ctrl_double_f64*) pDbrStruct ;
            auxiliaryValues = new AuxiliaryInfo(
              EGU  : pCtrlStruct->units.AsString(),
              PREC : pCtrlStruct->precision.precision,
              HOPR : pCtrlStruct->ctrlInfo.upper_disp_limit_HOPR,
              LOPR : pCtrlStruct->ctrlInfo.lower_disp_limit_LOPR,
              HIHI : pCtrlStruct->ctrlInfo.upper_alarm_limit_HIHI,
              HIGH : pCtrlStruct->ctrlInfo.upper_warning_limit_HIGH,
              LOW  : pCtrlStruct->ctrlInfo.lower_warning_limit_LOW,
              LOLO : pCtrlStruct->ctrlInfo.lower_alarm_limit_LOLO,
              DRVH : pCtrlStruct->ctrlInfo.upper_ctrl_limit_DRVH,
              DRVL : pCtrlStruct->ctrlInfo.lower_ctrl_limit_DRVL
            ) ;
            pFirstValueElement = &pCtrlStruct->value ;
          }
          break ;
        default:
          throw dbRecordDescriptor.DbFieldType.AsUnexpectedEnumValueException() ;
        }
      }
      else if ( dbRecordDescriptor.ValueAccessMode == ValueAccessMode.DBR_TIME_RequestValueAndServerTimeStamp )
      {
        // All 'DBR_TIME_' structures have these fields at the beginning
        var pStatusAndSeverity = (DBR_StatusAndSeverity*) pDbrStruct ;
        alarmStatusAndSeverity = new(
          AlarmStatus_STAT     : (AlarmStatus_STAT)   pStatusAndSeverity->status,
          AlarmSeverity_SEVR   : (AlarmSeverity_SEVR) pStatusAndSeverity->severity
        ) ;
        switch ( dbRecordDescriptor.DbFieldType )
        {
        case DbFieldType.DBF_STRING_s39:
          {
            var pTimeStruct = (dbr_time_string_s40*) pDbrStruct ;
            timeStampFromServer = pTimeStruct->TimeStampFromServer ;
            pFirstValueElement  = &pTimeStruct->value ;
          }
          break ;
        case DbFieldType.DBF_SHORT_i16: 
          {
            var pTimeStruct = (dbr_time_int_i16*) pDbrStruct ;
            timeStampFromServer = pTimeStruct->TimeStampFromServer ;
            pFirstValueElement  = &pTimeStruct->value ;
          }
          break ;
        case DbFieldType.DBF_FLOAT_f32: 
          {
            var pTimeStruct = (dbr_time_float_f32*) pDbrStruct ;
            timeStampFromServer = pTimeStruct->TimeStampFromServer ;
            pFirstValueElement  = &pTimeStruct->value ;
          }
          break ;
        case DbFieldType.DBF_ENUM_i16:  
          {
            var pTimeStruct = (dbr_time_enum*) pDbrStruct ;
            timeStampFromServer = pTimeStruct->TimeStampFromServer ;
            pFirstValueElement  = &pTimeStruct->value ;
          }
          break ;
        case DbFieldType.DBF_CHAR_byte: 
          {
            var pTimeStruct = (dbr_time_byte_i8*) pDbrStruct ;
            timeStampFromServer = pTimeStruct->TimeStampFromServer ;
            pFirstValueElement  = &pTimeStruct->value ;
          }
          break ;
        case DbFieldType.DBF_LONG_i32:  
          {
            var pTimeStruct = (dbr_time_long_i32*) pDbrStruct ;
            timeStampFromServer = pTimeStruct->TimeStampFromServer ;
            pFirstValueElement  = &pTimeStruct->value ;
          }
          break ;
        case DbFieldType.DBF_DOUBLE_f64:
          {
            var pTimeStruct = (dbr_time_double_f64*) pDbrStruct ;
            timeStampFromServer = pTimeStruct->TimeStampFromServer ;
            pFirstValueElement  = &pTimeStruct->value ;
          }
          break ;
        default:
          throw dbRecordDescriptor.DbFieldType.AsUnexpectedEnumValueException() ;
        }
      }
      else
      {
        pFirstValueElement = pDbrStruct ;
      }
    }

    // Note, we pass in the 'FieldInfo' here, but only so that
    // we can populate the ValueInfo.FieldInfo with that value ...

    public static unsafe ValueInfo CreateValueInfoFromDbRecordStruct ( 
      IChannel                  channel,
      void *                    pDbRecordStruct,
      DbRecordRequestDescriptor dbrDescriptor,
      int                       nElements,
      FieldInfo                 fieldInfo,
      out string[]?             enumOptionNames
    ) {
      DbFieldType fieldType = dbrDescriptor.DbFieldType ;
      object? primaryResult = null ;
      GetOptionalAuxiliaryInfoFromDbRecordStruct ( 
        pDbRecordStruct, 
        dbrDescriptor,
        out AlarmStatusAndSeverity? alarmStatusAndSeverity,
        out AuxiliaryInfo?          auxiliaryInfo,
        out System.DateTime?        timeStampFromServer,
        out                         enumOptionNames,
        out void *                  pFirstValueElement
      ) ;
      if ( nElements == 0 )
      {
        primaryResult.Should().BeNull() ;
      }
      else if ( nElements == 1 )
      {
        switch ( fieldType )
        {
        case DbFieldType.DBF_STRING_s39:
          primaryResult = GetScalarValueFromDbr<ByteArray_40>().AsString() ;
          break ;
        case DbFieldType.DBF_DOUBLE_f64:
          primaryResult = GetScalarValueFromDbr<double>() ;
          break ;
        case DbFieldType.DBF_SHORT_i16: 
          primaryResult = GetScalarValueFromDbr<short>() ;
          break ;
        case DbFieldType.DBF_FLOAT_f32:
          primaryResult = GetScalarValueFromDbr<float>() ;
          break ;
        case DbFieldType.DBF_ENUM_i16:  
          primaryResult = GetScalarValueFromDbr<short>() ;
          break ;
        case DbFieldType.DBF_CHAR_byte:  
          primaryResult = GetScalarValueFromDbr<byte>() ;
          break ;
        case DbFieldType.DBF_LONG_i32:  
          primaryResult = GetScalarValueFromDbr<int>() ;
          break ;
        default:
          throw fieldType.AsUnexpectedEnumValueException() ;
        }
      }
      else 
      {
        switch ( fieldType )
        {
        case DbFieldType.DBF_DOUBLE_f64:
          primaryResult = CreateArrayValueFromDbr<double>() ;
          break ;
        case DbFieldType.DBF_SHORT_i16: 
          primaryResult = CreateArrayValueFromDbr<short>() ;
          break ;
        case DbFieldType.DBF_STRING_s39:
          primaryResult = CreateArrayValueFromDbr<ByteArray_40>(
          ).Select(
            item => item.AsString()
          ).ToArray() ;
          break ;
        case DbFieldType.DBF_FLOAT_f32:
          primaryResult = CreateArrayValueFromDbr<float>() ;
          break ;
        case DbFieldType.DBF_ENUM_i16:  
          primaryResult = CreateArrayValueFromDbr<short>() ;
          break ;
        case DbFieldType.DBF_CHAR_byte:  
          primaryResult = CreateArrayValueFromDbr<byte>() ;
          break ;
        case DbFieldType.DBF_LONG_i32:  
          primaryResult = CreateArrayValueFromDbr<int>() ;
          break ;
        default:
          throw fieldType.AsUnexpectedEnumValueException() ;
        }
      }
      return new ValueInfo(
        Channel                : channel,
        Value                  : primaryResult.VerifiedAsNonNullInstance(),
        FieldInfo              : fieldInfo,
        AlarmStatusAndSeverity : alarmStatusAndSeverity,
        AuxiliaryInfo          : auxiliaryInfo,
        TimeStampFromServer    : timeStampFromServer
      ) ;
      // ----------------
      // Local functions
      unsafe T GetScalarValueFromDbr<T> ( ) where T : unmanaged
      {
        T value = *((T*) pFirstValueElement) ;
        return value ;
      }
      unsafe T[] CreateArrayValueFromDbr<T> ( ) where T : unmanaged
      {
        var array = new T[nElements] ;
        long nBytesToCopy = nElements * sizeof(T) ;
        fixed ( T * pFirstDestinationElement = array )
        {
          System.Buffer.MemoryCopy(
            pFirstValueElement,
            pFirstDestinationElement,
            nBytesToCopy,
            nBytesToCopy
          ) ;
        }
        return array ;
      }
    }
    
  }

}
