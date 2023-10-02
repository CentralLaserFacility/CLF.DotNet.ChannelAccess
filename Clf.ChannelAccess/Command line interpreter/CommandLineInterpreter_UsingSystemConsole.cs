//
// CommandLineInterpreter.cs
//

using System.Collections.Generic;
using System.Threading.Tasks;
using Clf.Common.ExtensionMethods;
using System.Linq;

namespace Clf.ChannelAccess
{

  public class CommandLineInterpreter_UsingSystemConsole : CommandLineInterpreter
  {

    // Hmm, could set these up as Attributes of the Category values ???

    public static Dictionary<TextCategory, System.ConsoleColor> ConsoleColoursMap = new(){
      { TextCategory.Prompt                     , System.ConsoleColor.White  },
      { TextCategory.UserInput                  , System.ConsoleColor.Green  },
      { TextCategory.Response_Normal            , System.ConsoleColor.White  },
      { TextCategory.Response_Exception         , System.ConsoleColor.Red    },
      { TextCategory.AsyncNotification_Normal   , System.ConsoleColor.Yellow },
      { TextCategory.AsyncNotification_Abnormal , System.ConsoleColor.Red    } ,
      { TextCategory.InformationalMessage       , System.ConsoleColor.Cyan   }
    };

    private static async Task<string> GetConsoleInputLineAsync()
    {
      // Initially tried the following :
      //   return await System.Console.In.ReadLineAsync() ?? "" ;
      // Hmm, you'd expect 'ReadLineAsync()' to work fine,
      // but although it appears to function correctly in that we can
      // get successive lines of command-line input, it BLOCKS THE CALLING THREAD
      // and this prevents the 'AsyncPump' mechanism from being able to handle
      // callbacks posted to the SynchronisationContext.
      // --------------------------
      // OK, this behaviour is actually documented in the small print ...
      // https://docs.microsoft.com/en-us/dotnet/api/system.io.textreader.readlineasync
      // ... which states 'If the current TextReader represents the standard input stream
      // returned by the Console.In property, the ReadLineAsync method executes synchronously
      // rather than asynchronously.'
      // Also here :
      // https://docs.microsoft.com/en-us/dotnet/api/system.console.in?view=netframework-4.7#Remarks
      // Read operations on the standard input stream execute synchronously. That is, they block
      // until the specified read operation has completed. This is true even if an asynchronous method,
      // such as ReadLineAsync, is called on the TextReader object returned by the In property.
      // ----------------------------------
      // But for goodness sake ... it really shouldn't be named with the 'Async' suffix
      // when every other 'Async' API works asynchronously, ie doesn't block your thread !!!
      // https://stackoverflow.com/questions/14724582/why-does-console-in-readlineasync-block
      // https://smellegantcode.wordpress.com/tag/c-5-0/
      // The real killer is : it kindof works asynchronously as you'd expect, but
      // the details of the threading behaviour are different from other Async methods
      // and that screws things up when you're relying on the expected behaviour
      // in more complex situations.
      return await Task.Run(
        () =>
        {
          using (var promptColourSwitcher = new ConsoleTextColourSwitcher(TextCategory.Prompt))
          {
            System.Console.Write(Prompt);
          }
          using (var inputColourSwitcher = new ConsoleTextColourSwitcher(TextCategory.UserInput))
          {
            return System.Console.ReadLine() ?? "";
          }
        }
      );
    }

    private sealed class ConsoleTextColourSwitcher : System.IDisposable
    {
      private readonly System.ConsoleColor m_originalColour;
      public ConsoleTextColourSwitcher(System.ConsoleColor colour)
      {
        m_originalColour = System.Console.ForegroundColor;
        System.Console.ForegroundColor = colour;
      }
      public ConsoleTextColourSwitcher(TextCategory category) :
      this(
        ConsoleColoursMap[category]
      )
      {
      }
      public void Dispose()
      {
        System.Console.ForegroundColor = m_originalColour;
      }
    }

    private sealed class ConsoleBackgroundColourSwitcher : System.IDisposable
    {
      public static System.ConsoleColor DefaultBackgroundColour = System.ConsoleColor.Black;
      private readonly System.ConsoleColor m_originalBackgroundColour;
      public ConsoleBackgroundColourSwitcher(System.ConsoleColor backgroundColour)
      {
        m_originalBackgroundColour = System.Console.BackgroundColor;
        System.Console.BackgroundColor = backgroundColour;
      }
      public void Dispose()
      {
        System.Console.BackgroundColor = m_originalBackgroundColour;
      }
    }

    private static void WriteLineToConsoleOutput(string line, System.ConsoleColor colour)
    {
      using var colourSwitcher = new ConsoleTextColourSwitcher(colour);
      System.Console.WriteLine(line);
    }

    private static void WriteLineToConsoleOutput(string line, TextCategory category)
    {
      switch (category)
      {
        case TextCategory.Prompt:
        case TextCategory.UserInput:
        case TextCategory.Response_Normal:
        case TextCategory.Response_Exception:
          WriteNormalResponseLine(line, category);
          break;
        case TextCategory.AsyncNotification_Normal:
        case TextCategory.AsyncNotification_Abnormal:
        case TextCategory.InformationalMessage:
          WriteAsyncNotificationLine(line, category);
          break;
        default:
          throw category.AsUnexpectedEnumValueException();
      }
      void WriteNormalResponseLine(string line, TextCategory category)
      {
        using (var colourSwitcher = new ConsoleTextColourSwitcher(category))
        {
          System.Console.WriteLine(line);
        }
      }
      void WriteAsyncNotificationLine(string line, TextCategory category)
      {
        // We want to write a 'notification' line which will start
        // at the extreme left hand edge of the console panel.
        // If we're waiting for user input at the time this line
        // needs to be written, we'll need to reset the cursor position
        // and erase the prompt characters we've already written.
        // 
        EnsurePositionedAtStart(
          out bool hadToEraseExistingPrompt
        );
        using (var colourSwitcher = new ConsoleTextColourSwitcher(category))
        {
          System.Console.WriteLine(line);
        }
        if (hadToEraseExistingPrompt)
        {
          // Rewrite the prompt ...
          using (var colourSwitcher = new ConsoleTextColourSwitcher(category))
          {
            System.Console.Write(Prompt);
          }
        }
      }
      void EnsurePositionedAtStart(out bool hadToEraseExistingPrompt)
      {
        hadToEraseExistingPrompt = false;
        var cursorPosition = System.Console.GetCursorPosition();
        if (cursorPosition.Left != 0)
        {
          System.Console.SetCursorPosition(
            0,
            cursorPosition.Top
          );
          System.Console.Write(
            new string(' ', Prompt.Length)
          );
          System.Console.SetCursorPosition(
            0,
            cursorPosition.Top
          );
          hadToEraseExistingPrompt = true;
        }
      }
    }

    private IEnumerable<string> CommandLineArgsSuppliedWhenLaunchingApp;

    public CommandLineInterpreter_UsingSystemConsole() :
    base(
      writeOutputLineAction: WriteLineToConsoleOutput
    )
    {
      CommandLineArgsSuppliedWhenLaunchingApp = System.Environment.GetCommandLineArgs().Skip(1)!;
      WriteLine("ChannelAccess CLI");
#if DEBUG
      WriteLine(
        $"SynchronizationContext is {(
            ChannelsRegistry.SynchronizationContextSupportsResumingOnCallingThread
            ? "valid"
            : "null"
          )}"
      );
#endif
      WriteLine("CLF ChannelAccess CLI");
      WriteLine($"Prototype, version {GetType().Assembly.GetName().Version}");
      WriteLine("h for help, ! to exit");
    }

    public async Task WaitForCommandLinesAndRunThem()
    {
      CommandLineArgsSuppliedWhenLaunchingApp.ForEachItem(
        async arg =>
        {
          // /monitor=pvName
          if (arg.StartsWith("/monitor="))
          {
            string pvName = arg.Split('=')[1];
            await StartMonitoringPV(pvName);
          }
        }
      );
      while (true)
      {
        string commandLine = await GetConsoleInputLineAsync();
        // Handle special commands that are
        // only relevant in a Console app
        if (
           commandLine == "?"
        || commandLine == "h"
        || commandLine == "help"
        )
        {
          WriteHelpLines();
        }
        else if (
          commandLine == "$colours"
        )
        {
          WriteLine("Colours :");
          WriteLine("User input", TextCategory.UserInput);
          WriteLine("Response to a command, ok", TextCategory.Response_Normal);
          WriteLine("Response to a command, error", TextCategory.Response_Exception);
          WriteLine("Async notification, ok", TextCategory.AsyncNotification_Normal);
          WriteLine("Async notification, abnormal", TextCategory.AsyncNotification_Abnormal);
          WriteLine("Informational message", TextCategory.InformationalMessage);
          WriteLine("------------------------", TextCategory.Response_Normal);
          foreach (
            var colour in System.Enum.GetValues<System.ConsoleColor>(
            ).Where(
              colour => colour != System.ConsoleColor.Black
            )
          )
          {
            WriteLineToConsoleOutput(
              $"This is text in {colour}",
              colour
            );
          }
        }
        else if (
           commandLine == "!"
        || commandLine == "exit"
        )
        {
          return;
        }
        else
        {
          await HandleCommandLineCommand(
            commandLine
          );
        }
      }
    }

    public static void CreateInstanceAndExecute()
    {
      Common.AsyncPump.RunAsyncFunctionReturningTask(
        async () =>
        {
          await CreateInstanceAndExecuteAsync();
        }
      );
    }

    public static async Task CreateInstanceAndExecuteAsync()
    {
      try
      {
        var cli = new CommandLineInterpreter_UsingSystemConsole();
        await cli.WaitForCommandLinesAndRunThem();
      }
      catch (System.Exception x)
      {
        System.Console.WriteLine(
          $"FATAL EXCEPTION : '{x.Message}'"
        );
        System.Console.WriteLine(
          $"Waiting to exit ..."
        );
        System.Console.ReadLine();
      }
    }

  }

}
