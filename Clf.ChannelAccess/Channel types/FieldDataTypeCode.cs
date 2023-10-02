//
// FieldDataTypeCode.cs
//

namespace Clf.ChannelAccess
{

  //
  // These are the types exposed in the 'Channel' API.
  //
  // Note that these field types (apart from 'Enum') are exchanged as ARRAYS of values.
  //
  // You can have 'strings' that are longer than 39 characters (in fact, up to 16383?)
  // but these are physically represented as arrays of DBF_CHAR ie 'byte'. 
  //
  // Hmm, we'll want a set of distinguishable options for an ENUM,
  // but ideally we'd like to parameterise the 'Enum' ...
  //

  internal enum FieldDataTypeCode {
    [Clf.Common.AssociatedType( null               )] Unknown, 
    [Clf.Common.AssociatedType( typeof( byte     ) )] UByte8,  
    [Clf.Common.AssociatedType( typeof( short    ) )] Int16,        
    [Clf.Common.AssociatedType( typeof( int      ) )] Int32,        
    [Clf.Common.AssociatedType( typeof( float    ) )] Float,      
    [Clf.Common.AssociatedType( typeof( double   ) )] Double, 
    [Clf.Common.AssociatedType( typeof( short    ) )] Enum,
    [Clf.Common.AssociatedType( typeof( string   ) )] ShortAsciiString, 
    [Clf.Common.AssociatedType( typeof( byte[]   ) )] ArrayOfUByte8,       
    [Clf.Common.AssociatedType( typeof( short[]  ) )] ArrayOfInt16,        
    [Clf.Common.AssociatedType( typeof( int[]    ) )] ArrayOfInt32,        
    [Clf.Common.AssociatedType( typeof( float[]  ) )] ArrayOfFloat,      
    [Clf.Common.AssociatedType( typeof( double[] ) )] ArrayOfDouble, 
    [Clf.Common.AssociatedType( typeof( string[] ) )] ArrayOfShortAsciiString 
    // [AssociatedType( typeof(char)                )] AsciiChar,
    // [AssociatedType( typeof(string)              )] LongAsciiString,
    // [AssociatedType( typeof(char[])              )] ArrayOfAsciiChar,      
    // [AssociatedType( typeof(IEnumerable<byte>)   )] ArrayOfUByte8,       
    // [AssociatedType( typeof(IEnumerable<char>)   )] ArrayOfAsciiChar,      
    // [AssociatedType( typeof(IEnumerable<short>)  )] ArrayOfInt16,        
    // [AssociatedType( typeof(IEnumerable<int>)    )] ArrayOfInt32,        
    // [AssociatedType( typeof(IEnumerable<float>)  )] ArrayOfFloat,      
    // [AssociatedType( typeof(IEnumerable<double>) )] ArrayOfDouble, 
    // [AssociatedType( typeof(IEnumerable<string>) )] ArrayOfShortAsciiString 
  }

}
