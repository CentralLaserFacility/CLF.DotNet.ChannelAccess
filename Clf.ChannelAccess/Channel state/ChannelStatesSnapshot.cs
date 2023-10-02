//
// ChannelStatesSnapshot.cs
//

namespace Clf.ChannelAccess
{

  //
  // Encapsulates the current state, the previous state,
  // and a description of the most recent single change that occurred
  // getting us to the 'current' state from the 'previous' state.
  //

  public record ChannelStatesSnapshot ( // NOT NECCESSARY, USE CURRENT STATE INSTEAD ???
    ChannelState   CurrentState,  
    ChannelState ? PreviousState, // Does anyone use this ??? Almost certainly not (AOUN will check)
    StateChange    StateChange    // Likewise - this used to be crucial for the 'StateChange' event scheme, but no longer relevant
  ) ;

}

