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
      p1.AddBattlefyInformation("user", "user", "p_persis_id", Builtins.ManualSource);
      p1.AddBattlefyInformation("p1_slug", "z", "p_persis_id", Builtins.ManualSource);
      p1.AddDiscordId("p_discord_id", Builtins.ManualSource);
      var id1 = p1.Id;

      // p2 -> p1 (persistent id match)
      Player p2 = new Player("p2_person", new[] { team1 }, new Source("p2"));
      p2.AddBattlefyInformation("unrelated", "unrelated", "p_persis_id", Builtins.ManualSource);
      p2.AddBattlefyInformation("p2_slug", "x", "p_persis_id", Builtins.ManualSource);

      // p3
      Player p3 = new Player("player_ign_team", new[] { team1 }, new Source("p3"));
      var id3 = p3.Id;

      // p4 -> p3 (name and team matches)
      Player p4 = new Player("player_ign_team", new[] { team1 }, new Source("p4"));

      // p5 -> p1 (slug match)
      Player p5 = new Player("p5_name", new[] { team2 }, new Source("p5"));
      p5.AddBattlefyInformation("another", "another", "p_persis_id", Builtins.ManualSource);
      p5.AddBattlefyInformation("p5_slug", "y", "p_persis_id", Builtins.ManualSource);

      // p6 -> p1 (discord id)
      Player p6 = new Player("p6_username", new[] { team2 }, new Source("p6"));
      p6.AddDiscordId("p_discord_id", Builtins.ManualSource);

      var players = new List<Player>() { p1, p2, p3, p4, p5, p6 };

      // Perform the merge
      DumpSourceable("Players before merge:", players);
      Merger.FinalisePlayers(players, Console.Out);
      DumpSourceable("Players after merge:", players);

      // Transform into a dictionary...
      var dict = players.ToDictionary(p => p.Id, p => p);

      // ...now check the dictionary.
      Assert.AreEqual(2, dict.Count,
        $"Expected 2 players remaining - the others should be merged, actually:\n {string.Join("\n", dict.Keys.Select(k => k.ToString() + " (" + dict[k] + ")"))}");
      Assert.IsTrue(dict.ContainsKey(id1), "Expected id1.");

      var expectedP1BattlefyNames = new[] { "user", "unrelated", "another", "x", "y", "z" };
      var expectedP1IGNs = new[] { "p1_username", "p2_person", "p5_name", "p6_username" };
      Assert.AreEqual(expectedP1BattlefyNames.Length, Matcher.NamesMatchCount(dict[id1].Battlefy.Usernames, Name.FromStrings(expectedP1BattlefyNames, Builtins.ManualSource)),
        $"Expected usernames to be merged, actually: {string.Join("\n", dict[id1].Battlefy.Usernames)}");
      Assert.AreEqual(expectedP1IGNs.Length, Matcher.NamesMatchCount(dict[id1].NamesInformation.GetItemsUnordered(), Name.FromStrings(expectedP1IGNs, Builtins.ManualSource)),
        "Expected names to be merged.");
      Assert.IsTrue(dict[id1].TeamInformation.Contains(team1) && dict[id1].TeamInformation.Contains(team2),
        "Expected teams to be merged. Teams: [" + string.Join(", ", dict[id1].TeamInformation.GetAllTeamsUnordered()) + "]");

      Assert.IsTrue(dict.ContainsKey(id3), "Expected id3.");
      Assert.AreEqual(1, Matcher.NamesMatchCount(dict[id3].NamesInformation.GetItemsUnordered(), Name.FromStrings(new[] { "player_ign_team" }, Builtins.ManualSource)),
        "Expected names to be merged.");
      Assert.AreEqual(team1, dict[id3].CurrentTeam, "Expected current team (p4 -> p3). p3 Teams: [" + string.Join(", ", dict[id3].TeamInformation.GetAllTeamsUnordered()) + "]");
      Assert.AreEqual(1, dict[id3].TeamInformation.Count, "Expected current team to be merged. p3 Teams: [" + string.Join(", ", dict[id3].TeamInformation.GetAllTeamsUnordered()) + "]");
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

      Team t1 = new Team("Shared Name", new Source("t1", DateTime.Now.AddDays(1)));
      t1.AddBattlefyId(T1_STRING, Builtins.ManualSource);

      Team t2 = new Team("TeamB", new Source("t2", DateTime.Now.AddDays(2)));
      t2.AddBattlefyId(T2_STRING, Builtins.ManualSource);

      // t3 -> t2
      Team t3 = new Team("AnotherTeam", new Source("t3", DateTime.Now.AddDays(3)));
      t3.AddBattlefyId(T2_STRING, Builtins.ManualSource);

      // t4 -> t1
      Team t4 = new Team("Shared Name", new Source("t4", DateTime.Now.AddDays(4)));
      t4.AddBattlefyId(T4_STRING, Builtins.ManualSource);

      // t5 should NOT merge into t1 because it has no players in common despite sharing a name.
      Team t5 = new Team("Shared Name", new Source("t5", DateTime.UtcNow.AddDays(-1)));
      t5.AddDivision(new Division(5, DivType.LUTI, "FirstSeason"), t5.Sources[0]);

      // t6 -> t5
      Team t6 = new Team("Shared Name", new Source("t6", DateTime.UtcNow));
      t6.AddDivision(new Division(4, DivType.LUTI, "LaterSeason"), t6.Sources[0]);

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

      DumpSourceable("Players before merge:", players);
      DumpSourceable("Teams before merge:", teams);
      var result = Merger.FinaliseTeams(players, teams, Console.Out);
      DumpSourceable("Players after merge:", players);
      DumpSourceable("Teams after merge:", teams);

      Assert.IsTrue(result.ContainsKey(t6.Id), "Expected t6 to be merged");
      Assert.AreEqual(t5.Id, result[t6.Id], "Expected t6 to be merged --> t5");
      Assert.AreEqual(4, t5.CurrentDiv.Value, "Expected t6 to be merged --> t5 (Div should now be 4, not 5)");

      Assert.IsTrue(result.ContainsKey(t4.Id), "Expected t4 to be merged");
      Assert.AreEqual(t1.Id, result[t4.Id], "Expected t4 to be merged --> t1");
      Assert.IsTrue(t1.BattlefyPersistentTeamIdInformation.Contains(T4_STRING), "Expected t4 to be merged --> t1 (Battlefy Id should have merged)");

      Assert.IsTrue(result.ContainsKey(t3.Id), "Expected t3 to be merged");
      Assert.AreEqual(t2.Id, result[t3.Id], "Expected t3 to be merged --> t2");
      Assert.AreEqual<string>("AnotherTeam", t2.Name.Value, "Expected t3 to be merged --> t2 (Names should have merged)");

      Assert.AreEqual(3, result.Count, "3 teams should have been merged.");
      Assert.AreEqual(3, teams.Count, "3 teams should be left.");

      // Fix the players.
      Merger.CorrectTeamIdsForPlayers(players, result, Console.Out);
      Assert.AreEqual(p1.CurrentTeam, t1.Id, "Expected p1's current team to still be team 1");
      Assert.AreEqual(2, p1.TeamInformation.Count, $"Expected p1's number of teams to now be 2, actually: {IdsToString(p1.TeamInformation.GetAllTeamsUnordered())}");
      Assert.AreEqual(1, p4.TeamInformation.Count, $"Expected p4's number of teams to now be 1, actually: {IdsToString(p4.TeamInformation.GetAllTeamsUnordered())}");
      Assert.AreEqual(p4.CurrentTeam, t5.Id, "Expected p4's current team to now be t5");
      Assert.AreEqual(1, p5.TeamInformation.Count, $"Expected p5's number of teams to now be 1, actually: {IdsToString(p5.TeamInformation.GetAllTeamsUnordered())}");
      Assert.AreEqual(p5.CurrentTeam, t5.Id, "Expected p5's current team to now be t5");
    }

    private static void DumpSourceable(string label, IEnumerable<IReadonlySourceable> sourceable)
    {
      Console.WriteLine(label);
      Console.WriteLine(SourceablesToString(sourceable));
    }

    private static string SourceablesToString(IEnumerable<IReadonlySourceable> sourceable)
    {
      StringBuilder sb = new StringBuilder();
      foreach (var s in sourceable)
      {
        sb
          .Append('[')
          .AppendJoin(", ", s.Sources)
          .Append("]: ")
          .AppendLine(s.ToString());
      }
      return sb.ToString();
    }

    private static string IdsToString(IEnumerable<Guid> ids)
    {
      return new StringBuilder()
        .AppendJoin("\n", ids)
        .ToString();
    }
  }
}