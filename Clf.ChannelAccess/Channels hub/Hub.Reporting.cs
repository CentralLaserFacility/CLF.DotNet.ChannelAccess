//
// Hub.Logging.cs
//
// REPORTS
//

using Clf.ChannelAccess.ExtensionMethods;
using System.Collections.Generic;

namespace Clf.ChannelAccess
{
  partial class Hub
  {
    public static IEnumerable<ChannelReportDescriptor> GetChannelsReport()
    {
      foreach (var item in Hub.GetRegisteredChannelsSnapshot())
      {
        if (item is LocalChannel localChannel)
        {
          bool isInvalid = localChannel.IsInvalid(out var whyNotValid);

          yield return new ChannelReportDescriptor
          {
            ChannelIdentifier = localChannel.ChannelIdentifier,
            ChannelName = localChannel.ChannelName,
            ValidatedChannelName = localChannel.ValidatedChannelName,
            ChannelState = localChannel.Snapshot().CurrentState,
            ChannelType = ChannelType.Local,
            IsSubscribedToValueChangeCallbacks = localChannel.IsSubscribedToValueChangeCallbacks,
            CloneReferencesArePermitted = localChannel.CloneReferencesArePermitted,
            HowManyClonedReferencesExist = localChannel.HowManyClonedReferencesExist,
            CreationTimeStamp = localChannel.CreationTimeStamp,
            TimeStampFromServer = localChannel.TimeStampFromServer,
            FieldInfo = localChannel.FieldInfo,
            ValueAccessMode = localChannel.ValueAccessMode,
            InstanceHasActuallyBeenDisposed = localChannel.InstanceHasActuallyBeenDisposed,
            IsInvalid = isInvalid,
            Comments = whyNotValid
          };

        }
        else if (item is RemoteChannel remoteChannel)
        {
          bool isInvalid = remoteChannel.IsInvalid(out var whyNotValid);

          yield return new ChannelReportDescriptor
          {
            ChannelIdentifier = remoteChannel.ChannelIdentifier,
            ChannelName = remoteChannel.ChannelName,
            ValidatedChannelName = remoteChannel.ValidatedChannelName,
            ChannelState = remoteChannel.Snapshot().CurrentState,
            ChannelHasBeenCreated = remoteChannel.ChannelHasBeenCreated,
            ChannelType = ChannelType.Remote,
            IsSubscribedToValueChangeCallbacks = remoteChannel.IsSubscribedToValueChangeCallbacks,
            CloneReferencesArePermitted = remoteChannel.CloneReferencesArePermitted,
            HowManyClonedReferencesExist = remoteChannel.HowManyClonedReferencesExist,
            CreationTimeStamp = remoteChannel.CreationTimeStamp,
            TimeStampFromServer = remoteChannel.TimeStampFromServer,
            FieldInfo = remoteChannel.FieldInfo,
            ValueAccessMode = remoteChannel.ValueAccessMode,
            InstanceHasActuallyBeenDisposed = remoteChannel.InstanceHasActuallyBeenDisposed,
            IsInvalid = isInvalid,
            Comments = whyNotValid
          };

        }
      }
    }
  }
}
