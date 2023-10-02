//
// ViewModel_01.cs
//

using Clf.ChannelAccess.ExtensionMethods;
using Clf.Common.ExtensionMethods;
using System.Threading.Tasks;

namespace Clf_ChannelAccess_UsageExamples
{

  public abstract class ObservableObject 
  : System.ComponentModel.INotifyPropertyChanged
  {
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged ;
    protected void OnPropertyChanged ( string propertyName )
    {
      var context = System.Threading.SynchronizationContext.Current ;
      PropertyChanged?.Invoke(
        propertyName, 
        new System.ComponentModel.PropertyChangedEventArgs(propertyName)
      ) ;
    }
    protected void OnDependentPropertiesChanged ( params string[] propertyNames )
    {
      var context = System.Threading.SynchronizationContext.Current ;
      propertyNames.ForEachItem(
        OnPropertyChanged
      ) ;
    }
  }

  public sealed class ViewModel_01 
  // : ObservableObject
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

    public object? A_ValueAsObject => A_Value ;

    public int? A_Value { get ; private set ; }

    public bool A_ValueIsEven => A_Value % 2 == 0 ;

    public bool A_IsConnected { get ; private set ; }

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
       A_IsConnected 
    && A_Value.HasValue 
    ) ;

    private bool m_isBlazorServerApp = System.Threading.SynchronizationContext.Current?.GetType().Name.Contains("RendererSynchronizationContext") == true ;

    public ViewModel_01 ( )
    {
      m_pv_A = Clf.ChannelAccess.Hub.GetOrCreateChannel(m_pvName_A) ;
      // var _ = m_pv_A.HasConnectedAndAcquiredValueAsync() ;
      // Hmm, need to implement IDisposable ... 
      ///////////// m_pv_A.StateChanged += A_StateChanged ;
    }

    // This helper can be in a base class.
    // Equivalent to the MVVM Toolkit 'SetProperty' function,
    // but works with properties rather than fields.

    private bool UpdateAccepted<T> (
      T                proposedNewValue,
      System.Func<T>   getCurrentValue,
      System.Action<T> setNewValue,
      string ?         propertyName = null
    ) {
      bool valueChangeWasAccepted ;
      T currentValue = getCurrentValue() ;
      if ( object.Equals(currentValue,proposedNewValue) )
      {
        valueChangeWasAccepted = false ;
      }
      else
      {
        setNewValue(proposedNewValue) ;
        if ( propertyName != null )
        {
          OnPropertyChanged(propertyName) ;
        }
        valueChangeWasAccepted = true ;
      }
      return valueChangeWasAccepted ;
    }

    // Check how this performs equality comparisons !!!

    private bool UpdateAcceptedEx<T> (
      T                                                  proposedNewValue,
      System.Linq.Expressions.Expression<System.Func<T>> getCurrentValueExpression,
      System.Action<T>                                   setNewValueAction,
      params string[]                                    dependentPropertyNames
    ) 
    // where T : System.IEquatable<T>
    {
      bool valueChangeWasAccepted ;
      System.Linq.Expressions.Expression<System.Func<T>> x = getCurrentValueExpression ;
      // Hmm, could cache this - but the overhead is probably insignificant
      var compiledExpression = getCurrentValueExpression.Compile() ;
      T currentValue = compiledExpression.Invoke() ;
      if ( object.Equals(currentValue,proposedNewValue) )
      {
        valueChangeWasAccepted = false ;
      }
      else
      {
        setNewValueAction(proposedNewValue) ;
        string propertyName = getCurrentValueExpression.Body.TreatedAs<
          System.Linq.Expressions.MemberExpression
        >().Member.Name ;
        OnPropertyChanged(propertyName) ;
        if ( m_isBlazorServerApp )
        {
          // If we're a Blazor app, updating any property
          // will cause the entire component to re-render,
          // so raising 'PropertyChanged' on dependent properties
          // is not necessary or desirable ...
        }
        else
        {
          dependentPropertyNames.ForEachItem(
            OnPropertyChanged
          ) ;
        }
        valueChangeWasAccepted = true ;
      }
      return valueChangeWasAccepted ;
    }

    private void A_StateChanged ( 
      Clf.ChannelAccess.StateChange  change, 
      Clf.ChannelAccess.ChannelState newChannelState 
    ) {
      if ( change.DescribesConnectionStatusChange( out var isConnected ) )
      {
        bool valueChanged ;
        if (
          // base.SetProperty(
          //   ref A_IsConnected,
          //   isConnected.Value,
          //   nameof(A_IsConnected)
          // )
          valueChanged = UpdateAccepted(
            proposedNewValue : isConnected.Value,
            getCurrentValue  : () => A_IsConnected,
            setNewValue      : newValue => A_IsConnected = newValue
          )
        ) {
          // Rather than supply the property name to the 'Update' method,
          // we issue 'OnPropertyChanged' here, not only for the property
          // that was updated but also for other 'dependent' properties
          // whose values will potentially have been affected by the change.
          OnPropertyChanged(
            nameof(A_IsConnected)
          ) ;
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
        if ( valueInfo.ValueAsObject is int incomingValue )
        {
          bool valueChanged ;
          if (
            valueChanged = UpdateAcceptedEx(
              proposedNewValue          : incomingValue,
              getCurrentValueExpression : () => A_Value,
              setNewValueAction         : newValue => A_Value = newValue,
              // Dependent property names :
              nameof(A_ValueAsObject),
              nameof(A_IsValid),
              nameof(A_ValueIsEven)
            )
          ) {
            // OnPropertyChanged(
            //   nameof(A_Value)
            // ) ;
            // No need to update these on Blazor ???
            // OnPropertyChanged(
            //   nameof(A_ValueAsObject)
            // ) ;
            // OnPropertyChanged(
            //   nameof(A_IsValid)
            // ) ;
            // OnPropertyChanged(
            //   nameof(A_ValueIsEven)
            // ) ;
          }
        }
      }
    }

  }

}
