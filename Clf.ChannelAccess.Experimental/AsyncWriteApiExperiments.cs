//
// AsyncWriteApiExperiments.cs
//

using System.Collections.Generic ;
using System.Threading.Tasks ;
using FluentAssertions ;

namespace Clf.ChannelAccess.Experimental
{

  public enum WriteCompletionStatus {
    WriteSucceeded,
    WriteFailed_Rejected,
    WriteFailed_Timeout
  }

  public enum WriteMode {
    FireAndForget,
    WaitForWriteAcceptance,
    WaitForWriteAcceptanceAndLocalStateUpdate
  }

  public static class AsyncWriteApiExperiments
  {

    public static async Task<bool> WriteReturningBooleanTaskIndicatingSuccessAsync ( object x )
    {
      await Task.Delay(100) ;
      return false ;
    }

    // Async methods cannot have ref, in or out parameters ...
    // public static async Task WriteReturningBooleanFlagIndicatingSuccessAsync ( object x, out bool success )
    // {
    //   await Task.Delay(100) ;
    //   success = true ;
    // }

    // https://stackoverflow.com/questions/18716928/how-to-write-an-async-method-with-out-parameter
    // The limitation of the async methods not accepting out parameters applies only to
    // compiler-generated async methods, declared with the async keyword. It doesn't apply
    // to hand-crafted async methods. In other words it is possible to create
    // Task returning methods accepting 'out' parameters. 
    public static void WriteReturningOutBooleanTaskIndicatingSuccessAsync ( object x, out Task<bool> success )
    {
      success = WriteReturningBooleanTaskIndicatingSuccessAsync(
        x
      ) ;
    }
    // NOPE, THERE REALLY ISN'T A WAY TO HAVE AN API RETURN A TASK-WITH-NO-RESULT,
    // WHILE REPORTING THE RESULT IN AN OUT PARAMETER ...
    public static Task WriteReturningOutBooleanFlagIndicatingSuccessAsync_OLD_01 ( object x, out bool success )
    {
      bool local_success = false ;
      var tcs = new TaskCompletionSource() ;
      WriteReturningBooleanTaskIndicatingSuccessAsync(
        x
      ).ContinueWith(
        continuationAction : t => {
          if ( t.IsFaulted )
          {
            // tcs.SetException(t.Exception!.InnerException!);
            tcs.SetResult() ;
            // return false ;
          }
          else
          {
            t.IsCompletedSuccessfully.Should().BeTrue() ;
            local_success = t.Result ;
            tcs.SetResult() ;
          }
        },
        cancellationToken   : default,
        continuationOptions : TaskContinuationOptions.None,
        scheduler           : TaskScheduler.Default
      ) ;   
      success = local_success ;
      return tcs.Task ;
    }
    public static Task WriteReturningOutBooleanFlagIndicatingSuccessAsync_OLD_ZZ ( object x, out bool success )
    {
      bool[] successArray = new bool[1] ;
      Task Helper ( object x, bool[] success )
      {
        var tcs = new TaskCompletionSource() ;
        WriteReturningBooleanTaskIndicatingSuccessAsync(
          x
        ).ContinueWith(
          continuationAction : t => {
            if ( t.IsFaulted )
            {
              success[0] = false ;
            }
            else
            {
              t.IsCompletedSuccessfully.Should().BeTrue() ;
              success[0] = t.Result ;
            } ;
            tcs.SetResult() ;
          }
        ) ;
        return tcs.Task ;
      }
      success = successArray[0] ;
      return Helper(x,successArray) ;
    }
    public static Task WriteReturningBooleanArrayIndicatingSuccessAsync ( object x, bool[] success )
    {
      var tcs = new TaskCompletionSource() ;
      WriteReturningBooleanTaskIndicatingSuccessAsync(
        x
      ).ContinueWith(
        continuationAction : t => {
          if ( t.IsFaulted )
          {
            success[0] = false ;
          }
          else
          {
            t.IsCompletedSuccessfully.Should().BeTrue() ;
            success[0] = t.Result ;
          } ;
          tcs.SetResult() ;
        }
      ) ;
      return tcs.Task ;
    }

    public class AsyncResult
    {
      public object? Value ;
    }

    public static Task WriteReturningOutResultAsync1 ( object x, AsyncResult result )
    {
      result = new() ;
      var tcs = new TaskCompletionSource() ;
      WriteReturningBooleanTaskIndicatingSuccessAsync(
        x
      ).ContinueWith(
        continuationAction : t => {
          if ( t.IsFaulted )
          {
            result.Value = false ;
          }
          else
          {
            t.IsCompletedSuccessfully.Should().BeTrue() ;
            result.Value = t.Result ;
          } ;
          tcs.SetResult() ;
        }
      ) ;
      return tcs.Task ;
    }

    public static Task WriteReturningOutResultAsync2 ( object x, out AsyncResult result )
    {
      result = new AsyncResult() ;
      return WriteReturningOutResultAsync1(x,result) ;
    }

    public static Task WriteReturningOutResultAsync ( object x, out AsyncResult result )
    {
      result = new AsyncResult() ;
      return WriteReturningOutResultAsync(x,result) ;
      static Task WriteReturningOutResultAsync ( object x, AsyncResult result )
      {
        result = new AsyncResult() ;
        var tcs = new TaskCompletionSource() ;
        WriteReturningBooleanTaskIndicatingSuccessAsync(
          x
        ).ContinueWith(
          continuationAction : t => {
            if ( t.IsFaulted )
            {
              result.Value = false ;
            }
            else
            {
              t.IsCompletedSuccessfully.Should().BeTrue() ;
              result.Value = t.Result ;
            } ;
            tcs.SetResult() ;
          }
        ) ;
        return tcs.Task ;
      }
    }

    // This top level function is not declared as 'async', so it
    // is allowed to have an 'out' parameter. However since it's not declared as 'async'
    // it isn't permitted to 'await' the inner task ...

    public class ResultWriter : System.IDisposable
    {
      public System.Action? DisposeAction ;
      public void Dispose ( )
      {
        DisposeAction!() ;
      }
    }

    // Hmm, this ain't going to work ...

    public static Task WriteReturningOutResultAsyncEx ( object x, out bool result )
    {
      result = false ;
      AsyncResult asyncResult = new() ;
      using ( 
        var resultWriter = new ResultWriter() {
          DisposeAction = () => {
            // UNFORTUNATELY ... NOT PERMITTED !!
            // result = asyncResult.Value ;
            WriteResult() ;
          }
        }
      ) {
        return WriteReturningOutResultAsyncEx(x,asyncResult) ;
      }
      void WriteResult ( )
      {
        // UNFORTUNATELY ... NOT PERMITTED !!
        // result = asyncResult.Value ;
      }
      // return WriteReturningOutResultAsyncEx(x,asyncResult) ;
    }

    public static Task WriteReturningOutResultAsyncEx ( object x, AsyncResult result )
    {
      // result = false ;
      bool tmpResult = false ;
      using ( 
        var resultWriter = new ResultWriter() {
          DisposeAction = () => {
            result.Value = tmpResult ;
          }
        }
      ) {
        return WriteReturningResultAsync(x) ;
      }
      Task WriteReturningResultAsync ( object x )
      {
        var tcs = new TaskCompletionSource() ;
        WriteReturningBooleanTaskIndicatingSuccessAsync(
          x
        ).ContinueWith(
          continuationAction : t => {
            if ( t.IsFaulted )
            {
              tcs.SetResult() ;
            }
            else
            {
              t.IsCompletedSuccessfully.Should().BeTrue() ;
              tmpResult = true ;
              tcs.SetResult() ;
            } ;
          }
        ) ;
        return tcs.Task ;
      }
    }

    // public static Task WriteReturningOutBoolAsync3 ( object x, out bool ok )
    // {
    //   var result = new WriteResult() ;
    //   return WriteReturningOutResultAsync(x,result) ;
    //   static Task WriteReturningOutResultAsync ( object x, WriteResult result )
    //   {
    //     result = new WriteResult() ;
    //     var tcs = new TaskCompletionSource() ;
    //     WriteReturningBooleanFlagIndicatingSuccessAsync(
    //       x
    //     ).ContinueWith(
    //       continuationAction : t => {
    //         if ( t.IsFaulted )
    //         {
    //           result.OK = false ;
    //         }
    //         else
    //         {
    //           t.IsCompletedSuccessfully.Should().BeTrue() ;
    //           result.OK = t.Result ;
    //         } ;
    //         tcs.SetResult() ;
    //       }
    //     ) ;
    //     return tcs.Task ;
    //   }
    // }

    public static async Task WriteWithCompletionCallbackAsync ( object x, System.Action<bool>? onCompleted = null )
    {
      bool writeSucceeded = await WriteReturningBooleanTaskIndicatingSuccessAsync(x) ;
      if ( onCompleted != null )
      {
        onCompleted?.Invoke(writeSucceeded) ;
      }
      else
      {
        if ( ! writeSucceeded )
        {
          throw new System.ApplicationException("Write failed") ;
        }
      }
    }

    public enum WhyWriteFailed {
      RejectedByServer,
      TimeoutWaitingForWriteAcknowledgement,
      TimeoutWaitingForLocalStateUpdate
    }

    public class WriteFailedException : System.ApplicationException
    {
      public WhyWriteFailed WhyWriteFailed ;
    }

    public static async Task WriteWithExceptionOnFailureAsync ( object x )
    {
      bool writeSucceeded = await WriteReturningBooleanTaskIndicatingSuccessAsync(x) ;
      if ( ! writeSucceeded )
      {
        throw new System.ApplicationException("Write failed") ;
      }
    }

    public static async Task Run ( )
    {

      // This is the recommended way, but we can't enforce it ...
      bool success = await WriteReturningBooleanTaskIndicatingSuccessAsync(123) ;

      // It's not obvious from looking at this code, that we're ignoring the result
      // which will be 'false' if something bad happened eg a timeout or a write failure
      await WriteReturningBooleanTaskIndicatingSuccessAsync(123) ; // BAD !!! Omitting the check for the return value !!!

      // This would be nice - but not possible !!
      // await WriteReturningResultAsOutBoolean(
      //   XmlAssertionExtensions,
      //   out bool succeeded
      // ) ;

      // await WriteWithCompletionCallbackAsync(123) ;

      // Here at least we're obliged to provide a named value
      // for the returned result, which is harder to ignore ...
      // however, it looks a bit strange ...
      WriteReturningOutBooleanTaskIndicatingSuccessAsync(
        123,
        out Task<bool> writeSucceeded
      ) ;
      if ( await writeSucceeded )
      {
      }
      else
      {
      }

      // This is sortof OK ...

      bool thisWriteSucceeded = false ;
      await WriteWithCompletionCallbackAsync(
        123,
        succeeded => thisWriteSucceeded = succeeded 
      ) ;
      if ( thisWriteSucceeded )
      {
      }

      // Can just use try / catch !!!

      try
      {
        await WriteWithExceptionOnFailureAsync(123) ;
      }
      catch ( WriteFailedException x )
      {
        switch ( x.WhyWriteFailed )
        {
        case WhyWriteFailed.RejectedByServer:
          break ;
        case WhyWriteFailed.TimeoutWaitingForWriteAcknowledgement:
          break ;
        case WhyWriteFailed.TimeoutWaitingForLocalStateUpdate:
          break ;
        }
      }

      // This works ... probably ...

      await WriteReturningOutResultAsync(
        123,
        out AsyncResult result
      ) ;
      if ( result.Value is null )
      {
      }

    }

  }

}

