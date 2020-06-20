using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagCore;
using System.Collections.Generic;

namespace SplatTagUnitTests
{
  /// <summary>
  /// Core matching teams unit tests
  /// </summary>
  [TestClass]
  public class MatchingTeamUnitTests
  {
    /// <summary>
    /// Test match a team by (no match).
    /// </summary>
    [TestMethod]
    public void MatchTeamNoMatchTest()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise(new string[] { "P.exe" });

      Team[] matched = controller.MatchTeam("WO");
      Assert.IsNotNull(matched);
      Assert.IsTrue(matched.Length == 0);
    }

    /// <summary>
    /// Test match two teams by an ambiguous query.
    /// </summary>
    [TestMethod]
    public void MatchTeamAmbiguous()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise(new string[] { "P.exe" });

      Team t1 = controller.CreateTeam();
      t1.Name = "Team 17";
      t1.Id = 17;
      t1.ClanTags = new string[] { "x" };
      t1.ClanTagOption = TagOption.Front;

      Team t2 = controller.CreateTeam();
      t2.Name = "Example 18";
      t2.Id = 18;
      t2.ClanTags = new string[] { "e" };
      t2.ClanTagOption = TagOption.Front;

      database.expectedTeams = new List<Team> { t1, t2 };

      controller.LoadDatabase();
      // Match 'e' which will match the  'e' for a tag and 'e' in team
      Team[] matched = controller.MatchTeam("e");
      Assert.IsNotNull(matched);
      Assert.IsTrue(matched.Length == 2);

      // Assert that the returned order is t2 then t1.
      Assert.IsTrue(matched[0] == t2);
      Assert.IsTrue(matched[1] == t1);
    }

    /// <summary>
    /// Test match a team by its name.
    /// </summary>
    [TestMethod]
    public void MatchTeamByNameTest()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise(new string[] { "P.exe" });

      Team t = controller.CreateTeam();
      t.Name = "Team 17"; // Purposefully mixed case
      t.Id = 17;
      t.ClanTags = new string[] { "WO" };
      t.ClanTagOption = TagOption.Front;
      database.expectedTeams = new List<Team> { t };

      controller.LoadDatabase();
      Team[] matched = controller.MatchTeam("team"); // Purposefully mixed case
      Assert.IsNotNull(matched);
      Assert.IsTrue(matched.Length == 1);
    }

    /// <summary>
    /// Test match a team by its tag.
    /// </summary>
    [TestMethod]
    public void MatchTeamByTagTest_Standard()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise(new string[] { "P.exe" });

      Team t = controller.CreateTeam();
      t.Name = "Team 17";
      t.Id = 17;
      t.ClanTags = new string[] { "WO" }; // Purposefully upper-case
      t.ClanTagOption = TagOption.Front;
      database.expectedTeams = new List<Team> { t };

      controller.LoadDatabase();
      Team[] matched = controller.MatchTeam("wo"); // Purposefully lower-case
      Assert.IsNotNull(matched);
      Assert.IsTrue(matched.Length == 1);
    }

    /// <summary>
    /// Test match a team by its tag.
    /// </summary>
    [TestMethod]
    public void MatchTeamByTagTest_ExactMatched()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise(new string[] { "P.exe" });

      Team t = controller.CreateTeam();
      t.Name = "Inkology";
      t.Id = 1337;
      t.ClanTags = new string[] { "¡g" };
      t.ClanTagOption = TagOption.Front;
      database.expectedTeams = new List<Team> { t };

      controller.LoadDatabase();
      Team[] matched = controller.MatchTeam("¡g", new MatchOptions { IgnoreCase = false, NearCharacterRecognition = false }); // Note i != ¡
      Assert.IsNotNull(matched);
      Assert.IsTrue(matched.Length == 1);
    }

    /// <summary>
    /// Test match a team by its near tag.
    /// </summary>
    [TestMethod]
    public void MatchTeamByTagTest_NearMatched()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise(new string[] { "P.exe" });

      Team t = controller.CreateTeam();
      t.Name = "Inkology";
      t.Id = 1337;
      t.ClanTags = new string[] { "¡g" };
      t.ClanTagOption = TagOption.Front;
      database.expectedTeams = new List<Team> { t };

      controller.LoadDatabase();
      Team[] matched = controller.MatchTeam("ig"); // Note i != ¡
      Assert.IsNotNull(matched);
      Assert.IsTrue(matched.Length == 1);
    }

    /// <summary>
    /// Test match a team by its tag exact.
    /// </summary>
    [TestMethod]
    public void MatchTeamByTagTest_NotMatched()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise(new string[] { "P.exe" });

      Team t = controller.CreateTeam();
      t.Name = "Inkology";
      t.Id = 1337;
      t.ClanTags = new string[] { "¡g" };
      t.ClanTagOption = TagOption.Front;
      database.expectedTeams = new List<Team> { t };

      controller.LoadDatabase();
      Team[] matched = controller.MatchTeam("ig", new MatchOptions { NearCharacterRecognition = false }); // Note i != ¡
      Assert.IsNotNull(matched);
      Assert.IsTrue(matched.Length == 0);
    }

    /// <summary>
    /// Test match a team by its name in Regex.
    /// </summary>
    [TestMethod]
    public void MatchTeamByRegex_Matched()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise(new string[] { "P.exe" });

      Team t1 = controller.CreateTeam();
      t1.Name = "Inkology";
      t1.Id = 1337;
      t1.ClanTags = new string[] { "¡g" };
      t1.ClanTagOption = TagOption.Front;

      Team t2 = controller.CreateTeam();
      t2.Name = "Inkfected";
      t2.Id = 18;
      t2.ClanTags = new string[] { "τイ" };
      t2.ClanTagOption = TagOption.Front;

      Team t3 = controller.CreateTeam();
      t3.Name = "Inky Sirens";
      t3.Id = 19;
      t3.ClanTags = new string[] { "InkS" };
      t3.ClanTagOption = TagOption.Front;

      database.expectedTeams = new List<Team> { t1, t2, t3 };

      controller.LoadDatabase();
      Team[] matched = controller.MatchTeam(@"ink\S+y$", new MatchOptions { IgnoreCase = true, QueryIsRegex = true });
      Assert.IsNotNull(matched);
      Assert.AreEqual(1, matched.Length);
      Assert.IsTrue(matched[0] == t1);
    }

    /// <summary>
    /// Test invalid Regex
    /// </summary>
    [TestMethod]
    public void MatchTeamByRegex_InvalidRegex()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise(new string[] { "P.exe" });

      Team t1 = controller.CreateTeam();
      t1.Name = "Inkology";
      t1.Id = 1337;
      t1.ClanTags = new string[] { "¡g" };
      t1.ClanTagOption = TagOption.Front;

      Team t2 = controller.CreateTeam();
      t2.Name = "Inkfected";
      t2.Id = 18;
      t2.ClanTags = new string[] { "τイ" };
      t2.ClanTagOption = TagOption.Front;

      database.expectedTeams = new List<Team> { t1, t2 };

      controller.LoadDatabase();
      Team[] matched = controller.MatchTeam(@"[", new MatchOptions { IgnoreCase = true, QueryIsRegex = true });
      Assert.IsNotNull(matched);
      Assert.AreEqual(0, matched.Length);
    }
  }
}