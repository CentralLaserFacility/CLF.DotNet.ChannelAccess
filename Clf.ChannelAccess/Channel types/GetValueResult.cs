//
// GetValueResult.cs
//

namespace Clf.ChannelAccess
{

  public enum WhyGetValueFailed
  {
    TimeoutOnThisQuery,
    ChannelWasNeverConnected,
    DisposedChannel
  }

  public class GetValueResult 
  {

    private ValueInfo? m_valueInfo = null ;

    private WhyGetValueFailed? m_whyFailed = null ;

    internal GetValueResult ( ValueInfo? valueInfo )
    {
      m_valueInfo = valueInfo ;
    }

    internal GetValueResult ( WhyGetValueFailed whyFailed )
    {
      m_whyFailed = whyFailed ;
    }

    public bool Succeeded => m_valueInfo != null ;

    public bool IsSuccess => m_valueInfo != null ;

    public ValueInfo ValueInfo => m_valueInfo ?? throw new UsageErrorException("Value not available") ;

    public WhyGetValueFailed? WhyFailed => m_whyFailed ;

    public static implicit operator bool ( GetValueResult result )
    {
      return result.Succeeded ;
    }

    // Is this useful ??? Keep it ???
    [System.Obsolete("You should check that the result is valid, before accessing the ValueInfo")]
    public static implicit operator ValueInfo ( GetValueResult getValueResult )
    => getValueResult.m_valueInfo ?? throw new ProgrammingErrorException("Value not available") ;

    public static void UsageExample ( GetValueResult outcomeOfApiCall )
    {
      ValueInfo valueInfo = outcomeOfApiCall ;
      if ( outcomeOfApiCall.IsSuccess )
      {
        var x = outcomeOfApiCall.ValueInfo ; 
      }
      else
      {
        string message = outcomeOfApiCall.WhyFailed.ToString() ?? "No failure" ;
      }

    }

  }

}
