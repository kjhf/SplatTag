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
    private const string EMPTY_PLAYERS = "\"Players\":[]";
    private const int MAX_WAIT_TIME_MILLIS = 300000;
    private const string MESSAGE_OK = "\"Message\":\"OK\"";
    private const string POPULATED_PLAYERS = "\"Players\":[{";
    private const string SLATE = "Slate";
    private static readonly string CHOOSE_A_FUNCTION = ("Choose a function:" + Environment.NewLine);
    private static readonly TimeSpan MAX_WAIT_TIME_TS = TimeSpan.FromMilliseconds(MAX_WAIT_TIME_MILLIS);
    private readonly TextWriter consoleError = Console.Error;
    private readonly TextWriter consoleOut = Console.Out;
    private static ConsoleController? consoleController;

    [ClassInitialize]
    public static void CreateAndLoadSnapshot(TestContext context)
    {
      Stopwatch stopwatch = new();
      stopwatch.Start();
      consoleController = new();
      consoleController.EnsureInitialised();
      stopwatch.Stop();
      Console.WriteLine("It took " + stopwatch.Elapsed + " to initialise and load the controller.");
    }

    /// <summary>
    /// Verify that the console handles case sensitive.
    /// </summary>
    [TestMethod]
    public async Task ConsoleCaseSensitiveQuery()
    {
      Stopwatch stopwatch = new();
      using StringWriter sw = new();
      Console.SetOut(sw);
      Console.SetError(sw);
      stopwatch.Start();

      // This task will end once Slapp completes as --keepOpen is not specified
      bool timedOut = false;
      try
      {
        await Task.Run(async () =>
        {
          await consoleController!.Handle("--query slAte --exactCase".Split(" ")).ConfigureAwait(false);
          await Task.Delay(500).ConfigureAwait(false); // Let any logging finish.
        }).WaitAsync(MAX_WAIT_TIME_TS);
      }
      catch (TimeoutException)
      {
        timedOut = true;
      }

      stopwatch.Stop();

      Console.SetOut(consoleOut);
      Console.SetError(consoleError);
      string actual = sw.ToString();
      Console.WriteLine("==================================================");
      Console.WriteLine(actual);
      Console.WriteLine("==================================================");
      actual = actual.Base64DecodeByLines();
      Console.WriteLine(actual);
      Console.WriteLine("==================================================");
      if (timedOut)
      {
        Assert.Fail("The test timed out.");
      }

      // Should NOT contain the result because the name is "Slate"
      Assert.IsFalse(actual.Contains("kjhf1273"));
      Assert.IsTrue(actual.Contains(MESSAGE_OK));

      if (stopwatch.ElapsedMilliseconds > 3000)
      {
        Assert.Inconclusive($"Test passed but it took {stopwatch.ElapsedMilliseconds}ms which is unacceptable for a Console query. Aim for < 3 seconds.");
      }
    }

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

      bool timedOut = false;
      try
      {
        var mainTask = Task.Run(() => consoleController!.Handle(Array.Empty<string>())).WaitAsync(MAX_WAIT_TIME_TS);
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
        }).WaitAsync(MAX_WAIT_TIME_TS);
        Task.WaitAny(mainTask, successfulCheck);
      }
      catch (TimeoutException)
      {
        timedOut = true;
      }
      Console.SetOut(consoleOut);
      Console.SetError(consoleError);
      string actual = sw.ToString();
      Console.WriteLine("==================================================");
      Console.WriteLine(actual);
      Console.WriteLine("==================================================");
      actual = actual.Base64DecodeByLines();
      Console.WriteLine(actual);
      Console.WriteLine("==================================================");
      if (timedOut)
      {
        Assert.Fail("The test timed out.");
      }
      Assert.IsTrue(actual.Contains(expected));
    }

    /// <summary>
    /// Verify that the console remains open with the keep open option.
    /// </summary>
    [TestMethod]
    public void ConsolePerist()
    {
      Task? mainTask = null;
      CancellationTokenSource persistTokenSource = new();
      CancellationToken persistToken = persistTokenSource.Token;

      using (StringWriter sw = new())
      {
        Console.SetOut(sw);
        Console.SetError(sw);
        Console.SetIn(new StringReader(string.Empty));

        using StringReader tr = new("--b64 " + "ig manny".Base64Encode() + "\r\n--b64 " + "Slate".Base64Encode() + "\r\n"); // a TextReader -- also tests spaces.
        using StringReader tr2 = new("--b64 " + "thatsrb2dude".Base64Encode() + "\r\n");

        bool timedOut = false;
        try
        {
          mainTask = Task.Run(async () =>
          {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ff}] mainTask: Beginning.");
            await consoleController!.Handle("--keepOpen".Split(" ")).ConfigureAwait(false);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ff}] mainTask: Finishing.");
            await Task.Delay(500).ConfigureAwait(false); // Let any logging finish.
          }).WaitAsync(MAX_WAIT_TIME_TS, persistToken);

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
          }).WaitAsync(MAX_WAIT_TIME_TS, persistToken);

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
          }).WaitAsync(MAX_WAIT_TIME_TS, persistToken);

          Task.WaitAny(externalInputTask, mainTask);
          bool wasSuccessfulCheck = Task.WaitAny(mainTask, successfulCheck) == 2;
          Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ff}] wasSuccessfulCheck={wasSuccessfulCheck}");
          Console.WriteLine($"mainTask.IsCompleted={mainTask.IsCompleted}");
          Console.WriteLine($"externalInputTask.IsCompleted={externalInputTask.IsCompleted}");
        }
        catch (TimeoutException)
        {
          timedOut = true;
        }

        bool? mainTaskCompleted = mainTask?.IsCompleted;
        persistTokenSource.Cancel();
        Console.SetOut(consoleOut);
        Console.SetError(consoleError);
        string actual = sw.ToString();
        Console.WriteLine("==================================================");
        Console.WriteLine(actual);
        Console.WriteLine("==================================================");
        actual = actual.Base64DecodeByLines();
        Console.WriteLine(actual);
        Console.WriteLine("==================================================");
        if (timedOut)
        {
          Assert.Fail("The test timed out.");
        }

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
          Assert.IsFalse(mainTaskCompleted, "The main task should not have completed because keepOpen is specified.");
        }
      }
    }

    /// <summary>
    /// Verify that the console starts.
    /// </summary>
    [TestMethod]
    public async Task ConsoleSingleQuery()
    {
      using StringWriter sw = new();
      Console.SetOut(sw);
      Console.SetError(sw);

      bool timedOut = false;
      try
      {
        // This task will end once Slapp completes as --keepOpen is not specified
        await Task.Run(async () =>
        {
          await consoleController!.Handle("--query Slate".Split(" ")).ConfigureAwait(false);
          await Task.Delay(1000).ConfigureAwait(false); // Let any logging finish.
        }).WaitAsync(MAX_WAIT_TIME_TS);
      }
      catch (TimeoutException)
      {
        timedOut = true;
      }

      Console.SetOut(consoleOut);
      Console.SetError(consoleError);
      string actual = sw.ToString();
      Console.WriteLine("==================================================");
      Console.WriteLine(actual);
      Console.WriteLine("==================================================");
      actual = actual.Base64DecodeByLines();
      Console.WriteLine(actual);
      Console.WriteLine("==================================================");

      if (timedOut)
      {
        Assert.Fail("The test timed out.");
      }

      Assert.IsTrue(actual.Contains(MESSAGE_OK));
      Assert.IsTrue(actual.Contains("kjhf1273"));
      Assert.IsTrue(actual.Contains("Inkology"));
      Assert.IsTrue(actual.Contains("Revitalize"));
      Assert.IsTrue(actual.Contains("2019-03-25-LUTI-S8"));  // Check sources populated
      Assert.IsTrue(actual.Count(Builtins.BuiltinSource.Name) >= 1);  // Check for built-in sources (one specified in Additional Sources)
      Assert.IsTrue(actual.Count(Builtins.ManualSource.Name) >= 1);  // Check for built-in sources (one specified in Additional Sources)
    }

    /// <summary>
    /// Verify that the console starts.
    /// </summary>
    [TestMethod]
    public async Task ConsoleSingleQueryB64()
    {
      using StringWriter sw = new();
      Console.SetOut(sw);
      Console.SetError(sw);

      bool timedOut = false;
      try
      {
        // This task will end once Slapp completes as --keepOpen is not specified
        await Task.Run(async () =>
        {
          await consoleController!.Handle($"--b64 {SLATE.Base64Encode()}".Split(" ")).ConfigureAwait(false);
          await Task.Delay(1000).ConfigureAwait(false); // Let any logging finish.
        }).WaitAsync(MAX_WAIT_TIME_TS);
      }
      catch (TimeoutException)
      {
        timedOut = true;
      }
      await Task.Delay(1000).ConfigureAwait(false); // Let any logging finish.

      Console.SetOut(consoleOut);
      Console.SetError(consoleError);
      string actual = sw.ToString();
      Console.WriteLine("==================================================");
      Console.WriteLine(actual);
      Console.WriteLine("==================================================");
      actual = actual.Base64DecodeByLines();
      Console.WriteLine(actual);
      Console.WriteLine("==================================================");
      if (timedOut)
      {
        Assert.Fail("The test timed out.");
      }

      Assert.IsTrue(actual.Contains(MESSAGE_OK));
      Assert.IsTrue(actual.Contains("kjhf1273"));
      Assert.IsTrue(actual.Contains("Inkology"));
      Assert.IsTrue(actual.Contains("Revitalize"));
      Assert.IsTrue(actual.Contains("2019-03-25-LUTI-S8"));  // Check sources populated
      Assert.IsTrue(actual.Count(Builtins.BuiltinSource.Name) >= 1);  // Check for built-in sources (one specified in Additional Sources)
      Assert.IsTrue(actual.Count(Builtins.ManualSource.Name) >= 1);  // Check for built-in sources (one specified in Additional Sources)
    }
  }
}