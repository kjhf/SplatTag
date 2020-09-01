using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("SplatTagUnitTests")]

namespace SplatTagCore
{
  public class SplatTagController
  {
    private readonly ISplatTagDatabase database;
    private SortedDictionary<uint, Player> players;
    private SortedDictionary<long, Team> teams;

    public SplatTagController(ISplatTagDatabase database)
    {
      this.database = database;
      this.players = new SortedDictionary<uint, Player>();
      this.teams = new SortedDictionary<long, Team>();
    }

    public void Initialise()
    {
      LoadDatabase();
    }

    public void LoadDatabase()
    {
      var (loadedPlayers, loadedTeams) = database.Load();
      if (loadedPlayers == null || loadedTeams == null)
      {
        Console.Error.WriteLine("ERROR: Failed to load.");
      }
      else
      {
        players = new SortedDictionary<uint, Player>(loadedPlayers.ToDictionary(x => x.Id, x => x));
        teams = new SortedDictionary<long, Team>(loadedTeams.ToDictionary(x => x.Id, x => x));
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
      var teamPlayers = players.Values.Where(p => p.Teams.Contains(t.Id));
      return teamPlayers.Select(p => (p, p.CurrentTeam == t.Id)).ToArray();
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
      if (string.IsNullOrEmpty(query))
      {
        return players.Values.ToArray();
      }

      List<Player> retVal = new List<Player>();
      Func<Player, bool> func;
      if (matchOptions.QueryIsRegex)
      {
        // Regex match
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
            List<string> names = new List<string>();
            names.AddRange(p.Names);
            if (p.DiscordName != null)
            {
              names.Add(p.DiscordName);
            }
            if (p.FriendCode != null)
            {
              names.Add(p.FriendCode);
            }
            if (matchOptions.NearCharacterRecognition)
            {
              for (int i = 0; i < names.Count; i++)
              {
                names[i] = names[i].TransformString();
              }
            }
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
          List<string> names = new List<string>();
          names.AddRange(p.Names);
          if (p.DiscordName != null)
          {
            names.Add(p.DiscordName);
          }
          if (p.FriendCode != null)
          {
            names.Add(p.FriendCode);
          }
          if (matchOptions.NearCharacterRecognition)
          {
            for (int i = 0; i < names.Count; i++)
            {
              names[i] = names[i].TransformString();
            }
          }
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
      if (string.IsNullOrEmpty(query))
      {
        return teams.Values.ToArray();
      }

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

    public Player CreatePlayer(string source)
    {
      Player p = new Player
      {
        Id = players.Keys.LastOrDefault() + 1,
        Sources = new List<string> { source }
      };
      players.Add(p.Id, p);
      return p;
    }

    public Team CreateTeam(string source)
    {
      Team t = new Team
      {
        Id = teams.Keys.LastOrDefault() + 1,
        Sources = new List<string> { source }
      };
      teams.Add(t.Id, t);
      return t;
    }

    /// <summary>
    /// Match a Player by its id.
    /// </summary>
    public Player GetPlayerById(uint id)
    {
      bool matched = players.TryGetValue(id, out Player found);
      return matched ? found : null;
    }

    /// <summary>
    /// Match a Team by its id.
    /// </summary>
    public Team GetTeamById(long id)
    {
      if (id == 0)
      {
        return Team.NoTeam;
      }
      bool matched = teams.TryGetValue(id, out Team found);
      return matched ? found : null;
    }

    public void TryLaunchTwitter(Team t)
    {
      if (t.Twitter != null)
      {
        try
        {
          var ps = new ProcessStartInfo(t.Twitter)
          {
            UseShellExecute = true,
            Verb = "open"
          };
          Process.Start(ps);
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Can't start the Twitter address at {t.Twitter}: {ex.Message}");
        }
      }
    }
  }
}