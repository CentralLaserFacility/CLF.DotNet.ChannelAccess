//
// CommandLineInterpreter.cs
//

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Clf.Common.ExtensionMethods;
using System.Linq;
using Clf.ChannelAccess.ExtensionMethods;

namespace Clf.ChannelAccess
{

  //
  // NOTE : THIS IS DEFINED IN THE CHANNEL ACCESS PROJECT 
  // RATHER THAN A CONSOLE APPLICATION, IN ORDER THAT IT CAN BE USED
  // IN OTHER UI'S SUCH AS A GUI IN WHICH THE COMMAND LINES ARE
  // ENTERED VIA A TEXT BOX.
  //

  // 
  // Exercises the 'client side' usage of ChannelAccess.
  //

  //
  // Having set up a 'monitor' on a PV (ie a 'Channel') the client will subsequently
  // receive notifications when the value of that PV suffers a change, ie when the
  // server that is 'producing' the value of that PV, decides to update its value.
  //

  public class CommandLineInterpreter
  {

    public enum TextCategory
    {
      Prompt,
      UserInput,
      Response_Normal,
      Response_Exception,
      AsyncNotification_Normal,
      AsyncNotification_Abnormal,
      InformationalMessage
    }

    //
    // We abstract the line-based I/O so that we can embed this in a GUI app ...
    //

    private System.Action<string, TextCategory> m_writeLineAction;

    public CommandLineInterpreter(
      System.Action<string, TextCategory> writeOutputLineAction
    )
    {
      m_writeLineAction = writeOutputLineAction;
      Hub.OnInterestingEvent += (interestingEvent) =>
      {
        if (m_verboseMode_showingChannelEvents)
        {
          WriteLine(
            interestingEvent.ToString(),
            interestingEvent is AnomalyNotification
            ? TextCategory.AsyncNotification_Abnormal
            : TextCategory.AsyncNotification_Normal
          );
        }
      };
    }

    public static string Prompt { get; set; } = "ca> ";

    private System.Diagnostics.Stopwatch m_stopwatch = new();

    public record EventHistoryItem(double ElapsedSeconds, TextCategory Category, string EventInfo)
    {
      public override string ToString() => $"{ElapsedSeconds:F3} {Category} {EventInfo}";
    }

    private List<EventHistoryItem> m_eventHistory = new();

    public void AddEventHistoryItem(TextCategory category, string eventInfo)
    {
      if (!m_stopwatch.IsRunning)
      {
        m_stopwatch.Start();
      }
      EventHistoryItem eventHistoryItem = new(
        m_stopwatch.Elapsed.TotalSeconds,
        category,
        eventInfo
      );
      System.Diagnostics.Debug.WriteLine(
        $"{eventHistoryItem}"
      );
      m_eventHistory.Add(
        eventHistoryItem
      );
    }

    public void WriteLine(string line, TextCategory category = TextCategory.Response_Normal)
    {
      line = line.TrimEnd(' ');
      AddEventHistoryItem(category, line);
      m_writeLineAction(line, category);
    }

    //
    // We maintain a history of the command lines we've handled.
    //
    // In a Console application we can normally rely on the default behaviour
    // of System.Console to handle cycling through and re-issuing previous commands,
    // as well as line editing and so on.
    //
    // In a GUI app, previous commands could be displayed in a drop-down list,
    // so it's useful to have saved them here.
    //

    private List<string> m_commandLineHistory = new();

    // We want to return the most recently executed command first.
    // That command will have been added to the end of the list,
    // so we'll enumerate the items in reverse order.

    public IEnumerable<string> CommandLineHistory => m_commandLineHistory.AsEnumerable().Reverse();

    public void ClearCommandLineHistory() => m_commandLineHistory.Clear();

    private string? m_mostRecentlyMentionedChannelName;

    private bool m_showAuxiliaryValues =
      // true 
      false
    ;

    private string m_valueAccessModeForValFields = "ctrl";

    private bool m_verboseMode_showingChannelEvents = false;

    private void DisplayCurrentSettings()
    {
      string timeoutsMessage = $"{(
          Settings.CommsTimeoutsAreEnabled
          ? $"yes ({Settings.CommsTimeoutPeriodInEffect.TotalSeconds:f1})"
          : "no"
        )}";
      WriteLine($"Settings :");
      WriteLine($"  Display 'aux' info  : {m_showAuxiliaryValues.AsYesOrNo()}");
      WriteLine($"  VAL access mode     : {m_valueAccessModeForValFields}");
      WriteLine($"  Verbose messages    : {m_verboseMode_showingChannelEvents.AsYesOrNo()}");
      WriteLine($"  Recent channel name : {m_mostRecentlyMentionedChannelName ?? "(none)"}");
      WriteLine($"  Timeouts            : {timeoutsMessage}");
    }

    public async Task HandleCommandLineCommand(string commandLine)
    {
      try
      {

        if (m_commandLineHistory.AsEnumerable().LastOrDefault() == commandLine)
        {
          // Avoid adding duplicate lines ...
        }
        else
        {
          m_commandLineHistory.Add(commandLine);
        }

        AddEventHistoryItem(
          TextCategory.UserInput,
          commandLine
        );

        //
        // PV names are case sensitive, but we can allow
        // case-insensitive matches for our 'command verbs'
        //

        string[] fields = commandLine.Split();
        string commandVerb_lowerCase = fields[0].ToLower();

        string arg_1_channelName =
          fields.Length > 1
          ? fields[1]
          : ""
        ;
        //
        // If you specify '=' as the pv name,
        // we'll substitute the last-mentioned pv name
        // eg
        //   g my:pv
        //   s = 123 // Equivalent to 's my:pv 123' because we replace '=' by 'my:pv'
        //
        if (arg_1_channelName.Length > 1)
        {
          if (arg_1_channelName.StartsWith('#'))
          {
            if (
              ChannelsRegistry.TryLookupRegisteredChannel(
                arg_1_channelName.TrimStart('#').ParsedAs<int>(),
                out var channel
              )
            )
            {
              arg_1_channelName = channel.ChannelName;
            }
          }
          m_mostRecentlyMentionedChannelName = arg_1_channelName;
        }
        else if (arg_1_channelName == "=")
        {
          if (m_mostRecentlyMentionedChannelName is null)
          {
            WriteLine(
              $"No channelName available"
            );
            return;
          }
          else
          {
            arg_1_channelName = m_mostRecentlyMentionedChannelName;
          }
        }

        string arg_2_newValue =
          fields.Length > 2
          ? fields.Skip(2).Aggregate(
              (a, b) => a + " " + b
            )
          : ""
        ;

        // 
        // Note that commands that are specific to a 'console' app
        // will have been handled already, and aren't dealt with here
        //

        switch (commandVerb_lowerCase)
        {
          case "a+":
            m_showAuxiliaryValues = true;
            DisplayCurrentSettings();
            break;
          case "a-":
            m_showAuxiliaryValues = false;
            DisplayCurrentSettings();
            break;
          case "v":
          case "v+":
          case "verbose":
          case "verbose+":
            m_verboseMode_showingChannelEvents = true;
            DisplayCurrentSettings();
            break;
          case "v-":
          case "verbose-":
            m_verboseMode_showingChannelEvents = false;
            DisplayCurrentSettings();
            break;
          // case ""
          case "q":
          case "query":
            FieldInfo? channelInfo = await Hub.GetFieldInfoAsync(arg_1_channelName);
            WriteLine(
              $"{channelInfo?.ToString() ?? "Channel is not available"}"
            );
            break;
          case "g":
          case "get":
          case "caget":
            // Hmm, if we don't receive an immediate reply, then after say 1 second
            // we could put out a message saying 'waiting for channel to respond ...'
            GetValueResult getValueResult = await Hub.GetValueInfoAsync(
              arg_1_channelName,
              ParseOptionalValueAccessMode()
            );
            if (getValueResult.IsSuccess)
            {
              getValueResult.ValueInfo.RenderAsStrings(
                line => WriteLine(line),
                m_showAuxiliaryValues
              );
            }
            else
            {
              WriteLine(
                $"Channel value '{arg_1_channelName}' is not available : {getValueResult.WhyFailed}"
              );
            }
            break;
          case "s":
          case "set":
          case "p":
          case "put":
          case "caput":
            await Hub.PutValueFromStringAsync(
              arg_1_channelName,
              arg_2_newValue
            );
            WriteLine(
              $"New value written : {await Hub.GetValueAsStringAsync(
                  arg_1_channelName
                )}"
            );
            break;
          case "sff":
          case "setff":
            await Hub.PutValueFromStringAsync(
              arg_1_channelName,
              arg_2_newValue
            );
            WriteLine(
              $"Fire-and-forget write succeeded, but new value has not been confirmed"
            );
            break;
          case "m+":
          case "monitor+":
          case "m":
          case "monitor":
          case "camonitor":
            if (
              m_monitoredChannelsHandler.TryGetInstalledChannel(
                channelName: arg_1_channelName,
                channel: out _,
                valueAccessMode: ParseOptionalValueAccessMode()
              )
            )
            {
              WriteLine(
                $"Already monitoring '{arg_1_channelName}' !"
              );
            }
            else
            {
              await StartMonitoringPV(
                arg_1_channelName,
                ParseOptionalValueAccessMode()
              );
            }
            break;
          case "m-":
          case "monitor-":
            StopMonitoringPV(
              arg_1_channelName,
              ParseOptionalValueAccessMode()
            );
            break;
          case "m!":
          case "monitor!":
            // ChannelsRegistry.DeregisterAllChannels() ;
            m_monitoredChannelsHandler.RemoveAndDisposeAllChannels();
            break;
          case "m?":
          case "monitor?":
            var channelsBeingMonitored = m_monitoredChannelsHandler.AllInstalledChannels;
            if (channelsBeingMonitored.Any())
            {
              WriteLine(
                $"Channels being monitored : {channelsBeingMonitored.Count()}"
              );
              channelsBeingMonitored.ForEachItem(
                (channel, i) => WriteLine(
                  $"{i + 1:D2}: {channel.WrappedChannel.GetChannelNameAndAccessModeAsString()}"
                )
              );
            }
            else
            {
              WriteLine(
                "No channels are being monitored"
              );
            }
            break;
          case "$v":
          case "$version":
            WriteLine(
              $"ChannelAccess DLL version is {ChannelsRegistry.Version}"
            );
            break;
          case "$throw":
            throw new ExceptionBase("Just testing");
          case "$h":
          case "$history":
            CommandLineHistory.ForEachItem(
              line => WriteLine(line)
            );
            break;
          case "$t-":
          case "$timeouts-":
            // Activate this option when you want to set debugger breakpoints
            // in the ChannelAccess code and step through the function calls.
            // In that scenario, you don't want timeouts to fire after the usual
            // two or three seconds ; you want a timeout period of several minutes
            // while you stare at code and figure out what's going on !!
            Settings_ForLowLevelDebugging.EnableCommsTimeoutsEvenWhenDebugging = false;
            DisplayCurrentSettings();
            break;
          case "$t+":
          case "$timeouts+":
            // Restore the timeout period to its nominal value of a few seconds.
            Settings_ForLowLevelDebugging.EnableCommsTimeoutsEvenWhenDebugging = true;
            DisplayCurrentSettings();
            break;
          default:
            DisplayCurrentSettings();
            break;
        }
        // Local functions
        ValueAccessMode? ParseOptionalValueAccessMode()
        {
          // g my:pv time
          // m+ my:pv time
          return fields.ElementAtOrDefault(2) switch
          {
            "ctrl" => ValueAccessMode.DBR_CTRL_RequestValueAndAuxiliaryInfo, // Useful for testing ...
            "time" => ValueAccessMode.DBR_TIME_RequestValueAndServerTimeStamp,
            "none" => ValueAccessMode.DBR_RequestValueAndNothingElse,        // Useful for testing ...
            _ => null
          };
        }
      }
      catch (System.Exception x)
      {
        WriteLine(
          $"EXCEPTION : '{x.Message}'",
          TextCategory.Response_Exception
        );
        WriteLine(
          $"{x.StackTrace}",
          TextCategory.Response_Exception
        );
      }

    }

    private ChannelsHandler m_monitoredChannelsHandler = new ChannelsHandler(
      autoRaiseSyntheticEvent: false
    );

    protected async Task StartMonitoringPV(string channelName_PV, ValueAccessMode? valueAccessMode = null)
    {
      m_monitoredChannelsHandler.TryGetInstalledChannel(
        channelName_PV,
        out var _,
        valueAccessMode
      ).Should().BeFalse();
      var channelBeingMonitored = Hub.GetOrCreateChannel(
        channelName_PV,
        valueAccessMode
      );
      m_monitoredChannelsHandler.InstallChannelAndEventHandlers(
        channelBeingMonitored,
        connectionStatusChangedHandler: (isConnected, state) =>
        {
          WriteLine(
            $"{state.ChannelNameAndAccessModeAsString} is now {(isConnected ? "CONNNECTED" : "DISCONNECTED")}",
            isConnected
            ? TextCategory.AsyncNotification_Normal
            : TextCategory.AsyncNotification_Abnormal
          );
        },
        valueChangedHandler: (valueInfo, state) =>
        {
          WriteLine(
            $"{state.ChannelNameAndAccessModeAsString} value is {valueInfo.Value_AsDisplayString(
                WhichValueInfoElementsToInclude.AllAvailableElements
              )}",
            TextCategory.AsyncNotification_Normal
          );
        }
      );
      ChannelsRegistry.TryGetRegisteredChannel(
        channelName_PV,
        valueAccessMode,
        out var registeredChannel
      ).Should().BeTrue();
      channelBeingMonitored.AsChannelWrapper().WrappedChannel.Should().Be(registeredChannel);
      WriteLine(
        $"Monitoring '{channelBeingMonitored.GetChannelNameAndAccessModeAsString()}' ..."
      );
      bool connected = await channelBeingMonitored.HasConnectedAndAcquiredValueAsync();
      // TODO : race condition here. When the connect event occurs, the message gets sent
      // in the ChannelAccess code *before* return from the 'await'. That's fair enough,
      // but the order of message lines on the console output is unexpected ...
      WriteLine(
        connected
        ? $"{channelBeingMonitored.GetChannelNameAndAccessModeAsString()} has connected"
        : $"{channelBeingMonitored.GetChannelNameAndAccessModeAsString()} has not connected yet, but may do so in future"
      );
    }

    private void StopMonitoringPV(string channelName, ValueAccessMode? valueAccessMode = null)
    {
      if (
        m_monitoredChannelsHandler.TryGetInstalledChannel(
          channelName,
          out var channelBeingMonitored,
          valueAccessMode
        )
      )
      {
        m_monitoredChannelsHandler.RemoveChannelAndDispose(channelName);
        WriteLine(
          $"{channelName} is no longer being monitored ..."
        );
      }
      else
      {
        WriteLine(
          $"{channelName} is not being monitored !"
        );
      }
    }

    public void WriteHelpLines()
    {
      WriteLine("Commands :                                          Alternatives :      ");
      WriteLine("  g  channelName [suffix]  // Get PV value          get caget           ");
      WriteLine("  s  channelName value     // Set/Put PV value      set p put caput     ");
      WriteLine("  q  channelName           // Query PV field type   query               "); // field ? info ??
      WriteLine("  m+ channelName [suffix]  // Monitor a PV value    m monitor camonitor ");
      WriteLine("  m- channelName           // Stop monitoring pv                        ");
      WriteLine("  m?                       // Show active monitors                      ");
      WriteLine("  m!                       // Stop monitoring all pv's                  ");
      WriteLine("  v+                       // Verbose mode on       verbose+            ");
      WriteLine("  v-                       // Verbose mode off      verbose-            ");
      WriteLine("  a+                       // Display 'auxiliary' info (CTRL)           ");
      WriteLine("  a-                       // Don't display auxiliary info              ");
      WriteLine("------------------------------------------------------------------      ");
      WriteLine("Optional 'ctrl' suffix requests CTRL info (default)                     ");
      WriteLine("Optional 'time' suffix requests ServerTimeStamp                         ");
      WriteLine("Optional 'val'  suffix requests just the value                          ");
      WriteLine("Channel name of '=' recalls the previously entered name                 ");
      WriteLine("To write an array value : 1 2 3 or 1 2 ... (cyclic repeat)              ");
      WriteLine("Arrays of strings : use '_' to get a space character                    ");
      WriteLine("! or 'exit' to exit                                                     ");
    }

  }

}
