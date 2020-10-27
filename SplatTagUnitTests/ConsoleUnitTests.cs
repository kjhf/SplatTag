using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagConsole;
using SplatTagCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        var mainTask = Task.Run(() =>
        {
          return ConsoleMain.Main(null);
        });
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
      const int millisecondsDelay = 300000; //5000;
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

        Assert.IsTrue(actual.Contains("kjhf1273"));
        Assert.IsTrue(actual.Contains("Inkology"));
        Assert.IsTrue(actual.Contains("Revitalize"));
      }
    }

    /// <summary>
    /// Verify that the console remains open with the keep open option.
    /// </summary>
    [TestMethod]
    public void ConsolePerist()
    {
      const int millisecondsDelay = 2000;
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

        Assert.IsTrue(actual.Contains("kjhf1273"));
        Assert.IsTrue(actual.Contains("Inkology"));
        Assert.IsTrue(actual.Contains("Revitalize"));
        Assert.IsTrue(actual.Contains("Ghost Gaming"));
        Assert.IsFalse(actual.Contains("Nothing to search!"), "Contains nothing to search, which means the query re-executed without input");
      }
    }
  }
}