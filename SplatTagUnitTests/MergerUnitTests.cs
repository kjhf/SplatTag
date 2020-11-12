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

      Player p2 = new Player
      {
        BattlefySlugs = new string[] { "unrelated", "slug" },
        Name = "person",
        CurrentTeam = team1
      };

      Player p3 = new Player
      {
        Name = "player merged by ign and team",
        CurrentTeam = team1
      };
      var id3 = p3.Id;

      Player p4 = new Player
      {
        Name = "player merged by ign and team",
        CurrentTeam = team1
      };

      Player p5 = new Player
      {
        BattlefySlugs = new string[] { "another", "slug" },
        Name = "different name but should match slug",
        CurrentTeam = team2
      };

      // Perform the merge
      var arr = new List<Player>() { p1, p2, p3, p4, p5 };
      Merger.FinalisePlayers(arr);

      // Transform into a dictionary...
      var dict = arr.ToDictionary(p => p.Id, p => p);

      // ...now check the dictionary.
      Assert.AreEqual(dict.Count, 2, "Expected 2 players remaining - the others should be merged.");
      Assert.IsTrue(dict.ContainsKey(id1), "Expected id1.");
      Assert.AreEqual(4, dict[id1].BattlefySlugs.Intersect(new string[] { "slug", "user", "unrelated", "another" }).Count(), "Expected slugs to be merged.");
      Assert.AreEqual(3, dict[id1].Names.Intersect(new string[] { "username", "person", "different name but should match slug" }).Count(), "Expected names to be merged.");
      Assert.AreEqual(team2, dict[id1].CurrentTeam, "Expected current team to be merged.");
      Assert.AreEqual(2, dict[id1].Teams.Intersect(new Guid[] { team1, team2 }).Count(), "Expected teams to be merged.");

      Assert.IsTrue(dict.ContainsKey(id3), "Expected id3.");
      Assert.AreEqual(1, dict[id3].Names.Intersect(new string[] { "player merged by ign and team" }).Count(), "Expected names to be merged.");
      Assert.AreEqual(team1, dict[id3].CurrentTeam, "Expected current team to be merged.");
      Assert.AreEqual(1, dict[id3].Teams.Count, "Expected current team to be merged.");
    }
  }
}