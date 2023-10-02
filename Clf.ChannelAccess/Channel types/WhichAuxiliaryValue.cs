//
// WhichAuxiliaryValue.cs
//

namespace Clf.ChannelAccess
{
  
  /// <summary>
  /// TODO_XML_DOCS
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
  public enum WhichAuxiliaryValue {
    EGU,
    PREC,
    HOPR,
    LOPR,
    HIHI,
    HIGH,
    LOW,
    LOLO,
    DRVH,
    DRVL           
  }

}
