//
// ApiCallResult.cs
//

namespace Clf.ChannelAccess.LowLevelApi
{

  internal record ApiCallResult ( int EcaCode )
  {

    public static implicit operator int ( ApiCallResult ecaReturnCode )
    => ecaReturnCode.EcaCode ;

    public static implicit operator ApiCallResult ( int ecaReturnCode )
    => new ApiCallResult(ecaReturnCode) ;

    public bool DenotesSuccess => CA_EXTRACT_SUCCESS(EcaCode) ;

    public EcaSeverity Severity => (EcaSeverity) CA_EXTRACT_SEVERITY(EcaCode) ;

    public EcaMessage MessageNumber => (EcaMessage) CA_EXTRACT_MSG_NO(EcaCode) ;

    public ApiCallResult VerifySuccess ( 
      [System.Runtime.CompilerServices.CallerMemberName] string? functionName = null 
    ) {
      return Severity switch {
        EcaSeverity.Success          => this,
        EcaSeverity.Info             => this,
        EcaSeverity.Warning          => LogWarningAndContinue(),
        EcaSeverity.RecoverableError => throw new UnexpectedConditionException(GetExceptionMessage(functionName!)),
        EcaSeverity.FatalError       => throw new UnexpectedConditionException(GetExceptionMessage(functionName!)),
        _                            => throw new UnexpectedConditionException($"Unexpected ECA code {EcaCode}"),
      } ;
    }

    public string GetExceptionMessage ( string functionName )
    => $"API call '{functionName}' on #{System.Environment.CurrentManagedThreadId} failed, message = {MessageNumber}, Severity={Severity}" ;
    
    public ApiCallResult LogWarningAndContinue ( )
    => (
      // TODO !!! Raise a warning ...
      this 
    ) ;

    //
    // Extract useful fields from an ECA_ code.
    // The names of these private methods could be improved,
    // but it's best to keep them the same as in the C code.
    // 
    //
    //            MESSAGE NUMBER 0 .. 60   |  SEVERITY |
    // +---+---+---+---+---+---+---+---+---+---+---+---+
    // |   |   |   |   |   |   |   |   |   |   |   |   |   
    // +---+---+---+---+---+---+---+---+---+---+---+---+
    //                                       2   1   0
    //

    private static int CA_EXTRACT_MSG_NO ( int eca_code )
    => ( 
      ( eca_code & ApiConstants.CA_M_MSG_NO ) 
    >> ApiConstants.CA_V_MSG_NO 
    ) ;

    private static int CA_EXTRACT_SEVERITY ( int eca_code )
    => ( 
      eca_code & ApiConstants.CA_M_SEVERITY 
    ) ;

    private static bool CA_EXTRACT_SUCCESS ( int eca_code)
    => ( 
      eca_code & ApiConstants.CA_M_SUCCESS
    ) == ApiConstants.CA_M_SUCCESS ;

  }

}
