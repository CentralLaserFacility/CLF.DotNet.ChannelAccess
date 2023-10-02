//
// ChannelWrapper.cs
//

using System.Threading.Tasks ;
using Clf.ChannelAccess.ExtensionMethods ;

namespace Clf.ChannelAccess
{

  // Hmm, if someone invokes 'Dispose' and then calls a method ...
  // In Dispose(), could substitute an InvalidChannel instance ???
  // No, let's just raise a null object exception ...

  public class ChannelWrapper : IChannel
  {

    internal static bool IfAnExceptionIsCaughtRethrowIt = false ; // Todo : MOVE TO SETTINGS ???

    internal ChannelBase WrappedChannel { get ; private set ; }

    internal ChannelWrapper ( ChannelBase wrappedChannel )
    {
      WrappedChannel = wrappedChannel ;
    }

    public ChannelName ChannelName => WrappedChannel.ChannelName ;

    public ValueAccessMode ValueAccessMode => WrappedChannel.ValueAccessMode ;

    // public ChannelNameAndAccessMode ChannelNameAndAccessMode => WrappedChannel.ChannelNameAndAccessMode ;

    public ChannelStatesSnapshot Snapshot ( ) => WrappedChannel.Snapshot() ;

    public System.DateTime? TimeStampFromServer => WrappedChannel.TimeStampFromServer ;

    public FieldInfo? FieldInfo => WrappedChannel.FieldInfo ;

    public Task<bool> HasConnectedAndAcquiredValueAsync ( )
    {
      if (WrappedChannel != null)
        return TryCatch(WrappedChannel.HasConnectedAndAcquiredValueAsync);
      else
        return Task.FromResult(false);
    }

    public Task<bool> HasConnectedAsync ( )
    {
      if (WrappedChannel != null)
        return TryCatch(WrappedChannel.HasConnectedAsync);
      else
        return Task.FromResult(false);
    }

    public void PutValue ( object valueToWrite )
    {
      if (WrappedChannel != null)
        TryCatch(WrappedChannel.PutValue, valueToWrite);
    }

    public Task<PutValueResult> PutValueAsync ( object valueToWrite )
    {
      if (WrappedChannel != null)
        return TryCatch(WrappedChannel.PutValueAsync, valueToWrite);
      else
        return Task.FromResult(PutValueResult.DisposedChannel);
    }

    public Task<PutValueResult> PutValueAckAsync ( object valueToWrite )
    {
      if (WrappedChannel != null)
        return TryCatch(WrappedChannel.PutValueAckAsync,valueToWrite);
      else
        return Task.FromResult(PutValueResult.DisposedChannel);
    }

    public Task<GetValueResult> GetValueAsync()
    {
      if (WrappedChannel != null)
        return TryCatch(WrappedChannel.GetValueAsync);
      else
        return Task.FromResult(new GetValueResult(WhyGetValueFailed.DisposedChannel));
    }

    public void Dispose ( )
    {
      // Decrement the WrappedChannel reference count.
      // If it becomes zero, invoke Dispose() on the channel.
      WrappedChannel.Dispose() ;
      // ??? WrappedChannel = new InvalidChannel() ?????
      WrappedChannel = null! ; // USE THIS ONE !!!!
    }

    private void HandleException ( System.Exception x )
    {
      // This is a mess, restructure it ...
      if ( x is ExceptionBase clfException )
      {
        if ( clfException.Response is ExceptionResponse.SetChannelAsInvalid )
        {
          WrappedChannel.DeclareChannelInvalid(
            x.Message
          ) ;
        }
      }
      else if ( x is FluentAssertions.Execution.AssertionFailedException assertFailedException )
      {
        WrappedChannel.DeclareChannelInvalid(
          x.Message
        ) ;
      }
      // Write a log message ...
      Hub.NotifyExceptionCaught(this,x) ;
    }

    private void CheckForInvalidStatus ( )
    {
      if ( WrappedChannel.IsInvalid( out var whyNotValid ) )
      {
        // Hmm, client code has invoked an operation on a channel that's invalid.
        // Do we really want to log every occurrence ??? YES, EVERY TIME !!!
        throw new ExceptionBase( 
          $"Channel is in an invalid state : {whyNotValid}"
        ) ;
      }
    }

    private void TryCatch ( System.Action action )
    {
      try
      {
        CheckForInvalidStatus() ;
        action() ;
      }
      catch ( System.Exception x )
      {
        HandleException(x) ;
        if ( IfAnExceptionIsCaughtRethrowIt )
        {
          throw ;
        }
      }
    }

    private void TryCatch ( System.Action<object> action, object arg )
    {
      try
      {
        CheckForInvalidStatus() ;
        action(arg) ;
      }
      catch ( System.Exception x )
      {
        HandleException(x) ;
        if ( IfAnExceptionIsCaughtRethrowIt )
        {
          throw ;
        }
      }
    }

    private T TryCatch<T> ( System.Func<T> func )
    {
      try
      {
        CheckForInvalidStatus() ;
        return func() ;
      }
      catch ( System.Exception x )
      {
        HandleException(x) ;
        if ( IfAnExceptionIsCaughtRethrowIt )
        {
          throw ;
        }
        else
        {
          // ???????
          return default(T)! ;
        }
      }
    }

    private T TryCatch<T> ( System.Func<object,T>func, object arg )
    {
      try
      {
        CheckForInvalidStatus() ;
        return func(arg) ;
      }
      catch ( System.Exception x )
      {
        HandleException(x) ;
        if ( IfAnExceptionIsCaughtRethrowIt )
        {
          throw ;
        }
        else
        {
          // ???????
          return default(T)! ;
        }
      }
    }

  }

}