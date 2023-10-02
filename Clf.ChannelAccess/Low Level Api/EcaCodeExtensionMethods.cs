//
// EcaCodeExtensionMethods.cs
//

using System.Diagnostics.CodeAnalysis ;

namespace Clf.ChannelAccess.LowLevelApi
{

  internal static class EcaCodeExtensionMethods
  {

    // Many of the 'ca_' functions return an integer code

    public static int VerifyEcaSuccess ( 
      this int                                                   ecaReturnCode, 
      [System.Runtime.CompilerServices.CallerMemberName] string? functionName = null 
    ) {
      // TODO : Refactor this to use 'ApiCallResult.VerifySuccess' ...
      if ( CA_EXTRACT_SUCCESS(ecaReturnCode) is false )
      {
        int messageNumber = CA_EXTRACT_MSG_NO(ecaReturnCode) ;
        var message = (EcaMessage) messageNumber ;
        var severity = (EcaSeverity) CA_EXTRACT_SEVERITY(ecaReturnCode) ;
        switch ( severity ) 
        {
        case EcaSeverity.Success:
        case EcaSeverity.Warning:
        case EcaSeverity.Info:
          break ;
        case EcaSeverity.RecoverableError:
        case EcaSeverity.FatalError:
          string apiNameOrEmpty = (
            functionName is null
            ? ""
            : $"({functionName}) "
          ) ;
          throw new UnexpectedConditionException(
            $"API call failed {apiNameOrEmpty}on #{System.Environment.CurrentManagedThreadId} : {message} (#{messageNumber})"
          ) ;
        }
      }
      return ecaReturnCode ;
    }

    // An ECA code of '1' indicates success ... kindof like boolean 'true' ...

    public static bool IsEcaSuccess ( 
      this int ecaReturnCode
    ) {
      return CA_EXTRACT_SUCCESS(ecaReturnCode) is true ;
    }

    public static bool IsEcaFailure ( 
      this int                             ecaReturnCode,
      [NotNullWhen(true)] out EcaSeverity? ecaSeverity,
      [NotNullWhen(true)] out EcaMessage?  ecaMessage
    ) {
      if ( CA_EXTRACT_SUCCESS(ecaReturnCode) is true )
      {
        // Success, ie no failure to report
        // Hmm, sometimes we still get an informational message ??? *************************
        ecaSeverity = null ;
        ecaMessage  = null ;
        return false ;
      }
      else
      {
        // Failed ...
        ecaSeverity = (EcaSeverity) CA_EXTRACT_SEVERITY(ecaReturnCode) ;
        ecaMessage  = (EcaMessage)  CA_EXTRACT_MSG_NO(ecaReturnCode) ;
        return true ;
      }
    }

    public static bool IsEcaFailure ( 
      this int                        ecaReturnCode,
      [NotNullWhen(true)] out string? message
    ) {
      if ( CA_EXTRACT_SUCCESS(ecaReturnCode) is true )
      {
        // Success, ie no failure to report
        // Hmm, sometimes we still get a message ???
        // TODO : LOG THE MESSAGE ???
        message = null ;
        return false ;
      }
      else
      {
        // Failed ...
        var ecaSeverity = (EcaSeverity) CA_EXTRACT_SEVERITY(ecaReturnCode) ;
        var ecaMessage  = (EcaMessage)  CA_EXTRACT_MSG_NO(ecaReturnCode) ;
        message = $"{ecaMessage} (severity:{ecaSeverity})" ;
        return true ;
      }
    }

    public static UnexpectedConditionException AsEcaFailureException ( 
      this int ecaReturnCode
    ) {
      var severity = (EcaSeverity) CA_EXTRACT_SEVERITY(ecaReturnCode) ;
      var message  = (EcaMessage)  CA_EXTRACT_MSG_NO(ecaReturnCode) ;
      string failureMessage = $"{message} (severity:{severity})" ;
      return new UnexpectedConditionException(
        $"ECA : {failureMessage}"
      ) ;
    }

    // Extract useful fields from an ECA_ code

    private static bool CA_EXTRACT_SUCCESS ( int eca_code)
    => ( 
      eca_code & LowLevelApi.ApiConstants.CA_M_SUCCESS
    ) == LowLevelApi.ApiConstants.CA_M_SUCCESS ;

    private static int CA_EXTRACT_MSG_NO ( int eca_code )
    => ( 
      ( eca_code & LowLevelApi.ApiConstants.CA_M_MSG_NO ) 
    >> LowLevelApi.ApiConstants.CA_V_MSG_NO 
    ) ;

    private static int CA_EXTRACT_SEVERITY ( int eca_code )
    => ( 
      eca_code & LowLevelApi.ApiConstants.CA_M_SEVERITY 
    ) ;

  }

}
