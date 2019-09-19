using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagCore;
using System.Collections;
using System.Collections.Generic;

namespace SplatTagUnitTests
{
  [TestClass]
  public class SplatTagUnitTest
  {
    /// <summary>
    /// Verify that the controller can be built without errors.
    /// </summary>
    [TestMethod]
    public void BuildControllerTest()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise(new string[] { "P.exe" });
      Assert.IsTrue(database.loadCalled);
    }

    /// <summary>
    /// Verify that the database can be reloaded.
    /// </summary>
    [TestMethod]
    public void LoadDatabaseTest()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise(new string[] { "P.exe" });
      Assert.IsTrue(database.loadCalled);

      database.loadCalled = false;

      const uint TEAM_ID = 29;
      Team exampleTeam =
        new Team
        {
          Id = TEAM_ID,
          Name = "Example Team",
          ClanTags = new string[] { "e.g" }
        };

      database.expectedTeams = new List<Team> { exampleTeam };
      const uint PLAYER_ID = 19;
      database.expectedPlayers = new List<Player>
      {
        new Player
        {
          Id = PLAYER_ID,
          Names = new string[] { "Example Name" },
          Teams = new Team[] { exampleTeam }
        }
      };

      controller.LoadDatabase();
      Assert.IsTrue(database.loadCalled);

      // Also check the player and team is now in the controller
      object playersDict = Util.GetPrivateMember(controller, "players");
      Assert.IsNotNull(playersDict);
      var dictionary1 = (IDictionary<uint, Player>)playersDict;
      Assert.IsNotNull(dictionary1);
      Assert.IsTrue(dictionary1.TryGetValue(PLAYER_ID, out Player target1));
      Assert.IsTrue(target1.Id == PLAYER_ID);
      
      object teamsDict = Util.GetPrivateMember(controller, "teams");
      Assert.IsNotNull(teamsDict);
      var dictionary2 = (IDictionary<uint, Team>)teamsDict;
      Assert.IsNotNull(dictionary2);
      Assert.IsTrue(dictionary2.TryGetValue(TEAM_ID, out Team target2));
      Assert.IsTrue(target2.Id == TEAM_ID);
    }

    /// <summary>
    /// Create a player.
    /// </summary>
    [TestMethod]
    public void CreatePlayerTest()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise(new string[] { "P.exe" });

      Player p = controller.CreatePlayer();
      Assert.IsNotNull(p);
      object playersDict = Util.GetPrivateMember(controller, "players");
      Assert.IsNotNull(playersDict);
      var dictionary = (IDictionary<uint, Player>)playersDict;
      Assert.IsNotNull(dictionary);
      Assert.IsTrue(dictionary.TryGetValue(p.Id, out Player target));
      Assert.IsTrue(target == p);
    }

    /// <summary>
    /// Create a team.
    /// </summary>
    [TestMethod]
    public void CreateTeamTest()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise(new string[] { "P.exe" });

      Team t = controller.CreateTeam();
      Assert.IsNotNull(t);
      object teamsDict = Util.GetPrivateMember(controller, "teams");
      Assert.IsNotNull(teamsDict);
      var dictionary = (IDictionary<uint, Team>)teamsDict;
      Assert.IsNotNull(dictionary);
      Assert.IsTrue(dictionary.TryGetValue(t.Id, out Team target));
      Assert.IsTrue(target == t);
    }

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
    public void MatchTeamByTagTest()
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
  }
}
