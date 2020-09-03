using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagCore;
using System.Collections.Generic;

namespace SplatTagUnitTests
{
  /// <summary>
  /// Core matching players unit tests
  /// </summary>
  [TestClass]
  public class MatchingPlayerUnitTests
  {
    /// <summary>
    /// Test match a player by (no match).
    /// </summary>
    [TestMethod]
    public void MatchPlayerNoMatchTest()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise();

      Player[] matched = controller.MatchPlayer("example");
      Assert.IsNotNull(matched);
      Assert.IsTrue(matched.Length == 0);
    }

    /// <summary>
    /// Test match a player by its name.
    /// </summary>
    [TestMethod]
    public void MatchPlayerByNameTest()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise();

      Player p1 = controller.CreatePlayer("Manual");
      p1.Name = "Player 17"; // Purposefully mixed case
      p1.Id = 17;
      database.expectedPlayers = new List<Player> { p1 };

      controller.LoadDatabase();
      Player[] matched = controller.MatchPlayer("player"); // Purposefully mixed case
      Assert.IsNotNull(matched);
      Assert.IsTrue(matched.Length == 1);
    }

    /// <summary>
    /// Test match two players by an ambiguous query.
    /// </summary>
    [TestMethod]
    public void MatchPlayersAmbiguous()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise();

      Player p1 = controller.CreatePlayer("Manual");
      p1.Name = "Player 17";
      p1.Id = 17;

      Player p2 = controller.CreatePlayer("Manual");
      p2.Name = "Player 18";
      p2.Id = 18;

      database.expectedPlayers = new List<Player> { p1, p2 };

      controller.LoadDatabase();
      Player[] matched = controller.MatchPlayer("lay");
      Assert.IsNotNull(matched);
      Assert.IsTrue(matched.Length == 2);
    }

    /// <summary>
    /// Test match a player by an exact match with non-Latin characters
    /// </summary>
    [TestMethod]
    public void MatchPlayerByTagTest_ExactMatched()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise();

      Player p1 = controller.CreatePlayer("Manual");
      p1.Name = "BΛÐ SΛVΛGΞ";
      p1.Id = 1337;
      database.expectedPlayers = new List<Player> { p1 };

      controller.LoadDatabase();
      Player[] matched = controller.MatchPlayer("BΛÐ SΛVΛGΞ", new MatchOptions { IgnoreCase = false, NearCharacterRecognition = false });
      Assert.IsNotNull(matched);
      Assert.IsTrue(matched.Length == 1);
    }

    /// <summary>
    /// Test match a player by its near match.
    /// </summary>
    [TestMethod]
    public void MatchPlayerByTagTest_NearMatched()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise();

      Player p1 = controller.CreatePlayer("Manual");
      p1.Name = "BΛÐ SΛVΛGΞ";
      p1.Id = 1337;
      database.expectedPlayers = new List<Player> { p1 };

      controller.LoadDatabase();
      Player[] matched = controller.MatchPlayer("bad savage");
      Assert.IsNotNull(matched);
      Assert.IsTrue(matched.Length == 1);
    }

    /// <summary>
    /// Test match a player by its tag exact.
    /// </summary>
    [TestMethod]
    public void MatchPlayerByTagTest_NotMatched()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise();

      Player p1 = controller.CreatePlayer("Manual");
      p1.Name = "BΛÐ SΛVΛGΞ";
      p1.Id = 1337;
      database.expectedPlayers = new List<Player> { p1 };

      controller.LoadDatabase();
      Player[] matched = controller.MatchPlayer("bad savage", new MatchOptions { NearCharacterRecognition = false });
      Assert.IsNotNull(matched);
      Assert.IsTrue(matched.Length == 0);
    }

    /// <summary>
    /// Test match a player by its name in Regex.
    /// </summary>
    [TestMethod]
    public void MatchPlayerByRegex_Matched()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise();

      Player p1 = controller.CreatePlayer("Manual");
      p1.Name = "¡g Slate";
      p1.Id = 1337;

      Player p2 = controller.CreatePlayer("Manual");
      p2.Name = "Slate*NBF";
      p2.Id = 18;

      database.expectedPlayers = new List<Player> { p1, p2 };

      controller.LoadDatabase();
      Player[] matched = controller.MatchPlayer("slate$", new MatchOptions { IgnoreCase = true, QueryIsRegex = true });
      Assert.IsNotNull(matched);
      Assert.AreEqual(1, matched.Length);
      Assert.IsTrue(matched[0] == p1);
    }

    /// <summary>
    /// Test an invalid Regex.
    /// </summary>
    [TestMethod]
    public void MatchPlayerByRegex_InvalidRegex()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise();

      Player p1 = controller.CreatePlayer("Manual");
      p1.Name = "¡g Slate";
      p1.Id = 1337;

      Player p2 = controller.CreatePlayer("Manual");
      p2.Name = "Slate*NBF";
      p2.Id = 18;

      database.expectedPlayers = new List<Player> { p1, p2 };

      controller.LoadDatabase();
      Player[] matched = controller.MatchPlayer("[", new MatchOptions { IgnoreCase = true, QueryIsRegex = true });
      Assert.IsNotNull(matched);
      Assert.AreEqual(0, matched.Length);
    }
  }
}