//
// ValueAccessMode.cs
//

namespace Clf.ChannelAccess
{

  /// <summary>
  /// Determines what information we'll request from the Channel
  /// when we access the VAL field. 
  /// <br></br>
  /// Note that a channel that specifies a field other than 'VAL' will only ever obtain
  /// the Value, even if specifies a mode of something other than 'RequestValueAndNothingElse'.
  /// In that case a warning will be written to the log.
  /// </summary>

  public enum ValueAccessMode {
    DBR_RequestValueAndNothingElse,
    DBR_CTRL_RequestValueAndAuxiliaryInfo, 
    DBR_TIME_RequestValueAndServerTimeStamp,
    // Default = DBR_CTRL_RequestValueAndAuxiliaryInfo
    // We could implement these as well ...
    // DBR_GR_RequestValueAndAuxiliaryInfo, 
    // DBR_STS_RequestValueAndStatus,
  }

  public static class ValueAccessModeExtensions
  {
    public static string AsString ( this ValueAccessMode valueAccessMode )
    => valueAccessMode switch {
    ValueAccessMode.DBR_RequestValueAndNothingElse          => "ONLY",
    ValueAccessMode.DBR_CTRL_RequestValueAndAuxiliaryInfo   => "CTRL", 
    ValueAccessMode.DBR_TIME_RequestValueAndServerTimeStamp => "TIME",
    _ => throw valueAccessMode.AsUnexpectedEnumValueException()
    } ;
  }

}

