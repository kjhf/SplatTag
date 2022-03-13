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
      Team exampleTeam = new Team("Example Team", Builtins.ManualSource);
      exampleTeam.AddClanTag("e.g", Builtins.ManualSource, TagOption.Front);
      exampleTeam.AddDivision(new Division(1, DivType.DSB), Builtins.ManualSource);
      var TEAM_ID = exampleTeam.Id;

      database.expectedTeams = new List<Team> { exampleTeam };
      var PLAYER_NAME = "Example Name";
      database.expectedPlayers = new List<Player>
      {
        new Player(PLAYER_NAME, new Guid[] { TEAM_ID }, Builtins.ManualSource)
      };
      var PLAYER_ID = database.expectedPlayers[0].Id;

      controller.LoadDatabase();
      Assert.IsTrue(database.loadCalled);

      // Also check the player and team is now in the controller
      var players = controller.MatchPlayer(PLAYER_ID.ToString(), new MatchOptions { FilterOptions = FilterOptions.SlappId });
      Assert.IsNotNull(players.FirstOrDefault());
      var player = players[0];
      Assert.IsTrue(player.Name.Value == PLAYER_NAME);

      var teams = controller.MatchTeam(TEAM_ID.ToString(), new MatchOptions { FilterOptions = FilterOptions.SlappId });
      Assert.IsNotNull(teams.FirstOrDefault());
      var team = teams[0];
      Assert.IsTrue(team.CurrentDiv.Value == 1);
      Assert.IsTrue(team.CurrentDiv.DivType == DivType.DSB);

      // Verify getting the players for that team returns our player
      var playersForExampleTeam = controller.GetPlayersForTeam(exampleTeam);
      Assert.IsNotNull(playersForExampleTeam);
      Assert.IsTrue(playersForExampleTeam.Count == 1);
      Assert.IsTrue(playersForExampleTeam[0].player.Equals(player));
    }
  }
}