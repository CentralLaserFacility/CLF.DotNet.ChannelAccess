//
// Program.cs
//

try
{
  System.Console.WriteLine(
    await Clf.ChannelAccess.Hub.GetValueAsStringAsync(
      channelName : args[0]
    ) 
  ) ;
}
catch ( Exception x )
{
  System.Console.Error.WriteLine(x) ;
}
