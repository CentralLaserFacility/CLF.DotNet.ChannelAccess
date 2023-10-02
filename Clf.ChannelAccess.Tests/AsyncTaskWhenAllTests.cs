//
// AsyncTaskWhenAllTests.cs
//

using Xunit ;
using FluentAssertions ;
using System.Threading.Tasks ;
using Clf.Common.ExtensionMethods ;
using Clf.ChannelAccess.ExtensionMethods ;
using System.Linq ;

namespace Clf_ChannelAccess_Tests
{

  public class AsyncTaskWhenAllTests 
  {

    private static Xunit.Abstractions.ITestOutputHelper? g_output ;

    public AsyncTaskWhenAllTests ( Xunit.Abstractions.ITestOutputHelper? testOutputHelper )
    {
      g_output = testOutputHelper ;
      g_output?.WriteLine(
        $"Test starting at {System.DateTime.Now.ToString("h:mm:ss.fff")}"
      ) ;
    }

    [Theory]
    [InlineData(123)]
    [InlineData(999)]
    public async Task Test_01 ( int arg )
    {
      bool exceptionWasThrown = false ;
      try
      {
        await Task.Run(
          () => {
            if ( arg == 999 )
            {
              throw new System.Exception() ;
            }
          }
        ) ;
        g_output?.WriteLine(
          $"Test completed with no exception"
        ) ;       
      }
      catch ( System.Exception x )
      {
        g_output?.WriteLine(
          $"Test threw an exception : {x.Message}"
        ) ;  
        exceptionWasThrown = true ;
      }
      exceptionWasThrown.Should().Be(arg == 999) ;
    }

    private int DelayTime_Millisecs = 110 ;

    // Tasks returning no result

    [Theory]
    [InlineData(new[]{1,1,1})]
    [InlineData(new[]{1,1,0})]
    [InlineData(new[]{0,1,0})]
    [InlineData(new[]{0,0,0})]
    public async Task Test_02 ( int[] args )
    {
      bool exceptionWasThrown = false ;
      int iTask = 0 ;
      Task[] tasksReturningNoResult = args.Select(
        arg => CreateTaskReturningNoResult(iTask++,arg)
      ).ToArray() ;
      // Declare this here so that even when we've performed 
      // the 'await', we can access the task's properties to
      // query its status and the 'AggregateException' that
      // will tell us about the individual exceptions ...
      Task task_whenAll = Task.WhenAll(
        tasksReturningNoResult
      ) ;
      System.Diagnostics.Stopwatch stopwatch = new() ;
      stopwatch.Start() ;
      try
      {
        // If all of our Tasks run to completion without throwing
        // an exception, 'WaitAll' just returns nothing. 
        // If one or more of the tasks *do* throw an exception,
        // then the first of those exceptions will be thrown
        // at the point we do this 'await'. However the 'await'
        // does not actually return until *all* the tasks have reached
        // a 'Completed' state, either because they succeeded
        // (with 'IsCompleted'==true) or because they threw
        // an exception ('IsFaulted'==true). 
        // The 'await' takes as long as it takes !!!
        // If one of our tasks takes several seconds to either return
        // or to throw an exception, so be it ...
        await task_whenAll ;
        stopwatch.Stop() ;
        g_output?.WriteLine(
          $"await Task.WhenAll() completed with no exception after {stopwatch.ElapsedMilliseconds} millisecs"
        ) ; 
        task_whenAll.IsCompleted.Should().BeTrue() ;
        task_whenAll.IsCompletedSuccessfully.Should().BeTrue() ;
      }
      catch ( System.Exception x )
      {
        stopwatch.Stop() ;
        g_output?.WriteLine(
          $"await Task.WhenAll() threw an exception after {stopwatch.ElapsedMilliseconds} millisecs : '{x.Message}'"
        ) ;  
        task_whenAll.IsCompleted.Should().BeTrue() ;
        task_whenAll.IsFaulted.Should().BeTrue() ;
        exceptionWasThrown = true ;
      }
      // Did we expect the exception to be thrown ??
      exceptionWasThrown.Should().Be(
        args.Any(
          arg => arg == 0
        )
      ) ;
      // All our individual tasks are now in a Completed state.
      tasksReturningNoResult.ForEachItem(
        task => task.IsCompleted.Should().BeTrue()
      ) ;
      // We can query each task individually to find out
      // whether it 'faulted' (ie threw an exception),
      // and thereby discover the entire set of faulted tasks.
      // As mentioned above, if the 'await' threw an exception
      // it will be telling us about the *first* faulted task,
      // but we might want to know about *all* the faulted tasks,
      // and this one one way of doing it.
      iTask = 0 ;
      tasksReturningNoResult.ForEachItem(
        task => {
          if ( task.IsFaulted )
          {
            System.AggregateException aggregateException = task.Exception! ;
            g_output?.WriteLine(
              $"Task #{iTask} state is 'faulted' : aggregate exception shows '{aggregateException.Message}'"
            ) ;
            aggregateException.InnerExceptions.ForEachItem(
              (innerException,i) => {
                g_output?.WriteLine(
                  $"  Inner exception #{i} shows '{innerException.Message}'"
                ) ;
              }
            ) ;
          }
          else if ( task.IsCompletedSuccessfully )
          {
            g_output?.WriteLine(
              $"Task #{iTask} completed successfully"
            ) ;
          }
          iTask++ ;
        }
      ) ;
      // Another way to find out about *all* the faulted tasks,
      // when the 'await' did throw an exception, is to query that 
      // 'awaited' task's Exception property.
      // This is always an AggregateException, and we can iterate through
      // the collection to discover the individual exceptions.
      if ( task_whenAll.IsFaulted )
      {
        // Note that the AggregateException we get from the 'Exception' property
        // produces a message that mentions *all* the inner exceptions !!!
        System.AggregateException aggregateException = task_whenAll.Exception! ;
        g_output?.WriteLine(
          $"Task 'whenAll' state is 'faulted' : aggregate exception shows '{aggregateException.Message}'"
        ) ;
        aggregateException.InnerExceptions.ForEachItem(
          (innerException,i) => {
            g_output?.WriteLine(
              $"  Inner exception #{i} shows '{innerException.Message}'"
            ) ;
          }
        ) ;
      }
      // Local function
      async Task CreateTaskReturningNoResult ( int iTask, int arg )
      {
        g_output?.WriteLine(
          $"Task #{iTask} is running, with arg = {arg}"
        ) ;
        System.Diagnostics.Stopwatch stopwatch = new() ;
        stopwatch.Start() ;
        await Task.Delay(100 + DelayTime_Millisecs*iTask) ;
        stopwatch.Stop() ;
        if ( arg == 0 )
        {
          g_output?.WriteLine(
            $"Task #{iTask} is about to throw an exception after {stopwatch.ElapsedMilliseconds} mS"
          ) ;
          throw new System.ApplicationException($"#{iTask} : arg == 0") ;
        }
        g_output?.WriteLine(
          $"Task #{iTask} is about to return after {stopwatch.ElapsedMilliseconds} mS"
        ) ;
        // return (
        //   arg == 0 
        //   ? Task.FromException(
        //       new System.ApplicationException($"#{iTask} : arg == 0")
        //     )
        //   : Task.CompletedTask 
        // ) ;
      }
    }

    // Tasks returning a result

    [Theory]
    [InlineData(new[]{1,1,1})]
    [InlineData(new[]{1,1,0})]
    [InlineData(new[]{0,1,0})]
    [InlineData(new[]{0,0,0})]
    public async Task Test_03 ( int[] args )
    {
      bool exceptionWasThrown = false ;
      int iTask = 0 ;
      Task<bool>[] tasksReturningBooleanResult = args.Select(
        arg => CreateTaskReturningBooleanResult(iTask++,arg)
      ).ToArray() ;
      // Declare this 'task_whenAll' task here so that even when we've performed 
      // the 'await', we can subsequently access the task's properties to
      // query its final status, and in particular the 'AggregateException'
      // that will tell us about the individual exceptions that were thrown ...
      Task<bool[]> task_representingWhenAll = Task.WhenAll(
        tasksReturningBooleanResult
      ) ;
      bool[]? resultsArray = null ;
      try
      {
        // If all of our Tasks run to completion without throwing
        // an exception, 'WaitAll' just returns nothing. 
        // If one or more of the tasks *do* throw an exception,
        // then the first of those exceptions will be thrown
        // at the point we do this 'await'. However the 'await'
        // does not actually return until *all* the tasks have reached
        // a 'Completed' state, either because they succeeded
        // (with 'IsCompleted'==true) or because they threw
        // an exception ('IsFaulted'==true). 
        // The 'await' takes as long as it takes !!!
        // If one of our tasks takes several seconds to either return
        // or to throw an exception, so be it ...
        resultsArray = await task_representingWhenAll ;
        g_output?.WriteLine(
          $"await 'task_representingWhenAll' completed with no exception"
        ) ; 
        task_representingWhenAll.IsCompleted.Should().BeTrue() ;
        task_representingWhenAll.IsCompletedSuccessfully.Should().BeTrue() ;
        resultsArray.Should().NotBeNull() ;
        resultsArray!.Length.Should().Be(tasksReturningBooleanResult.Length) ;
      }
      catch ( System.Exception x )
      {
        g_output?.WriteLine(
          $"await 'task_representingWhenAll' threw an exception : '{x.Message}'"
        ) ;  
        task_representingWhenAll.IsCompleted.Should().BeTrue() ;
        task_representingWhenAll.IsFaulted.Should().BeTrue() ;
        resultsArray.Should().BeNull() ;
        exceptionWasThrown = true ;
      }
      // Did we expect the exception to be thrown ??
      exceptionWasThrown.Should().Be(
        args.Any(
          arg => arg == 0
        )
      ) ;
      // OK, we've already checked this ...
      if ( exceptionWasThrown )
      {
        // We won't have *any* result ...
        resultsArray.Should().BeNull() ;
      }
      else
      {
        resultsArray.Should().NotBeNull() ;
        resultsArray!.Length.Should().Be(tasksReturningBooleanResult.Length) ;
      }
      // All our individual tasks are now in a Completed state.
      // We expect each and every result to be 'true'
      tasksReturningBooleanResult.ForEachItem(
        task => {
          task.IsCompleted.Should().BeTrue() ;
          if ( task.IsCompletedSuccessfully )
          {
            task.Result.Should().BeTrue() ;
          }
        }
      ) ;
      // We can query each task individually to find out
      // whether it 'faulted' (ie threw an exception),
      // and thereby discover the entire set of faulted tasks.
      // As mentioned above, if the 'await' threw an exception
      // it will be telling us about the *first* faulted task,
      // but we might want to know about *all* the faulted tasks,
      // and this one one way of doing it.
      iTask = 0 ;
      tasksReturningBooleanResult.ForEachItem(
        task => {
          if ( task.IsFaulted )
          {
            System.AggregateException aggregateException = task.Exception! ;
            g_output?.WriteLine(
              $"Task #{iTask} state is 'faulted' : aggregate exception shows '{aggregateException.Message}'"
            ) ;
            aggregateException.InnerExceptions.ForEachItem(
              (innerException,i) => {
                g_output?.WriteLine(
                  $"  Inner exception #{i} shows '{innerException.Message}'"
                ) ;
              }
            ) ;
            // If we were to attempt to access the 'Result' property,
            // then (quite rightly!) an Exception will be thrown,
            // because this task threw an exception and didn't produce a result
            task.Invoking(
              task => task.Result
            ).Should().Throw<System.AggregateException>() ;
          }
          else if ( task.IsCompletedSuccessfully )
          {
            g_output?.WriteLine(
              $"Task #{iTask} completed successfully with result '{task.Result}'"
            ) ;
          }
          iTask++ ;
        }
      ) ;
      // Another way to find out about *all* the faulted tasks,
      // when the 'await' did throw an exception, is to query that 
      // 'awaited' task's Exception property.
      // This is always an AggregateException, and we can iterate through
      // the collection to discover the individual exceptions.
      if ( task_representingWhenAll.IsFaulted )
      {
        // Note that the AggregateException we get from the 'Exception' property
        // produces a message that mentions *all* the inner exceptions !!!
        System.AggregateException aggregateException = task_representingWhenAll.Exception! ;
        g_output?.WriteLine(
          $"Task 'whenAll' state is 'faulted' : aggregate exception shows '{aggregateException.Message}'"
        ) ;
        aggregateException.InnerExceptions.ForEachItem(
          (innerException,i) => {
            g_output?.WriteLine(
              $"  Inner exception #{i} shows '{innerException.Message}'"
            ) ;
          }
        ) ;
      }
      // Local function
      Task<bool> CreateTaskReturningBooleanResult ( int iTask, int arg )
      {
        g_output?.WriteLine(
          $"Task #{iTask} is running, with arg = {arg}"
        ) ;
        return (
          arg == 0 
          ? Task.FromException<bool>(
              new System.ApplicationException($"#{iTask} : arg == 0")
            )
          : Task.FromResult(true) 
        ) ;
      }
    }

  }

}

