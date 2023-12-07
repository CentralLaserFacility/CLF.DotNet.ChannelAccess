//
// ChannelReportDescriptor.cs
// 

using System;

namespace Clf.ChannelAccess
{
  public record ChannelReportDescriptor
  {
    public int ChannelIdentifier { get; init; }
    public string ChannelName { get; init; }
    public string ValidatedChannelName { get; init; }
    public ChannelState ChannelState { get; init; }
    public bool? ChannelHasBeenCreated { get; init; }
    public ChannelType ChannelType { get; init; }
    public bool IsSubscribedToValueChangeCallbacks { get; init; }
    public bool CloneReferencesArePermitted { get; init; }
    public int HowManyClonedReferencesExist { get; init; }
    public int ReferencesToThisInstance { get; init; }
    public DateTime? CreationTimeStamp { get; init; }
    public DateTime? TimeStampFromServer { get; init; }
    public FieldInfo? FieldInfo { get; init; }
    public ValueAccessMode ValueAccessMode { get; init; }
    public bool InstanceHasActuallyBeenDisposed { get; init; }
    public bool IsInvalid { get; init; }
    public string Comments { get; init; }
    
  }
}