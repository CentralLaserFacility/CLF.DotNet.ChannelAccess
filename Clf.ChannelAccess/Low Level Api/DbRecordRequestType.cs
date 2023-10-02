//
// DbRecordRequestType.cs
//

namespace Clf.ChannelAccess.LowLevelApi ;

internal enum DbRecordRequestType : short {

  DBR_STRING        = LowLevelApi.ApiConstants.DBR_STRING        ,
  DBR_SHORT         = LowLevelApi.ApiConstants.DBR_SHORT         ,
  DBR_FLOAT         = LowLevelApi.ApiConstants.DBR_FLOAT         ,
  DBR_ENUM          = LowLevelApi.ApiConstants.DBR_ENUM          ,
  DBR_CHAR          = LowLevelApi.ApiConstants.DBR_CHAR          ,
  DBR_LONG          = LowLevelApi.ApiConstants.DBR_LONG          ,
  DBR_DOUBLE        = LowLevelApi.ApiConstants.DBR_DOUBLE        ,

  DBR_STS_          = LowLevelApi.ApiConstants.DBR_STS_STRING    ,
  DBR_STS_STRING    = LowLevelApi.ApiConstants.DBR_STS_STRING    ,
  DBR_STS_SHORT     = LowLevelApi.ApiConstants.DBR_STS_SHORT     ,
  DBR_STS_FLOAT     = LowLevelApi.ApiConstants.DBR_STS_FLOAT     ,
  DBR_STS_ENUM      = LowLevelApi.ApiConstants.DBR_STS_ENUM      ,
  DBR_STS_CHAR      = LowLevelApi.ApiConstants.DBR_STS_CHAR      ,
  DBR_STS_LONG      = LowLevelApi.ApiConstants.DBR_STS_LONG      ,
  DBR_STS_DOUBLE    = LowLevelApi.ApiConstants.DBR_STS_DOUBLE    ,

  DBR_TIME_         = LowLevelApi.ApiConstants.DBR_TIME_STRING   ,
  DBR_TIME_STRING   = LowLevelApi.ApiConstants.DBR_TIME_STRING   ,
  DBR_TIME_SHORT    = LowLevelApi.ApiConstants.DBR_TIME_SHORT    ,
  DBR_TIME_FLOAT    = LowLevelApi.ApiConstants.DBR_TIME_FLOAT    ,
  DBR_TIME_ENUM     = LowLevelApi.ApiConstants.DBR_TIME_ENUM     ,
  DBR_TIME_CHAR     = LowLevelApi.ApiConstants.DBR_TIME_CHAR     ,
  DBR_TIME_LONG     = LowLevelApi.ApiConstants.DBR_TIME_LONG     ,
  DBR_TIME_DOUBLE   = LowLevelApi.ApiConstants.DBR_TIME_DOUBLE   ,

  DBR_GR_           = LowLevelApi.ApiConstants.DBR_GR_STRING     ,
  DBR_GR_STRING     = LowLevelApi.ApiConstants.DBR_GR_STRING     ,
  DBR_GR_SHORT      = LowLevelApi.ApiConstants.DBR_GR_SHORT      ,
  DBR_GR_FLOAT      = LowLevelApi.ApiConstants.DBR_GR_FLOAT      ,
  DBR_GR_ENUM       = LowLevelApi.ApiConstants.DBR_GR_ENUM       ,
  DBR_GR_CHAR       = LowLevelApi.ApiConstants.DBR_GR_CHAR       ,
  DBR_GR_LONG       = LowLevelApi.ApiConstants.DBR_GR_LONG       ,
  DBR_GR_DOUBLE     = LowLevelApi.ApiConstants.DBR_GR_DOUBLE     ,

  DBR_CTRL_         = LowLevelApi.ApiConstants.DBR_CTRL_STRING   ,
  DBR_CTRL_STRING   = LowLevelApi.ApiConstants.DBR_CTRL_STRING   ,
  DBR_CTRL_SHORT    = LowLevelApi.ApiConstants.DBR_CTRL_SHORT    ,
  DBR_CTRL_FLOAT    = LowLevelApi.ApiConstants.DBR_CTRL_FLOAT    ,
  DBR_CTRL_ENUM     = LowLevelApi.ApiConstants.DBR_CTRL_ENUM     ,
  DBR_CTRL_CHAR     = LowLevelApi.ApiConstants.DBR_CTRL_CHAR     ,
  DBR_CTRL_LONG     = LowLevelApi.ApiConstants.DBR_CTRL_LONG     ,
  DBR_CTRL_DOUBLE   = LowLevelApi.ApiConstants.DBR_CTRL_DOUBLE   ,

  DBR_PUT_ACKT      = LowLevelApi.ApiConstants.DBR_PUT_ACKT      ,
  DBR_PUT_ACKS      = LowLevelApi.ApiConstants.DBR_PUT_ACKS      ,
  DBR_STSACK_STRING = LowLevelApi.ApiConstants.DBR_STSACK_STRING ,

  DBR_CLASS_NAME    = LowLevelApi.ApiConstants.DBR_CLASS_NAME    ,

}
