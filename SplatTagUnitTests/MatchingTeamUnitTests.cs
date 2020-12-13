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
    /// Test match two teams by an ambiguous query.
    /// </summary>
    [TestMethod]
    public void MatchTeamAmbiguous()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);

      Team t1 = controller.CreateTeam("Team 17");
      t1.AddClanTag("x", Builtins.ManualSource, TagOption.Front);

      Team t2 = controller.CreateTeam("Example 18");
      t2.AddClanTag("e", Builtins.ManualSource, TagOption.Front);

      database.expectedTeams = new List<Team> { t1, t2 };
      controller.Initialise();

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
      controller.Initialise();

      Team t = controller.CreateTeam("Team 17"); // Purposefully mixed case
      t.AddClanTag("WO", Builtins.ManualSource, TagOption.Front); // Purposefully upper-case
      database.expectedTeams = new List<Team> { t };

      controller.LoadDatabase();
      Team[] matched = controller.MatchTeam("team"); // Purposefully mixed case
      Assert.IsNotNull(matched);
      Assert.IsTrue(matched.Length == 1);
    }

    /// <summary>
    /// Test invalid Regex
    /// </summary>
    [TestMethod]
    public void MatchTeamByRegex_InvalidRegex()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise();

      Team t1 = controller.CreateTeam("Inkology");
      t1.AddClanTag("¡g", Builtins.ManualSource, TagOption.Front);

      Team t2 = controller.CreateTeam("Inkfected");
      t2.AddClanTag("τイ", Builtins.ManualSource, TagOption.Front);

      database.expectedTeams = new List<Team> { t1, t2 };

      controller.LoadDatabase();
      Team[] matched = controller.MatchTeam("[", new MatchOptions { IgnoreCase = true, QueryIsRegex = true });
      Assert.IsNotNull(matched);
      Assert.AreEqual(0, matched.Length);
    }

    /// <summary>
    /// Test match a team by its name in Regex.
    /// </summary>
    [TestMethod]
    public void MatchTeamByRegex_Matched()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise();

      Team t1 = controller.CreateTeam("Inkology");
      t1.AddClanTag("¡g", Builtins.ManualSource, TagOption.Front);

      Team t2 = controller.CreateTeam("Inkfected");
      t2.AddClanTag("τイ", Builtins.ManualSource, TagOption.Front);

      Team t3 = controller.CreateTeam("Inky Sirens");
      t3.AddClanTag("InkS", Builtins.ManualSource, TagOption.Front);

      database.expectedTeams = new List<Team> { t1, t2, t3 };

      controller.LoadDatabase();
      Team[] matched = controller.MatchTeam(@"ink\S+y$", new MatchOptions { IgnoreCase = true, QueryIsRegex = true });
      Assert.IsNotNull(matched);
      Assert.AreEqual(1, matched.Length);
      Assert.IsTrue(matched[0] == t1);
    }

    /// <summary>
    /// Test match a team by its tag.
    /// </summary>
    [TestMethod]
    public void MatchTeamByTagTest_ExactMatched()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise();

      Team t = controller.CreateTeam("Inkology");
      t.AddClanTag("¡g", Builtins.ManualSource, TagOption.Front);
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
      controller.Initialise();

      Team t = controller.CreateTeam("Inkology");
      t.AddClanTag("¡g", Builtins.ManualSource, TagOption.Front);
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
      controller.Initialise();

      Team t = controller.CreateTeam("Inkology");
      t.AddClanTag("¡g", Builtins.ManualSource, TagOption.Front);
      database.expectedTeams = new List<Team> { t };

      controller.LoadDatabase();
      Team[] matched = controller.MatchTeam("ig", new MatchOptions { NearCharacterRecognition = false }); // Note i != ¡
      Assert.IsNotNull(matched);
      Assert.IsTrue(matched.Length == 0);
    }

    /// <summary>
    /// Test match a team by its tag.
    /// </summary>
    [TestMethod]
    public void MatchTeamByTagTest_Standard()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise();

      Team t = controller.CreateTeam("Team 17");
      t.AddClanTag("WO", Builtins.ManualSource, TagOption.Front); // Purposefully upper-case
      database.expectedTeams = new List<Team> { t };

      controller.LoadDatabase();
      Team[] matched = controller.MatchTeam("wo"); // Purposefully lower-case
      Assert.IsNotNull(matched);
      Assert.IsTrue(matched.Length == 1);
    }

    /// <summary>
    /// Test match a team by (no match).
    /// </summary>
    [TestMethod]
    public void MatchTeamNoMatchTest()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise();

      Team[] matched = controller.MatchTeam("WO");
      Assert.IsNotNull(matched);
      Assert.IsTrue(matched.Length == 0);
    }
  }
}