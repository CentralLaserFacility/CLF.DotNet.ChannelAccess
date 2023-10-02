//
// EcaMessage.cs
//

namespace Clf.ChannelAccess.LowLevelApi
{

  //
  // These are defined as enum values so that we can conveniently
  // display the value as a string, as well as a number.
  //

  internal enum EcaMessage {
    ECA_MESSAGE_NORMAL         =  0,
    ECA_MESSAGE_MAXIOC         =  1,
    ECA_MESSAGE_UKNHOST        =  2,
    ECA_MESSAGE_UKNSERV        =  3,
    ECA_MESSAGE_SOCK           =  4,
    ECA_MESSAGE_CONN           =  5,
    ECA_MESSAGE_ALLOCMEM       =  6,
    ECA_MESSAGE_UKNCHAN        =  7,
    ECA_MESSAGE_UKNFIELD       =  8,
    ECA_MESSAGE_TOLARGE        =  9,
    ECA_MESSAGE_TIMEOUT        = 10,
    ECA_MESSAGE_NOSUPPORT      = 11,
    ECA_MESSAGE_STRTOBIG       = 12,
    ECA_MESSAGE_DISCONNCHID    = 13,
    ECA_MESSAGE_BADTYPE        = 14,
    ECA_MESSAGE_CHIDNOTFND     = 15,
    ECA_MESSAGE_CHIDRETRY      = 16,
    ECA_MESSAGE_INTERNAL       = 17,
    ECA_MESSAGE_DBLCLFAIL      = 18,
    ECA_MESSAGE_GETFAIL        = 19,
    ECA_MESSAGE_PUTFAIL        = 20,
    ECA_MESSAGE_ADDFAIL        = 21,
    ECA_MESSAGE_BADCOUNT       = 22,
    ECA_MESSAGE_BADSTR         = 23,
    ECA_MESSAGE_DISCONN        = 24,
    ECA_MESSAGE_DBLCHNL        = 25,
    ECA_MESSAGE_EVDISALLOW     = 26,
    ECA_MESSAGE_BUILDGET       = 27,
    ECA_MESSAGE_NEEDSFP        = 28,
    ECA_MESSAGE_OVEVFAIL       = 29,
    ECA_MESSAGE_BADMONID       = 30,
    ECA_MESSAGE_NEWADDR        = 31,
    ECA_MESSAGE_NEWCONN        = 32,
    ECA_MESSAGE_NOCACTX        = 33,
    ECA_MESSAGE_DEFUNCT        = 34,
    ECA_MESSAGE_EMPTYSTR       = 35,
    ECA_MESSAGE_NOREPEATER     = 36,
    ECA_MESSAGE_NOCHANMSG      = 37,
    ECA_MESSAGE_DLCKREST       = 38,
    ECA_MESSAGE_SERVBEHIND     = 39,
    ECA_MESSAGE_NOCAST         = 40,
    ECA_MESSAGE_BADMASK        = 41,
    ECA_MESSAGE_IODONE         = 42,
    ECA_MESSAGE_IOINPROGRESS   = 43,
    ECA_MESSAGE_BADSYNCGRP     = 44,
    ECA_MESSAGE_PUTCBINPROG    = 45,
    ECA_MESSAGE_NORDACCESS     = 46,
    ECA_MESSAGE_NOWTACCESS     = 47,
    ECA_MESSAGE_ANACHRONISM    = 48,
    ECA_MESSAGE_NOSEARCHADDR   = 49,
    ECA_MESSAGE_NOCONVERT      = 50,
    ECA_MESSAGE_BADCHID        = 51,
    ECA_MESSAGE_BADFUNCPTR     = 52,
    ECA_MESSAGE_ISATTACHED     = 53,
    ECA_MESSAGE_UNAVAILINSERV  = 54,
    ECA_MESSAGE_CHANDESTROY    = 55,
    ECA_MESSAGE_BADPRIORITY    = 56,
    ECA_MESSAGE_NOTTHREADED    = 57,
    ECA_MESSAGE_16KARRAYCLIENT = 58,
    ECA_MESSAGE_CONNSEQTMO     = 59,
    ECA_MESSAGE_UNRESPTMO      = 60,
  } ;

  // ?? There are lots of other ECA_ return codes
  // https://epics.anl.gov/EpicsDocumentation/AppDevManuals/ChannelAccess/cadoc_6.htm#MARKER-9-121
  //
  //   ECA_NORMAL          SUCCESS    
  //   
  //   ECA_IODONE          INFO      
  //   ECA_IOINPROGRESS    INFO      
  //   ECA_CHIDNOTFND      INFO       defunct
  //   ECA_CHIDRETRY       INFO       defunct
  //   ECA_NEWCONN         INFO       defunct
  //   
  //   ECA_ALLOCMEM        WARNING    
  //   ECA_TOLARGE         WARNING    
  //   ECA_TIMEOUT         WARNING   
  //   ECA_GETFAIL         WARNING   
  //   ECA_PUTFAIL         WARNING   
  //   ECA_BADCOUNT        WARNING   
  //   ECA_DISCONN         WARNING   
  //   ECA_DBLCHNL         WARNING   
  //   ECA_NORDACCESS      WARNING   
  //   ECA_NOWTACCESS      WARNING   
  //   ECA_NOSEARCHADDR    WARNING   
  //   ECA_NOCONVERT       WARNING   
  //   ECA_ISATTACHED      WARNING   
  //   ECA_UNAVAILINSERV   WARNING   
  //   ECA_CHANDESTROY     WARNING   
  //   ECA_16KARRAYCLIENT  WARNING   
  //   ECA_CONNSEQTMO      WARNING   
  //   ECA_UNRESPTMO       WARNING   
  //   ECA_CONN            WARNING    defunct
  //   ECA_UKNCHAN         WARNING    defunct
  //   ECA_UKNFIELD        WARNING    defunct
  //   ECA_NOSUPPORT       WARNING    defunct
  //   ECA_STRTOBIG        WARNING    defunct
  //   ECA_DBLCLFAIL       WARNING    defunct
  //   ECA_ADDFAIL         WARNING    defunct
  //   ECA_BUILDGET        WARNING    defunct
  //   ECA_NEEDSFP         WARNING    defunct
  //   ECA_OVEVFAIL        WARNING    defunct
  //   ECA_NEWADDR         WARNING    defunct
  //   ECA_NOCACTX         WARNING    defunct
  //   ECA_EMPTYSTR        WARNING    defunct
  //   ECA_NOREPEATER      WARNING    defunct
  //   ECA_NOCHANMSG       WARNING    defunct
  //   ECA_DLCKREST        WARNING    defunct
  //   ECA_SERVBEHIND      WARNING    defunct
  //   ECA_NOCAST          WARNING    defunct
  //   
  //   ECA_BADSTR          ERROR     
  //   ECA_BADTYPE         ERROR     
  //   ECA_EVDISALLOW      ERROR     
  //   ECA_BADMONID        ERROR     
  //   ECA_BADMASK         ERROR     
  //   ECA_BADSYNCGRP      ERROR     
  //   ECA_PUTCBINPROG     ERROR     
  //   ECA_ANACHRONISM     ERROR     
  //   ECA_BADCHID         ERROR     
  //   ECA_BADFUNCPTR      ERROR     
  //   ECA_BADPRIORITY     ERROR     
  //   ECA_NOTTHREADED     ERROR     
  //   ECA_MAXIOC          ERROR      defunct
  //   ECA_UKNHOST         ERROR      defunct
  //   ECA_UKNSERV         ERROR      defunct
  //   ECA_SOCK            ERROR      defunct
  //   ECA_DISCONNCHID     ERROR      defunct
  //   
  //   ECA_INTERNAL        FATAL   
  //   ECA_DEFUNCT         FATAL      defunct
  //   
  //   Hmm, can't seem to find a detailed description of these.
  //
  // Many are 'defunct', but there are a couple of dozen 'current' ones.
  // The docs indicate which ECA_ codes can be returned by each function.
  // For example ca_test_io() can return ECA_IODONE or ECA_IOINPROGRESS
  // BEST TO DEFINE THESE AS INTEGER CODES HERE ???
  // Note - a client might receive 'defunct' codes from an old IOC !!!

}
