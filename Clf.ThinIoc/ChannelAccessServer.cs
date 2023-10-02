//
// ChannelAccessServer.cs
// 

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clf.Common.ExtensionMethods;

namespace Clf.ThinIoc
{

  public class ChannelAccessServer
  {

    public IEnumerable<Clf.ChannelAccess.ChannelDescriptor> ChannelDescriptors => m_channelDescriptorsList ;

    private Clf.ChannelAccess.ChannelDescriptorsList m_channelDescriptorsList ;

    private ChannelAccessServer (
      Clf.ChannelAccess.ChannelDescriptorsList channelDescriptorsList
    ) {
      m_channelDescriptorsList = channelDescriptorsList ;
    }
    
    private ChannelAccessServer (
      params Clf.ChannelAccess.ChannelDescriptor[] channelDescriptors
    ) {
      m_channelDescriptorsList = new(channelDescriptors) ;
    }
    
    public static ChannelAccessServer CreateInstance (
      params Clf.ChannelAccess.ChannelDescriptor[] channelDescriptors
    ) {
      return new ChannelAccessServer(channelDescriptors) ;
    }

    public static ChannelAccessServer CreateInstance (
      Clf.ChannelAccess.ChannelDescriptorsList channelDescriptorsList
    ) {
      return new ChannelAccessServer(
        channelDescriptorsList
      ) ;
    }

    // System.IO.Path.GetRandomFileName() ;
    // System.IO.Path.GetTempFileName() ;

    public void StartThinIoc ( )
    {

      // Write to a tmp file whose name includes the ID of this process,
      // so that there won't be a filename clash if we have two or more
      // instances active at the same time (held by different app instances).

      int currentProcessID = System.Diagnostics.Process.GetCurrentProcess().Id ;
      string tempPath = System.IO.Path.GetTempPath() ;
      string pathToTmpDbFile = (
        $"{tempPath}ChannelAccessServer_{currentProcessID}.db"
      ) ;

      m_channelDescriptorsList.WriteToFile(pathToTmpDbFile) ;

      ThinIocDllFunctions.Initialise() ;
      ThinIocDllFunctions.LoadDbFile(pathToTmpDbFile) ;

      System.IO.File.Delete(pathToTmpDbFile) ;

      ThinIocDllFunctions.StartThinIocOnNewBackgroundThread() ;
    }

    public async Task StartThinIoc_ApplyingInitialValues ( )
    {
      StartThinIoc() ;
      await ApplyInitialValues() ;
    }

    public async Task ApplyInitialValues ( )
    {
      // Since we've declared the DbField etc, there's no need
      // for us to await access to the running PV, we can just
      // do a 'fire-and-forget' write of the value.
      // Hmm, there's a risk however that the 'IOC' might not have started ...
      // so maybe this DOES need to be async after all ???
      foreach ( var channelDescriptor in m_channelDescriptorsList )
      {
        if ( 
          channelDescriptor.DbFieldDescriptor.TryParseValue(
            channelDescriptor.InitialValueAsString ?? "", // Null gives us an empty string ...
            out var initialValue
          )
        ) {
          await Clf.ChannelAccess.Hub.PutValueAsync(
            channelDescriptor.ChannelName,
            initialValue
          ) ;
        }
      }
    }
     
    public void RequestStopThinIoc ( )
    {
      if ( ThinIocDllFunctions.Status == ThinIocDllFunctions.RunningStatus.IsRunning )
      {
        ThinIocDllFunctions.RequestThinIocStop() ;
      }
    }

  }

  public class ChannelAccessServer_UsageExample
  {

    public static void Run ( )
    {
      var server = ChannelAccessServer.CreateInstance(
        new Clf.ChannelAccess.ChannelDescriptor<int>("myInt","3"),
        new Clf.ChannelAccess.ChannelDescriptor<float>("myFloat","1.23f"),
        new Clf.ChannelAccess.ChannelDescriptor<string>("myString","hello")
      ) ;
      server.StartThinIoc() ;
    }

  }

}