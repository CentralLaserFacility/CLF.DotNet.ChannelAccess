//
// Helpers.cs
//

//
// Note that there are various additional 'partial' Helper definitions
// providing useful extension methods ...
//

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Clf.ChannelAccess.LowLevelApi.ExtensionMethods;
using System.Diagnostics.CodeAnalysis;
using Clf.Common.ExtensionMethods;
using System.Linq;

namespace Clf.ChannelAccess
{

  public static partial class Helpers
  {

    public static bool IsValidChannelName ( 
      this string?                     name,
      [NotNullWhen(false)] out string? whyNotValid
    ) {
      return IsValidChannelName(
        name, 
        out var baseName,
        out var fieldName,
        out whyNotValid
      ) ;
    }

    public static bool IsValidChannelName ( 
      this string?                                   name,
      [NotNullWhen(true)]  out ValidatedChannelName? validatedChannelName,
      [NotNullWhen(false)] out string?               whyNotValid
    ) {
      validatedChannelName = null ;
      if (
        IsValidChannelName(
          name, 
          out var baseName,
          out var fieldName,
          out whyNotValid
        ) 
      ) {
        validatedChannelName = new ValidatedChannelName(
          name!,
          baseName,
          fieldName
        ) ;
      }
      return validatedChannelName != null ;
    }

    internal static bool IsValidChannelName ( 
      this string?                     name,
      [NotNullWhen(true)]  out string? baseName,
      [NotNullWhen(true)]  out string? fieldName,
      [NotNullWhen(false)] out string? whyNotValid
    ) {
      // Max length is 60
      // Valid characters :
      //   a..z
      //   A..Z
      //   0..9
      //   _ - + [ ] < > ; : 
      // And at most one '.' near the end, delimiting a field name
      // of between 1 and 4 capital alphabetic characters (alphanumeric??)
      baseName = null ;
      fieldName = null ;
      if ( string.IsNullOrEmpty(name) )
      {
        whyNotValid = "Length is zero" ;
        return false ;
      }
      if ( name.Length > 60 )
      {
        whyNotValid = "Length is greater than 60" ;
        return false ;
      }
      int nDotsSeen = 0 ;
      foreach ( char ch in name )
      {
        // Hmm, do this with 'split' below ...
        if ( ch == '.' )
        {
          nDotsSeen++ ;
        }
        // NOTE : WE'RE TEMPORARILY PERMITTING '?' CHARACTERS ... IS THIS CORRECT ???
        else if ( ! ch.IsValidCharacter("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-+[]<>;:?") )
        {
          whyNotValid = $"Contains an invalid character '{ch}'" ;
          return false ;
        }
      }
      switch ( nDotsSeen )
      {
      case 0:
        baseName = name ;
        fieldName = "VAL" ;
        break ;
      case 1:
        string[] fields = name.Split('.') ;
        baseName = fields[0] ;
        fieldName = fields[1] ;
        if ( fieldName.Length == 0 )
        {
          whyNotValid = "Field name is empty" ;
          return false ;
        }
        if ( fieldName != fieldName.ToUpper() )
        {
          whyNotValid = "Field name contains lower case letters" ;
          return false ;
        }
        // TODO : Field name should contain only alphanumeric characters
        break ;
      default:
        whyNotValid = "Contains more than one '.'" ;
        return false ;
      }
      whyNotValid = null ;
      return true ;
    }

    public static void IsValidChannelName_tests_give_expected_results ( )
    {
      string? whyNotValid ;

      // Hmm, could verify the 'whyNot' messages also ???

      "abc".IsValidChannelName( out whyNotValid ).Should().BeTrue() ;
      "abc.ZZZZ".IsValidChannelName( out whyNotValid ).Should().BeTrue() ;
      "abcdefghijklmnopqrstuvwxyz".IsValidChannelName( out whyNotValid ).Should().BeTrue() ;
      "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-+[]<>;:.VAL".IsValidChannelName( out whyNotValid ).Should().BeTrue() ;

      "".IsValidChannelName( out whyNotValid ).Should().BeFalse() ;
      "a.".IsValidChannelName( out whyNotValid ).Should().BeFalse() ;
      "a.ZZZZZ".IsValidChannelName( out whyNotValid ).Should().BeFalse() ;
      new string('a',60).IsValidChannelName( out whyNotValid ).Should().BeTrue() ;
      new string('a',61).IsValidChannelName( out whyNotValid ).Should().BeFalse() ;
    }

    // Throws an exception if 's' can't be converted to
    // an 'object' that is compatible with the Channel's data type

    public static object ConvertStringToChannelValue ( 
      string            s, 
      DbFieldDescriptor dbFieldDescriptor
    ) {
      if (
        dbFieldDescriptor.TryParseValue(
          s,
          out var value
        )
      ) {
        return value ;
      }
      else
      {
        throw new UsageErrorException(
          $"Failed to convert '{s}' to {dbFieldDescriptor.FieldTypeAsString}"
        ) ;
      }
    }

    // Give a System.Type, such as typeof(int[]),
    // report whether or not 'values' of that type
    // are supported (in principle!) by the Channel Access protocol
    // and if yes, return the 'DbFieldType' and whether or not
    // the type requires an array of elements.

    public static bool ValueTypeIsSupportedByChannelAccess (
      System.Type                          valueType,
      [NotNullWhen(true)] out DbFieldType? dbFieldType,
      [NotNullWhen(true)] out bool         isArray
    ) {
      (valueType,isArray) = (
        valueType.IsArray
        ? (valueType.GetElementType().Plinged(),true)
        : (valueType,false)
      ) ;
      dbFieldType = System.Type.GetTypeCode(valueType) switch {
        System.TypeCode.Object => DbFieldType.DBF_STRING_s39, // !!!!!!!
        System.TypeCode.Double => DbFieldType.DBF_DOUBLE_f64,
        System.TypeCode.String => DbFieldType.DBF_STRING_s39,
        System.TypeCode.Int16  => DbFieldType.DBF_SHORT_i16,
        System.TypeCode.Single => DbFieldType.DBF_FLOAT_f32,
        System.TypeCode.Byte   => DbFieldType.DBF_CHAR_byte,
        System.TypeCode.Int32  => DbFieldType.DBF_LONG_i32,
        _                      => (
                                     valueType.IsEnum 
                                  && ! isArray
                                  )
                                  ? DbFieldType.DBF_SHORT_i16
                                  : null
      } ;
      return dbFieldType != null ;
    }

    public static DbFieldType GetDbFieldTypeRepresentingSystemType ( 
      System.Type type
    ) {
      if ( 
        ValueTypeIsSupportedByChannelAccess(
          type,
          out var  dbFieldType,
          out bool isArray
        )
      ) {
        return dbFieldType.Value ;
      }
      else
      {
        throw new UsageErrorException(
          $"Type {type} is not supported n ChannelAccess"
        ) ;
      }
    }

    public static object CreateIncrementedValue ( object x )
    {
      return x switch {
        // Scalar types
        byte   i   => ( byte  ) ( i + 1 ),
        short  i   => ( short ) ( i + 1 ),
        int    i   => ( int   ) ( i + 1 ),
        float  f32 => f32 + 1,
        double f64 => f64 + 1,
        string s   => s + "+1",
        // Array types
        byte[]   a => Clf.Common.Helpers.CreateArrayOfObjects( a.Length, (byte)   CreateIncrementedValue(a[0]) ),
        short[]  a => Clf.Common.Helpers.CreateArrayOfObjects( a.Length, (short)  CreateIncrementedValue(a[0]) ),
        int[]    a => Clf.Common.Helpers.CreateArrayOfObjects( a.Length, (int)    CreateIncrementedValue(a[0]) ),
        float[]  a => Clf.Common.Helpers.CreateArrayOfObjects( a.Length, (float)  CreateIncrementedValue(a[0]) ),
        double[] a => Clf.Common.Helpers.CreateArrayOfObjects( a.Length, (double) CreateIncrementedValue(a[0]) ),
        string[] a => Clf.Common.Helpers.CreateArrayOfObjects( a.Length, (string) CreateIncrementedValue(a[0]) ),
        _ => null!      
      } ;
    }

  }

}
