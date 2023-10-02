//
// ChannelsRegistry.cs
//

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Clf.ChannelAccess.ExtensionMethods;
using Clf.Common.ExtensionMethods;
using System.Diagnostics.CodeAnalysis;

namespace Clf.ChannelAccess
{

  //
  // TODO : Change this back to being a non-static class,
  // with instance fields instead of static fields ; and provide
  // a static 'Instance' property. That will make it evident that
  // client code is using a shared instance (potentially with multi
  // threading issues) and opens up the possibility of using AsyncLocal<>.
  //

  internal static partial class ChannelsRegistry 
  {

    // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/lock-statement

    private static readonly object m_syncLock = new() ;

    //
    // Hmm, use of this 'concurrent' dictionary does not in fact
    // provide all the thread safety we need - locking is still necessary.
    //

    //
    // Note that the 'Values' property does do what you'd hope - it returns
    // a snapshot of the values at the point in time when the property was accessed.
    // https://stackoverflow.com/questions/3216636/is-a-linq-query-to-concurrentdictionary-values-threadsafe
    // This isn't mentioned in the documentation, but the source code shows that a lock
    // is being used while a temporary clone of the collection is being created.
    // --------------------------------------
    // The internal locks cannot cause Deadlock. Deadlock requires operations waiting
    // on one another, and none of the things done inside the locks can call back out
    // to other code that could try to do something that also needs a lock.
    // That's the theory anyway - I wonder, how could we tell if that's not the case ???
    //

    private static System.Collections.Concurrent.ConcurrentDictionary<
      int,     // key   : UniqueIdentifier
      IChannel // value : RemoteChannel or LocalChannel
    > m_channelsMap = new() ;

    internal static bool TryGetRegisteredChannel ( 
      ValidatedChannelNameAndAccessMode          channelNameAndAccessMode, 
      [NotNullWhen(true)] out IChannel? channelFound
    ) {
      // lock ( m_syncLock ) // NOT NECESSARY BECAUSE 'Values' returns a clone !!
      {
        channelFound = m_channelsMap.Values.Where(
          activeChannel => activeChannel.AsChannelBase().ValidatedChannelNameAndAccessMode == channelNameAndAccessMode
        ).SingleOrDefault() ; // ??? SingleOrDefault ???
        return channelFound != default ;
      }
    }

    public static bool TryGetRegisteredChannel ( 
      ChannelName                       channelName,
      ValueAccessMode?                  valueAccessMode, 
      [NotNullWhen(true)] out IChannel? channelFound
    ) => TryGetRegisteredChannel(
      new ValidatedChannelNameAndAccessMode(
        channelName.Validated(),
        valueAccessMode ?? channelName.Validated().DefaultValueAccessMode()
      ), 
      out channelFound
    ) ;

    // public static IChannel? GetRegisteredChannelOrNull ( 
    //   ChannelName      channelName,
    //   ValueAccessMode? valueAccessMode
    // ) {
    //   // lock ( m_syncLock ) // NOT NECESSARY BECAUSE 'Values' returns a clone !!
    //   {
    //     return m_channelsMap.Values.Where(
    //       activeChannel => activeChannel.ChannelNameAndAccessMode == channelNameAndAccessMode
    //     ).FirstOrDefault() ;
    //   }
    // }

    public static bool HasRegisteredChannel ( 
      ChannelName      channelName, 
      ValueAccessMode? valueAccessMode = null 
    ) => TryGetRegisteredChannel(
      channelName,
      valueAccessMode,
      out _ 
    ) is true ;

    // This returns a snapshot of the channels that were registered
    // at the time the property was evaluated.
    // This will remain stable even if the underlying collection
    // is modified by other threads.

    public static IEnumerable<IChannel> GetRegisteredChannelsSnapshot ( ) 
    {
      // lock ( m_syncLock ) // NOT NECESSARY BECAUSE 'Values' returns a clone !!
      {
        return m_channelsMap.Values ;
      }
    }

    public static void DisplayRegisteredChannels ( System.Action<string> writeLine )
    {
      IEnumerable<IChannel> registeredChannelsSnapshot = GetRegisteredChannelsSnapshot() ;
      if ( registeredChannelsSnapshot.Any() )
      {
        writeLine(
          $"There are {registeredChannelsSnapshot.Count()} active channels registered :"
        ) ;
        registeredChannelsSnapshot.ForEachItem(
          (channel,i) => writeLine(
            $"  {i:D2} : {channel.ChannelNameWithValueAccessModeAndChannelIdentifier()}"
          )
        ) ;
      }
      else
      {
        writeLine(
          $"There are no active channels registered"
        ) ;
      }
    }

    public static bool TryLookupRegisteredChannel ( 
      int                     channelIdentifier, 
      [NotNullWhen(true)] out IChannel? channel 
    ) {
      return m_channelsMap.TryGetValue(
        channelIdentifier,
        out channel
      ) ;
    }

    public static IChannel? FindChannelFromLowLevelHandle ( System.IntPtr pChannel )
    {
      return m_channelsMap.Values.OfType<RemoteChannel>().SingleOrDefault(
        channel => channel.LowLevelHandleIs(pChannel)
      ) ;
    }

    // Hmm, this will expand indefinitely !!! Need a proper 'logging' mechanism

    private static int? m_nextAvailableChannelIdentifier = null ;

    public static int AllocateNextAvailableChannelIdentifier ( )
    {
      lock ( m_syncLock )
      {
        // This lock can't lead to a deadlock because
        // it doesn't invoke any other method that might
        // try to (A) acquire a different 'lock' and
        // then (B) invoke this method recursively.
        m_nextAvailableChannelIdentifier ??= /*InstanceNumber * */ 1000 ;
        m_nextAvailableChannelIdentifier += 1 ;
        return m_nextAvailableChannelIdentifier.Value ;
      }
    }

    public static void RegisterChannel ( ChannelBase channel )
    {
      lock ( m_syncLock )
      {
        // This lock can't lead to a deadlock because
        // it doesn't invoke any other method that might
        // try to (A) acquire a different 'lock' and
        // then (B) invoke this method recursively.
        // ACTUALLY THE ONLY REASON WE NEED TO LOCK HERE IS
        // SO THAT WE CAN ASSERT THE PRECONDITIONS !!!
        m_channelsMap.ContainsKey(channel.ChannelIdentifier).Should().BeFalse() ;
        HasRegisteredChannel(channel.ChannelName,channel.ValueAccessMode).Should().BeFalse() ;
        bool channelWasRegisteredSuccessfully = m_channelsMap.TryAdd(
          channel.ChannelIdentifier,
          channel
        ) ;
        channelWasRegisteredSuccessfully.Should().BeTrue() ;
      }
    }

    public static void DeRegisterChannel ( ChannelBase channel )
    {
      m_channelsMap.TryRemove(
        channel.ChannelIdentifier,
        out var channelRemoved
      ).Should().BeTrue() ;
      channelRemoved.Should().Be(channel) ;
    }

    public static void DeregisterAllChannels ( )
    {
      GetRegisteredChannelsSnapshot().OfType<ChannelBase>().ToList().ForEach(
        DeRegisterChannel
      ) ;
    }

  }

}
