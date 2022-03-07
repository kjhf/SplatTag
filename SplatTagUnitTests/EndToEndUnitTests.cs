using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagConsole;
using SplatTagCore;
using SplatTagCore.Social;
using SplatTagDatabase;
using System;
using System.IO;
using System.Linq;
using System.Text;
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
    private const string MESSAGE_PATCHED = "\"Message\":\"Database patched from";

    private static readonly string current;
    private static readonly string sourcesPath;
    private static readonly string patchPath;

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
      patchPath = Path.Combine(current, "sources_patch.yaml");

      foreach (FileInfo snapshot in SplatTagJsonSnapshotDatabase.GetSnapshots(current).ToArray())
      {
        snapshot.Delete();
      }
    }

    /// <summary>
    /// Verify that the end to end runs.
    /// </summary>
    [Ignore("Ignoring this test as the Patch test uses this one.")]
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

      // Contains the snapshots?
      Assert.AreEqual(3, SplatTagJsonSnapshotDatabase.GetSnapshots(current).Count());

      // Load.
      SplatTagJsonSnapshotDatabase splatTagJsonSnapshotDatabase = new SplatTagJsonSnapshotDatabase(current);
      (var players, var teams, var sources) = splatTagJsonSnapshotDatabase.Load();
      Assert.IsNotNull(players, "Not expecting null return for players");
      Assert.IsNotNull(teams, "Not expecting null return for teams");
      Assert.IsNotNull(sources, "Not expecting null return for sources");

      // Assertions.
      Assert.AreEqual(2, players.Count(p => p.Name.Value.Contains("Slate")), "Incorrect number of Slates:\n - " +
        new StringBuilder()
        .AppendJoin("\n - ",
          players
          .Where(p => p.Name.Value.Contains("Slate"))
          .Select(p => new StringBuilder()
                  .Append('[')
                  .AppendJoin(", ", p.Names)
                  .Append(']')
                  .Append(" -- Sourced from [")
                  .AppendJoin(", ", p.Sources)
                  .Append("] and teams [")
                  .AppendJoin(", ", p.TeamInformation.GetAllTeamsUnordered())
                  .Append("] and Battlefy Slugs [")
                  .AppendJoin(", ", p.Battlefy.Slugs ?? Array.Empty<BattlefyUserSocial>())
                  .Append("] top500=")
                  .Append(p.Top500)
                  .AppendLine(".")
          )
        ));
      var splatarians = teams.Where(p => p.Name.Value == "Splatarians");
      Assert.AreEqual(1, splatarians.Count(), $"Incorrect number of Splatarians:\n - { string.Join("\n - ", splatarians)}");
      Assert.AreEqual("SX", splatarians.First().CurrentDiv.Season);
      Assert.IsTrue(splatarians.First().DivisionInformation.GetDivisionsUnordered().Any(d => d.Season == "S9"));
      Assert.AreEqual(4, sources.Count, "Unexpected number of sources loaded.");
      Assert.IsTrue(players.Where(p => p.Name.Value.Contains("Slate")).All(p => !p.Top500), "Expected Slate top 500 flag to be false.");
    }

    /// <summary>
    /// Verify that we can patch a ready-database.
    /// </summary>
    [TestMethod]
    public void EndToEndPatch()
    {
      // First run the Rebuild function.
      EndToEndBuild();

      // Now we can patch
      using StringWriter sw = new StringWriter();
      Console.SetOut(sw);
      Console.SetError(sw);

      var mainTask = Task.Run(async () =>
      {
        await ConsoleMain.Main($"--verbose --patch {patchPath}".Split(" ")).ConfigureAwait(false);
        await Task.Delay(500).ConfigureAwait(false); // Let any logging finish.
      });

      Task.WaitAny(mainTask);

      Console.SetOut(consoleOut);
      Console.SetError(consoleError);
      string actual = sw.ToString();
      Console.WriteLine("==================================================");
      Console.WriteLine(actual = actual.Base64DecodeByLines());
      Console.WriteLine("==================================================");

      Assert.IsTrue(actual.Contains(MESSAGE_PATCHED), "Database patched message not found.");

      SplatTagJsonSnapshotDatabase splatTagJsonSnapshotDatabase = new SplatTagJsonSnapshotDatabase(current);
      (var players, var teams, var sources) = splatTagJsonSnapshotDatabase.Load();

      var slates = players.Where(p => p.AllKnownNames.NamesMatch(new[] { new Name("Slate", Builtins.ManualSource) }));

      // Assertions.
      StringBuilder slateFailMessage = new StringBuilder()
        .AppendJoin("\n - ",
          slates
          .Select(p => new StringBuilder()
                  .Append('[')
                  .AppendJoin(", ", p.AllKnownNames)
                  .Append(']')
                  .Append(" -- Sourced from [")
                  .AppendJoin(", ", p.Sources)
                  .Append("] and teams [")
                  .AppendJoin(", ", p.TeamInformation.GetAllTeamsUnordered())
                  .Append("] and Battlefy Slugs [")
                  .AppendJoin(", ", p.Battlefy.Slugs ?? Array.Empty<BattlefyUserSocial>())
                  .Append("] top500=")
                  .Append(p.Top500)
                  .AppendLine(".")
          )
        );
      Assert.AreEqual(3, slates.Count(), "Incorrect number of Slates:\n - " + slateFailMessage.ToString());

      var splatarians = teams.Where(p => p.Name.Value == "Splatarians");
      if (splatarians.Count() != 1)
      {
        StringBuilder failMessage = new StringBuilder();
        failMessage.Append("Incorrect number of Splatarians (").Append(splatarians.Count()).Append("):\n - ").AppendJoin("\n - ", splatarians).AppendLine();
        failMessage.Append("Sources (").Append(sources.Count).Append("):\n - ").AppendJoin("\n - ", sources).AppendLine();
        failMessage.Append("From:\n - ").AppendJoin("\n - ", splatarians.Select(t => string.Join(", ", t.Sources))).AppendLine();
        Assert.Fail(failMessage.ToString());
      }
      Assert.AreEqual("SX", splatarians.First().CurrentDiv.Season);
      Assert.IsTrue(splatarians.First().DivisionInformation.GetDivisionsUnordered().Any(d => d.Season == "S9"));

      // Check that the patching has happened
      Assert.AreEqual(5, sources.Count, "Unexpected number of sources loaded.");
      Assert.IsTrue(slates.Any(p => p.Top500), "Expected the patching to Slate to set top 500 flag -- " + slateFailMessage.ToString());
    }
  }
}