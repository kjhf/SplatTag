using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SplatTagUnitTests
{
  public class UnitTestDatabase : ISplatTagDatabase
  {
    public List<Player> expectedPlayers = new List<Player>();
    public List<Team> expectedTeams = new List<Team>();

    public bool loadCalled;
    public bool saveCalled;

    public (Player[], Team[]) Load()
    {
      loadCalled = true;
      return (expectedPlayers.ToArray(), expectedTeams.ToArray());
    }

    public void Save(IEnumerable<Player> players, IEnumerable<Team> teams)
    {
      saveCalled = true;
      expectedPlayers = players.ToList();
      expectedTeams = teams.ToList();
    }
  }
}
