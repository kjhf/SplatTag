using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SplatTagUnitTests
{
  public class UnitTestDatabase : ISplatTagDatabase
  {
    public List<Player> expectedPlayers = new List<Player>();
    public List<Team> expectedTeams = new List<Team>();
    public Dictionary<Guid, Source> expectedSources = new Dictionary<Guid, Source>();

    public bool loadCalled;
    public bool saveCalled;

    public (Player[], Team[], Dictionary<Guid, Source>) Load()
    {
      loadCalled = true;
      return (expectedPlayers.ToArray(), expectedTeams.ToArray(), expectedSources);
    }
  }
}