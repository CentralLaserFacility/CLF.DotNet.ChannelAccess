//
// ViewModel_UsingEvents_01.cs
//

using System.Threading.Tasks ;
using Clf.ChannelAccess.ExtensionMethods ;

namespace Clf_ChannelAccess_UsageExamples
{

  //
  // Example illustrating a reasonable way of instantiating Channels
  // and giving them a chance to connect.
  //
  // Hmm, in a 'real' app we should be using Dependency Injection to
  // access the ViewModels !!!
  //

  public class ViewModel_UsingEvents_01 : System.IDisposable // Also inherits from a ViewModelBase class ...
  {

    public static async Task<ViewModel_UsingEvents_01> CreateInstanceAsync ( )
    {
      ViewModel_UsingEvents_01 myViewModel = new() ;
      bool allChannelsConnected = await myViewModel.TryFinishInitialisationAsync() ;
      return myViewModel ;
    }

    // Public read-only properties reporting values acquired from our Channels.
    // Note that because we've 'awaited' all our channels before making this
    // view-model instance available, we can guarantee that 'Value()' will
    // never return null.

    // public double SomePropertyBeingReported => (double) m_channel_A.Value()! ;

    public double SomeOtherPropertyBeingReported => ( (double?) m_channel_B.Value() ) ?? 999.0 ;

    // Private variables representing our channels.
    // Note that these don't have to be declared as 'IChannel?'
    // because they are never null - they're created in the constructor.

    private Clf.ChannelAccess.IChannel m_channel_A ;

    private Clf.ChannelAccess.IChannel m_channel_B ;

    private Clf.ChannelAccess.IChannel m_channel_C ;

    public ViewModel_UsingEvents_01 ( )
    {
      // In the constructor we can create all our IChannel instances.
      // Each instance will attempt to create a connection to a PV.
      // If that PV is available on the network, that channel will receive
      // a 'connected' message and shortly afterwards a 'value-changed' message
      // that tells us its current value (and data type etc).
      m_channel_A = Clf.ChannelAccess.Hub.GetOrCreateChannel("aaa") ;
      // Don't do this !!! Because when it fires, it runs code that
      // expects the value of 'C' to be available. So we should ****************** EXPLAIN WHY NOT !!!!
      // m_channel_A.StateChanged += Channel_A_StateChanged ;
      m_channel_B = Clf.ChannelAccess.Hub.GetOrCreateChannel("bbb") ;
      m_channel_C = Clf.ChannelAccess.Hub.GetOrCreateChannel("ccc") ;
    }

    private void Channel_A_StateChanged ( Clf.ChannelAccess.StateChange stateChange, Clf.ChannelAccess.ChannelState state )
    {
      // if ( m_channel_C.Value() is null )
      // {
      //   object x = m_channel_C.GetValue() ; // We need to know that the Value is available !!!
      // }
    }

    private async void Channel_B_StateChanged ( Clf.ChannelAccess.StateChange stateChange, Clf.ChannelAccess.ChannelState state )
    {
      if ( m_channel_C.Value() is null )
      {
        object x = await m_channel_C.GetValueAsync() ; // We need to know that the Value is available !!!
      }
    }

    private void Channel_C_StateChanged ( Clf.ChannelAccess.StateChange stateChange, Clf.ChannelAccess.ChannelState state )
    {
    }

    public bool AllChannelsConnectedSuccessfully { get ; private set ; }

    // Returns true when all channels have initialised,
    // ie have connected and acquired their values ...

    public async Task<bool> TryFinishInitialisationAsync ( )
    {
      // Here we wait a while for all our channels to report that they've
      // successfully connected. Once that's happened, we can query any
      // of their values etc and also set up event handlers that will 
      // tell us about subsequent changes.
      var allChannels = new[]{
        m_channel_A,
        m_channel_B, 
        m_channel_C
      } ;
      // This 'AllChannelsConnectedSuccessfully' flag could just be a local variable,
      // but it's useful for client code to know whether or not all the channels connected ...
      AllChannelsConnectedSuccessfully = await allChannels.WaitForAllChannelsToConnectAndAcquireValues() ;
      if ( AllChannelsConnectedSuccessfully )
      {
        // Now that we're sure that all channels have connected,
        // we can set up event handlers that will respond to changes.
        // Safe in the knowledge that any channel may interact with
        // any other channel (eg to get its current value) without
        // any risk that the value might not yet be available.
        /////////////////// m_channel_A.StateChanged += Channel_A_StateChanged ;
        /////////////////// m_channel_B.StateChanged += Channel_B_StateChanged ;
        /////////////////// m_channel_C.StateChanged += Channel_C_StateChanged ;
        return true ;
      }
      else
      {
        // ?? // m_channel_A.StateChanged += Channel_A_StateChanged ;
        // ?? // m_channel_B.StateChanged += Channel_B_StateChanged ;
        // ?? // m_channel_C.StateChanged += Channel_C_StateChanged ;
        // Hmm, not all our channels were available !!
        // This is bad, and how we deal with this depends 
        // on the particular situation : which channels failed,
        // which channels were critical, and so on.
        // One option would be to just keep trying,
        // ie jump back to do the 'await' again ... ???
        allChannels.ForEachChannelThatFailedToConnectAndAcquireValue(
          channel => {
            // Log a warning message ???
          }
        ) ;
        return false ;
      }
    }

    // We need to ensure that when the ViewModel ceases to exist,
    // 'Dispose' is called on all our Channels ...

    private bool m_disposeHasBeenInvoked = false ;

    protected virtual void Dispose ( bool disposing )
    {
      // Hmm, pretty fiddly and error prone ...
      if ( ! m_disposeHasBeenInvoked )
      {
        if ( disposing )
        {
          // Clean up our references to channel A
          //////////////// m_channel_A.StateChanged -= Channel_A_StateChanged ;
          //////////////// m_channel_A.Dispose() ;
          //////////////// // Clean up our references to channel B
          //////////////// m_channel_B.StateChanged -= Channel_B_StateChanged ;
          //////////////// m_channel_B.Dispose() ;
          //////////////// // Clean up our references to channel C
          //////////////// m_channel_C.StateChanged -= Channel_C_StateChanged ;
          //////////////// m_channel_C.Dispose() ;
        }
        m_disposeHasBeenInvoked = true ;
      }
    }

    public void Dispose ( )
    {
      Dispose(disposing:true) ;
      System.GC.SuppressFinalize(this) ;
    }

  }

  public class MyBlazorComponent_01_A // : BlazorComponent
  {

    // Our 'razor' code can access the ViewModel properties.
    //
    // The ViewModel 'instance' is always non-null,
    // and exposes Properties that can be displayed.
    //
    // The 'view' code must accommodate the possibility that
    // any ViewModel property that is reliant on a PV might be null,
    // (or represent a default value) - because it's entirely likely
    // that any given PV will not be available on the network.
    //
    // The ViewModel is 'displayable' as soon as the constructor has completed,
    // but it won't be properly functional until 'InitialiseAsync'
    // has returned.
    //

    private ViewModel_UsingEvents_01 ViewModel ;

    public MyBlazorComponent_01_A ( )
    {
      ViewModel = new ViewModel_UsingEvents_01() ;
    }

    public async Task OnInitialisedAsync ( )
    {
      // await base.OnInitialisedAsync() ;
      bool allOK = await ViewModel.TryFinishInitialisationAsync() ;
    }

    public async Task OnAfterRenderAsync ( )
    {
      await ViewModel.TryFinishInitialisationAsync() ;
    }

  }

  // Another possibility ???

  public class MyBlazorComponent_01_B // : BlazorComponent
  {

    // Our 'razor' code can access the ViewModel properties

    private ViewModel_UsingEvents_01 ViewModel = null! ;

    public MyBlazorComponent_01_B ( )
    {
    }

    public async Task OnInitialisedAsync ( )
    {
      ViewModel = await ViewModel_UsingEvents_01.CreateInstanceAsync() ;
    }

  }


}
