using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagConsole;
using SplatTagCore;
using SplatTagDatabase;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SplatTagUnitTests
{
  /// <summary>
  /// Unit tests for the end to end test data
  /// </summary>
  [TestClass]
  public class EndToEndUnitTests
  {
    private readonly TextWriter consoleOut = Console.Out;
    private readonly TextWriter consoleError = Console.Error;
    private const string MESSAGE_REBUILT = "\"Message\":\"Database rebuilt from";

    private static readonly string current;
    private static readonly string sourcesPath;

    static EndToEndUnitTests()
    {
      current = Directory.GetCurrentDirectory();
      Console.WriteLine("Working from " + current);
      if (current.Contains("SplatTagUnitTests"))
      {
        current = current.Substring(0, current.IndexOf("SplatTagUnitTests") + "SplatTagUnitTests".Length);
        current = Path.Combine(current, "EndToEndData");
      }
      sourcesPath = Path.Combine(current, "sources.yaml");

      foreach (FileInfo snapshot in SplatTagJsonSnapshotDatabase.GetSnapshots(current).ToArray())
      {
        snapshot.Delete();
      }
    }

    /// <summary>
    /// Verify that the end to end runs.
    /// </summary>
    [TestMethod]
    public void EndToEndBuild()
    {
      using StringWriter sw = new StringWriter();
      Console.SetOut(sw);
      Console.SetError(sw);

      var mainTask = Task.Run(async () =>
      {
        await ConsoleMain.Main($"--verbose --rebuild {sourcesPath}".Split(" ")).ConfigureAwait(false);
        await Task.Delay(500).ConfigureAwait(false); // Let any logging finish.
      });

      Task.WaitAny(mainTask);

      Console.SetOut(consoleOut);
      Console.SetError(consoleError);
      string actual = sw.ToString();
      Console.WriteLine("==================================================");
      Console.WriteLine(actual = actual.Base64DecodeByLines());
      Console.WriteLine("==================================================");

      Assert.IsTrue(actual.Contains(MESSAGE_REBUILT), "Database rebuilt message not found.");

      // Load.
      SplatTagJsonSnapshotDatabase splatTagJsonSnapshotDatabase = new SplatTagJsonSnapshotDatabase(current);
      (var players, var teams, var sources) = splatTagJsonSnapshotDatabase.Load();

      // Assertions.
      Assert.AreEqual(2, players.Count(p => p.Name.Value == "Slate"), "Incorrect number of Slates!");
    }
  }
}