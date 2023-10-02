//
// ActiveChannelsList.cs
// 

using System.Collections.Generic ;
using Clf.Common.ExtensionMethods ;

namespace Clf.ThinIoc
{

  internal class ActiveChannelsList : List<ActiveChannel>
  {

    // Hmm, how to handle incoming state changes ?

    public ActiveChannelsList ( 
      IEnumerable<Clf.ChannelAccess.ChannelDescriptor> channelDescriptors 
    ) {
      channelDescriptors.ForEachItem(
        channelDescriptor => Add(
          new ActiveChannel(channelDescriptor)
        )
      ) ;
    }

  }

}