//
// Exceptions.cs
//

namespace Clf.ChannelAccess
{

  // TODO : REVIEW THIS ...

  // Hmm, we can encode the behaviour to be actioned in the
  // wrapper exception handler, into these exception classes ...

  // Alternatively, use a class hierarchy that defines the severity ... ???
  // FatalException ; RecoverableException ...

  public enum ExceptionResponse {
    Continue,
    SetChannelAsInvalid
  }

  // Hmm, this could hold a structured 'notification' ...

  public /*abstract*/ class ExceptionBase : System.ApplicationException
  {
    public ExceptionResponse? Response ;
    public ExceptionBase ( string message, ExceptionResponse? response = null ) :
    base(message)
    { 
      Response = response ;
    }
  }

  //
  // TODO : Document these exception types !!!
  //

  public class TimeoutException : ExceptionBase
  {
    public TimeoutException ( string message ) :
    base(message,ExceptionResponse.Continue)
    { }
  }

  public class UnexpectedConditionException : ExceptionBase
  {
    public UnexpectedConditionException ( string message ) :
    base(message,ExceptionResponse.SetChannelAsInvalid)
    { }
  }

  public class UsageErrorException : ExceptionBase
  {
    public UsageErrorException ( string message ) :
    base(message,ExceptionResponse.SetChannelAsInvalid)
    { }
  }

  public class ProgrammingErrorException : ExceptionBase
  {
    // For example, should have checked for null
    public ProgrammingErrorException ( string message ) :
    base(message,ExceptionResponse.SetChannelAsInvalid)
    { }
  }

  // Not currently used - we expect a FluentAssertions.Execution.AssertionFailedException
  // to be thrown if an assert fails, but we could in principle implement our own
  // assertion mechanism in which case this is what we'd use ...

  // public class AssertFailedException : ExceptionBase
  // {
  //   public AssertFailedException ( string message ) :
  //   base(message,ExceptionResponse.SetChannelAsInvalid)
  //   { }
  // }

}
