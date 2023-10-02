//
// MyClassThatNeedsToBeDisposed.cs
//

namespace Clf.ChannelAccess.Experimental
{

  //
  // Hmm, the idea here is that we can rely on a Finaliser to
  // eventually clean up our object's resources, if a client
  // forgets to invoke IDisposabe.Dispose().
  //
  // By 'resources' we mean not only 'unmanaged' resources such
  // as Win32 file handles, but also : revoking registrations of
  // interest in events published by other objects :
  //
  //    OtherObject.SomeEvent -= MyEventHandler ;
  //
  //    WeakReferenceMessenger.UnRegister(this) ;
  //
  // If we forget to '-=' an Event, that represents a memory leak.
  //
  // Omitting to UnRegister from the WeakReferenceMessenger isn't so bad
  // in that it doesn't cause a memory leak ; our instance will not be prevented
  // from being garbage-collected by that registration. However, until such time as
  // the garbage collector has to do a GC pass, our instance will remain active
  // and will receive messages and take action !!!
  //
  // MIGHT BE BETTER TO RELY ON AN ANALYSER TO DETECT MISSING DISPOSES ???
  //   

  public class MyClassThatNeedsToBeDisposed : System.IDisposable
  {

    // See 
    // https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
    // C# 5 Unleashed, Bart DeSmet, p606 etc.
    //
    // This class is declared as 'IDisposable' in order that clients
    // can invoke 'Dispose()' when they no longer need the instance.
    //
    // Dispose can release resources used by the instance,
    // for example
    //
    //   - Calling functions such as 'Close' on a Win32 file handle,
    //     which is an 'unmanaged resource'
    //     It's essential that this is done, otherwise Windows will
    //     keep that file open and no-one else will be able to access it.
    //
    //   - Setting references to large data arrays to null.
    //     That is less crucial as that data is a managed resource
    //     that will be cleaned up by the garbage collector as soon as
    //     the GC detects that no-one is referring to the array.
    //     However, setting our reference to null might give the GC
    //     an opportunity to reclaim them memory a bit sooner.
    //
    // If client code forgets to call Dispose(), what can we do ??
    //
    // Defining a finaliser gives us an opportunity to have the
    // Garbage Collector invoke our 'Dispose' method while it's
    // reclaiming the memory that was used by the instance.
    // 

    private System.Action<string> m_writeMessageLine ;

    private readonly int m_instanceNumber ;

    private static int m_nInstancesCreated = 0 ;

    public MyClassThatNeedsToBeDisposed ( System.Action<string>? writeMessageLine = null ) 
    {
      m_instanceNumber = ++m_nInstancesCreated ;
      m_writeMessageLine = writeMessageLine ?? WriteLine ;
      m_writeMessageLine(
        $"Instance #{m_instanceNumber} ctor"
      ) ;
    }

    // This finaliser will be invoked by the garbage collector, on the GC thread,
    // when a garbage collection occurs. Depending on memory pressure, this might
    // be a very long time after the instance becomes *eligible* for garbage collection.

    ~MyClassThatNeedsToBeDisposed ( ) 
    {
      Dispose(
        // Indicate that the 'Dispose' has been invoked
        // via the finaliser ... that is, when the GC has determined
        // that this instance is no longer being referred to.
        wasCalledFromClientCode : false
      ) ;
    }

    // Public method as promised by 'System.IDisposable'.

    public void Dispose ( )
    {
      // Call our private 'helper' method
      Dispose(
        // Here we indicate that the 'Dispose' has been invoked
        // explicitly by client code, ie not via the finaliser ...
        wasCalledFromClientCode : true
      ) ;
      // We've defined a finaliser, but since we've just explicity
      // performed the Dispose, there would be nothing further
      // for Dispose(false) to do, even if it was to be called
      // by the finaliser.
      System.GC.SuppressFinalize(this) ;
    }

    // This flag lets us protect against doing the Dispose
    // work more than once, in cases where client code
    // calls Dispose() several times.
    //
    // Ideally we should check this flag each time 
    // any method is called, just in case client code
    // invokes 'Dispose()' but then continues to call
    // methods on the object ...

    private bool m_thisInstanceHasBeenDisposed = false ;

    // This does the actual work of 'disposing'.
    // It's unnecessarily confusing to be calling this Dispose(),
    // and relying on the boolean parameter to distinguish it from IDisposable.Dispose(),
    // but that's the conventional name so we've stuck with it.

    private void Dispose ( bool wasCalledFromClientCode )
    {
      m_writeMessageLine(
        $"Instance #{m_instanceNumber} Dispose(wasCalledFromClientCode:{wasCalledFromClientCode})"
      ) ;
      if ( m_thisInstanceHasBeenDisposed )
      {
        // Unusual, Dispose() has been invoked more than once.
        // Perhaps issue a warning ??
        m_writeMessageLine(
          $"  m_thisInstanceHasBeenDisposed is TRUE !!!"
        ) ;
        return ;
      }
      bool wasCalledFromFinaliserByGC = !wasCalledFromClientCode ;
      if ( wasCalledFromFinaliserByGC )
      {
        // Hmm, unusual, because ideally the client code
        // should have explicitly 'Disposed' the instance.
        // Maybe issue a warning ???
        // Anyhow, we still need to clean up resources
        // so let's continue ...
      }
      if ( wasCalledFromClientCode )
      {
        // TODO : set object references to null
      }
      // TODO : call 'Close()' on unmanaged resources eg file handles
      // TODO : if our base class is IDisposable 
      // base.Dispose() ;
    }

    public static void WriteLine ( string line )
    {
      System.Diagnostics.Debug.WriteLine(
        $"Thread #{System.Environment.CurrentManagedThreadId:D2} : {line}"
      ) ;
    }

    public static void RunTest_01 ( )
    {
      using var instance = new MyClassThatNeedsToBeDisposed() ;
    }

    public static void RunTest_02 ( )
    {
      var instance = new MyClassThatNeedsToBeDisposed() ;
      instance = null ;
      // System.GC.Collect(
      //   System.GC.MaxGeneration,
      //   System.GCCollectionMode.Forced,
      //   blocking : true,
      //   compacting : true
      // ) ;
      System.GC.Collect() ;
      System.GC.WaitForPendingFinalizers() ;
      System.GC.Collect() ;
    }

  }

}
