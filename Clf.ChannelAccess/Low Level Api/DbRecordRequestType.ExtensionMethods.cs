//
// DbRecordRequestType.ExtensionMethods.cs
//

using Clf.Common.ExtensionMethods ;

namespace Clf.ChannelAccess.LowLevelApi.ExtensionMethods ;

internal static partial class Helpers
{
  
  public static DbrRequestedInfoCategory GetDbrRequestedInfoCategory ( 
    this DbRecordRequestType dbrType 
  )=> dbrType switch { 
    DbRecordRequestType.DBR_STRING        => DbrRequestedInfoCategory.DBR_valueOnly              ,
    DbRecordRequestType.DBR_SHORT         => DbrRequestedInfoCategory.DBR_valueOnly              ,
    DbRecordRequestType.DBR_FLOAT         => DbrRequestedInfoCategory.DBR_valueOnly              ,
    DbRecordRequestType.DBR_ENUM          => DbrRequestedInfoCategory.DBR_valueOnly              ,
    DbRecordRequestType.DBR_CHAR          => DbrRequestedInfoCategory.DBR_valueOnly              ,
    DbRecordRequestType.DBR_LONG          => DbrRequestedInfoCategory.DBR_valueOnly              ,
    DbRecordRequestType.DBR_DOUBLE        => DbrRequestedInfoCategory.DBR_valueOnly              ,
    DbRecordRequestType.DBR_STS_STRING    => DbrRequestedInfoCategory.DBR_STS           ,
    DbRecordRequestType.DBR_STS_SHORT     => DbrRequestedInfoCategory.DBR_STS           ,
    DbRecordRequestType.DBR_STS_FLOAT     => DbrRequestedInfoCategory.DBR_STS           ,
    DbRecordRequestType.DBR_STS_ENUM      => DbrRequestedInfoCategory.DBR_STS           ,
    DbRecordRequestType.DBR_STS_CHAR      => DbrRequestedInfoCategory.DBR_STS           ,
    DbRecordRequestType.DBR_STS_LONG      => DbrRequestedInfoCategory.DBR_STS           ,
    DbRecordRequestType.DBR_STS_DOUBLE    => DbrRequestedInfoCategory.DBR_STS           ,
    DbRecordRequestType.DBR_TIME_STRING   => DbrRequestedInfoCategory.DBR_TIME          ,
    DbRecordRequestType.DBR_TIME_SHORT    => DbrRequestedInfoCategory.DBR_TIME          ,
    DbRecordRequestType.DBR_TIME_FLOAT    => DbrRequestedInfoCategory.DBR_TIME          ,
    DbRecordRequestType.DBR_TIME_ENUM     => DbrRequestedInfoCategory.DBR_TIME          ,
    DbRecordRequestType.DBR_TIME_CHAR     => DbrRequestedInfoCategory.DBR_TIME          ,
    DbRecordRequestType.DBR_TIME_LONG     => DbrRequestedInfoCategory.DBR_TIME          ,
    DbRecordRequestType.DBR_TIME_DOUBLE   => DbrRequestedInfoCategory.DBR_TIME          ,
    DbRecordRequestType.DBR_GR_STRING     => DbrRequestedInfoCategory.DBR_GR            ,
    DbRecordRequestType.DBR_GR_SHORT      => DbrRequestedInfoCategory.DBR_GR            ,
    DbRecordRequestType.DBR_GR_FLOAT      => DbrRequestedInfoCategory.DBR_GR            ,
    DbRecordRequestType.DBR_GR_ENUM       => DbrRequestedInfoCategory.DBR_GR            ,
    DbRecordRequestType.DBR_GR_CHAR       => DbrRequestedInfoCategory.DBR_GR            ,
    DbRecordRequestType.DBR_GR_LONG       => DbrRequestedInfoCategory.DBR_GR            ,
    DbRecordRequestType.DBR_GR_DOUBLE     => DbrRequestedInfoCategory.DBR_GR            ,
    DbRecordRequestType.DBR_CTRL_STRING   => DbrRequestedInfoCategory.DBR_CTRL          ,
    DbRecordRequestType.DBR_CTRL_SHORT    => DbrRequestedInfoCategory.DBR_CTRL          ,
    DbRecordRequestType.DBR_CTRL_FLOAT    => DbrRequestedInfoCategory.DBR_CTRL          ,
    DbRecordRequestType.DBR_CTRL_ENUM     => DbrRequestedInfoCategory.DBR_CTRL          ,
    DbRecordRequestType.DBR_CTRL_CHAR     => DbrRequestedInfoCategory.DBR_CTRL          ,
    DbRecordRequestType.DBR_CTRL_LONG     => DbrRequestedInfoCategory.DBR_CTRL          ,
    DbRecordRequestType.DBR_CTRL_DOUBLE   => DbrRequestedInfoCategory.DBR_CTRL          ,
    DbRecordRequestType.DBR_PUT_ACKT      => DbrRequestedInfoCategory.DBR_PUT_ACKT      ,
    DbRecordRequestType.DBR_PUT_ACKS      => DbrRequestedInfoCategory.DBR_PUT_ACKS      ,
    DbRecordRequestType.DBR_STSACK_STRING => DbrRequestedInfoCategory.DBR_STSACK_STRING ,
    DbRecordRequestType.DBR_CLASS_NAME    => DbrRequestedInfoCategory.DBR_CLASS_NAME    ,
    _                                     => throw dbrType.AsUnexpectedEnumValueException()
  } ;

  public static (
    DbFieldType     dbFieldType, 
    ValueAccessMode valueAccessMode
  ) GetDbFieldTypeAndValueAccessMode ( 
    this DbRecordRequestType dbrType
  ) {
    return dbrType.GetDbrRequestedInfoCategory() switch {
      DbrRequestedInfoCategory.DBR_valueOnly => (
        (DbFieldType) dbrType, 
        ValueAccessMode.DBR_RequestValueAndNothingElse
      ),
      DbrRequestedInfoCategory.DBR_CTRL => (
        (DbFieldType) dbrType - ApiConstants.DBR_CTRL_offset_28,
        ValueAccessMode.DBR_CTRL_RequestValueAndAuxiliaryInfo
      ),
      DbrRequestedInfoCategory.DBR_TIME => (
        (DbFieldType) dbrType - ApiConstants.DBR_TIME_offset_14,
        ValueAccessMode.DBR_TIME_RequestValueAndServerTimeStamp
      ),
      _ => throw dbrType.AsUnexpectedEnumValueException()
    } ;
  }

}

