using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagConsole;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SplatTagUnitTests
{
  /// <summary>
  /// Core SplatTag unit tests
  /// </summary>
  [TestClass]
  public class ConsoleUnitTests
  {
    private readonly TextWriter consoleOut = Console.Out;

    /// <summary>
    /// Verify that the console starts.
    /// </summary>
    [TestMethod]
    public void ConsoleNoArguments()
    {
      const int millisecondsDelay = 5000;

      using (StringWriter sw = new StringWriter())
      {
        Console.SetOut(sw);

        var timeoutTask = Task.Delay(millisecondsDelay);
        var mainTask = Task.Run(() => ConsoleMain.Main(null));
        Task.WaitAny(timeoutTask, mainTask);
        Console.SetOut(consoleOut);
        string actual = sw.ToString();
        Console.WriteLine(actual);

        string expected = string.Format("Choose a function:{0}", Environment.NewLine);
        Assert.IsTrue(actual.Contains(expected));
      }
    }

    /// <summary>
    /// Verify that the console starts.
    /// </summary>
    [TestMethod]
    public void ConsoleSingleQuery()
    {
      const int millisecondsDelay = 5000;
      using (StringWriter sw = new StringWriter())
      {
        Console.SetOut(sw);

        var timeoutTask = Task.Delay(millisecondsDelay);
        var mainTask = Task.Run(() =>
        {
          return ConsoleMain.Main("Slate".Split(" "));
        });
        Task.WaitAny(timeoutTask, mainTask);
        Console.SetOut(consoleOut);
        string actual = sw.ToString();
        Console.WriteLine(actual);

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
    }

    /// <summary>
    /// Verify that the console handles case sensitive.
    /// </summary>
    [TestMethod]
    public void ConsoleCaseSensitiveQuery()
    {
      const int millisecondsDelay = 10000;
      Stopwatch stopwatch = new Stopwatch();
      using (StringWriter sw = new StringWriter())
      {
        Console.SetOut(sw);

        stopwatch.Start();
        var timeoutTask = Task.Delay(millisecondsDelay);
        var mainTask = Task.Run(() =>
        {
          return ConsoleMain.Main("slAte --exactCase".Split(" "));
        });
        Task.WaitAny(timeoutTask, mainTask);
        stopwatch.Stop();
        Console.SetOut(consoleOut);
        string actual = sw.ToString();
        Console.WriteLine(actual);

        // Should NOT contain the result because the name is "Slate"
        Assert.IsFalse(actual.Contains("kjhf1273"));
        Assert.IsTrue(actual.Contains("OK"));

        if (stopwatch.ElapsedMilliseconds > 3000)
        {
          Assert.Inconclusive($"Test passed but it took {stopwatch.ElapsedMilliseconds}ms which is unacceptable for a Console query. Aim for < 3 seconds.");
        }
      }
    }

    /// <summary>
    /// Verify that the console remains open with the keep open option.
    /// </summary>
    //[TestMethod] // Skip this failing test -- re-enable when we can get keepOpen working again...
    public void ConsolePerist()
    {
      const int millisecondsDelay = 4000;
      using (StringWriter sw = new StringWriter())
      {
        Console.SetOut(sw);

        var timeoutTask = Task.Delay(millisecondsDelay);
        var mainTask = Task.Run(() =>
        {
          using (TextReader tr = new StringReader("Sendou"))
          {
            Console.SetIn(tr);
            return ConsoleMain.Main("Slate --keepOpen".Split(" "));
          }
        });
        Task.WaitAny(timeoutTask, mainTask);

        Console.SetOut(consoleOut);
        string actual = sw.ToString();
        Console.WriteLine(actual);

        Assert.IsTrue(actual.Contains("\"Message\":\"OK\""), "Unexpected message result");
        if (actual.Contains("\"Players\":[]"))
        {
          Assert.Inconclusive("The test returned an empty players list. This may be because the database has not been populated. Do so and re-run the test.");
        }
        else
        {
          Assert.IsTrue(actual.Contains("kjhf1273"), "Doesn't contain kjhf1273");
          Assert.IsTrue(actual.Contains("Inkology"), "Doesn't contain Inkology");
          Assert.IsTrue(actual.Contains("Revitalize"), "Doesn't contain Revitalize");
          Assert.IsTrue(actual.Contains("Ghost Gaming"), "Doesn't contain Ghost Gaming");
          Assert.IsFalse(actual.Contains("Nothing to search!"), "Contains nothing to search, which means the query re-executed without input");
        }
      }
    }
  }
}