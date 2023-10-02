//
// Structs.Validation.cs
//

using System ;
using System.Runtime.InteropServices ;
using System.Collections.Generic;
using System.Linq ;
using Clf.Common.ExtensionMethods ;
using FluentAssertions ;

namespace Clf.ChannelAccess.LowLevelApi
{

  internal partial class ApiWrapper
  {

    public static int GetOffsetOfField<TStruct> ( string fieldName )
    => Marshal.OffsetOf<TStruct>(fieldName).ToInt32() ;

    public static int GetOffsetOfField ( Type type, string fieldName )
    => Marshal.OffsetOf(type,fieldName).ToInt32() ;

    public static IEnumerable<string> GetFieldNamesOfStruct<TStruct> ( )
    => (
      typeof(TStruct).GetFields().Select(
        fieldInfo => fieldInfo.Name
      ) 
    ) ;

    public class OriginalStructTypeAttribute : System.Attribute
    {
      public readonly Type OriginalStructType ;
      public OriginalStructTypeAttribute ( Type originalStructType )
      {
        OriginalStructType = originalStructType ;
      }
    }

    public class OriginalNameAttribute : System.Attribute
    {
      public readonly string OriginalName ;
      public OriginalNameAttribute ( string originalName )
      {
        OriginalName = originalName ;
      }
    }

    public static Type GetTypeOfOriginalStruct<TStruct> ( )
    => (
      typeof(TStruct).GetCustomAttributes(
        inherit : false
      ).OfType<OriginalStructTypeAttribute>(
      ).Single(
      ).OriginalStructType 
    ) ;

    public static IEnumerable<(string name,string originalName)> GetFieldNamesAndOriginalNamesOfStruct<TStruct> ( )
    {
      return typeof(TStruct).GetFields().Where(
        fieldInfo => ! fieldInfo.IsLiteral
      ).Select(
        fieldInfo => {
          string name = fieldInfo.Name ;
          string originalName = (
            fieldInfo.GetCustomAttributes(
              inherit  :false
            ).OfType<OriginalNameAttribute>(
            ).SingleOrDefault(
            )?.OriginalName ?? name
          ) ;
          return (
            name         : name,
            originalName : name
          ) ;
        }
      ) ;
    }

    public static void ValidateStructUsingFieldNames<TApiWrapperStruct> ( )
    {
      var apiWrapperType             = typeof(TApiWrapperStruct) ;
      var originalStructType         = GetTypeOfOriginalStruct<TApiWrapperStruct>() ;
      var fieldNamesAndOriginalNames = GetFieldNamesAndOriginalNamesOfStruct<TApiWrapperStruct>() ;
      foreach ( 
        (
          string apiWrapperFieldName,
          string fieldNameInOrginalStruct
        ) in fieldNamesAndOriginalNames 
      ) {
        int offsetInApiWrapper     = GetOffsetOfField(apiWrapperType,apiWrapperFieldName) ;
        int offsetInOriginalStruct = GetOffsetOfField(originalStructType,fieldNameInOrginalStruct) ;
        offsetInApiWrapper.Should().Be(offsetInOriginalStruct) ;
      }
    }

    public static unsafe void ValidateAllStructsUsingFieldNames ( )
    {
      ValidateStructUsingFieldNames<ConnectionStatusChangedEventArgs>() ;
      ValidateStructUsingFieldNames<ValueUpdateNotificationEventArgs>() ;
      ValidateStructUsingFieldNames<EpicsTimeStamp>() ;
      ValidateStructUsingFieldNames<dbr_ctrl_int_i16>() ;
    }

    public static unsafe void ValidateStructUsingSize<TApiWrapperStruct> ( )
    {
      var apiWrapperType     = typeof(TApiWrapperStruct) ;
      var originalStructType = GetTypeOfOriginalStruct<TApiWrapperStruct>() ;
      var apiWrapperTypeSize = Marshal.SizeOf(apiWrapperType) ;
      var originalStructTypeSize = Marshal.SizeOf(originalStructType) ;
      apiWrapperTypeSize.Should().Be(originalStructTypeSize) ;
    }

    public static unsafe void ValidateAllStructsUsingSizes ( )
    {
      ValidateStructUsingSize<ConnectionStatusChangedEventArgs>() ;
      ValidateStructUsingSize<ValueUpdateNotificationEventArgs>() ;
      ValidateStructUsingSize<EpicsTimeStamp>() ;
      ValidateStructUsingSize<dbr_ctrl_int_i16>() ;
    }

  }

}
