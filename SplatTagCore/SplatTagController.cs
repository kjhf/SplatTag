using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("SplatTagUnitTests")]

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
      var (loadedPlayers, loadedTeams) = database.Load();
      if (loadedPlayers == null || loadedTeams == null)
      {
        Console.WriteLine("ERROR: Failed to load.");
      }
      else
      {
        players = new SortedDictionary<uint, Player>(loadedPlayers.ToDictionary(x => x.Id, x => x));
        teams = new SortedDictionary<uint, Team>(loadedTeams.ToDictionary(x => x.Id, x => x));
        Console.WriteLine("Database loaded successfully.");
      }
    }

    public void SaveDatabase()
    {
      database.Save(players.Values, teams.Values);
    }

    /// <summary>
    /// Get all players associated with a team.
    /// The tuple represents the Player and if they are a current player of the team (true) or not (false).
    /// </summary>
    public (Player, bool)[] GetPlayersForTeam(Team t)
    {
      return players.Values
        .Where(p => p.Teams.Contains(t))
        .Select(p => (p, p.CurrentTeam.Equals(t))).ToArray();
    }

    /// <summary>
    /// Match a query to players with default options.
    /// </summary>
    public Player[] MatchPlayer(string query)
    {
      return MatchPlayer(query, new MatchOptions());
    }

    /// <summary>
    /// Match a query to players with given options.
    /// </summary>
    public Player[] MatchPlayer(string query, MatchOptions matchOptions)
    {
      List<Player> retVal = new List<Player>();
      Func<Player, bool> func;
      if (matchOptions.QueryIsRegex)
      {
        try
        {
          Regex regex;
          if (matchOptions.IgnoreCase)
          {
            regex = new Regex(query, RegexOptions.IgnoreCase);
          }
          else
          {
            regex = new Regex(query);
          }

          func = (p) =>
          {
            string[] names = matchOptions.NearCharacterRecognition ? ASCIIFold.TransformEnumerable(p.Names) : p.Names;
            return names.Any(n => regex.IsMatch(n));
          };
        }
        catch (ArgumentException)
        {
          // Regex parsing error
          return new Player[0];
        }
      }
      else
      {
        // Standard query
        StringComparison comparion = matchOptions.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        func = (p) =>
        {
          string[] names = matchOptions.NearCharacterRecognition ? ASCIIFold.TransformEnumerable(p.Names) : p.Names;
          return names.Contains(query, comparion);
        };
      }

      retVal.AddRange(players.Values.Where(p => func(p)));
      return retVal.ToArray();
    }

    /// <summary>
    /// Match a query to teams with default options.
    /// </summary>
    public Team[] MatchTeam(string query)
    {
      return MatchTeam(query, new MatchOptions());
    }

    /// <summary>
    /// Match a query to teams with given options.
    /// </summary>
    public Team[] MatchTeam(string query, MatchOptions matchOptions)
    {
      List<Team> retVal = new List<Team>();

      // First, derive the function for clan tags.
      Func<Team, bool> func;
      if (matchOptions.QueryIsRegex)
      {
        try
        {
          Regex regex = matchOptions.IgnoreCase ? new Regex(query, RegexOptions.IgnoreCase) : new Regex(query);
          func = (t) => (matchOptions.NearCharacterRecognition ? ASCIIFold.TransformEnumerable(t.ClanTags) : t.ClanTags).Any(n => regex.IsMatch(n));
        }
        catch (ArgumentException)
        {
          // Regex parsing error
          return new Team[0];
        }
      }
      else
      {
        // Standard query
        StringComparison comparion = matchOptions.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        func = (t) => (matchOptions.NearCharacterRecognition ? ASCIIFold.TransformEnumerable(t.ClanTags) : t.ClanTags).Contains(query, comparion);
      }

      // Add matching clan tags to the result.
      retVal.AddRange(teams.Values.Where(t => func(t)));

      // Now the names.
      if (matchOptions.QueryIsRegex)
      {
        try
        {
          Regex regex = matchOptions.IgnoreCase ? new Regex(query, RegexOptions.IgnoreCase) : new Regex(query);
          func = (t) => regex.IsMatch(matchOptions.NearCharacterRecognition ? t.Name.TransformString() : t.Name);
        }
        catch (ArgumentException)
        {
          // Regex parsing error
          return new Team[0];
        }
      }
      else
      {
        // Standard query
        StringComparison comparion = matchOptions.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        func = (t) => (matchOptions.NearCharacterRecognition ? t.Name.TransformString() : t.Name).Contains(query, comparion);
      }

      // Add matching names to the result.
      retVal.AddRange(teams.Values.Where(t => func(t)));

      // Filter unique
      retVal = retVal.Distinct().ToList();
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