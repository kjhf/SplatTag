using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using SplatTagCore;
using SplatTagDatabase;
using SplatTagDatabase.Merging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SplatTagUnitTests
{
  /// <summary>
  /// Unit tests for <see cref="Merger"/>
  /// </summary>
  [TestClass]
  public class MergerUnitTests
  {
    [TestInitialize]
    public void TestInitialize()
    {
      LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(PathUtils.FindFileUpToRoot("nlog.config"));
    }

    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Test the <see cref="Merger.FinalisePlayers(IDictionary{uint, Player})"/> function.
    /// </summary>
    [TestMethod]
    public void FinalisePlayersTest()
    {
      var team1 = Guid.NewGuid();
      var team2 = Guid.NewGuid();

      // p1
      var p1Source = new Source("p1", DateTime.Now.AddDays(1)); // Latest
      Player p1 = new("p1_username", new[] { team1 }, p1Source);
      p1.AddBattlefyInformation("user", "user", "p_persis_id", p1Source);
      p1.AddBattlefyInformation("p1_slug", "z", "p_persis_id", p1Source);
      p1.AddDiscordId("p_discord_id", p1Source);
      var id1 = p1.Id;

      // p2 -> p1 (persistent id match)
      var p2Source = new Source("p2", DateTime.Now);
      Player p2 = new("p2_person", new[] { team1 }, p2Source);
      p2.AddBattlefyInformation("unrelated", "unrelated", "p_persis_id", p2Source);
      p2.AddBattlefyInformation("p2_slug", "x", "p_persis_id", p2Source);
      var id2 = p2.Id;

      // p3
      Player p3 = new("player_ign_team", new[] { team1 }, new Source("p3", DateTime.Now.AddDays(1)));
      var id3 = p3.Id;

      // p4 -> p3 (name and team matches)
      Player p4 = new("player_ign_team", new[] { team1 }, new Source("p4", DateTime.Now));
      var id4 = p4.Id;

      // p5 -> p1 (slug match)
      var p5Source = new Source("p5", DateTime.Now);
      Player p5 = new("p5_name", new[] { team2 }, p5Source);
      p5.AddBattlefyInformation("another", "another", "p_persis_id", p5Source);
      p5.AddBattlefyInformation("p5_slug", "y", "p_persis_id", p5Source);
      var id5 = p5.Id;

      // p6 -> p1 (discord id)
      var p6Source = new Source("p6", DateTime.Now);
      Player p6 = new("p6_username", new[] { team2 }, p6Source);
      p6.AddDiscordId("p_discord_id", p6Source);
      var id6 = p6.Id;

      List<Player> players = new() { p1, p2, p3, p4, p5, p6 };

      // Perform the merge
      DumpCoreObj("Players before merge:", players);
      CoreMergeHandler mergeHandler = new();
      mergeHandler.AddPlayers(players);
      var result = mergeHandler.MergeKnown();

      var finalPlayers = result.Last().ResultingPlayers.ToList();
      DumpCoreObj("Players after merge:", finalPlayers);

      // Dump the merge log
      logger.Info("Merge Log:");
      for (int i = 0; i < result.Length; i++)
      {
        logger.Info("Iteration #" + (i + 1));
        result[i].ToLog();
      }

      Assert.AreEqual(2, finalPlayers.Count,
        $"Expected 2 players remaining - the others should be merged, actually:\n {string.Join("\n", finalPlayers)}");

      // Check merged player 1 (made from p1 and p2, p5, p6)
      var expectedP1BattlefyNames = new[] { "user", "unrelated", "another", "x", "y", "z" };
      var expectedP1IGNs = new[] { "p1_username", "p2_person", "p5_name", "p6_username" };
      var actualP1 = finalPlayers.Find(p => p.Name.Value == "p1_username");
      Assert.IsNotNull(actualP1, "Expected player 1 to be merged");
      Assert.AreEqual(expectedP1BattlefyNames.Length, Matcher.NamesMatchCount(actualP1.BattlefyNames, Name.FromStrings(expectedP1BattlefyNames, Builtins.ManualSource)),
        $"Expected usernames to be merged, actually: {string.Join("\n", actualP1.BattlefyNames)}");
      Assert.AreEqual(expectedP1IGNs.Length, Matcher.NamesMatchCount(actualP1.Names, Name.FromStrings(expectedP1IGNs, Builtins.ManualSource)),
        "Expected names to be merged.");
      Assert.IsTrue((actualP1.TeamInformation?.Contains(team1) == true) && (actualP1.TeamInformation?.Contains(team2) == true),
        "Expected teams to be merged. Teams: [" + string.Join(", ", actualP1.Teams) + "]");

      // Check merged player 3 (made from p3 and p4)
      var actualP3 = finalPlayers.Find(p => p.Name.Value == "player_ign_team");
      Assert.IsNotNull(actualP3, "Expected player 3 to be merged");
      Assert.AreEqual(1, Matcher.NamesMatchCount(actualP3.Names, Name.FromStrings(new[] { "player_ign_team" }, Builtins.ManualSource)),
        "Expected names to be merged.");
      Assert.AreEqual(team1, actualP3.CurrentTeam, "Expected current team (p4 -> p3). p3 Teams: [" + string.Join(", ", actualP3.Teams) + "]");
      Assert.AreEqual(1, actualP3.Teams.Count, "Expected current team to be merged. p3 Teams: [" + string.Join(", ", actualP3.Teams) + "]");
    }

    /// <summary>
    /// Test the Merger.FinaliseTeams(...) function.
    /// </summary>
    [TestMethod]
    public void FinaliseTeamsTest()
    {
      const string T1_STRING = "cafef00d";
      const string T2_STRING = "deadb33f";
      const string T4_STRING = "b335";

      Source t1Source = new("t1_source", DateTime.Now.AddDays(1));
      Team t1 = new("Shared Name", t1Source);
      t1.AddBattlefyId(T1_STRING, Builtins.ManualSource);

      Team t2 = new("TeamB", new Source("t2_source", DateTime.Now.AddDays(2)));
      t2.AddBattlefyId(T2_STRING, Builtins.ManualSource);

      // t2 -> t3 (t3 is newer)
      Team t3 = new("AnotherTeam", new Source("t3_source", DateTime.Now.AddDays(3)));
      t3.AddBattlefyId(T2_STRING, Builtins.ManualSource);

      // t4 -> t1
      Team t4 = new("Shared Name", new Source("t4_source", DateTime.Now.AddDays(4)));
      t4.AddBattlefyId(T4_STRING, Builtins.ManualSource);

      // t5 should NOT merge into t1 because it has no players in common despite sharing a name.
      Source t5Source = new("t5_source", DateTime.UtcNow.AddDays(-1));
      Team t5 = new("Shared Name", t5Source);
      t5.AddDivision(new Division(5, DivType.LUTI, "FirstSeason"), t5Source);

      // t6 -> t5 (shared names and players)
      Source t6Source = new("t6_source", DateTime.UtcNow.AddDays(1));
      Team t6 = new("Shared Name", t6Source);
      t6.AddDivision(new Division(4, DivType.LUTI, "LaterSeason"), t6Source);

      Player p1 = new("username", new[] { t1.Id, t2.Id, t4.Id }, new Source("p1_source"));
      p1.AddBattlefyInformation("user", "user", "p1id", Builtins.ManualSource);
      Assert.IsNotNull(t1.BattlefyPersistentTeamId);
      p1.AddBattlefyInformation("slug", "user", "p1id2", Builtins.ManualSource);

      Player p2 = new("player", new[] { t1.Id, t2.Id, t4.Id }, new Source("p2_source"));
      Player p3 = new("another player", new[] { t1.Id, t2.Id, t4.Id }, new Source("p3_source"));
      Player p4 = new("player 4", new[] { t6.Id, t5.Id }, new Source("p4_source"));
      Player p5 = new("player 5", new[] { t6.Id, t5.Id }, new Source("p5_source"));

      // Perform the merge
      var playersIncoming = new List<Player>() { p1, p2, p3, p4, p5 };
      var teamsIncoming = new List<Team>() { t1, t2, t3, t4, t5, t6 };

      // Perform the merge
      DumpCoreObj("playersIncoming:", playersIncoming);
      DumpCoreObj("teamsIncoming:", teamsIncoming);
      CoreMergeHandler mergeHandler = new();
      mergeHandler.AddPlayers(playersIncoming);
      mergeHandler.AddTeams(teamsIncoming);
      var results = mergeHandler.MergeKnown();

      List<Player> players = results.Last().ResultingPlayers.ToList();
      List<Team> teams = results.Last().ResultingTeams.ToList();

      // Dump the merge log
      logger.Info("Merge Log:");
      for (int i = 0; i < results.Length; i++)
      {
        logger.Info("Iteration #" + (i + 1));
        results[i].ToLog();
      }

      DumpCoreObj("Players after merge:", players);
      DumpCoreObj("Teams after merge:", teams);

      //Assert.IsTrue(result.ContainsKey(t6.Id), "Expected t6 to be merged");
      //Assert.AreEqual(t5.Id, result[t6.Id], "Expected t6 to be merged --> t5");
      //Assert.IsTrue(result.ContainsKey(t4.Id), "Expected t4 to be merged");
      //Assert.AreEqual(t1.Id, result[t4.Id], "Expected t4 to be merged --> t1");
      //Assert.IsTrue(result.ContainsKey(t3.Id), "Expected t3 to be merged");
      //Assert.AreEqual(t2.Id, result[t3.Id], "Expected t3 to be merged --> t2");
      //Assert.AreEqual(3, result.Count, "3 teams should have been merged.");

      Team? mergedT5Team = teams.Find(t => t.Names.Contains(t5.Name) && t.Sources.Contains(t5Source));
      Assert.IsNotNull(mergedT5Team, "Expected t5 name to be found in merged teams.");

      Team? mergedT1Team = teams.Find(t => t.Names.Contains(t1.Name) && t.Sources.Contains(t1Source));
      Assert.IsNotNull(mergedT1Team, "Expected t1 name to be found in merged teams.");

      Team? mergedT2And3Team = teams.Find(t => t.Names.Contains(t2.Name) && t.Names.Contains(t3.Name));
      Assert.IsNotNull(mergedT2And3Team, "Expected t2 and t3 name to be found in merged teams.");

      Assert.AreEqual(4, mergedT5Team.CurrentDiv.Value, "Expected t6 to be merged --> t5 (Div should now be 4, not 5)");
      Assert.IsTrue(mergedT1Team.BattlefyTeamInformation?.Contains(T4_STRING), "Expected t4 to be merged --> t1 (Battlefy Id should have merged)");
      Assert.AreEqual<string>("AnotherTeam", mergedT2And3Team.Name.Value, "Expected t2 to be merged --> t3 (Names should have merged)");
      Assert.AreEqual(3, teams.Count, "3 teams should be left.");

      // Fix the players.
      // Merger.CorrectTeamIdsForPlayers(ref players, result);
      Assert.AreEqual(p1.CurrentTeam, mergedT1Team.Id, "Expected p1's current team to still be team 1");
      Assert.AreEqual(2, p1.Teams.Count, $"Expected p1's number of teams to now be 2, actually: {IdsToString(p1.Teams)}");
      Assert.AreEqual(1, p4.Teams.Count, $"Expected p4's number of teams to now be 1, actually: {IdsToString(p4.Teams)}");
      Assert.AreEqual(p4.CurrentTeam, mergedT5Team.Id, "Expected p4's current team to now be t5");
      Assert.AreEqual(1, p5.Teams.Count, $"Expected p5's number of teams to now be 1, actually: {IdsToString(p5.Teams)}");
      Assert.AreEqual(p5.CurrentTeam, mergedT5Team.Id, "Expected p5's current team to now be t5");
    }

    private static void DumpCoreObj(string label, IEnumerable<ISplatTagCoreObject> sourceable)
    {
      logger.Info(label);
      logger.Info(CoreObjToString(sourceable));
    }

    private static string CoreObjToString(IEnumerable<ISplatTagCoreObject> sourceable)
    {
      StringBuilder sb = new();
      foreach (var s in sourceable)
      {
        sb
          .Append(s.ToString())
          .Append(" (")
          .Append(s.Id)
          .Append("): [")
          .AppendJoin(", ", s.Sources)
          .Append(']')
          .AppendLine();
      }
      return sb.ToString();
    }

    private static string IdsToString(IEnumerable<Guid> ids)
    {
      return new StringBuilder()
        .AppendJoin("\n", ids)
        .AppendLine()
        .ToString();
    }
  }
}