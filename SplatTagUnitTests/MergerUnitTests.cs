using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagCore;
using SplatTagDatabase;
using System;
using System.Collections.Generic;
using System.Linq;

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

      Player p1 = new Player
      {
        BattlefySlugs = new string[] { "user", "slug" },
        Name = "username",
        CurrentTeam = team1
      };
      var id1 = p1.Id;

      // p2 -> p1
      Player p2 = new Player
      {
        BattlefySlugs = new string[] { "unrelated", "slug" },
        Name = "person",
        CurrentTeam = team1
      };

      Player p3 = new Player
      {
        Name = "player_ign_team",
        CurrentTeam = team1
      };
      var id3 = p3.Id;

      // p4 -> p3
      Player p4 = new Player
      {
        Name = "player_ign_team",
        CurrentTeam = team1
      };

      // p5 -> p1
      Player p5 = new Player
      {
        BattlefySlugs = new string[] { "another", "slug" },
        Name = "name_matching_slug",
        CurrentTeam = team2
      };

      // Perform the merge
      var arr = new List<Player>() { p1, p2, p3, p4, p5 };
      Merger.FinalisePlayers(arr, Console.Out);

      // Transform into a dictionary...
      var dict = arr.ToDictionary(p => p.Id, p => p);

      // ...now check the dictionary.
      Assert.AreEqual(dict.Count, 2, "Expected 2 players remaining - the others should be merged.");
      Assert.IsTrue(dict.ContainsKey(id1), "Expected id1.");
      Assert.AreEqual(4, dict[id1].BattlefySlugs.Intersect(new string[] { "slug", "user", "unrelated", "another" }).Count(), "Expected slugs to be merged.");
      Assert.AreEqual(3, dict[id1].Names.Intersect(new string[] { "username", "person", "name_matching_slug" }).Count(), "Expected names to be merged.");
      Assert.AreEqual(2, dict[id1].Teams.Intersect(new Guid[] { team1, team2 }).Count(), "Expected teams to be merged. Teams: [" + string.Join(", ", dict[id1].Teams) + "]");

      Assert.IsTrue(dict.ContainsKey(id3), "Expected id3.");
      Assert.AreEqual(1, dict[id3].Names.Intersect(new string[] { "player_ign_team" }).Count(), "Expected names to be merged.");
      Assert.AreEqual(team1, dict[id3].CurrentTeam, "Expected current team (p4 -> p3). p3 Teams: [" + string.Join(", ", dict[id3].Teams) + "]");
      Assert.AreEqual(1, dict[id3].Teams.Count, "Expected current team to be merged. p3 Teams: [" + string.Join(", ", dict[id3].Teams) + "]");
    }

    /// <summary>
    /// Test the <see cref="Merger.FinaliseTeams(SplatTagController, IList{Team})"/> function.
    /// </summary>
    [TestMethod]
    public void FinaliseTeamsTest()
    {
      const string T1_STRING = "cafef00d";
      const string T2_STRING = "deadb33f";
      const string T3_STRING = "b335";

      Team t1 = new Team
      {
        BattlefyPersistentTeamId = T1_STRING,
        Name = "Shared Name"
      };

      Team t2 = new Team
      {
        BattlefyPersistentTeamId = T2_STRING,
        Name = "TeamB"
      };

      // t3 -> t2
      Team t3 = new Team
      {
        BattlefyPersistentTeamId = T2_STRING,
        Name = "AnotherTeam"
      };

      // t4 -> t1
      Team t4 = new Team
      {
        BattlefyPersistentTeamId = T3_STRING,
        Name = "Shared Name"
      };

      // t5 should be left because it has no players in common.
      Team t5 = new Team
      {
        Name = "Shared Name",
        Div = new Division(5, DivType.LUTI)
      };

      // t6 -> t5
      Team t6 = new Team
      {
        Name = "Shared Name",
        Div = new Division(4, DivType.LUTI)
      };

      Player p1 = new Player
      {
        BattlefySlugs = new string[] { "user", "slug" },
        Name = "username",
        Teams = new[] { t1.Id, t2.Id, t4.Id }
      };

      Player p2 = new Player
      {
        Name = "player",
        Teams = new[] { t1.Id, t2.Id, t4.Id }
      };

      Player p3 = new Player
      {
        Name = "another player",
        Teams = new[] { t1.Id, t2.Id, t4.Id }
      };

      Player p4 = new Player
      {
        Name = "player 4",
        Teams = new[] { t6.Id, t5.Id }
      };

      Player p5 = new Player
      {
        Name = "player 5",
        Teams = new[] { t6.Id, t5.Id }
      };

      // Perform the merge
      var players = new List<Player>() { p1, p2, p3, p4, p5 };
      var teams = new List<Team>() { t1, t2, t3, t4, t5, t6 };
      var result = Merger.FinaliseTeams(players, teams);

      Assert.AreEqual(3, result.Count, "3 teams should have been merged.");
      Assert.AreEqual(3, teams.Count, "3 teams should be left.");
      Assert.IsTrue(result.ContainsKey(t6.Id), "Expected t6 to be merged");
      Assert.AreEqual(t5.Id, result[t6.Id], "Expected t6 to be merged --> t5");
      Assert.AreEqual(4, t5.Div.Value, "Expected t6 to be merged --> t5 (Div should now be 4, not 5)");

      Assert.IsTrue(result.ContainsKey(t4.Id), "Expected t4 to be merged");
      Assert.AreEqual(t1.Id, result[t4.Id], "Expected t4 to be merged --> t1");
      Assert.AreEqual(T3_STRING, t1.BattlefyPersistentTeamId, "Expected t4 to be merged --> t1 (Battlefy Id should have merged)");

      Assert.IsTrue(result.ContainsKey(t3.Id), "Expected t3 to be merged");
      Assert.AreEqual(t2.Id, result[t3.Id], "Expected t3 to be merged --> t2");
      Assert.AreEqual("AnotherTeam", t2.Name, "Expected t3 to be merged --> t2 (Names should have merged)");

      // Fix the players.
      Merger.CorrectTeamIdsForPlayers(players, result, Console.Out);
      Assert.AreEqual(p1.CurrentTeam, t1.Id, "Expected p1's current team to still be team 1");
      Assert.AreEqual(2, p1.Teams.Count, "Expected p1's number of teams to now be 2");
      Assert.AreEqual(1, p4.Teams.Count, "Expected p4's number of teams to now be 1");
      Assert.AreEqual(p4.CurrentTeam, t5.Id, "Expected p4's current team to now be t5");
      Assert.AreEqual(1, p5.Teams.Count, "Expected p5's number of teams to now be 1");
      Assert.AreEqual(p5.CurrentTeam, t5.Id, "Expected p5's current team to now be t5");
    }
  }
}