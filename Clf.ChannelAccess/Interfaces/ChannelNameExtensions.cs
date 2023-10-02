//
// ChannelNameExtensions.cs
//

namespace Clf.ChannelAccess.ExtensionMethods
{

  public static class ChannelNameExtensions
  {

    public static ValidatedChannelName Validated ( 
      this ChannelName name 
    ) => new ValidatedChannelName(name) ;

    public static ChannelName WithOptionalValSuffixRemoved ( 
      this ChannelName name 
    ) => name.Validated().ShortName_OmittingVAL ;

  }

}

