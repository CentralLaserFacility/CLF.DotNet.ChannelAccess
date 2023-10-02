//
// DbFieldDescriptor.cs
//

using System.Diagnostics.CodeAnalysis ;
using System.Collections.Generic ;
using System.Linq ;

using Clf.Common.ExtensionMethods ;

namespace Clf.ChannelAccess
{

  //
  // Note that it's entirely possible to have a DbFieldDescriptor
  // that describes an 'enum' type but doesn't yet describe the
  // available Option Names. This circumstance would arise briefly
  // when we're getting 'field info' from the server, and the data type
  // is reported as 'enum' ; in that case we have to wait for a
  // further 'dbr_ctrl_enum' query to return the enum option strings.
  //
  // An enum field will *always* have valid option names
  // when we're creating a 'ChannelDescriptor' that will be used
  // to drive a 'ThinIoc' server, or to represent a LocalChannel.
  //

  /// <summary>
  /// Describes a PV 'field' in terms of the type of its value 
  /// string, int, float, double, enum etc) and the number 
  /// of elements, which is 1 for a scalar value.
  /// </summary>

  public record DbFieldDescriptor ( 
    DbFieldType DbFieldType, 
    int         ElementsCountOnServer,
    bool        IsWriteable,
    string?     InitialValueAsString = null
  ) {

    //
    // This property is mutable, because we don't get to find out
    // the names of the 'enum' options until we've had a reply
    // to our 'dbr_ctrl_enum' query that reports the names.
    //
    // Note that if we're accessing the Server Time Stamp, the string values
    // will not be available, unless we make a separate call that retrieves
    // the 'auxiliary info'.
    //

    /// <summary>
    /// Enum Option Names
    /// In a previous version, the field representing the enumerated values
    /// was being stored as an array. That was problematic : arrays with the same
    /// content don't necessarily compare as equal, so two instances of 
    /// DbFieldDescriptor could compare as unequal even if they contained
    /// equivalent arrays of 'enum' values. The fix is to hold the enum names
    /// in a single string, and unpack the values when they're accessed.
    /// String comparisons work in a special way that makes them behave 
    /// in the same way as primitive values such as ints.
    /// --------------------------------
    /// An alternative fix would be to override the 'Equals()' method that will
    /// have been synthesised by the compiler, and substitute an implementation
    /// that compares the contents of the 'enum-names' arrays rather than comparing
    /// the array references. 
    /// https://stackoverflow.com/questions/64326511/custom-equality-check-for-c-sharp-9-records
    /// </summary>
    
    private string? m_enumNames_commaSeparated ;

    public string[]? EnumNames 
    { 
      get => (
        m_enumNames_commaSeparated is null
        ? null
        : m_enumNames_commaSeparated.Split(
            ",",
            System.StringSplitOptions.RemoveEmptyEntries
          )
      ) ;
    }

    // FIX_THIS : should be internal ... but currently
    // has to be public for use by LogicSystem.
    // Add ctor arg to set enumNames.

    public void SetEnumNames ( string[]? enumNames )
    {  
      if ( enumNames is null )
      {
        m_enumNames_commaSeparated = null ;
      }
      else
      {
        m_enumNames_commaSeparated = string.Join(',',enumNames) ;
      }
    }

    // In principle we could support defining explicit numeric values
    // to be associated with each option, but this is really quite complicated
    // in EPICS as those values are *not* returned along with the option names
    // in a 'dbr_ctrl_enum' query - you need to make separate queries to
    // find the values of the fields, by name.
    // private int[]? m_enumOptionValues = null ;
    // public int[]? EnumOptionValues 
    // => (
    //   EnumOptionNames is null
    //   ? null
    //   : Enumerable.Range(0,EnumOptionNames.Length-1).ToArray()
    // ) ;

    public bool IsArray => ElementsCountOnServer > 1 ;

    public bool IsScalarValue => ElementsCountOnServer == 1 ;

    public bool IsEnumField ( ) => EnumNames != null ;

    public bool IsEnumField ( [NotNullWhen(true)] out string[]? enumOptionNames )
    {
      // Hmm, if we're an enum field but the option names
      // have not yet been determined ... should we return
      // a 'faked' set of strings, eg 'Option_0', 'Option_1' etc ??
      // At this point we won't even know how many valid options there are.
      enumOptionNames = EnumNames ;
      return enumOptionNames != null ;
    }

    public static bool TryCreateFromEncodedString ( 
      string                                     s,
      [NotNullWhen(true)] out DbFieldDescriptor? dbFieldDescriptor
    ) {  
      dbFieldDescriptor = (
        TryDecodeDbTypeAndLength(
          s,
          out Clf.ChannelAccess.DbFieldType dbFieldType,
          out int                           elementsCount,
          out IEnumerable<string>           enumNames
        )
        ? new DbFieldDescriptor(
            DbFieldType           : dbFieldType,
            ElementsCountOnServer : elementsCount,
            IsWriteable           : true
          ) {
            m_enumNames_commaSeparated = (
              enumNames.Any()
              ? string.Join(',',enumNames)
              : null
            )
          }
        : null
      ) ;
      return dbFieldDescriptor is not null ;
    }

    internal static DbFieldDescriptor CreateWithEnumValues (
      Clf.ChannelAccess.DbFieldType dbFieldType,
      int                           elementsCount,
      bool                          isWriteable,
      string[]?                     enumValues
    ) {  
      DbFieldDescriptor instance = new(dbFieldType,elementsCount,isWriteable) ;
      instance.SetEnumNames(enumValues) ;
      return instance ;
    }

    public static DbFieldDescriptor CreateFromEncodedString ( string s )
    {  
      return (
        TryDecodeDbTypeAndLength(
          s,
          out Clf.ChannelAccess.DbFieldType dbFieldType,
          out int                           elementsCount,
          out IEnumerable<string>           enumNames
        )
        ? new DbFieldDescriptor(
            DbFieldType           : dbFieldType,
            ElementsCountOnServer : elementsCount,
            IsWriteable           : true
          ) {
            m_enumNames_commaSeparated = (
              enumNames.Any()
              ? string.Join(',',enumNames)
              : null
            )
          }
        : throw new UsageErrorException(
            $"Not a valid 'DbFieldDescriptor' : '{s}'"
          ) 
      ) ;
    }

    internal FieldDataTypeCode GetFieldDataTypeCode ( ) 
    {
      return DbFieldType switch {
        DbFieldType.DBF_STRING_s39 => this.IsArray ? FieldDataTypeCode.ArrayOfShortAsciiString : FieldDataTypeCode.ShortAsciiString,
        DbFieldType.DBF_SHORT_i16  => this.IsArray ? FieldDataTypeCode.ArrayOfInt16            : FieldDataTypeCode.Int16, 
        DbFieldType.DBF_FLOAT_f32  => this.IsArray ? FieldDataTypeCode.ArrayOfFloat            : FieldDataTypeCode.Float, 
        DbFieldType.DBF_ENUM_i16   => this.IsArray ? FieldDataTypeCode.Enum                    : FieldDataTypeCode.Enum, 
        DbFieldType.DBF_CHAR_byte  => this.IsArray ? FieldDataTypeCode.ArrayOfUByte8           : FieldDataTypeCode.UByte8, 
        DbFieldType.DBF_LONG_i32   => this.IsArray ? FieldDataTypeCode.ArrayOfInt32            : FieldDataTypeCode.Int32, 
        DbFieldType.DBF_DOUBLE_f64 => this.IsArray ? FieldDataTypeCode.ArrayOfDouble           : FieldDataTypeCode.Double, 
        _                          => throw DbFieldType.AsUnexpectedEnumValueException() 
      } ;
    }

    public System.Type GetFieldDataType ( ) 
    => (
      GetFieldDataTypeCode().GetCustomAttribute<
        Clf.Common.AssociatedTypeAttribute
      >(
      ).AssociatedType.VerifiedAsNonNullInstance() 
    ) ;

    // Moved from 'EnumFieldInfo' ...
    /// <summary>
    /// How Many Enum Options
    /// </summary>
    
    public int EnumCount => EnumNames?.Length ?? 0 ;

    // public string this [ short enumValue ] => EnumOptionNames.ToArray()[enumValue] ;
    /// <summary>
    /// Try Get Enum Option Name As String.
    /// </summary>
    /// <param name="enumIndex"></param>
    /// <param name="enumOptionName"></param>
    /// <returns></returns>
    
    public bool TryGetEnumNameAsString (
      short                           enumIndex,
      [NotNullWhen(true)] out string? enumOptionName
    ) {
      enumOptionName = null ;
      if ( 
         EnumNames != null 
      && enumIndex < EnumNames.Length
      ) {
        enumOptionName = EnumNames[enumIndex] ;
      }
      return enumOptionName != null ;
    }

    /// <summary>
    /// Enum Option Name As String
    /// </summary>
    /// <param name="enumIndex"></param>
    /// <returns></returns>
    
    public string? EnumNameAsString (
      short enumIndex
    ) => (
      TryGetEnumNameAsString(
        enumIndex,
        out var enumOptionName
      )
      ? enumOptionName    
      : null 
    ) ;

    internal void RenderOptionNamesAsStrings ( System.Action<string> writeLine )
    {
      if ( EnumNames != null )
      {
        writeLine("Enum option names :") ;
        EnumNames.ForEachItem(
          (optionName,i) => writeLine(
            $"  {i:D2} : '{optionName}'"
          )
        ) ;
      }
    }

    // This cheap-and-cheerful parsing handles valid sequences
    // but also doesn't reject a lot of nonsensical inputs !!!

    internal static bool TryDecodeDbTypeAndLength(
      string                                                dbTypeAndLength,
      [NotNullWhen(true)] out Clf.ChannelAccess.DbFieldType dbFieldType,
      [NotNullWhen(true)] out int                           elementsCount,
      [NotNullWhen(true)] out IEnumerable<string>           enumNames
    ) {
      try
      {
        // We allow formats such as
        //  i16
        //  i16:4
        //  i16[4]
        //  enum:aa,bb,cc
        string[] fields = dbTypeAndLength.Split(
          ',',
          ':',
          '[',
          ']'
        ) ;
        dbFieldType = fields[0] switch {
          "s39"   => Clf.ChannelAccess.DbFieldType.DBF_STRING_s39,
          "i16"   => Clf.ChannelAccess.DbFieldType.DBF_SHORT_i16,
          "f32"   => Clf.ChannelAccess.DbFieldType.DBF_FLOAT_f32,
          "enum"  => Clf.ChannelAccess.DbFieldType.DBF_ENUM_i16,
          "byte"  => Clf.ChannelAccess.DbFieldType.DBF_CHAR_byte,
          "i32"   => Clf.ChannelAccess.DbFieldType.DBF_LONG_i32,
          "f64"   => Clf.ChannelAccess.DbFieldType.DBF_DOUBLE_f64,
          _ => throw fields[0].AsUnexpectedValueException()
        } ;
        if ( dbFieldType == ChannelAccess.DbFieldType.DBF_ENUM_i16 )
        {
          elementsCount = 1 ;
          enumNames     = fields.Skip(1) ;
        }
        else
        {
          elementsCount = ( 
            (
               dbTypeAndLength.Contains(':')
            || dbTypeAndLength.Contains('[')
            )
            ? int.Parse(fields[1]) // We have :N or [N]
            : 1
          ) ;
          enumNames = Enumerable.Empty<string>() ;
        }
        return true ;
      }
      catch
      {
        dbFieldType   = default ; 
        elementsCount = default ;
        enumNames     = null! ;
        return false ;
      }
    }

    // Returns a string such as 'DBF_SHORT_i16[4]'

    public string FieldTypeAsString => (
      IsArray
      ? $"{DbFieldType}[{ElementsCountOnServer}]" 
      : $"{DbFieldType}" 
    ) ;

    /// <summary>
    /// Try Create Compatible Value From String
    /// </summary>
    /// <param name="s"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    
    public bool TryParseValue ( 
      string                          s,
      [NotNullWhen(true)] out object? value 
    ) {
      return ( 
        IsArray
        ? TryCreateCompatibleVectorValueFromString( s, out value )
        : TryCreateCompatibleScalarValueFromString( s, out value )
      ) ;
    }

    private bool TryCreateCompatibleScalarValueFromString ( 
      string                          s, 
      [NotNullWhen(true)] out object? value 
    ) {
      value = null ;
      object? result = null ;
      switch ( DbFieldType )
      {
      case DbFieldType.DBF_STRING_s39:
        // THIS SUBSTITUTION WILL ALREADY HAVE BEEN DONE !!!
        // If '' is passed as the string value,
        // we'll strip those (thereby allowing an empty string)
        // result = s.Trim('\'') ;
        result = s ;
        break ;
      case DbFieldType.DBF_SHORT_i16:  
        s?.CanParseAs<short>( 
          parseSucceededAction : parsedValue => result = parsedValue
        ) ;
        break ;
      case DbFieldType.DBF_FLOAT_f32:  
        s?.CanParseAs<float>( 
          parseSucceededAction : parsedValue => result = parsedValue
        ) ;
        break ;
      case DbFieldType.DBF_ENUM_i16:   
        s?.CanParseAs<short>( 
          parseSucceededAction : parsedValue => result = parsedValue
        ) ;
        break ;
      case DbFieldType.DBF_CHAR_byte:  
        s?.CanParseAs<byte>( 
          parseSucceededAction : parsedValue => result = parsedValue
        ) ;
        break ;
      case DbFieldType.DBF_LONG_i32:   
        s?.CanParseAs<int>( 
          parseSucceededAction : parsedValue => result = parsedValue
        ) ;
        break ;
      case DbFieldType.DBF_DOUBLE_f64: 
        s?.CanParseAs<double>(
          parseSucceededAction : parsedValue => result = parsedValue
        ) ;
        break ;
      default:
        throw DbFieldType.AsUnexpectedEnumValueException() ;
      }
      value = result ;
      return value != null ;
    }

    //
    // Suppose that the type is declared to have 8 elements on the server.
    //
    // We can define the value to be written, in various ways as follows :
    //  
    //   [ 1 2 3 4 5 6 7 8 ]   Write all 8 elements.
    //
    //   [ 1 2 3 ]             Write just the first 3 elements.
    //                         The remaining elements don't get written 
    //                         and will probably be set to zero in the IOC ???
    //
    //   [ 1 2 3 ... ]         Write 1 2 3 ; 1 2 3 ; 1 2 ie with cyclic repeat
    //
    //   [ 1 ... ]             Write 1 ; 1 ; 1 ; 1 ; 1 ; 1 ; 1 ; 1 ie with cyclic repeat
    //
    // The '[]' brackets are optional and can be omitted.
    // The separator can be either a space, or a comma or a semicolon.

    private bool TryCreateCompatibleVectorValueFromString ( 
      string                          s, 
      [NotNullWhen(true)] out object? value 
    ) {
      // Cheap and cheerful but adequate for the purpose ...
      // The handling of 'string' values is trivial,
      // it doesn't accommodate strings with spaces !
      value = null ;
      s = s.Trim().Replace(',',' ').Replace(';',' ') ;
      if ( s.StartsWith('[') )
      {
        s = s.Trim('[',']') ;
      }
      bool fillMissingElements ;
      if ( s.EndsWith(" ...") )
      {
        s = s.Substring(0,s.Length-4) ;
        fillMissingElements = true ;
      }
      else
      {
        fillMissingElements = false ;
      }
      // We assume that the array is represented like this ...
      //  1 2 3 4
      // ... and create an object with that many elements
      IEnumerable<string> valuesAsStrings = s.Split(' ') ;
      System.Array arrayOfValues ;
      switch ( DbFieldType )
      {
      case DbFieldType.DBF_STRING_s39:
        arrayOfValues = valuesAsStrings.Select(
          s => s.Replace("_"," ") 
        ).ToArray() ;
        break ;
      case DbFieldType.DBF_SHORT_i16:  
        arrayOfValues = valuesAsStrings.Select(
          element => element.ParsedAs<short>()
        ).ToArray() ;
        break ;
      case DbFieldType.DBF_FLOAT_f32:  
        arrayOfValues = valuesAsStrings.Select(
          element => element.ParsedAs<float>()
        ).ToArray() ;
        break ;
      case DbFieldType.DBF_ENUM_i16:   
        // Hmm, not allowed ???
        arrayOfValues = valuesAsStrings.Select(
          element => element.ParsedAs<short>()
        ).ToArray() ;
        break ;
      case DbFieldType.DBF_CHAR_byte:  
        arrayOfValues = valuesAsStrings.Select(
          element => element.ParsedAs<byte>()
        ).ToArray() ;
        break ;
      case DbFieldType.DBF_LONG_i32:   
        arrayOfValues = valuesAsStrings.Select(
          element => element.ParsedAs<int>()
        ).ToArray() ;
        break ;
      case DbFieldType.DBF_DOUBLE_f64: 
        arrayOfValues = valuesAsStrings.Select(
          element => element.ParsedAs<double>()
        ).ToArray() ;
        break ;
      default:
        throw DbFieldType.AsUnexpectedEnumValueException() ;
      }
      if ( fillMissingElements )
      {
        arrayOfValues = Clf.Common.Helpers.CreateExpandedArrayOfObjects(
          arrayOfValues,
          ElementsCountOnServer
        ) ;
      }
      value = arrayOfValues ;
      return value != null ;
    }

    // This code is currently located in the LogicSystem,
    // and is tuned for Value Types representing Computed Nodes.
    // Might be more generally useful though so copied here as well.
    // public static bool CanInferDbFieldDescriptorFromValueType ( 
    //   System.Type                                valueType,
    //   [NotNullWhen(true)] out DbFieldDescriptor? dbFieldDescriptor
    // ) {
    //   DbFieldType? dbFieldType ; 
    //   if ( valueType.IsEnum ) 
    //   {
    //     dbFieldType = DbFieldType.DBF_ENUM_i16 ;
    //   }
    //   else 
    //   {
    //     dbFieldType = valueType switch
    //     {
    //     System.Type when valueType == typeof(string) => DbFieldType.DBF_STRING_s39,
    //     System.Type when valueType == typeof(short)  => DbFieldType.DBF_SHORT_i16,
    //     System.Type when valueType == typeof(float)  => DbFieldType.DBF_FLOAT_f32,
    //     System.Type when valueType == typeof(char)   => DbFieldType.DBF_CHAR_byte,
    //     System.Type when valueType == typeof(int)    => DbFieldType.DBF_LONG_i32,
    //     System.Type when valueType == typeof(double) => DbFieldType.DBF_DOUBLE_f64,
    //     System.Type when valueType == typeof(bool)   => DbFieldType.DBF_SHORT_i16,
    //     _ => null
    //     } ;
    //   }
    //   if ( dbFieldType == null ) 
    //   {
    //     dbFieldDescriptor = null ;
    //   }
    //   else
    //   {
    //     dbFieldDescriptor = new Clf.ChannelAccess.DbFieldDescriptor(
    //       DbFieldType           : dbFieldType.Value,
    //       ElementsCountOnServer : 1,
    //       IsWriteable           : true
    //     ) ;
    //     if ( dbFieldType == Clf.ChannelAccess.DbFieldType.DBF_ENUM_i16 )
    //     {
    //       dbFieldDescriptor.SetEnumNames(
    //         System.Enum.GetNames(valueType)
    //       ) ;
    //     }
    //   }
    //   return dbFieldDescriptor != null ;
    // }

  }

}
