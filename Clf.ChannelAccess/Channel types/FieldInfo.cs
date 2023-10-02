//
// FieldInfo.cs
//

using System.Diagnostics.CodeAnalysis ;

using Clf.Common.ExtensionMethods ;

namespace Clf.ChannelAccess
{

  // FieldInfo can have an 'internal' ValidatedChannelName mentioned in
  // an internal constructor, an expose that as a public ChannelName.
  // ValidatedChannelName is fine, because we'll only get non-null FieldInfo
  // if we've got a valid channel ie not an InvalidChannel ...

  public record FieldInfo ( 
    ValidatedChannelName ChannelName,
    string               HostIpAndPort,
    DbFieldDescriptor    DbFieldDescriptor
  ) {

    // Hmm, are these individual properties useful ???

    // Is it really worth cacheing this ??

    private FieldCategory? m_fieldCategory ;
    
    internal FieldCategory FieldCategory => (
      m_fieldCategory ??= InternalHelpers.GetFieldCategory(
        ChannelName, 
        DbFieldDescriptor.DbFieldType
      )
    ) ;

    // Is it really worth cacheing this ??

    private FieldDataTypeCode? m_fieldDataTypeCode = null ;

    internal FieldDataTypeCode FieldDataTypeCode => (
      m_fieldDataTypeCode ??= DbFieldDescriptor.GetFieldDataTypeCode() 
    ) ;
    
    public System.Type FieldDataType => DbFieldDescriptor.GetFieldDataType() ;

    // Hmm, we'd never expect a null value !!!

    internal object GetValueParsedFromString ( string s )
    {
      return s.ParsedAs(FieldDataType)! ;
    }

    public override string ToString ( ) 
    => (
        // $"{ChannelName} on {HostIpAndPort} ; {FieldTypeAsString} {(IsWriteable?"(read/write)":"(read-only)")}" 
        $"{ChannelName} on {HostIpAndPort} ; "
      + $"{DbFieldDescriptor.FieldTypeAsString}"
      + $"{(DbFieldDescriptor.IsWriteable?" (read/write)":" (read-only)")}" 
    ) ;

    public void RenderAsStrings ( System.Action<string> writeLine )
    {
      writeLine("FieldInfo :") ;
      writeLine($"  ChannelName   : {ChannelName}") ;
      writeLine($"  HostIpAndPort : {HostIpAndPort}") ;
      writeLine($"  FieldType     : {
          DbFieldDescriptor.FieldTypeAsString
        }{
          (
            DbFieldDescriptor.IsScalarValue 
            ? 
            " (scalar)" 
            : "" 
          )
        }{
          (
            DbFieldDescriptor.IsWriteable
            ? " (read/write)"
            : " (read-only)"
          )
        }"
      ) ;
      writeLine($"  DataType      : {FieldDataType.FullName}") ;
      DbFieldDescriptor.RenderOptionNamesAsStrings(writeLine) ; 
    }

    //
    // If the field is an ENUM, we can potentially access the string values ...
    //

    // public EnumFieldInfo? EnumFieldInfo { get ; internal set ; }
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
      return DbFieldDescriptor.TryGetEnumNameAsString(
        enumIndex,
        out enumOptionName
      ) ;    
    }

    // HMM, MESSY !!! Move to DbFieldDescriptor ?
    // Note : using [n] here for an array ...
    // THAT IS INCONSISTENT !!!

    // public string FieldTypeAsString => (
    //   DbFieldDescriptor.IsArray
    //   ? $"{DbFieldDescriptor.DbFieldType}[{DbFieldDescriptor.ElementsCountOnServer}]" 
    //   : $"{DbFieldDescriptor.DbFieldType}" 
    // ) ;

    internal static FieldInfo FromChannel ( System.IntPtr channel )
    {
      // Once we've connected to a Channel, can create this
      // by making appropriate 'ca_' queries ...
      throw new System.NotImplementedException () ;
    }

  }

}
