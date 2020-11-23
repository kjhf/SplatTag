using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SplatTagUnitTests
{
  /// <summary>
  /// Core SplatTag unit tests
  /// </summary>
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
      controller.Initialise();
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
      controller.Initialise();
      Assert.IsTrue(database.loadCalled);

      database.loadCalled = false;

      Team exampleTeam =
        new Team
        {
          Name = "Example Team",
          ClanTags = new string[] { "e.g" },
          Div = new Division(1)
        };
      var TEAM_ID = exampleTeam.Id;

      database.expectedTeams = new List<Team> { exampleTeam };
      database.expectedPlayers = new List<Player>
      {
        new Player
        {
          Names = new string[] { "Example Name" },
          Teams = new Guid[] { TEAM_ID }
        }
      };
      var PLAYER_ID = database.expectedPlayers[0].Id;

      controller.LoadDatabase();
      Assert.IsTrue(database.loadCalled);

      // Also check the player and team is now in the controller
      object playersDict = Util.GetPrivateMember(controller, "players");
      Assert.IsNotNull(playersDict);
      var players = (IList<Player>)playersDict;
      Assert.IsNotNull(players);
      Player player1 = players.FirstOrDefault(p => p.Id == PLAYER_ID);
      Assert.IsNotNull(player1);
      Assert.IsTrue(player1.Id == PLAYER_ID);

      object teamsDict = Util.GetPrivateMember(controller, "teams");
      Assert.IsNotNull(teamsDict);
      var teams = (IList<Team>)teamsDict;
      Assert.IsNotNull(teams);
      Team team1 = teams.FirstOrDefault(t => t.Id == TEAM_ID);
      Assert.IsNotNull(team1);
      Assert.IsTrue(team1.Id == TEAM_ID);
      Assert.IsTrue(team1.Div.Value == 1);

      // Verify getting the players for that team returns our player
      (Player, bool)[] playersForExampleTeam = controller.GetPlayersForTeam(exampleTeam);
      Assert.IsNotNull(playersForExampleTeam);
      Assert.IsTrue(playersForExampleTeam.Length == 1);
      Assert.IsTrue(playersForExampleTeam[0].Item1.Equals(player1));
    }

    /// <summary>
    /// Create a player.
    /// </summary>
    [TestMethod]
    public void CreatePlayerTest()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise();

      Player p = controller.CreatePlayer();
      Assert.IsNotNull(p);
      object players = Util.GetPrivateMember(controller, "players");
      Assert.IsNotNull(players);
    }

    /// <summary>
    /// Create a team.
    /// </summary>
    [TestMethod]
    public void CreateTeamTest()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise();

      Team t = controller.CreateTeam();
      Assert.IsNotNull(t);
      object teams = Util.GetPrivateMember(controller, "teams");
      Assert.IsNotNull(teams);
    }
  }
}