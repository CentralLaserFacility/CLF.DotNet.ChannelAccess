//
// ChannelDescriptor_T.cs
// 

using System.Collections.Generic ;

namespace Clf.ChannelAccess
{

  //
  // Hmm, is this useful ?
  //
  // It will be more so when we have proper 'type conversions' ...
  //

  public record ChannelDescriptor<T> (
    Clf.ChannelAccess.ChannelName ChannelName,
    string?                       InitialValueAsString,        // Needs to be able to be an object of type T !!!
    int                           ElementsCount = 1,
    string?                       Description   = null,
    string[]?                     EnumValues    = null
  ) : ChannelDescriptor(
    ChannelName, 
    DbFieldDescriptor.CreateWithEnumValues(
      dbFieldType   : Clf.ChannelAccess.Helpers.GetDbFieldTypeRepresentingSystemType(typeof(T)),
      elementsCount : ElementsCount,
      isWriteable   : true,
      enumValues    : EnumValues
    ),
    InitialValueAsString,
    Description
  ) ;

}