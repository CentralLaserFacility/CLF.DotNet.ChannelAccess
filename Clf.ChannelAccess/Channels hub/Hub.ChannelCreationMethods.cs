//
// Hub_channel_creation_methods.cs
//

using System.Collections.Generic ;
using System.Linq ;
using System.Runtime.InteropServices ;
using FluentAssertions ;
using static Clf.ChannelAccess.Helpers ;
using Clf.ChannelAccess.ExtensionMethods ;
using Clf.ChannelAccess.LowLevelApi.ExtensionMethods ;
using Clf.Common.ExtensionMethods ;
using static Clf.ChannelAccess.LowLevelApi.EcaCodeExtensionMethods ;
using System.Diagnostics.CodeAnalysis ;
using static CommunityToolkit.Mvvm.Messaging.IMessengerExtensions ;

namespace Clf.ChannelAccess
{

  partial class Hub
  {

    public static IChannel GetOrCreateChannel ( 
      ChannelName      channelName,
      ValueAccessMode? valueAccessModeBeingRequested = null
    ) {
      return GetOrCreateChannel_ReturningInvalidChannelOnException(
        channelName,
        GetOrCreateChannel
      ) ;
      IChannel GetOrCreateChannel ( )
      {
        if ( 
          ChannelsRegistry.TryGetRegisteredChannel(
            channelName,
            valueAccessModeBeingRequested,
            out var channel 
          ) 
        ) {
          return channel.AsChannelBase().WithReferenceCountIncremented() ;
        }
        else
        {
          return CreateChannelInstance(
            channelName,
            valueAccessModeBeingRequested
          ) ;
        }
      }
    }

    public static IChannel CreateLocalChannel ( 
      ChannelDescriptor channelDescriptor, 
      bool              initiallyConnected = true 
    ) {
      if ( 
        ChannelsRegistry.TryGetRegisteredChannel(
          channelDescriptor.ChannelName,
          (
            // Hmm, with a local channel we don't get any CTRL info
            // but nevertheless it's necessary to use the same 
            // access mode that we employ for Remote channels ...
            ValueAccessMode.DBR_CTRL_RequestValueAndAuxiliaryInfo
            // ValueAccessMode.DBR_RequestValueAndNothingElse
          ),
          out var channelAlreadyRegistered
        ) 
      ) {
        throw new UsageErrorException(
          channelAlreadyRegistered is LocalChannel
          ? $"LocalChannel {channelDescriptor.ChannelName} is already registered"
          : $"LocalChannel {channelDescriptor.ChannelName} is already known as a Remote Channel"
        ) ;
      }
      return GetOrCreateChannel_ReturningInvalidChannelOnException(
        channelDescriptor.ChannelName,
        () => new Clf.ChannelAccess.LocalChannel(
          channelDescriptor,
          initiallyConnected : initiallyConnected
        )
      ) ;
    }
    
    public static IChannel GetOrCreateLocalChannel ( 
      ChannelDescriptor channelDescriptor, 
      bool              initiallyConnected = true 
    ) {
      return GetOrCreateChannel_ReturningInvalidChannelOnException(
        channelDescriptor.ChannelName,
        GetOrCreateLocalChannel
      ) ;
      IChannel GetOrCreateLocalChannel ( )
      {
        if ( 
          ChannelsRegistry.TryGetRegisteredChannel(
            channelDescriptor.ChannelName,
            // With a local channel we don't get any CTRL info
            // but nevertheless it's necessary to use the same 
            // access mode that we employ for Remote channels ...
            ValueAccessMode.DBR_CTRL_RequestValueAndAuxiliaryInfo,
            out var channelAlreadyRegistered
          ) 
        ) {
          // We already have a channel of that name. But that's OK 
          // provided that the data type of the existing channel matches
          // the data type of the channel we would be creating.
          // In this case we just return the existing channel.
          if ( channelAlreadyRegistered is LocalChannel localChannelAlreadyRegistered )
          {
            return localChannelAlreadyRegistered.AsChannelBase().WithReferenceCountIncremented() ;
          }
          else
          {
            throw new UsageErrorException(
              $"Channel {channelDescriptor.ChannelName} is already known as a Remote channel"
            ) ;
          }
        }
        else
        {
          return new Clf.ChannelAccess.LocalChannel(
            channelDescriptor,
            initiallyConnected : initiallyConnected
          ) ;
        }
      }
    }
    
    public static IChannel GetLocalChannel ( 
      ChannelDescriptor channelDescriptor
    ) {
      return GetOrCreateChannel_ReturningInvalidChannelOnException(
        channelDescriptor.ChannelName,
        GetLocalChannel
      ) ;
      IChannel GetLocalChannel ( )
      {
        if ( 
          ChannelsRegistry.TryGetRegisteredChannel(
            channelDescriptor.ChannelName,
            // With a local channel we don't get any CTRL info
            // but nevertheless it's necessary to use the same 
            // access mode that we employ for Remote channels ...
            ValueAccessMode.DBR_CTRL_RequestValueAndAuxiliaryInfo,
            out var channelAlreadyRegistered
          ) 
        ) {
          // We already have a channel of that name. But that's OK 
          // provided that the data type of the existing channel matches
          // the data type of the channel we would be creating.
          // In this case we just return the existing channel.
          if ( channelAlreadyRegistered is LocalChannel localChannelAlreadyRegistered )
          {
            if ( 
              channelDescriptor != localChannelAlreadyRegistered.ChannelDescriptor // TODO : CHECK THIS !!!
            ) {
              throw new UsageErrorException(
                $"Data type mismatch : "
              + $"{channelDescriptor} doesn't match existing record {localChannelAlreadyRegistered}"
              ) ;
            }
            return localChannelAlreadyRegistered.AsChannelBase().WithReferenceCountIncremented() ;
          }
          else
          {
            throw new UsageErrorException(
              $"Channel {channelDescriptor.ChannelName} is already known as a Remote channel"
            ) ;
          }
        }
        else
        {
          throw new UsageErrorException(
            $"LocalChannel {channelDescriptor.ChannelName} does not exist"
          ) ;
        }
      }
    }
    
    // Create a bunch of LocalChannels from RecordDescriptors.

    // When we create a local channel, we'll need to specify the ValueAccessMode.
    // So the ChannelDescriptor needs to specify that ?? NO, ACCESS MODE IS ALWAYS VAL-ONLY.
    // In principle we could enhance the descriptor to specify fields other than VAL ...
    // BUT NO - LOCAL CHANNELS WILL STAY SIMPLE, JUST A VAL !!!

    public static IEnumerable<IChannel> CreateLocalChannels ( 
      ChannelDescriptorsList channelDescriptors,
      bool                   initiallyConnected = true
    ) {
      List<IChannel> localChannels = new List<IChannel>() ;
      channelDescriptors.ForEach(
        channelDescriptor => localChannels.Add(
          Hub.CreateLocalChannel(
            channelDescriptor,
            initiallyConnected
          )
        )
      ) ;
      return localChannels ;
    }

  }

}
