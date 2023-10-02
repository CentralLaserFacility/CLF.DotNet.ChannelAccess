//
// DbrRequestedInfoCategory.cs
//

namespace Clf.ChannelAccess.LowLevelApi ;

/// <summary>
/// Summarises the additional info we're requesting along with the Value ; eg CTRL, TIME and so on. 
/// Note that 'additional info' is available only if thw field we're accessing is a VAL field.
/// </summary>

internal enum DbrRequestedInfoCategory {
  DBR_valueOnly,       // Value only (for a non-'VAL' field)
  DBR_STS,             // Value and status only
  DBR_TIME,            // Value and status, plus 'stamp'
  DBR_GR,              // ... plus 'graphics' info
  DBR_CTRL,            // ... plus 'control' info
  DBR_PUT_ACKT,
  DBR_PUT_ACKS,
  DBR_STSACK_STRING,
  DBR_CLASS_NAME,      // Get the 'record' name
}

