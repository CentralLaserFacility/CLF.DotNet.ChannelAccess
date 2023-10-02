//
// ViewModel_01_old_01.cs
//

using Clf.ChannelAccess.ExtensionMethods ;
using System.Threading.Tasks ;

namespace Clf_ChannelAccess_UsageExamples_old_01
{

  public sealed class ViewModel_01
  : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
  // , System.IDisposable TODO !!! YES !!!
  {

    private string m_pvName_A = "xx:one_long" ;

    private Clf.ChannelAccess.IChannel m_pv_A ;

    // Given that we have 'A_IsValid', should this be Nullable ??
    // If not, then we'd have to provide a 'default' value ...

    private int? m_A_Value = null ;
    public int? A_Value
    {
      get => m_A_Value ;
      private set {
        // Hmm, we don't want to allow a null value to be set !!!
        // And actually, DO WE WANT TO MAKE THIS SETTABLE ???
        if (
          base.SetProperty(
            ref m_A_Value,
            value
          )
        ) {
          OnPropertyChanged(
            nameof(A_IsValid)
          ) ;
          if ( value.HasValue)
          {
            m_pv_A.PutValueAsync(value.Value) ;
          }
        }
      }
    }

    // Hmm, NASTY !!!

    public async void Put_A_Value_Async ( int value )
    {
      await m_pv_A.PutValueAsync(
        value
      ) ;
    }

    public void Put_A_Value_FireAndForget ( int value )
    {
      // ??????????????? NASTY ...
      Task _ = Task.Run(
        async () => await m_pv_A.PutValueAsync(
          value
        )
      ) ;
    }

    private bool m_A_IsValid = false ;
    public bool A_IsValid
    {
      get => m_A_IsValid ;
      private set {
        base.SetProperty(
          ref m_A_IsValid,
          value
        ) ;
      }
    }

    public ViewModel_01 ( )
    {
      // ??? Do we need an out parameter here to say whether the
      // channel has been Created ?? If so, what would we do differently ???
      m_pv_A = Clf.ChannelAccess.Hub.GetOrCreateChannel(m_pvName_A) ;
      // Hmm, what's the best way of ensuring we do -= ???
      // Implement IDisposable ??? 
      //////////////// m_pv_A.StateChanged += A_StateChanged ;
    }

    private void A_StateChanged ( 
      Clf.ChannelAccess.StateChange  change, 
      Clf.ChannelAccess.ChannelState newChannelState 
    ) {
      if ( change.DescribesConnectionStatusChange( out var isConnected ) )
      {
        A_IsValid = (
           A_Value.HasValue
        && isConnected.Value
        ) ;
      }
      else if ( change.DescribesValueChange( out var valueInfo ) )
      {
        A_Value = valueInfo.ValueAsObject as int? ;
        A_IsValid = (
           A_Value.HasValue
        && newChannelState.IsConnected
        ) ;
      }
    }

  }

}
