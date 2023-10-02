//
// ViewModel_02.cs
//

using Clf.ChannelAccess.ExtensionMethods ;
using Clf.Common.ExtensionMethods ;
using FluentAssertions ;
using System.Threading.Tasks;

namespace Clf_ChannelAccess_UsageExamples
{

  public abstract class ObservableObject_02 
  : System.ComponentModel.INotifyPropertyChanged
  // , System.IDisposable
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

    public void OnChildPropertyChanged ( ObservableObject_02 child )
    {
      // Hmm, how to get the name of the child property ???
      OnPropertyChanged(
        child.ChildName ?? ""
      ) ;
    }

    public string? ChildName { get ; init ; } // !!!!!!!

  }

  // Subclasses provide strongly typed Value properties.
  // If a value is undefined, attempting to access it will
  // throw an exception. So client code must always either
  // check the 'ValueIsDefined' property.
  // 

  public class ObservablePv_numeric<T> : ObservablePv where T : unmanaged
  {

    public readonly T? Placeholder = null ;

    public ObservablePv_numeric ( Clf.ChannelAccess.ChannelName channelName, T? placeholder = null ) :
    base(channelName)
    { 
      Placeholder = placeholder ;
    }

    protected override void ValueChanged ( )
    => OnDependentPropertiesChanged(
      nameof(Value),
      nameof(ValueOrPlaceholder)
    ) ;
    
    public T Value 
    => (
      ValueAsObject != null
      ? (T) ValueAsObject
      : throw new System.ApplicationException(
          $"Value of {base.PvName} is undefined"
        )
    ) ;

    public T ValueOrPlaceholder
    => (
      ValueAsObject != null
      ? (T) ValueAsObject
      : (
          Placeholder.HasValue
          ? Placeholder.Value
          : throw new System.ApplicationException( 
              $"Value of {base.PvName} is undefined, and no placeholder has been supplied"
            )
        )    
    ) ;

  }

  public class ObservablePv_int : ObservablePv_numeric<int>
  {
    public ObservablePv_int ( 
      Clf.ChannelAccess.ChannelName channelName, 
      int?                          placeholder = null 
    ) :
    base(channelName,placeholder)
    { }
  }

  public class ObservablePv_A : ObservablePv_numeric<int>
  {
    public ObservablePv_A ( 
      Clf.ChannelAccess.ChannelName channelName, 
      int?                          placeholder = null 
    ) :
    base(channelName,placeholder)
    { }
    public bool IsEven 
    => (
      base.ValueIsDefined 
      ? base.Value % 2 == 0
      : false
    ) ;
    // Hmm, this works, but alternatively we could
    // have the constructor call a base class method
    // to install additional 'dependent property' names ...
    protected override void ValueChanged ( )
    {
      base.ValueChanged() ;
      base.OnPropertyChanged(
        nameof(IsEven)
      ) ;
    }
    public void Increment ( )
    {
      base.PutValue(
        ValueIsDefined
        ? Value + 1
        : 0
      ) ;
    }
    public void SetDefault ( )
    {
      base.PutValue(42) ;
    }
  }

  public class ObservablePv_double : ObservablePv_numeric<double>
  {
    public ObservablePv_double ( 
      Clf.ChannelAccess.ChannelName channelName, 
      double?                       placeholder = null 
    ) :
    base(channelName,placeholder)
    { }
  }

  public class ObservablePv : ObservableObject_02
  {

    public ObservableObject_02? Parent { get ; init ; }

    private Clf.ChannelAccess.IChannel m_pvChannel ;

    public ObservablePv ( Clf.ChannelAccess.ChannelName channelName )
    {
      m_pvChannel = Clf.ChannelAccess.Hub.GetOrCreateChannel(channelName) ;
      ////////////////// m_pvChannel.StateChanged += StateChanged ;
    }

    public async Task<bool> ConnectAsync ( )
    {
      bool connected = await m_pvChannel.HasConnectedAndAcquiredValueAsync() ;
      if ( connected )
      {
        // m_pvChannel.StateChanged += StateChanged ;
      }
      return connected ;
    }

    public Clf.ChannelAccess.ChannelName PvName => m_pvChannel.ChannelName ;

    public object? ValueAsObject { get ; private set ; }

    public bool IsConnected { get ; private set ; }

    public bool ValueIsUndefined => ValueAsObject is null ;

    public bool ValueIsDefined => ! ValueIsUndefined ;

    public bool ValueIsValid => (
       IsConnected 
    && ValueIsDefined
    ) ;

    public void PutValue ( object value )
    {
      m_pvChannel.PutValue(
        value
      ) ;
    }    
    
    private readonly bool m_isBlazorServerApp = System.Threading.SynchronizationContext.Current?.GetType().Name.Contains("RendererSynchronizationContext") == true ;

    // Check how this performs equality comparisons !!!
    // Might need to be able install a custom 'equality comparison' function ...

    // Hmm, maybe we want an option to propagate changes even if the new value is the same ?

    private bool UpdateAcceptedEx<T> (
      T                                                  proposedNewValue,
      System.Linq.Expressions.Expression<System.Func<T>> getCurrentValueExpression,
      System.Action<T>                                   setNewValueAction,
      params string[]                                    dependentPropertyNames
    ) {
      System.Collections.Generic.IEqualityComparer<T> equalityComparer
      = System.Collections.Generic.EqualityComparer<T>.Default ;
      bool valueChangeWasAccepted ;
      System.Linq.Expressions.Expression<System.Func<T>> x = getCurrentValueExpression ;
      // Hmm, could cache this - but the 'compile' overhead is probably insignificant
      var compiledExpression = getCurrentValueExpression.Compile() ;
      T currentValue = compiledExpression.Invoke() ;
      if ( equalityComparer.Equals(currentValue,proposedNewValue) )
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
        ValueChanged() ;
        this.Parent?.OnChildPropertyChanged(this) ; // ????????????
        if ( m_isBlazorServerApp )
        {
          // If we're a Blazor app, updating *any* property
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

    protected virtual void ValueChanged ( )
    { }

    private void StateChanged ( 
      Clf.ChannelAccess.StateChange  change, 
      Clf.ChannelAccess.ChannelState newChannelState 
    ) {
      if ( change.DescribesConnectionStatusChange( out var isConnected ) )
      {
        isConnected.HasValue.Should().BeTrue() ;
        bool valueChanged ;
        if (
          valueChanged = UpdateAcceptedEx(
            proposedNewValue          : isConnected.Value,
            getCurrentValueExpression : () => IsConnected,
            setNewValueAction         : newValue => IsConnected = newValue,
            nameof(ValueIsValid)
          )
        ) {
        }
      }
      else if ( change.DescribesValueChange( out var valueInfo ) )
      {
        object? incomingValue = valueInfo.ValueAsObject ;
        bool valueChanged ;
        if (
          valueChanged = UpdateAcceptedEx(
            proposedNewValue          : incomingValue,
            getCurrentValueExpression : () => ValueAsObject,
            setNewValueAction         : newValue => ValueAsObject = newValue,
            // Dependent property names ...
            nameof(ValueIsValid),
            nameof(ValueIsDefined),
            nameof(ValueIsUndefined)
          )
        ) {
          // The value changed, but we've already
          // done everything that's necessary ...
        }
      }
    }

  }

  public sealed class ViewModel_02 : ObservableObject_02
  {

    public ObservablePv_A A = new("xx:one_long"){ChildName="A"} ;

    public ObservablePv_double B = new("xx:one_double"){ChildName="B"} ;

    public async Task ConnectAsync ( )
    {
      await A.ConnectAsync() ;
    }

  }

}
