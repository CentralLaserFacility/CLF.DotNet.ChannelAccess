//
// ChannelStateSnapshot.cs
//

namespace Clf.ChannelAccess.ExtensionMethods
{

  public static class ChannelStateExtensions
  {

    public static string Value_AsDisplayString ( 
      this ChannelState?               channelState, 
      WhichValueInfoElementsToInclude? whichValueInfoElementsToInclude = null
    ) => (
      channelState?.ValueInfo?.Value_AsDisplayString(
        whichValueInfoElementsToInclude
      ) ?? "null" 
    ) ;

    public static string ValueAsString ( 
      this ChannelState? channelState
    ) => (
      channelState?.ValueInfo?.ValueAsString(
      ) ?? "null" 
    ) ;

    public static T ValueAs<T> ( 
      this ChannelState? channelState
    ) {
      var valueInfo = channelState?.ValueInfo ;
      if ( valueInfo is null )
      {
        throw new System.NullReferenceException("Value is not available") ;
      }
      else
      {
        return valueInfo.ValueAs<T>() ;
      }
    }

  }

}

