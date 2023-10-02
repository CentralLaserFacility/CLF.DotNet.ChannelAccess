//
// AlarmStatusAndSeverity.cs
//

namespace Clf.ChannelAccess
{

  public record AlarmStatusAndSeverity ( 
    AlarmStatus_STAT   AlarmStatus_STAT,
    AlarmSeverity_SEVR AlarmSeverity_SEVR
  ) {

    public override string ToString ( ) => (
      $"STAT = {AlarmStatus_STAT} ({(int)AlarmStatus_STAT}) ; SEVR = {AlarmSeverity_SEVR} ({(int)AlarmSeverity_SEVR})"
    ) ;

    public void RenderAsStrings ( System.Action<string> writeLine )
    {
      writeLine($"Alarm status and severity :") ;
      writeLine($"  STAT = {AlarmStatus_STAT} ({(int)AlarmStatus_STAT})") ;
      writeLine($"  SEVR = {AlarmSeverity_SEVR} ({(int)AlarmSeverity_SEVR})") ;
    }

  } ;

}
