//
// ChannelsRegistry_static_constructor.cs
//

using System.Collections.Generic ;
using System.Linq ;
using System.Threading.Tasks ;
using FluentAssertions ;

using Clf.Common.ExtensionMethods ;
using static Clf.ChannelAccess.LowLevelApi.EcaCodeExtensionMethods ;
using Clf.ChannelAccess.ExtensionMethods ;

namespace Clf.ChannelAccess
{

  partial class ChannelsRegistry 
  {

    static ChannelsRegistry ( )
    {

      OriginalSynchronizationContext = System.Threading.SynchronizationContext.Current ;
      string originalContextTypeName = "null" ;
      if ( OriginalSynchronizationContext is null )
      {
        // Fair enough, eg this is a console app
      }
      else
      {
        originalContextTypeName = OriginalSynchronizationContext.GetType().FullName! ;
        if ( originalContextTypeName.Contains("AsyncPump") ) 
        {
          // This is an app using our 'AsyncPump' app so Posting will work !!
        }
        else if ( originalContextTypeName.Contains("WindowsFormsSynchronizationContext") ) 
        {
          // This is a WinForms app so Posting will work !!
        }
        else if ( originalContextTypeName.Contains("DispatcherSynchronizationContext") ) 
        {
          // This is a WPF app so Posting will work !!
        }
        else if ( originalContextTypeName.Contains("DispatcherQueueSynchronizationContext") ) 
        {
          // This is a WinUI app so Posting will work !!
        }
        else if ( originalContextTypeName.Contains("Render") ) 
        {
          // OK, this is a Blazor server app so Posting won't work ?
          // CHECK THIS WITH AOUN !!!
          // OriginalSynchronizationContext = null ;
        }
        else
        {
          // Not recognised, so play safe ...
          OriginalSynchronizationContext = null ;
        }
        SendMessageToSystemLog(
          Common.LogMessageLevel.InformationalMessage,
          OriginalSynchronizationContext is null
          ? $"Synchronisation context is null, events will be raised on worker thread"
          : $"Synchronisation context is {originalContextTypeName}, events will be posted to UI thread"
        ) ;
      }

      Clf.ChannelAccess.EpicsDllFunctions.EnsureAvailable() ;

      try
      {
        
        //
        // If any 'ca_' functions have been called already, then a 'default context'
        // will have been created with 'pre-emptive callback' disabled.
        // If that's the case, it's an error !!
        //

        LowLevelApi.DllFunctions.ca_current_context().IsNull.Should().BeTrue() ;

        //
        // This, or 'ca_attach_context', must be called at least once on every thread.
        // Since we'll be using callbacks, we specify 'preemptive:true'.
        //

        LowLevelApi.DllFunctions.ca_context_create(
          preemptive : true
          // preemptive : false // FOR TESTING LEWIS STUFF !!! DOESN'T WORK ... WE REQUIRE PRE-EMPTIVE :)
        ) ;

        ClientContextHandle = LowLevelApi.DllFunctions.ca_current_context() ;
        ClientContextHandle.IsValidHandle.Should().BeTrue() ;

        Version = LowLevelApi.DllFunctions.ca_version() ;

        //
        // Hmm, this DOESN'T stop exception messages being written to the console !!
        //

        LowLevelApi.DllFunctions.ca_add_exception_event(
          DllCallbackHandlers.ExceptionCallbackHandler,
          System.IntPtr.Zero
        ) ;

        //
        // Hmm, this DOESN'T stop informational messages being written to the console !!
        // But it *does* intercept certain 'CA repeater' messages ...
        //

        LowLevelApi.DllFunctions.ca_replace_printf_handler(
          DllCallbackHandlers.PrintfCallbackHandler
        ) ;

      }
      catch ( System.Exception x )
      {
        // THIS IS VERY BAD NEWS INDEED !!!
        // System.Diagnostics.Debugger.Break() ;
        x.ToString(); //TODO: Handle exception in Log... suppressing warning
        throw ;
      }

      // ClientContextHandle.IsValidHandle.Should().BeTrue() ;

    }

  }

}
