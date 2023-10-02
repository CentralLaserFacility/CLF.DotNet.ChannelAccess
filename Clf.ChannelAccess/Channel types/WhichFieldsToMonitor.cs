//
// WhichFieldsToMonitor.cs
//

namespace Clf.ChannelAccess
{

  [System.Flags]
  public enum WhichFieldsToMonitor {
    MonitorValField    = LowLevelApi.ApiConstants.DBE_VALUE,   
    MonitorOtherFields = LowLevelApi.ApiConstants.DBE_PROPERTY,
    MonitorAlarmFields = LowLevelApi.ApiConstants.DBE_ALARM,   
    MonitorLogFields   = LowLevelApi.ApiConstants.DBE_LOG,     
    MonitorAllFields   = (
      MonitorValField 
    | MonitorOtherFields 
    | MonitorAlarmFields 
    | MonitorLogFields 
    )
  }

}
