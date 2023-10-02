//
// InternalHelpers.cs
//

using System.Collections.Generic ;
using FluentAssertions ;

namespace Clf.ChannelAccess
{

  internal static partial class InternalHelpers
  {

    public static FieldCategory GetFieldCategory (
      ValidatedChannelName channelName, 
      DbFieldType          fieldType
    ) {
      if ( channelName.IdentifiesValField )
      {
        if ( fieldType is DbFieldType.DBF_ENUM_i16 )
        {
          return new EnumValField() ;
        }
        else
        {
          return new ValField() ;
        }
      }
      else
      {
        return new OtherField() ;
      }
    }

    private static unsafe object? GetStringsArrayPayload ( byte * pFirstPayloadElement, int nStringElements )
    {
      if ( pFirstPayloadElement is null)
      {
        return null ;
      }
      var result = new string[nStringElements] ;
      int nTotalExpectedBytesInPayload = nStringElements*40 ;
      byte[] bytes = new byte[nTotalExpectedBytesInPayload] ;
      for ( int iByte = 0 ; iByte < nTotalExpectedBytesInPayload ; iByte++ )
      {
        bytes[iByte] = *pFirstPayloadElement++ ;
      }
      for ( int iString = 0 ; iString < nStringElements ; iString++ )
      {
        // Start at index 0,40,80 etc, and count the number of
        // valid bytes, ie scanning until we reach a zero byte.
        int iFirstByteInCurrentString = iString * 40 ;
        int nBytesInCurrentString = 0 ;
        int iByte = iFirstByteInCurrentString ;
        for(;;)
        {
          byte ch = bytes[iByte++] ;
          if ( ch == '\0' )
          {
            break ;
          }
          nBytesInCurrentString++ ;
        }
        string s = System.Text.Encoding.ASCII.GetString(
          bytes,
          iFirstByteInCurrentString,
          nBytesInCurrentString
        ) ;
        s.Length.Should().BeLessOrEqualTo(39) ;
        result[iString] = s ;
      }
      return result ;
    }

    //
    // Neat idea ?
    //
    // public static bool SpecifiesLocalChannel ( 
    //   this ChannnelName                         channelName, 
    //   [NotNullWhen(true)] out RecordDescriptor? recordDescriptor
    // ) {
    //   // local(i16[4]=1,2,3,4):restOfChannelName
    // }
    //

    internal static System.DateTime? ConvertEpicsTimeStamp ( LowLevelApi.EpicsTimeStamp epicsTimeStamp )
    {
      if ( epicsTimeStamp.secPastEpoch == 0 )
      {
        return null ;
      }
      else
      {
        // Hmm, this seems to be an hour out (1400 at 3pm, with British Summer Time)
        // regardless of whether we use Utc or Local ...
        // Presumably Epics is publishing time as GMT ??
        // Yes, that would explain the discrepancy.
        // GMT is UTC + zero offset.
        // --------------------------------
        // Coordinated Universal Time (UTC) is a high-precision, atomic time standard.
        // The world's time zones are expressed as positive or negative offsets from UTC.
        // Thus, UTC provides a kind of time-zone free or time-zone neutral time.
        // The use of UTC time is recommended when a date and time's portability
        // across computers is important.
        System.DateTime epicsEpoch = new System.DateTime(
          1990,
          1,
          1,
          0,
          0,
          0,
          System.DateTimeKind.Utc
        ).AddSeconds(epicsTimeStamp.secPastEpoch) ;
        return epicsEpoch.AddMilliseconds(
          epicsTimeStamp.nsec / 1000_000.0
        ) ;
      }
    }

    // Where 'x' is an enumerated type, this overload will be chosen
    // in preference to the one that takes an 'object'. Typically it's used
    // in a switch statement that is intended to have a specific clause
    // for all the expected values, and if the 'default' case is taken
    // it means we've got a coding error.

    internal static UnexpectedConditionException AsUnexpectedEnumValueException<T> ( 
      this T x 
    ) where T : System.Enum
    => new UnexpectedConditionException(
      $"Unexpected enum value {x}"
    ) ;

    internal static UnexpectedConditionException AsUnexpectedValueException ( 
      this object x 
    ) => new UnexpectedConditionException(
      $"Unexpected value {x}"
    ) ;

    internal static string GetChannelValueSummaryAsFriendlyString ( 
      this object? pvValue, 
      int          maxArrayValuesToShow = 4 
    ) {
      if ( pvValue is null )
      {
        return "null" ;
      }
      // else if ( pvValue is System.Exception exception )
      // {
      //   return $"EXCEPTION : {exception.Message}" ;
      // }
      if ( pvValue is System.Array array )
      {
        var arrayValuesAsStrings = new List<string>() ;
        foreach ( object element in array )
        {
          arrayValuesAsStrings.Add(
            ToStringWithQuotesIfNecessary(element)
          ) ;
          if ( 
             arrayValuesAsStrings.Count == maxArrayValuesToShow 
          && array.Length > arrayValuesAsStrings.Count
          ) {
            // We've added the maximum number of values,
            // and there are still more to add,
            // so indicate there are more ...
            arrayValuesAsStrings.Add("...") ;
            break ;
          }
        }
        return (
          $"({array.Length} elements) : "
        + string.Join(' ',arrayValuesAsStrings) 
        ) ;
      }
      else
      {
        return ToStringWithQuotesIfNecessary(pvValue) ;
      }
      static string ToStringWithQuotesIfNecessary ( object x )
      => ( 
        x is string s 
        ? $"'{s}'"
        : x?.ToString() ?? "null"
      ) ;
    }

    public static string AsYesOrNo ( this bool x ) => x ? "yes" : "no" ;

    public static string WithExplanation ( this string status, string? explanation )
    => (
      explanation is null
      ? status
      : status + " : " + explanation
    ) ;

    public static bool ContainsOnly ( this string s, string validCharacters )
    {
      foreach ( char ch in s )
      {
        if ( ! ch.IsValidCharacter(validCharacters) )
        {
          return false ;
        }
      }
      return true ;
    }

    public static bool IsValidCharacter ( this char ch, string validCharacters )
    => (
      validCharacters.Contains(ch)
    ) ;

    public static ValueAccessMode DefaultValueAccessMode (
      this ValidatedChannelName channelName
    ) => (
      channelName.IdentifiesValField
      ? ValueAccessMode.DBR_CTRL_RequestValueAndAuxiliaryInfo
      : ValueAccessMode.DBR_RequestValueAndNothingElse
    ) ;

    //
    // For a given PV name, we can have more than one Channel,
    // eg one reporting CTRL, another reporting TIME info ...
    //
    // ChannelName might or might not be valid
    // If valid :
    //   .VAL => default mode is 'CTRL' ; 'TIME' and 'NONE' also possible
    //   .XXX => only the 'NONE' mode is permitted
    // If not valid :
    //   Might be an empty string
    //   Might be a very long string
    //   Might contain wierd characters
    //   Access mode not relevant
    //

    internal static string GetChannelNameAndAccessModeAsString (
      ChannelName     channelName,
      ValueAccessMode valueAccessMode
    ) => (
      $"{channelName}${valueAccessMode.AsString()}"
    ) ;

  }

}
