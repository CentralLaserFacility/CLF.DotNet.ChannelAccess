//
// WhichValueInfoElementsToInclude.cs
//

namespace Clf.ChannelAccess
{

  [System.Flags]
  public enum WhichValueInfoElementsToInclude {
    Value               = 1 << 0,
    AlarmStatus         = 1 << 1,
    EnumOptionName      = 1 << 2,
    TimeStampFromServer = 1 << 3,
    AllAvailableElements = (
      Value
    | AlarmStatus
    | EnumOptionName
    | TimeStampFromServer
    ),
    Default = Value
  }

}
