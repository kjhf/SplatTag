using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagConsole;
using System;
using System.Diagnostics;
using System.IO;
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
    private const int MAX_WAIT_TIME = 20000;
    private readonly TextWriter consoleOut = Console.Out;
    private readonly TextWriter consoleError = Console.Error;

    /// <summary>
    /// Verify that the console starts.
    /// </summary>
    [TestMethod]
    public void ConsoleNoArguments()
    {
      using StringWriter sw = new StringWriter();
      Console.SetOut(sw);
      Console.SetError(sw);

      var timeoutTask = Task.Delay(MAX_WAIT_TIME);
      var mainTask = Task.Run(() => ConsoleMain.Main(null));
      var successfulCheck = Task.Run(() =>
      {
        for (; ; )
        {
          string actual = sw.ToString();
          if (actual.Contains("Choose a function:"))
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

      string expected = string.Format("Choose a function:{0}", Environment.NewLine);
      Assert.IsTrue(actual.Contains(expected));
    }

    /// <summary>
    /// Verify that the console starts.
    /// </summary>
    [TestMethod]
    public void ConsoleSingleQuery()
    {
      using StringWriter sw = new StringWriter();
      Console.SetOut(sw);
      Console.SetError(sw);

      var timeoutTask = Task.Delay(MAX_WAIT_TIME);
      // This task will end once Slapp completes as --keepOpen is not specified
      var mainTask = Task.Run(() =>
      {
        return ConsoleMain.Main("Slate".Split(" "));
      });
      Task.WaitAny(timeoutTask, mainTask);
      Console.SetOut(consoleOut);
      Console.SetError(consoleError);
      string actual = sw.ToString();
      Console.WriteLine("==================================================");
      Console.WriteLine(actual);
      Console.WriteLine("==================================================");

      Assert.IsTrue(actual.Contains("\"Message\":\"OK\""));
      if (actual.Contains("\"Players\":[]"))
      {
        Assert.Inconclusive("The test returned an empty players list. This may be because the database has not been populated. Do so and re-run the test.");
      }
      else
      {
        Assert.IsTrue(actual.Contains("kjhf1273"));
        Assert.IsTrue(actual.Contains("Inkology"));
        Assert.IsTrue(actual.Contains("Revitalize"));
      }
    }

    /// <summary>
    /// Verify that the console handles case sensitive.
    /// </summary>
    [TestMethod]
    public void ConsoleCaseSensitiveQuery()
    {
      Stopwatch stopwatch = new Stopwatch();
      using StringWriter sw = new StringWriter();
      Console.SetOut(sw);
      Console.SetError(sw);

      stopwatch.Start();
      var timeoutTask = Task.Delay(MAX_WAIT_TIME);
      // This task will end once Slapp completes as --keepOpen is not specified
      var mainTask = Task.Run(() =>
      {
        return ConsoleMain.Main("slAte --exactCase".Split(" "));
      });
      Task.WaitAny(timeoutTask, mainTask);
      stopwatch.Stop();
      Console.SetOut(consoleOut);
      Console.SetError(consoleError);
      string actual = sw.ToString();
      Console.WriteLine("==================================================");
      Console.WriteLine(actual);
      Console.WriteLine("==================================================");

      // Should NOT contain the result because the name is "Slate"
      Assert.IsFalse(actual.Contains("kjhf1273"));
      Assert.IsTrue(actual.Contains("OK"));

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
      using (StringWriter sw = new StringWriter())
      {
        Console.SetOut(sw);
        Console.SetError(sw);

        using StringReader tr = new StringReader("sendou\r\nSlate\r\n"); // a TextReader
        using StringReader tr2 = new StringReader("thatsrb2dude\r\n");
        var timeoutTask = Task.Delay(25000);
        var mainTask = Task.Run(() =>
        {
          Console.WriteLine("mainTask: Beginning.");
          ConsoleMain.Main(" --keepOpen".Split(" "));
          Console.WriteLine("mainTask: Finishing.");
        });
        var externalInputTask = Task.Run(async () =>
        {
          await Task.Delay(3000).ConfigureAwait(false);
          Console.WriteLine("externalInputTask: Setting first input.");
          Console.SetIn(tr);

          // Only move on when the input stream has been consumed.
          while (tr.Peek() != -1)
          {
            await Task.Delay(250).ConfigureAwait(false);
          }

          Console.WriteLine("externalInputTask: Setting second input in 2 seconds.");
          await Task.Delay(2000).ConfigureAwait(false);
          Console.SetIn(tr2);
          Console.WriteLine("externalInputTask: Finishing.");
        });
        var successfulCheck = Task.Run(async () =>
        {
          for (; ; )
          {
            string actual = sw.ToString();
            if (actual.Contains("Slate") && actual.Contains("sendou") && actual.Contains("thatsrb2dude"))
            {
              Console.WriteLine("successfulCheck: Passed.");
              break;
            }
            await Task.Delay(250).ConfigureAwait(false);
          }

          Console.WriteLine("successfulCheck: Finishing.");
        });
        bool timedOut = Task.WaitAny(timeoutTask, externalInputTask, mainTask) == 0;
        Console.WriteLine($"timedOut={timedOut}");
        bool wasSuccessfulCheck = Task.WaitAny(timeoutTask, mainTask, successfulCheck) == 2;
        Console.WriteLine($"wasSuccessfulCheck={wasSuccessfulCheck}");
        Console.WriteLine($"mainTask.IsCompleted={mainTask.IsCompleted}");
        Console.WriteLine($"externalInputTask.IsCompleted={externalInputTask.IsCompleted}");

        Console.SetOut(consoleOut);
        Console.SetError(consoleError);
        string actual = sw.ToString();
        Console.WriteLine("==================================================");
        Console.WriteLine(actual);
        Console.WriteLine("==================================================");

        Assert.IsTrue(actual.Contains("\"Message\":\"OK\""), "Unexpected message result");
        if (actual.Contains("\"Players\":[]"))
        {
          Assert.Inconclusive("The test returned an empty players list. This may be because the database has not been populated. Do so and re-run the test.");
        }
        else
        {
          Assert.IsTrue(actual.Contains("Slate"), "Doesn't contain Slate query");
          Assert.IsTrue(actual.Contains("sendou"), "Doesn't contain sendou query");
          Assert.IsTrue(actual.Contains("thatsrb2dude"), "Doesn't contain thatsrb2dude query");

          Assert.IsTrue(actual.Contains("kjhf1273"), "Doesn't contain kjhf1273");
          Assert.IsTrue(actual.Contains("Inkology"), "Doesn't contain Inkology");
          Assert.IsTrue(actual.Contains("Revitalize"), "Doesn't contain Revitalize");
          Assert.IsTrue(actual.Contains("Ghost Gaming"), "Doesn't contain Ghost Gaming");
          Assert.IsTrue(actual.Contains("Team Olive"), "Doesn't contain Team Olive");
          Assert.IsFalse(actual.Contains("Nothing to search!"), "Contains nothing to search, which means the query re-executed without input");
          Assert.IsFalse(mainTask.IsCompleted, "The main task should not have completed because keepOpen is specified."); // Test last
        }
      }
    }
  }
}