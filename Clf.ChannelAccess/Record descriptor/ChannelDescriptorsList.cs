//
// ChannelDescriptorsList.cs
// 

using System.Collections.Generic ;
using Clf.Common.ExtensionMethods ;
using System.Linq ;
using System.Threading.Tasks;

namespace Clf.ChannelAccess
{

  public class ChannelDescriptorsList : List<Clf.ChannelAccess.ChannelDescriptor> 
  {

    public ChannelDescriptorsList ( params Clf.ChannelAccess.ChannelDescriptor[] recordDescriptors )
    {
      AddRange(recordDescriptors) ;
    }

    public static ChannelDescriptorsList Create ( params string[] channelDescriptorStrings )
    {
      return new ChannelDescriptorsList(
        channelDescriptorStrings.Where(
          // Ignore blank lines and 'comment' lines
          s => (
             s.Length != 0
          && ! (
                  s.StartsWith("//")
               || s.StartsWith("#")
               )
          )
        ).Select(
          channelDescriptor => Clf.ChannelAccess.ChannelDescriptor.FromEncodedString(channelDescriptor)
        ).ToArray()
      ) ;
    }

    public IEnumerable<string> AsEncodedStrings 
    => this.Select(
      channelDescriptor => channelDescriptor.ToString() 
    ) ;

    public static ChannelDescriptorsList FromFile ( string filePath )
    {
      return Create(
        System.IO.File.ReadAllLines(filePath)
      ) ;
    }

    public void WriteToFile ( string filePath )
    {
      using ( 
        var streamWriter = System.IO.File.CreateText(
          filePath
        )
      ) {
        this.ForEachItem(
          channelDescriptor => channelDescriptor.ToDbTextLines().ForEachItem(
            line => streamWriter.WriteLine(line)
          )
        ) ;
      }
    }

    public Task<PutValueResult[]> ApplyInitialValuesAsync_old_01 ( )
    {
      // This would work, but it needs to be async.
      var tasks = this.Where(
        recordDescriptor => recordDescriptor.InitialValueAsString != null
      ).
      Select(
        channelDescriptor => {
          return Clf.ChannelAccess.Hub.PutValueFromStringAsync(
            channelDescriptor.ChannelName,
            channelDescriptor.InitialValueAsString!
          ) ;
        }
      ) ;
      // Intellisense greys out the Task<> prefix
      // but actually it *is* necessary here ...
      // return WhenAll(tasks) ;
      return Task<PutValueResult>.WhenAll(tasks) ;
    }

    public Task ApplyInitialValuesAsync_old_02 ( )
    {
      // This would work, but it needs to be async.
      var tasks = this.Select(
        async channelDescriptor => {
          if ( channelDescriptor.InitialValueAsString != null )
          {
            var result_ignored = await Clf.ChannelAccess.Hub.PutValueFromStringAsync(
              channelDescriptor.ChannelName,
              channelDescriptor.InitialValueAsString
            ) ;
          }
        }
      ) ;
      return Task.WhenAll(tasks) ;
    }

    public async Task ApplyInitialValues ( )
    {
      // Since we've declared the DbField etc, there's no need
      // for us to await access to the running PV, we can just
      // do a 'fire-and-forget' write of the value.
      // Hmm, there's a risk however that the 'IOC' might not have started ...
      // so maybe this DOES need to be async after all ???

      foreach ( var channelDescriptor in this )
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

      // Hmm, we really need a 'ForEachItemAsync' ...
      // this.ForEachItem(
      //   async recordDescriptor => {
      //     if ( 
      //       recordDescriptor.DbFieldDescriptor.TryParseValue(
      //         recordDescriptor.InitialValueAsString ?? "", // Null gives us an empty string ...
      //         out var initialValue
      //       )
      //     ) {
      //       await Clf.ChannelAccess.Hub.PutValueAsync(
      //         recordDescriptor.ChannelName,
      //         initialValue
      //       ) ;
      //     }
      //   }
      // ) ;

    }

  }

}