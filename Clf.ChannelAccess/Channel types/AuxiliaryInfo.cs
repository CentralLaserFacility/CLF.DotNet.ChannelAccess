//
// AuxiliaryInfo.cs
//

using Clf.Common.ExtensionMethods ;

namespace Clf.ChannelAccess
{

  // Make these 'null' if NaN, or if both zero ?
  // Hmm, maybe later, not for the time being ...
  
  /// <summary>
  /// 
  /// </summary>
  /// <param name="EGU">
  /// Engineering Units
  /// </param>
  /// <param name="PREC">
  /// Display Precision
  /// </param>
  /// <param name="HOPR">
  /// High Operating Range Limit
  /// </param>
  /// <param name="LOPR">
  /// Low Operating Range Limit
  /// </param>
  /// <param name="HIHI">
  /// Upper Alarm Limit
  /// </param>
  /// <param name="HIGH">
  /// Upper Warning Limit
  /// </param>
  /// <param name="LOW">
  /// Lower Warning Limit
  /// </param>
  /// <param name="LOLO">
  /// Lower Alarm Limit
  /// </param>
  /// <param name="DRVH">
  /// Drive High Limit
  /// </param>
  /// <param name="DRVL">
  /// Drive Low Limit
  /// </param>
  public record AuxiliaryInfo (
    string  EGU,         // Units of measure (up to 7 characters)
    int?    PREC = null, // Precision with which to display a floating point Value
    object? HOPR = null, // Display limits
    object? LOPR = null,
    object? HIHI = null, // Alarm limits ...
    object? HIGH = null, // Note that these often come over as NaN !!!
    object? LOW  = null, // https://epics.anl.gov/tech-talk/2019/msg00365.php
    object? LOLO = null,
    object? DRVH = null, // Control limits
    object? DRVL = null
  ) {

    public override string ToString ( ) => (
      $"EGU={EGU} PREC={PREC??-1} DRVL={DRVL:F2}" 
    ) ;

    public void RenderAsStrings ( System.Action<string> writeLine )
    {
      writeLine("Auxiliary values :") ;
      WriteLineIfNonNull( "  EngineeringUnits        EGU  " , EGU ) ;
      WriteLineIfNonNull( "  DisplayPrecision        PREC " , PREC ) ;
      WriteLineIfNonNull( "  HighOperatingRangeLimit HOPR " , HOPR ) ;
      WriteLineIfNonNull( "  LowOperatingRangeLimit  LOPR " , LOPR ) ;
      WriteLineIfNonNull( "  UpperAlarmLimit         HIHI " , HIHI ) ;
      WriteLineIfNonNull( "  UpperWarningLimit       HIGH " , HIGH ) ;
      WriteLineIfNonNull( "  LowerWarningLimit       LOW  " , LOW ) ;
      WriteLineIfNonNull( "  LowerAlarmLimit         LOLO " , LOLO ) ;
      WriteLineIfNonNull( "  DriveHighLimit          DRVH " , DRVH ) ;
      WriteLineIfNonNull( "  DriveLowLimit           DRVL " , DRVL ) ;
      void WriteLineIfNonNull ( string label, object? item )
      {
        if ( item != null )
        {
          writeLine($"{label} : {item}") ;
        }
      }
    }

    public object? this [ WhichAuxiliaryValue whichAuxiliaryValue ]
    => whichAuxiliaryValue switch {
      WhichAuxiliaryValue.EGU  => EGU,
      WhichAuxiliaryValue.PREC => PREC,
      WhichAuxiliaryValue.HOPR => HOPR,
      WhichAuxiliaryValue.LOPR => LOPR,
      WhichAuxiliaryValue.HIHI => HIHI,
      WhichAuxiliaryValue.HIGH => HIGH,
      WhichAuxiliaryValue.LOW  => LOW,
      WhichAuxiliaryValue.LOLO => LOLO,
      WhichAuxiliaryValue.DRVH => DRVH,
      WhichAuxiliaryValue.DRVL => DRVL,
      _ => throw whichAuxiliaryValue.AsUnexpectedEnumValueException()
    } ;

  }

}
