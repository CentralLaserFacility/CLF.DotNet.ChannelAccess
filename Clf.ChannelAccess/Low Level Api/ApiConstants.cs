//
// ApiConstants.cs
//

namespace Clf.ChannelAccess.LowLevelApi
{

  internal class ApiConstants
  {

    // Use these flags in 'create_subscription' to specify
    // which changes cause an event to be raised.
    // !!! If you set the 'PROPERTY' flag : whenever any field changes,
    // a change message is issued for *all* fields that are being monitored.
    // See http://isacwserv.triumf.ca/epics09html/SA1/BaseR3.14.11.pdf page 7
    // This event will be triggered by the IOC whenever the properties (extended
    // attributes) of the channel change :
    //  Enum strings, Display and Alarm limits, Units string, Precision etc

    public const int DBE_VALUE    = 1 << 0 ;
    public const int DBE_LOG      = 1 << 1 ;
    public const int DBE_ALARM    = 1 << 2 ;
    public const int DBE_PROPERTY = 1 << 3 ; // Enable sub-properties monitoring (ie 'fields')

    // These are the values of the 'severity' field (SEVR) ...
    // See also 'AlarmSeverity.cs'

    public const int NO_ALARM      = 0 ;
    public const int MINOR_ALARM   = 1 ;
    public const int MAJOR_ALARM   = 2 ;
    public const int INVALID_ALARM = 3 ;

    // Severity codes, 0..4 packed into the lower 3 bits

    public const int CA_K_WARNING = 0 ; // Unsuccessful
    public const int CA_K_SUCCESS = 1 ; // Successful
    public const int CA_K_ERROR   = 2 ; // Recoverable failure
    public const int CA_K_INFO    = 3 ; // Successful
    public const int CA_K_SEVERE  = 4 ; // Non recoverable
    
    // Masks etc to do with ECA return codes

    public const int CA_M_SUCCESS  = 0x00000001 ; // Least significant bit
    public const int CA_M_SEVERITY = 0x00000007 ; // Lower 3 bits
    public const int CA_M_MSG_NO   = 0x0000FFF8 ; // Bits above the lower 3

    public const int CA_V_MSG_NO   = 3 ; // Shift left by 3 bits when 'or-ing' the message unmber
    public const int CA_V_SEVERITY = 0 ;
    public const int CA_V_SUCCESS  = 0 ;

    public const int ECA_NORMAL = 1 ;

    public const int ECA_TIMEOUT       = 42 ;
    public const int ECA_IODONE       = 42 ;
    public const int ECA_IOINPROGRESS = 43 ;


    // These are the base definitions for the dbRecord 'field' types,
    // as supplied in the 'type' fields of the API calls.

    // Various docs talk about a 40-character limit, but this presumably
    // includes a terminating null ??
    // Apparently the 40 limit is imposed by the CA protocol,
    // see http://isacwserv.triumf.ca/epics09html/SA1/BaseR3.14.11.pdf
    // String values should be able to be > 40 chars, if accessed via .ABC$

    // Hmm, NAME can be up to 61 chars, DESC up to 41 ???
    // https://epics.anl.gov/base/R3-15/8-docs/dbCommonRecord.html

    //
    // These are the SEVEN (docs say six??!!!) basic data types 
    // https://epics.anl.gov/base/R3-15/6-docs/CAproto/index.html#payload-data-types
    // Each of these types can represent an array of elements.
    // In addition to element values, some DBR types include meta-data.
    // These types are
    //   status (DBR_STS_*),
    //   time stamp (DBR_TIME_*),
    //   graphic (DBR_GR_*)
    //   control (DBR_CTRL_*).
    // All these structures contain value as the last field.
    //

    public const int DBF_STRING    = 0 ; // Up to 39 characters (not 40!)
    public const int DBF_SHORT     = 1 ; // 16 bit integer
    public const int DBF_FLOAT     = 2 ;
    public const int DBF_ENUM      = 3 ;
    public const int DBF_CHAR      = 4 ; // Char == Byte
    public const int DBF_LONG      = 5 ; // Long is 32 bit int
    public const int DBF_DOUBLE    = 6 ;

    public const int DBF_NO_ACCESS = 7 ; // Query failed ... ???

    // These codes are supplied as the 'pvValueType' in the 'ca_array_get'
    // and 'ca_array_put' functions. Note that these symbols are not actually used
    // in the code - the numbers are computed from the 'DBF_' code and
    // an enum value that identifies whether we want 'DBR_TIME_' or 'DBR_CTRL_'.

    // WOULD BE BETTER TO HAVE DEFINED THESE AS AN ENUM SO THAT WE RETAIN THE SYMBOL NAMES !!!

    public const int DBR_offset_zero = 0 ;          // Offset into DB record for DBR_XXX records with no 'header' fields
    public const int DBR_STRING      = DBF_STRING ; // 0
    public const int DBR_SHORT       = DBF_SHORT ;  // 1
    public const int DBR_FLOAT       = DBF_FLOAT ;  // 2
    public const int DBR_ENUM        = DBF_ENUM ;   // 3
    public const int DBR_CHAR        = DBF_CHAR ;   // 4
    public const int DBR_LONG        = DBF_LONG ;   // 5
    public const int DBR_DOUBLE      = DBF_DOUBLE ; // 6

    public const int DBR_STS_offset_7  = 7 ; // Offset into DB record for DBR_STS_XXX records
    public const int DBR_STS_STRING    = 7 ; // Record types with 'status' (status and severity, with value)
    public const int DBR_STS_SHORT     = 8 ;
    public const int DBR_STS_FLOAT     = 9 ;
    public const int DBR_STS_ENUM      = 10 ;
    public const int DBR_STS_CHAR      = 11 ;
    public const int DBR_STS_LONG      = 12 ;
    public const int DBR_STS_DOUBLE    = 13 ;

    public const int DBR_TIME_offset_14 = 14 ; // Offset into DB record for DBR_TIME_XXX records
    public const int DBR_TIME_STRING    = 14 ; // Record types with TimeStamp (status and severity and time-stamp, with value)
    public const int DBR_TIME_SHORT     = 15 ;
    public const int DBR_TIME_FLOAT     = 16 ;
    public const int DBR_TIME_ENUM      = 17 ;
    public const int DBR_TIME_CHAR      = 18 ;
    public const int DBR_TIME_LONG      = 19 ;
    public const int DBR_TIME_DOUBLE    = 20 ;

    public const int DBR_GR_offset_21   = 21 ; // Offset into DB record for DBR_GR_XXX records
    public const int DBR_GR_STRING      = 21 ; // Record types with Graphics info (incl units, and various limit fields)
    public const int DBR_GR_SHORT       = 22 ;
    public const int DBR_GR_FLOAT       = 23 ;
    public const int DBR_GR_ENUM        = 24 ;
    public const int DBR_GR_CHAR        = 25 ;
    public const int DBR_GR_LONG        = 26 ;
    public const int DBR_GR_DOUBLE      = 27 ;

    public const int DBR_CTRL_offset_28 = 28 ; // Offset into DB record for DBR_CTRL_XXX records
    public const int DBR_CTRL_STRING    = 28 ; // Record types with 'control' info (incl units, and various limit fields including control limits)
    public const int DBR_CTRL_SHORT     = 29 ;
    public const int DBR_CTRL_FLOAT     = 30 ;
    public const int DBR_CTRL_ENUM      = 31 ;
    public const int DBR_CTRL_CHAR      = 32 ;
    public const int DBR_CTRL_LONG      = 33 ;
    public const int DBR_CTRL_DOUBLE    = 34 ;

    // Aha ... we *can* find out the 'record type',
    // by specifying DBR_CLASS_NAME. This will return
    // a pointer to a string, in the DBR_ structure.

    public const int DBR_PUT_ACKT      = DBR_CTRL_DOUBLE + 1 ;   // 35
    public const int DBR_PUT_ACKS      = DBR_PUT_ACKT + 1 ;      // 36
    public const int DBR_STSACK_STRING = DBR_PUT_ACKS + 1 ;      // 37
    public const int DBR_CLASS_NAME    = DBR_STSACK_STRING + 1 ; // 38

    // Sizes of various strings exchanged in the DBR_ structs
    // Note that these are the buffer sizes - which include a terminating null !!
    // So as far as a user is concerned, the max length is one less !!

    public const int MAX_UNITS_SIZE		    =  8	;
    public const int MAX_ENUM_STRING_SIZE	= 26 ;
    public const int MAX_ENUM_STATES		  = 16;

    public const int MAX_STRING_SIZE		    = 40	; // !!!!!!!!!!!!!!!!! Some docs say 41, it's actually 39 !!!
    public const int MAX_NAME_SIZE		      = 36	; // PV Name !!! DISCUSS_WITH_TIME
    public const int MAX_DESC_SIZE		      = 24	; // DESC field ?? !!! DISCUSS_WITH_TIME

    // ca_preemptive_callback_select

    public const int ca_disable_preemptive_callback = 0 ;
    public const int ca_enable_preemptive_callback  = 1 ;

    // Returned by the 'ca_state' function

    public const int cs_never_conn = 0 ; // Server not found or unavailable
    public const int cs_prev_conn  = 1 ; // Was previously connected to server
    public const int cs_conn       = 2 ; // Is currently connected to server
    public const int cs_closed     = 3 ; // Channel has been closed

    public const int CA_PRIORITY_MIN     =  0 ;
    public const int CA_PRIORITY_MAX     = 99 ;
    public const int CA_PRIORITY_DEFAULT = CA_PRIORITY_MIN ;

  }

}
