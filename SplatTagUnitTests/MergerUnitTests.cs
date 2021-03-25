using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagCore;
using SplatTagDatabase;
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
    /// <summary>
    /// Test the <see cref="Merger.FinalisePlayers(IDictionary{uint, Player})"/> function.
    /// </summary>
    [TestMethod]
    public void FinalisePlayersTest()
    {
      var team1 = Guid.NewGuid();
      var team2 = Guid.NewGuid();

      // p1
      Player p1 = new Player("p1_username", new[] { team1 }, new Source("p1"));
      p1.AddBattlefyInformation("user", "user", "p1id", Builtins.ManualSource);
      p1.AddBattlefyInformation("slug", "z", "p1id2", Builtins.ManualSource);
      var id1 = p1.Id;

      // p2 -> p1 (slug match)
      Player p2 = new Player("p2_person", new[] { team1 }, new Source("p2"));
      p2.AddBattlefyInformation("unrelated", "unrelated", "p2id", Builtins.ManualSource);
      p2.AddBattlefyInformation("slug", "x", "p2id", Builtins.ManualSource);

      // p3
      Player p3 = new Player("player_ign_team", new[] { team1 }, new Source("p3"));
      var id3 = p3.Id;

      // p4 -> p3 (name and team matches)
      Player p4 = new Player("player_ign_team", new[] { team1 }, new Source("p4"));

      // p5 -> p1 (slug match)
      Player p5 = new Player("p5_slug", new[] { team2 }, new Source("p5"));
      p5.AddBattlefyInformation("another", "another", "p5id", Builtins.ManualSource);
      p5.AddBattlefyInformation("slug", "y", "p5id2", Builtins.ManualSource);

      // p6 -> p1 (persistent id)
      Player p6 = new Player("p6_username", new[] { team2 }, new Source("p6"));
      p6.AddBattlefyInformation("p6_slug", "p6_user", "p1id", Builtins.ManualSource);

      var players = new List<Player>() { p1, p2, p3, p4, p5, p6 };

      // Perform the merge
      DumpPlayers("Players before merge:", players);
      Merger.FinalisePlayers(players, Console.Out);
      DumpPlayers("Players after merge:", players);

      // Transform into a dictionary...
      var dict = players.AsParallel().ToDictionary(p => p.Id, p => p);

      // ...now check the dictionary.
      Assert.AreEqual(2, dict.Count,
        $"Expected 2 players remaining - the others should be merged, actually: {string.Join("\n", dict.Keys.Select(k => k.ToString()))}");
      Assert.IsTrue(dict.ContainsKey(id1), "Expected id1.");
      Assert.AreEqual(5, Matcher.NamesMatch(dict[id1].Battlefy.Slugs, Name.FromStrings(new[] { "slug", "user", "unrelated", "another", "p6_slug" }, Builtins.ManualSource)),
        "Expected slugs to be merged.");
      Assert.AreEqual(7, Matcher.NamesMatch(dict[id1].Battlefy.Usernames, Name.FromStrings(new[] { "user", "unrelated", "another", "x", "y", "z", "p6_user" }, Builtins.ManualSource)),
        $"Expected usernames to be merged, actually: {string.Join("\n", dict[id1].Battlefy.Usernames)}");
      Assert.AreEqual(3, Matcher.NamesMatch(dict[id1].Names, Name.FromStrings(new[] { "p1_username", "p2_person", "p5_slug" }, Builtins.ManualSource)),
        "Expected names to be merged.");
      Assert.AreEqual(2, Matcher.GenericMatch(dict[id1].Teams, new Guid[] { team1, team2 }),
        "Expected teams to be merged. Teams: [" + string.Join(", ", dict[id1].Teams) + "]");

      Assert.IsTrue(dict.ContainsKey(id3), "Expected id3.");
      Assert.AreEqual(1, Matcher.NamesMatch(dict[id3].Names, Name.FromStrings(new[] { "player_ign_team" }, Builtins.ManualSource)),
        "Expected names to be merged.");
      Assert.AreEqual(team1, dict[id3].CurrentTeam, "Expected current team (p4 -> p3). p3 Teams: [" + string.Join(", ", dict[id3].Teams) + "]");
      Assert.AreEqual(1, dict[id3].Teams.Count, "Expected current team to be merged. p3 Teams: [" + string.Join(", ", dict[id3].Teams) + "]");
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

      Team t1 = new Team("Shared Name", new Source("t1"));
      t1.AddBattlefyId(T1_STRING, Builtins.ManualSource);

      Team t2 = new Team("TeamB", new Source("t2"));
      t2.AddBattlefyId(T2_STRING, Builtins.ManualSource);

      // t3 -> t2
      Team t3 = new Team("AnotherTeam", new Source("t3"));
      t3.AddBattlefyId(T2_STRING, Builtins.ManualSource);

      // t4 -> t1
      Team t4 = new Team("Shared Name", new Source("t4"));
      t4.AddBattlefyId(T4_STRING, Builtins.ManualSource);

      // t5 should be left because it has no players in common.
      Team t5 = new Team("Shared Name", new Source("t5"));
      t5.AddDivision(new Division(5, DivType.LUTI));

      // t6 -> t5
      Team t6 = new Team("Shared Name", new Source("t6"));
      t5.AddDivision(new Division(4, DivType.LUTI));

      Player p1 = new Player("username", new[] { t1.Id, t2.Id, t4.Id }, new Source("p1"));
      p1.AddBattlefyInformation("user", "user", "p1id", Builtins.ManualSource);
      Assert.IsNotNull(t1.BattlefyPersistentTeamId);
      p1.AddBattlefyInformation("slug", "user", "p1id2", Builtins.ManualSource);

      Player p2 = new Player("player", new[] { t1.Id, t2.Id, t4.Id }, new Source("p2"));
      Player p3 = new Player("another player", new[] { t1.Id, t2.Id, t4.Id }, new Source("p3"));
      Player p4 = new Player("player 4", new[] { t6.Id, t5.Id }, new Source("p4"));
      Player p5 = new Player("player 5", new[] { t6.Id, t5.Id }, new Source("p5"));

      // Perform the merge
      var players = new List<Player>() { p1, p2, p3, p4, p5 };
      var teams = new List<Team>() { t1, t2, t3, t4, t5, t6 };

      DumpPlayers("Players before merge:", players);
      DumpTeams("Teams before merge:", teams);
      var result = Merger.FinaliseTeams(players, teams, Console.Out);
      DumpPlayers("Players after merge:", players);
      DumpTeams("Teams after merge:", teams);

      Assert.IsTrue(result.ContainsKey(t6.Id), "Expected t6 to be merged");
      Assert.AreEqual(t5.Id, result[t6.Id], "Expected t6 to be merged --> t5");
      Assert.AreEqual(4, t5.CurrentDiv.Value, "Expected t6 to be merged --> t5 (Div should now be 4, not 5)");

      Assert.IsTrue(result.ContainsKey(t4.Id), "Expected t4 to be merged");
      Assert.AreEqual(t1.Id, result[t4.Id], "Expected t4 to be merged --> t1");
      Assert.AreEqual<string>(T4_STRING, t1.BattlefyPersistentTeamId.Value, "Expected t4 to be merged --> t1 (Battlefy Id should have merged)");

      Assert.IsTrue(result.ContainsKey(t3.Id), "Expected t3 to be merged");
      Assert.AreEqual(t2.Id, result[t3.Id], "Expected t3 to be merged --> t2");
      Assert.AreEqual<string>("AnotherTeam", t2.Name.Value, "Expected t3 to be merged --> t2 (Names should have merged)");

      Assert.AreEqual(3, result.Count, "3 teams should have been merged.");
      Assert.AreEqual(3, teams.Count, "3 teams should be left.");

      // Fix the players.
      Merger.CorrectTeamIdsForPlayers(players, result, Console.Out);
      Assert.AreEqual(p1.CurrentTeam, t1.Id, "Expected p1's current team to still be team 1");
      Assert.AreEqual(2, p1.Teams.Count, $"Expected p1's number of teams to now be 2, actually: {IdsToString(p1.Teams)}");
      Assert.AreEqual(1, p4.Teams.Count, $"Expected p4's number of teams to now be 1, actually: {IdsToString(p4.Teams)}");
      Assert.AreEqual(p4.CurrentTeam, t5.Id, "Expected p4's current team to now be t5");
      Assert.AreEqual(1, p5.Teams.Count, $"Expected p5's number of teams to now be 1, actually: {IdsToString(p5.Teams)}");
      Assert.AreEqual(p5.CurrentTeam, t5.Id, "Expected p5's current team to now be t5");
    }

    private static void DumpPlayers(string label, IList<Player> players)
    {
      Console.WriteLine(label);
      Console.WriteLine(PlayersToString(players));
    }

    private static string PlayersToString(IEnumerable<Player> players)
    {
      StringBuilder sb = new StringBuilder();
      foreach (var p in players)
      {
        sb.AppendLine("[" + (p.Sources.Any() ? string.Join(", ", p.Sources) : "") + "]: " + p.ToString());
      }
      return sb.ToString();
    }

    private static void DumpTeams(string label, IList<Team> teams)
    {
      Console.WriteLine(label);
      Console.WriteLine(TeamsToString(teams));
    }

    private static string TeamsToString(IEnumerable<Team> teams)
    {
      StringBuilder sb = new StringBuilder();
      foreach (var t in teams)
      {
        sb.AppendLine("[" + (t.Sources.Any() ? string.Join(", ", t.Sources) : "") + "]: " + t.ToString());
      }
      return sb.ToString();
    }

    private static string IdsToString(IEnumerable<Guid> ids)
    {
      StringBuilder sb = new StringBuilder();
      foreach (var t in ids)
      {
        sb.AppendLine(t.ToString());
      }
      return sb.ToString();
    }
  }
}