//
// ChannelStateSnapshot.cs
//

using Clf.ChannelAccess.ExtensionMethods ;
using System.Diagnostics.CodeAnalysis;
using static Clf.ChannelAccess.InternalHelpers ;

namespace Clf.ChannelAccess
{

  //
  // This immutable record encapsulates a snapshot
  // of the entire 'state' of a Channel at a particular moment in time.
  //
  //   Are we currently connected ?
  //   
  //   Has the remote PV told us about
  //     the type of its field ?
  //     the field's Value ?
  //

  // public record ChannelState (
  //   IChannel                Channel,
  //   int                     SequenceNumber,
  //   ChannelStatusDescriptor ConnectionAndValidityStatus,
  //   FieldInfo?              FieldInfo,
  //   ValueInfo?              ValueInfo
  // ) {
  //   
  //   public ChannelIsConnected ConnectionStatus => ConnectionAndValidityStatus.ConnectionStatus ;
  // 
  //   public ChannelIsValid ValidityStatus => ConnectionAndValidityStatus.ValidityStatus ;

  // Instead of providing an IChannelProperty, we provide a ChannelName.
  // That solves the problem of the 'identity' of the Channel to which the ChannelState pertains,
  // and it works even though the Channel might be an InvalidChannel ...

  public record ChannelState (
    ChannelName             ChannelName,
    ValueAccessMode         ValueAccessMode,
    int                     SequenceNumber,
    ChannelConnectionStatus ConnectionStatus,
    ChannelValidityStatus   ValidityStatus,
    FieldInfo?              FieldInfo,
    ValueInfo?              ValueInfo
  ) {

    public string ChannelNameAndAccessModeAsString => InternalHelpers.GetChannelNameAndAccessModeAsString(
      ChannelName, 
      ValueAccessMode
    ) ;
    
    public ChannelStatusDescriptor ConnectionAndValidityStatus 
    => new ChannelStatusDescriptor(
      ConnectionStatus,ValidityStatus
    ) ;

    public bool IsConnected => ConnectionStatus.IsConnected ;

    public bool IsValid => ValidityStatus.IsValid ;

    public bool IsInvalid ( [NotNullWhen(true)] out string? whyNotValid ) 
    => ValidityStatus.IsInvalid( 
      out whyNotValid 
    ) ;

    public System.DateTime TimeStamp { get ; } = System.DateTime.Now ;

    // Hmm, could make these extension methods ?

    public void RenderAsStrings ( System.Action<string> writeLine )
    {
      writeLine($"ChannelState for {ChannelName} :") ;
      writeLine($"  SequenceNumber is {SequenceNumber}") ;
      writeLine($"  Connection status : {(ConnectionStatus.IsConnected?"connected":"DISCONNECTED")}") ;
      ValueInfo?.RenderAsStrings(writeLine,showAuxiliaryValues:true) ;
      FieldInfo?.RenderAsStrings(writeLine) ;
    }

    public bool ChannelHasConnected => FieldInfo is not null ;
    
    public bool FieldInfoIsKnown => FieldInfo is not null ;

  } ;

}

