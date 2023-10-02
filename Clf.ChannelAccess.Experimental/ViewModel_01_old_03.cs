//
// ViewModel_01.old_03.cs
//

using Clf.ChannelAccess.ExtensionMethods ;
using System.Threading.Tasks ;

namespace Clf_ChannelAccess_UsageExamples.old_03
{

  public sealed class ViewModel_01
  : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
  // , System.IDisposable TODO !!!
  {

    private string m_pvName_A = "xx:one_long" ;

    private Clf.ChannelAccess.IChannel m_pv_A ;

    // private async Task MyFunc ( )
    // {
    //   await Task.Delay(0) ;
    // }

    public string A_PvName => m_pvName_A ;

    public int A_DefaultValue => 42 ;

    private int? m_A_Value = null ;
    public int? A_Value => m_A_Value ;

    public object? A_ValueAsObject => m_A_Value ;

    public bool A_ValueIsEven => m_A_Value % 2 == 0 ;

    private bool m_A_IsConnected = false ;
    public bool A_IsConnected => m_A_IsConnected ;

    public void Put_A ( int value )
    {
      m_pv_A.PutValue(
        value
      ) ;
    }

    public void Increment_A ( )
    {
      if ( A_Value.HasValue )
      {
        Put_A(
          A_Value.Value + 1
        ) ;
      }
      else
      {
        Put_A(0) ;
      }
    }

    public void SetDefault_A ( )
    {
      Put_A(
        A_DefaultValue
      ) ;
    }

    public bool A_IsValid => (
       m_A_IsConnected 
    && m_A_Value.HasValue 
    ) ;

    public ViewModel_01 ( )
    {
      m_pv_A = Clf.ChannelAccess.Hub.GetOrCreateChannel(m_pvName_A) ;
      // var _ = m_pv_A.HasConnectedAndAcquiredValueAsync() ;
      // Hmm, need to implement IDisposable ... 
      ////////////// m_pv_A.StateChanged += A_StateChanged ;
    }

    private void A_StateChanged ( 
      Clf.ChannelAccess.StateChange  change, 
      Clf.ChannelAccess.ChannelState newChannelState 
    ) {
      System.Diagnostics.Debug.WriteLine(
        $"CHANGE : {change}"
      ) ;
      if ( change.DescribesConnectionStatusChange( out var isConnected ) )
      {
        System.Diagnostics.Debug.WriteLine(
          $"Connected => {isConnected}"
        ) ;
        if (
          base.SetProperty(
            ref m_A_IsConnected,
            isConnected.Value,
            nameof(A_IsConnected)
          )
        ) {
          OnPropertyChanged(
            nameof(A_IsValid)
          ) ;
          OnPropertyChanged(
            nameof(A_ValueIsEven)
          ) ;
        }
      }
      else if ( change.DescribesValueChange( out var valueInfo ) )
      {
        System.Diagnostics.Debug.WriteLine(
          $"Value change => {
            valueInfo.Value_AsDisplayString(
              Clf.ChannelAccess.WhichValueInfoElementsToInclude.AllAvailableElements
            )
          }"
        ) ;
        if ( valueInfo.ValueAsObject is int a_value )
        {
          bool valueChanged ;
          if (
            valueChanged = base.SetProperty(
              ref m_A_Value,
              a_value,
              nameof(A_Value)
            )
          ) {
            OnPropertyChanged(
              nameof(A_IsValid)
            ) ;
            OnPropertyChanged(
              nameof(A_ValueAsObject)
            ) ;
            OnPropertyChanged(
              nameof(A_ValueIsEven)
            ) ;
          }
        }
      }
    }

  }

}
