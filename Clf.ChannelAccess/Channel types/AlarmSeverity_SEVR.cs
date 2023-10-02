//
// AlarmSeverity.cs
//

namespace Clf.ChannelAccess
{

  public enum AlarmSeverity_SEVR { 
    NoAlarm           = LowLevelApi.ApiConstants.NO_ALARM,      // 0
    MinorAlarm        = LowLevelApi.ApiConstants.MINOR_ALARM,   // 1
    MajorAlarm        = LowLevelApi.ApiConstants.MAJOR_ALARM,   // 2
    InvalidValueAlarm = LowLevelApi.ApiConstants.INVALID_ALARM, // 3
  } ;

}
