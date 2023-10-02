//
// Settings_ForLowLevelDebugging_DO_NOT_CHECK_IN.cs
//

namespace Clf.ChannelAccess
{

  //
  // THE VALUE OF 'EnableCommsTimeoutsEvenWhenDebugging' MUST BE CONFIGURED AS 'TRUE'
  // WHENEVER THIS FILE IS CHECKED IN, SO THAT THAT TIMEOUT EXCEPTIONS ARE THROWN
  // WHEN APPLICATION CODE USING CHANNEL-ACCESS IS BEING RUN UNDER THE DEBUGGER.
  //
  // IT'S PERMISSIBLE TO CHANGE 'EnableCommsTimeoutsEvenWhenDebugging' TO 'FALSE'
  // IF YOU NEED TO DO SOME LOW LEVEL DEBUGGING OF CHANNEL ACCESS OR RUN THE
  // CHANNEL ACCESS TESTS UNDER THE DEBUGGER, BUT BE SURE TO REVERT THE VALUE
  // BACK TO 'TRUE' PRIOR TO CHECKING IN THIS FILE.
  // 

  internal class Settings_ForLowLevelDebugging
  {

    /// <summary>
    /// When debugging application code that uses ChannelAccess, you normally want timeouts 
    /// to fire, so you should leave this property at its default value 'true'. 
    /// <br/><br/>
    /// <i>The value must be configured as 'true' whenever this file is checked in. It's permissible
    /// to change it to 'false' if you need to do some low level debugging or run the Tests,
    /// but the value must be changed back to 'true' prior to checking in this file.</i>
    /// <br/><br/>
    /// When you're running the ChannelAccess Tests under the debugger, where you'll be setting
    /// breakpoints and single stepping after hitting them, you don't want timeouts to fire 
    /// because that just gets in the way. In that situation, set this flag to 'false',
    /// by adjusting the position of the comment - thereby disabling the timeouts from firing 
    /// during the debug session. But be sure to never check in this file with the 'false' setting selected.
    /// <br/><br/>
    /// The value can also be set from the Command Line Interpreter.
    /// </summary>
    
    public static bool EnableCommsTimeoutsEvenWhenDebugging = (
      // EnableCommsTimeoutsEvenWhenDebugging_DefaultForLowLevelCode_FALSE          
      EnableCommsTimeoutsEvenWhenDebugging_DefaultForApplicationLevelCode_TRUE // THIS MUST BE SELECTED WHEN THE FILE IS CHECKED IN !!!
    ) ;

    public const bool EnableCommsTimeoutsEvenWhenDebugging_DefaultForLowLevelCode_FALSE        = false ;
    public const bool EnableCommsTimeoutsEvenWhenDebugging_DefaultForApplicationLevelCode_TRUE = true ;

  }

}
