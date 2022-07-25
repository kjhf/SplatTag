using SplatTagCore;
using SplatTagCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SplatTagUnitTests
{
  public class UnitTestDatabase : ISplatTagDatabase
  {
    public List<Player> expectedPlayers = new List<Player>();
    public List<Team> expectedTeams = new List<Team>();
    public Dictionary<string, Source> expectedSources = new Dictionary<string, Source>();

    public bool loadCalled;
    public bool saveCalled;

    public IReadOnlyList<Player> Players => expectedPlayers;
    public IReadOnlyDictionary<Guid, Team> Teams => expectedTeams.ToDictionary(t => t.Id, t => t);
    public IReadOnlyDictionary<string, Source> Sources => expectedSources;
    public bool Loaded => loadCalled;

    public (Player[], Team[], Dictionary<string, Source>) Load()
    {
      loadCalled = true;
      return (expectedPlayers.ToArray(), expectedTeams.ToArray(), expectedSources);
    }

    bool ISplatTagDatabase.Load()
    {
      loadCalled = true;
      return expectedPlayers.Count > 0 || expectedTeams.Count > 0 || expectedSources.Count > 0;
    }

    public bool GetTeamById(Guid id, out Team team)
    {
      var result = Teams.TryGetValue(id, out Team? tResult);
      if (result)
      {
        team = tResult!;
      }
      else
      {
        team = Team.UnknownTeam;
      }
      return result;
    }

    public Team GetTeamById(Guid id) => Teams.Get(id, Team.UnknownTeam);

    public IReadOnlyList<(Player player, bool mostRecent)> GetPlayersForTeam(Team t)
    {
      return t.GetPlayers(Players)
          .AsParallel()
          .Select(p => (p, p.CurrentTeam == t.Id))
          .ToArray();
    }
  }
}