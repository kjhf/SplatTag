using System;
using System.Collections.Generic;
using System.Linq;

namespace SplatTagCore
{
  public class SplatTagController
  {
    private readonly ISplatTagDatabase database;
    private SortedDictionary<uint, Player> players;
    private SortedDictionary<uint, Team> teams;

    public SplatTagController(ISplatTagDatabase database)
    {
      this.database = database;
      this.players = new SortedDictionary<uint, Player>();
      this.teams = new SortedDictionary<uint, Team>();
    }

    public void Initialise(string[] commandArgs)
    {
      // TODO parse command line arguments.

      LoadDatabase();
    }

    public void LoadDatabase()
    {
      var result = database.Load();
      players = new SortedDictionary<uint, Player>(result.Item1.ToDictionary(x => x.Id, x => x));
      teams = new SortedDictionary<uint, Team>(result.Item2.ToDictionary(x => x.Id, x => x));
    }

    public void SaveDatabase()
    {
      database.Save(players.Values, teams.Values);
    }

    public Player[] MatchPlayer(string query)
    {
      List<Player> retVal = new List<Player>();
      retVal.AddRange(players.Values.Where(p => p.Names.Contains(query, StringComparison.OrdinalIgnoreCase)));
      return retVal.ToArray();
    }

    public Team[] MatchTeam(string query)
    {
      List<Team> retVal = new List<Team>();
      retVal.AddRange(teams.Values.Where(t => t.Name.Contains(query, StringComparison.OrdinalIgnoreCase)));
      retVal.AddRange(teams.Values.Where(t => t.ClanTags.Contains(query, StringComparison.OrdinalIgnoreCase)));
      return retVal.ToArray();
    }

    public Player CreatePlayer()
    {
      Player p = new Player
      {
        Id = players.Keys.LastOrDefault() + 1
      };
      players.Add(p.Id, p);
      return p;
    }

    public Team CreateTeam()
    {
      Team t = new Team
      {
        Id = teams.Keys.LastOrDefault() + 1
      };
      teams.Add(t.Id, t);
      return t;
    }
  }
}