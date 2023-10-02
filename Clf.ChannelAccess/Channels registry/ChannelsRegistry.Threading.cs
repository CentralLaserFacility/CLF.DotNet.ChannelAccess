//
// ChannelsRegistry_threading.cs
//

using System.Collections.Generic ;
using System.Linq ;
using FluentAssertions ;
using Clf.ChannelAccess.ExtensionMethods ;
using Clf.Common.ExtensionMethods ;
using System.Diagnostics.CodeAnalysis ;

namespace Clf.ChannelAccess
{

  partial class ChannelsRegistry 
  {

    // This tells us the ID of the thread that initially created the ChannelsRegistry instance

    private readonly static int MainThreadId = System.Environment.CurrentManagedThreadId ;

    private static System.Threading.SynchronizationContext? OriginalSynchronizationContext ;

    public static bool SynchronizationContextSupportsResumingOnCallingThread => OriginalSynchronizationContext != null ;

    public static void VerifyCurrentThreadIsMainThread ( )
    {
      System.Environment.CurrentManagedThreadId.Should().Be(MainThreadId) ;
    }

    // The static constructor will have captured the SynchronisationContext
    // associated with the Application. If the app is a WPF app or a WinUI app,
    // or a Blazor WASM app, the synchronisation context will be non-null
    // and we can ask it to invoke a 'callback' delegate on the UI thread.
    //

    public static void PostOrInvoke ( 
      System.Threading.SendOrPostCallback sendOrPostCallbackDelegate, 
      object?                             stateParameterToPassToDelegate 
    ) {
      if ( OriginalSynchronizationContext is not null )
      {
        // Ask the SynchronizationContext to invoke our 'callback'
        // on the UI thread associated with the app, passing in
        // the specified 'state' value that will be supplied as 
        // an argument to the callback function.
        // The current thread resumes immediately after the 'Post',
        // having placed the callback and the 'state' on a queue that
        // will be polled by the UI thread. At some future time,
        // the UI thread will get around to invoking the callback function.
        // Invoking the callback might throw an exception,
        // but it'll happen on the UI thread that we're posting to,
        // so there's no need for a 'catch' here.
        OriginalSynchronizationContext.Post(
          sendOrPostCallbackDelegate,
          stateParameterToPassToDelegate
        ) ;
      }
      else
      { 
        // There's no SynchronizationContext available, so our only
        // option is to 'directly' invoke the callback delegate
        // on the current thread ...
        try
        {
          sendOrPostCallbackDelegate(
            stateParameterToPassToDelegate
          ) ;
        }
        catch ( System.Exception x )
        {
          x.ToString(); //TODO: Handle exception in Log... suppressing warning
          // We invoked the callback delegate on our own thread,
          // and it threw a naughty exception which we'll ignore
          // NotifyExceptionCaught(null,x) ; // REVIEW_THIS ...
        }
      }
    }

  }

}
