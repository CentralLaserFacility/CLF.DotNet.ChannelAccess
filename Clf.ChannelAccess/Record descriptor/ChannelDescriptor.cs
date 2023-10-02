//
// ChannelDescriptor.cs
// 

using System.Collections.Generic ;
using Clf.Common.ExtensionMethods ;
using System.Linq ;
using System.Diagnostics.CodeAnalysis ;

using Clf.ChannelAccess.ExtensionMethods ;

namespace Clf.ChannelAccess
{

  //
  // This describes a db-file 'record()' that provides a VAL of a specified type,
  // implemented as a 'waveform' record (for numeric and string types)
  // or as an 'mbbi' record (for an 'enum' type). 
  //

  //
  // Note that the ChannelName must be a valid name
  // that doesn't specify a macro to be substituted.
  //
  // Syntax such as '${prefix}:xxx' is NOT SUPPORTED.
  //

  //
  // TODO : ??? Instead of 'ChannelAccess.ChannelName', which in principle
  // can specify a Field name such as .VAL or .DESC, we should use
  // a new type 'ChannelAccess.ChannelName' which is equivalent to the 'Base'
  // part of a Channel Name ie without the '.FIELD-NAME' suffix.
  //
  // Until that's in place, might be better to just specify ChannelName as a string ???
  //

  //
  // TODO :
  //   Handle specified enum values, and missing values
  //   enum:aa=0,bb=2,,dd=4 
  //

  //
  // TODO :
  //   Populating arrays:
  //   1.23
  //   1,2,3,4   CYCLIC !!!
  //

  public record ChannelDescriptor (
    ChannelName       ChannelName,
    DbFieldDescriptor DbFieldDescriptor,
    string?           InitialValueAsString = null,
    string?           Description          = null
  ) {

    public void CheckValidity ( )
    {
      // TODO !!!
    }

    public bool IsValid ( [NotNullWhen(false)] out string? whyNotValid )
    {
      whyNotValid = null ;
      if ( TryGetInitialValueAsObject( out var value ) is false )
      {
        whyNotValid = $"Can't convert initial value specified as '{InitialValueAsString}' to the expected type" ;
      }
      return whyNotValid is null ; 
    }

    public bool TryGetInitialValueAsObject ( [NotNullWhen(true)] out object? initialValue )
    {
      initialValue = null ;
      return (
        InitialValueAsString is null
        ? false
        : DbFieldDescriptor.TryParseValue(
            InitialValueAsString,
            out initialValue
          )
      ) ; 
    }

    public bool TryCreateCompatibleValueFromString ( string valueAsString, [NotNullWhen(true)] out object? value )
    {
      return DbFieldDescriptor.TryParseValue(
        valueAsString,
        out value
      ) ; 
    }

    public static IEnumerable<string> SyntaxExampleLines = ( 
    @"
Compact syntax for defining 'records' for ThinIoc
-------------------------------------------------

A typical definition looks like this :

  xx:myFloat|f32[4]|1.23,...|Single_float_value // Optional comment

There are three required fields, separated by '|',
and an optional 4th 'description' field
  1. PV name
  2. Data Type
      s39 byte i16 i32 f32 f64 enum
      For an array, specify the number of elements 
        eg 'i16[4]'
      For an enum, specify the option names 
        eg 'enum:a,b,c'
  3. Initial value (optional)
       For an array, every element will be set
  4. Description (optional)
       Use '_' to represent a space.
       This convention is helpful when you specify records 
       on the command line, where allowing embedded spaces 
       would make parsing the arguments more awkward.

The data-type options are                            Epics type
                                                     |
  s39  : string of up to 39 characters             : DBF_STRING  
  byte : 8-bit unsigned integer                    : DBF_CHAR    
  i16  : 16 bit signed integer                     : DBF_SHORT   
  i32  : 32 bit signed integer                     : DBF_LONG    
  f32  : single precision floating point, 32 bits  : DBF_FLOAT   
  f64  : double precision floating point, 64 bits  : DBF_DOUBLE  
  enum : enumerated value with up to 16 options    : DBF_ENUM    

Blank lines are ignored

Comments are allowed - anything following '//' is ignored

Examples declaring a PV whose value is an array :

  xx:myArrayOfFloats|f32:3|1.23|Array_of_three_floats
  
  xx:myArrayOfFloats|f32[3]|1.23|Array_of_three_floats

Example declaring an 'enum' :

  xx:myEnum|enum:aa,bb,cc|0|Enum_with_three_options

".Split("\r\n")
    ) ;

    public sealed override string ToString ( )
    {
      return $"{ChannelName.Validated().BaseName}|{GetDbFieldTypeCode()}:{GetElementsCountOrEnumNames()}|{InitialValueAsString??""}|{Description??""}" ;
      string GetDbFieldTypeCode ( ) 
      => (
        DbFieldDescriptor.DbFieldType == ChannelAccess.DbFieldType.DBF_ENUM_i16
        ? "enum"
        : DbFieldDescriptor.DbFieldType.ToString().Split('_')[2] 
      ) ;
      string GetElementsCountOrEnumNames ( ) 
      => (
        DbFieldDescriptor.IsEnumField()
        ? string.Join(',',DbFieldDescriptor.EnumNames!)
        : DbFieldDescriptor.ElementsCountOnServer.ToString()
      ) ;
    }

    public string ToMinimalString ( )
    {
      return $"{ChannelName.Validated().BaseName}|{GetDbFieldTypeCode()}:{GetElementsCountOrEnumNames()}" ;
      string GetDbFieldTypeCode ( ) 
      => (
        DbFieldDescriptor.DbFieldType == ChannelAccess.DbFieldType.DBF_ENUM_i16
        ? "enum"
        : DbFieldDescriptor.DbFieldType.ToString().Split('_')[2] 
      ) ;
      string GetElementsCountOrEnumNames ( ) 
      => (
        DbFieldDescriptor.IsEnumField()
        ? string.Join(',',DbFieldDescriptor.EnumNames!)
        : DbFieldDescriptor.ElementsCountOnServer.ToString()
      ) ;
    }

    // TODO : Do this without relying on catching an exception ...

    public static bool CouldCreateFromEncodedString ( string s )
    {
      try
      {
        FromEncodedString(s) ;
        return true ;
      }
      catch 
      {
        return false ;
      }
    }

    // TODO : We really need to be more helpful describing syntax errors !!!

    public static ChannelDescriptor FromEncodedString ( string s )
    {
      try
      {
        // Tacky but slick way (!!) of ignoring a trailing comment 
        string[] fields = s.Trim().Split("//")[0].Split('|') ;
        string channelName = fields[0] ;
        string dbTypeAndLength = fields[1] ;
        string? initialValue = (
          GetFieldIfAvailable(fields,2) 
          // ?? throw new System.ApplicationException("Initial value is required") 
        ) ;
        // Not a good idea to permit spaces in the 'description' field,
        // Because these strings might be passed on a command line.
        string? description = GetFieldIfAvailable(fields,3) ; // ?.Replace('_',' ') ;
        var dbFieldDescriptor = Clf.ChannelAccess.DbFieldDescriptor.CreateFromEncodedString(
          dbTypeAndLength
        ) ;
        if ( 
           dbFieldDescriptor.DbFieldType == DbFieldType.DBF_STRING_s39 
        && initialValue != null
        ) {
          if ( dbFieldDescriptor.IsArray )
          {
            // We don't yet have a syntax for representing
            // arrays-of-string where an element can be null ...
          }
          else
          {
            // We'll represent an empty string as ''
            initialValue = initialValue.Trim('\'') ;
          }
        }
        return new ChannelDescriptor(
          channelName, 
          dbFieldDescriptor,
          initialValue,
          description
        ) ;
      }
      catch ( System.Exception x )
      {
        x.ToString(); //TODO: Handle exception in Log... suppressing warning
        throw new ProgrammingErrorException(
          $"RecordDescriptor '{s}' has incorrect syntax"
        ) ;
      }
    }

    // private static void DecodeDbTypeAndLength (
    //   string                            dbTypeAndLength,
    //   out Clf.ChannelAccess.DbFieldType dbFieldType,
    //   out int                           elementsCount,
    //   out IEnumerable<string>           enumNames
    // ) {
    //   string[] fields = dbTypeAndLength.Split(':',',') ;
    //   dbFieldType = fields[0] switch {
    //     "s39"   => Clf.ChannelAccess.DbFieldType.DBF_STRING_s39,
    //     "i16"   => Clf.ChannelAccess.DbFieldType.DBF_SHORT_i16,
    //     "f32"   => Clf.ChannelAccess.DbFieldType.DBF_FLOAT_f32,
    //     "enum"  => Clf.ChannelAccess.DbFieldType.DBF_ENUM_i16,
    //     "byte"  => Clf.ChannelAccess.DbFieldType.DBF_CHAR_byte,
    //     "i32"   => Clf.ChannelAccess.DbFieldType.DBF_LONG_i32,
    //     "f64"   => Clf.ChannelAccess.DbFieldType.DBF_DOUBLE_f64,
    //     _ => throw fields[0].AsUnexpectedValueException()
    //   } ;
    //   if ( dbFieldType == ChannelAccess.DbFieldType.DBF_ENUM_i16 )
    //   {
    //     elementsCount = 1 ;
    //     enumNames     = fields.Skip(1) ;
    //   }
    //   else
    //   {
    //     elementsCount = ( 
    //       dbTypeAndLength.Contains(':')
    //       ? int.Parse(fields[1])
    //       : 1
    //     ) ;
    //     enumNames = Enumerable.Empty<string>() ;
    //   }
    // }
    // 
    // private static bool CanDecodeDbTypeAndLength(
    //   string                                                dbTypeAndLength,
    //   [NotNullWhen(true)] out Clf.ChannelAccess.DbFieldType dbFieldType,
    //   [NotNullWhen(true)] out int                           elementsCount,
    //   [NotNullWhen(true)] out IEnumerable<string>           enumNames
    // ) {
    //   try
    //   {
    //     string[] fields = dbTypeAndLength.Split(':',',') ;
    //     dbFieldType = fields[0] switch {
    //      "s39"   => Clf.ChannelAccess.DbFieldType.DBF_STRING_s39,
    //      "i16"   => Clf.ChannelAccess.DbFieldType.DBF_SHORT_i16,
    //      "f32"   => Clf.ChannelAccess.DbFieldType.DBF_FLOAT_f32,
    //      "enum"  => Clf.ChannelAccess.DbFieldType.DBF_ENUM_i16,
    //      "byte"  => Clf.ChannelAccess.DbFieldType.DBF_CHAR_byte,
    //      "i32"   => Clf.ChannelAccess.DbFieldType.DBF_LONG_i32,
    //      "f64"   => Clf.ChannelAccess.DbFieldType.DBF_DOUBLE_f64,
    //      _ => throw fields[0].AsUnexpectedValueException()
    //     } ;
    //     if ( dbFieldType == ChannelAccess.DbFieldType.DBF_ENUM_i16 )
    //     {
    //       elementsCount = 1 ;
    //       enumNames     = fields.Skip(1) ;
    //     }
    //     else
    //     {
    //       elementsCount = ( 
    //         dbTypeAndLength.Contains(':')
    //         ? int.Parse(fields[1])
    //         : 1
    //       ) ;
    //       enumNames = Enumerable.Empty<string>() ;
    //     }
    //     return true ;
    //   }
    //   catch
    //   {
    //     dbFieldType   = default ; 
    //     elementsCount = default ;
    //     enumNames     = null! ;
    //     return false ;
    //   }
    // }

    private static string? GetFieldIfAvailable ( string[] fields, int index )
    {
      return (
        index < fields.Length
        ? fields[index]
        : null
      ) ;
    }

    // Create a bunch of text lines from the record descriptor,
    // in a format that corresponds to the equivalent 'record' definition
    // that you'd specify in a '.db' file

    public IEnumerable<string> ToDbTextLines ( )
    {
      return ( 
        DbFieldDescriptor.IsEnumField()
        ? ToDbEnumRecordTextLines()
        : ToDbWaveformRecordTextLines()
      ) ;
    }

    public IEnumerable<string> ToDbEnumRecordTextLines ( )
    {
      yield return $"record(mbbi,\"{ChannelName.Validated().BaseName}\")" ;
      yield return "{" ;
      if ( Description != null )
      {
        yield return $"  field(DESC,\"{Description}\")" ;
      }
      int iOption = 0 ;
      string[] optionFieldNames = new[]{
        "ZRST",
        "ONST",
        "TWST",
        "THST",
        "FRST",
        "FVST",
        "SXST",
        "SVST",
        "EIST",
        "NIST",
        "TEST",
        "ELST",
        "TVST",
        "TTST",
        "FTST",
        "FFST"
      } ;
      foreach ( string option in DbFieldDescriptor.EnumNames! )
      {
        yield return $"  field({optionFieldNames[iOption]},\"{option}\")" ;
        iOption++ ;
      }
      yield return "}" ;
    }

    public IEnumerable<string> ToDbWaveformRecordTextLines ( )
    {
      yield return $"record(waveform,\"{ChannelName.Validated().BaseName}\")" ;
      yield return "{" ;
      if ( Description != null )
      {
        yield return $"  field(DESC,\"{Description}\")" ;
      }
      yield return $"  field(FTVL,\"{FTVL()}\")" ;
      yield return $"  field(NELM,\"{DbFieldDescriptor.ElementsCountOnServer}\")" ;
      yield return "}" ;
      string FTVL ( )
      => DbFieldDescriptor.DbFieldType switch {
        Clf.ChannelAccess.DbFieldType.DBF_STRING_s39 => "STRING",
        Clf.ChannelAccess.DbFieldType.DBF_SHORT_i16  => "SHORT",
        Clf.ChannelAccess.DbFieldType.DBF_FLOAT_f32  => "FLOAT",
        Clf.ChannelAccess.DbFieldType.DBF_ENUM_i16   => "ENUM",
        Clf.ChannelAccess.DbFieldType.DBF_CHAR_byte  => "CHAR",
        Clf.ChannelAccess.DbFieldType.DBF_LONG_i32   => "LONG",
        Clf.ChannelAccess.DbFieldType.DBF_DOUBLE_f64 => "DOUBLE",
        _ => throw new ChannelAccess.UsageErrorException(
            $"Type {DbFieldDescriptor.DbFieldType} is not supported"
          ) 
      } ;
    }

  } ;

}