//
// AlarmStatus_STAT.cs
//

namespace Clf.ChannelAccess
{

  //
  // These integer values are returned from the STAT field.
  //
  // Note that the STAT value refers only to the VAL field.
  //

  public enum AlarmStatus_STAT {
    NoAlarm     =  0, 
    Read        =  1, // ???
    Write       =  2, // ???
    HiHi        =  3, // Useful 
    High        =  4, // Useful 
    LoLo        =  5, // Useful 
    Low         =  6, // Useful 
    State       =  7,
    Cos         =  8,
    Comm        =  9,
    Timeout     = 10, // Useful ??? 
    HwLimit     = 11,
    Calc        = 12,
    Scan        = 13,
    Link        = 14,
    Soft        = 15,
    BadSub      = 16,
    Udf         = 17, // Undefined value, not yet initialised
    Disable     = 18,
    Simm        = 19,
    ReadAccess  = 20, // ??? 
    WriteAccess = 21  // ??? 
  } ;

}
