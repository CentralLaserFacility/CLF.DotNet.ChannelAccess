//
// ValueInfo.cs
//

using System.Diagnostics.CodeAnalysis;
using Clf.Common.ExtensionMethods;
using System;

namespace Clf.ChannelAccess
{

  //
  // Note that 'Value' is either a reference type such as
  // a string on an array, or a 'boxed' representation of a numeric
  // value type such as an int or a double. It is never 'null',
  // as it's impossible for Channel Access to return a null value.
  //

  public record ValueInfo ( 
    IChannel                Channel,
    object                  Value,
    FieldInfo               FieldInfo,
    AlarmStatusAndSeverity? AlarmStatusAndSeverity = null,
    AuxiliaryInfo?          AuxiliaryInfo          = null,
    System.DateTime?        TimeStampFromServer    = null
  ) {

    // Internal because ...
    internal object ValueAsObject => Value ;

    public void RenderAsStrings ( 
      System.Action<string> writeLine, 
      bool                  showAuxiliaryValues = true
    ) {
      if ( showAuxiliaryValues )
      {
        writeLine($"Value etc :") ;
        writeLine(
          $"  Value ({this.FieldInfo.DbFieldDescriptor.FieldTypeAsString}) is {
            Value_AsDisplayString(
              whichValueInfoElementsToInclude : WhichValueInfoElementsToInclude.Value
            )
          }"
        ) ;
        switch ( Channel.ValueAccessMode )
        {
        case ValueAccessMode.DBR_RequestValueAndNothingElse:
          break ;
        case ValueAccessMode.DBR_CTRL_RequestValueAndAuxiliaryInfo:
          AlarmStatusAndSeverity?.RenderAsStrings(writeLine) ; 
          AuxiliaryInfo?.RenderAsStrings(writeLine) ; 
          break ;
        case ValueAccessMode.DBR_TIME_RequestValueAndServerTimeStamp:
          AlarmStatusAndSeverity?.RenderAsStrings(writeLine) ; 
          writeLine(
            $"Time stamp (from server) :"
          ) ;
          writeLine(
            $"  {TimeStampFromServer_AsString}"
          ) ;
          break ;
        default:
          throw Channel.ValueAccessMode.AsUnexpectedEnumValueException() ;
        }
        FieldInfo.RenderAsStrings(writeLine) ; 
      }
      else
      {
        writeLine(
          $"Value is {
            Value_AsDisplayString(
              WhichValueInfoElementsToInclude.AllAvailableElements
            )
          }"
        ) ;
      }
    }

    // Cast the Value to the specified type T.

    public T ValueAs<T> ( ) 
    {
      return (T) ValueAsObject ;
    }

    // Convert the Value to a string.
    // This can only give a meaningful result if the Value is a scalar value
    // such as a single numeric value or a string. If the Value is an array,
    // we return a string that contains a representation of the first few elements.

    public string ValueAsString ( )
    {
      // return Value_AsDisplayString(
      //   WhichValueInfoElementsToInclude.Value
      // ) ;
      return (
        ValueAsObject is System.Array array
        ? Value_AsDisplayString()
        : ValueAsObject.ToString()!
      ) ;
    }

    // Hmm, to get a 'complete' string representation of the value we need :
    //  - Value as a string
    //  - Elements count, if the object is an array
    //  - Option name, for an enum
    //  - Time stamp from server

    public string Value_AsDisplayString ( 
      WhichValueInfoElementsToInclude? whichValueInfoElementsToInclude = null
    ) {
      whichValueInfoElementsToInclude ??= WhichValueInfoElementsToInclude.Value ;
      // We'll add strings to this 'elementsList', and create the final result
      // by concatenating the elements with a separating space character.
      System.Collections.Generic.List<string> elementsList = new() ;
      if ( 
        whichValueInfoElementsToInclude.Value.HasFlag(
          WhichValueInfoElementsToInclude.Value
        )
      ) {
        elementsList.Add(
          InternalHelpers.GetChannelValueSummaryAsFriendlyString(
            this.ValueAsObject
          )
        ) ;
      }
      if ( 
        whichValueInfoElementsToInclude.Value.HasFlag(
          WhichValueInfoElementsToInclude.AlarmStatus
        )
      ) {
        if ( this.AlarmStatusAndSeverity is null )
        {
          elementsList.Add(
            "(??AlarmStatus??)"
          ) ;
        }
        else
        {
          elementsList.Add(
            $"({this.AlarmStatusAndSeverity.AlarmStatus_STAT})"
          ) ;
        }
      }
      if (
         Channel.ValueAccessMode is ValueAccessMode.DBR_TIME_RequestValueAndServerTimeStamp
      && whichValueInfoElementsToInclude.Value.HasFlag(
           WhichValueInfoElementsToInclude.TimeStampFromServer
         )
      ) {
        elementsList.Add(
          $"(ServerTimeStamp={TimeStampFromServer_AsString})"
        ) ;
      }
      if ( 
         this.FieldInfo.DbFieldDescriptor.IsEnumField()
      && whichValueInfoElementsToInclude.Value.HasFlag(
           WhichValueInfoElementsToInclude.EnumOptionName
         )
      ) {
        if ( this.ValueAsObject is short enumOption )
        {
          string? optionNameToShow = this.FieldInfo.DbFieldDescriptor.EnumNameAsString(
            enumOption
          ) ;
          elementsList.Add(
            optionNameToShow == null
            ? $" (??Option_#{enumOption}??)"
            : $" ('{optionNameToShow}')"
          ) ;
        }
      }
      return string.Join(
        ' ',
        elementsList.ToArray() 
      ) ;
    }

    public bool TryGetValueAsEnumString ( 
      [NotNullWhen(true)] out string? enumString
    ) {
      enumString = null ;
      if ( FieldInfo?.FieldDataTypeCode == FieldDataTypeCode.Enum )
      {
        short enumValue = (short)ValueAsObject;
        enumString = FieldInfo.DbFieldDescriptor.EnumNameAsString(enumValue) ;
      }
      return enumString != null ;
    }

    private string EnumOptionNameIfAppropriate_InBrackets // ??????????????
    {
      get 
      {
        if ( ValueAsObject is short enumOption )
        {
          string? optionName = FieldInfo.DbFieldDescriptor.EnumNameAsString(
            enumOption
          ) ;
          return (
            optionName == null
            ? $" (??Option_#{enumOption}??)"
            : $" ('{optionName}')"
          ) ;
        }
        else
        {
          return "" ;
        }
      }
    }

    private string AlarmStatusIfAvailable => (
      AlarmStatusAndSeverity != null 
      // && AlarmStatusAndSeverity.AlarmStatus_STAT != ChannelAccessApi.AlarmStatus_STAT.NoAlarm
      ? $" ({(AlarmStatusAndSeverity.AlarmStatus_STAT)})"
      : ""
    ) ;

    public string TimeStampFromServer_AsString => (
      TimeStampFromServer.HasValue
      ? TimeStampFromServer.Value.ToString(
          Settings.DefaultFormatForServerTimeStampDisplay
        )
      : "NONE"
    ) ;

    public string GetTimeStampFromServer_AsString ( string? format = null ) 
    => (
      TimeStampFromServer?.ToString(
        format ?? Settings.DefaultFormatForServerTimeStampDisplay
      ) ?? "NONE"
    ) ;

    public System.DateTime LocalTimeStamp { get ; } = System.DateTime.Now ; // Yuk, nasty hack, should get this from a service !!
  
  } ;

}
