using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagConsole;
using SplatTagCore;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SplatTagUnitTests
{
  /// <summary>
  /// Core SplatTag unit tests
  /// </summary>
  [TestClass]
  public class ConsoleUnitTests
  {
    private const int MAX_WAIT_TIME = 60000;
    private readonly TextWriter consoleOut = Console.Out;
    private readonly TextWriter consoleError = Console.Error;

    private static readonly string CHOOSE_A_FUNCTION = ("Choose a function:" + Environment.NewLine);
    private const string MESSAGE_OK = "\"Message\":\"OK\"";
    private const string EMPTY_PLAYERS = "\"Players\":[]";
    private const string POPULATED_PLAYERS = "\"Players\":[{";
    private const string SLATE = "Slate";

    /// <summary>
    /// Verify that the console starts.
    /// </summary>
    [TestMethod]
    public void ConsoleNoArguments()
    {
      using StringWriter sw = new();
      Console.SetOut(sw);
      Console.SetError(sw);
      string expected = CHOOSE_A_FUNCTION;

      var timeoutTask = Task.Delay(MAX_WAIT_TIME);
      var mainTask = Task.Run(() => ConsoleMain.Main(Array.Empty<string>()));
      var successfulCheck = Task.Run(() =>
      {
        for (; ; )
        {
          string actual = sw.ToString().Base64DecodeByLines();
          if (actual.Contains(expected))
          {
            break;
          }
          Thread.Sleep(250);
        }
      });
      Task.WaitAny(timeoutTask, mainTask, successfulCheck);
      Console.SetOut(consoleOut);
      Console.SetError(consoleError);
      string actual = sw.ToString();
      Console.WriteLine("==================================================");
      Console.WriteLine(actual);
      Console.WriteLine("==================================================");
      Console.WriteLine(actual = actual.Base64DecodeByLines());
      Console.WriteLine("==================================================");
      Assert.IsTrue(actual.Contains(expected));
    }

    /// <summary>
    /// Verify that the console starts.
    /// </summary>
    [TestMethod]
    public void ConsoleSingleQuery()
    {
      using StringWriter sw = new();
      Console.SetOut(sw);
      Console.SetError(sw);

      var timeoutTask = Task.Delay(MAX_WAIT_TIME);
      // This task will end once Slapp completes as --keepOpen is not specified
      var mainTask = Task.Run(async () =>
      {
        await ConsoleMain.Main("--query Slate".Split(" ")).ConfigureAwait(false);
        await Task.Delay(500).ConfigureAwait(false); // Let any logging finish.
      });
      Task.WaitAny(timeoutTask, mainTask);
      Console.SetOut(consoleOut);
      Console.SetError(consoleError);
      string actual = sw.ToString();
      Console.WriteLine("==================================================");
      Console.WriteLine(actual);
      Console.WriteLine("==================================================");
      Console.WriteLine(actual = actual.Base64DecodeByLines());
      Console.WriteLine("==================================================");

      Assert.IsTrue(actual.Contains(MESSAGE_OK));
      Assert.IsTrue(actual.Contains("kjhf1273"));
      Assert.IsTrue(actual.Contains("Inkology"));
      Assert.IsTrue(actual.Contains("Revitalize"));
      Assert.IsTrue(actual.Contains("2019-03-25-LUTI-S8"));  // Check sources populated
      Assert.IsTrue(actual.Count("UNLINKED") == 1);  // Check no unlinked teams (except the one specified in Additional Sources)
      Assert.IsTrue(actual.Count("Built-in") >= 1);  // Check for built-in sources (one specified in Additional Sources)
    }

    /// <summary>
    /// Verify that the console starts.
    /// </summary>
    [TestMethod]
    public void ConsoleSingleQueryB64()
    {
      using StringWriter sw = new();
      Console.SetOut(sw);
      Console.SetError(sw);

      var timeoutTask = Task.Delay(MAX_WAIT_TIME);
      // This task will end once Slapp completes as --keepOpen is not specified
      var mainTask = Task.Run(async () =>
      {
        await ConsoleMain.Main(("--b64 " + SLATE.Base64Encode()).Split(" ")).ConfigureAwait(false);
        await Task.Delay(500).ConfigureAwait(false); // Let any logging finish.
      });
      Task.WaitAny(timeoutTask, mainTask);
      Console.SetOut(consoleOut);
      Console.SetError(consoleError);
      string actual = sw.ToString();
      Console.WriteLine("==================================================");
      Console.WriteLine(actual);
      Console.WriteLine("==================================================");
      Console.WriteLine(actual = actual.Base64DecodeByLines());
      Console.WriteLine("==================================================");

      Assert.IsTrue(actual.Contains(MESSAGE_OK));
      Assert.IsTrue(actual.Contains("kjhf1273"));
      Assert.IsTrue(actual.Contains("Inkology"));
      Assert.IsTrue(actual.Contains("Revitalize"));
      Assert.IsTrue(actual.Contains("2019-03-25-LUTI-S8"));  // Check sources populated
      Assert.IsTrue(actual.Count("UNLINKED") == 1);  // Check no unlinked teams (except the one specified in Additional Sources)
      Assert.IsTrue(actual.Count("Built-in") >= 1);  // Check for built-in sources (one specified in Additional Sources)
    }

    /// <summary>
    /// Verify that the console handles case sensitive.
    /// </summary>
    [TestMethod]
    public void ConsoleCaseSensitiveQuery()
    {
      Stopwatch stopwatch = new();
      using StringWriter sw = new();
      Console.SetOut(sw);
      Console.SetError(sw);

      stopwatch.Start();
      var timeoutTask = Task.Delay(MAX_WAIT_TIME);
      // This task will end once Slapp completes as --keepOpen is not specified
      var mainTask = Task.Run(async () =>
      {
        await ConsoleMain.Main("--query slAte --exactCase".Split(" ")).ConfigureAwait(false);
        await Task.Delay(500).ConfigureAwait(false); // Let any logging finish.
      });
      Task.WaitAny(timeoutTask, mainTask);
      stopwatch.Stop();
      Console.SetOut(consoleOut);
      Console.SetError(consoleError);
      string actual = sw.ToString();
      Console.WriteLine("==================================================");
      Console.WriteLine(actual);
      Console.WriteLine("==================================================");
      Console.WriteLine(actual = actual.Base64DecodeByLines());
      Console.WriteLine("==================================================");

      // Should NOT contain the result because the name is "Slate"
      Assert.IsFalse(actual.Contains("kjhf1273"));
      Assert.IsTrue(actual.Contains(MESSAGE_OK));

      if (stopwatch.ElapsedMilliseconds > 3000)
      {
        Assert.Inconclusive($"Test passed but it took {stopwatch.ElapsedMilliseconds}ms which is unacceptable for a Console query. Aim for < 3 seconds.");
      }
    }

    /// <summary>
    /// Verify that the console remains open with the keep open option.
    /// </summary>
    [TestMethod]
    public void ConsolePerist()
    {
      using (StringWriter sw = new())
      {
        Console.SetOut(sw);
        Console.SetError(sw);
        Console.SetIn(new StringReader(string.Empty));

        using StringReader tr = new("--b64 " + "ig manny".Base64Encode() + "\r\n--b64 " + "Slate".Base64Encode() + "\r\n"); // a TextReader -- also tests spaces.
        using StringReader tr2 = new("--b64 " + "thatsrb2dude".Base64Encode() + "\r\n");
        var timeoutTask = Task.Delay(MAX_WAIT_TIME);
        var mainTask = Task.Run(async () =>
        {
          Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ff}] mainTask: Beginning.");
          await ConsoleMain.Main("--keepOpen".Split(" ")).ConfigureAwait(false);
          Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ff}] mainTask: Finishing.");
          await Task.Delay(500).ConfigureAwait(false); // Let any logging finish.
        });
        var externalInputTask = Task.Run(async () =>
        {
          Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ff}] externalInputTask: Waiting 3 seconds.");
          await Task.Delay(3000).ConfigureAwait(false);
          Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ff}] externalInputTask: Setting first input.");
          Console.SetIn(tr);

          // Only move on when the input stream has been consumed.
          while (tr.Peek() != -1)
          {
            await Task.Delay(250).ConfigureAwait(false);
          }

          Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ff}] externalInputTask: Setting second input in 2 seconds.");
          await Task.Delay(2000).ConfigureAwait(false);
          Console.SetIn(tr2);
          Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ff}] externalInputTask: Input set, finishing.");
        });

        var successfulCheck = Task.Run(async () =>
        {
          for (; ; )
          {
            string actual = sw.ToString().Base64DecodeByLines();
            if (actual.Contains("Slate") && actual.Contains("ig manny") && actual.Contains("thatsrb2dude"))
            {
              Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ff}] successfulCheck: Passed.");
              break;
            }
            await Task.Delay(250).ConfigureAwait(false);
          }

          Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ff}] successfulCheck: Finishing.");
        });

        bool timedOut = Task.WaitAny(timeoutTask, externalInputTask, mainTask) == 0;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ff}] timedOut={timedOut}");
        bool wasSuccessfulCheck = Task.WaitAny(timeoutTask, mainTask, successfulCheck) == 2;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ff}] wasSuccessfulCheck={wasSuccessfulCheck}");
        Console.WriteLine($"mainTask.IsCompleted={mainTask.IsCompleted}");
        Console.WriteLine($"externalInputTask.IsCompleted={externalInputTask.IsCompleted}");

        Console.SetOut(consoleOut);
        Console.SetError(consoleError);
        string actual = sw.ToString();
        Console.WriteLine("==================================================");
        Console.WriteLine(actual);
        Console.WriteLine("==================================================");
        Console.WriteLine(actual = actual.Base64DecodeByLines());
        Console.WriteLine("==================================================");

        Assert.IsTrue(actual.Contains(MESSAGE_OK), "Unexpected message result");
        if (actual.Contains(EMPTY_PLAYERS) && !actual.Contains(POPULATED_PLAYERS))
        {
          Assert.Inconclusive("The test returned an empty players list. This may be because the database has not been populated. Do so and re-run the test.");
        }
        else
        {
          Assert.IsTrue(actual.Contains(POPULATED_PLAYERS), "Doesn't contain matched players");
          Assert.IsTrue(actual.Contains(SLATE), "Doesn't contain Slate");
          Assert.IsTrue(actual.Contains("ig manny"), "Doesn't contain ig manny query");
          Assert.IsTrue(actual.Contains("thatsrb2dude"), "Doesn't contain thatsrb2dude query");

          Assert.IsTrue(actual.Contains("kjhf1273"), "Doesn't contain kjhf1273");
          Assert.IsTrue(actual.Contains("Inkology"), "Doesn't contain Inkology");
          Assert.IsTrue(actual.Contains("Revitalize"), "Doesn't contain Revitalize");
          Assert.IsFalse(mainTask.IsCompleted, "The main task should not have completed because keepOpen is specified."); // Test last
        }
      }
    }
  }
}