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
      Console.WriteLine("Loading Database... ");
      var start = DateTime.Now;
      var (loadedPlayers, loadedTeams) = database.Load();
      if (loadedPlayers == null || loadedTeams == null)
      {
        Console.Error.WriteLine("ERROR: Failed to load.");
      }
      else
      {
        players = new SortedDictionary<uint, Player>(loadedPlayers.ToDictionary(x => x.Id, x => x));
        teams = new SortedDictionary<long, Team>(loadedTeams.ToDictionary(x => x.Id, x => x));
        var diff = DateTime.Now - start;
        Console.WriteLine("Database loaded successfully.");
        Console.WriteLine($"...Done in {diff.TotalSeconds} seconds.");
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

      if (matchOptions.NearCharacterRecognition)
      {
        query = query.TransformString();
      }

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

      return players.Values.Where(p => func(p)).ToArray();
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

      if (matchOptions.NearCharacterRecognition)
      {
        query = query.TransformString();
      }

      // First, derive the function for clan tags.
      Func<Team, bool> clanTagFunc;
      if (matchOptions.QueryIsRegex)
      {
        try
        {
          Regex regex = matchOptions.IgnoreCase ? new Regex(query, RegexOptions.IgnoreCase) : new Regex(query);
          clanTagFunc = (t) => (matchOptions.NearCharacterRecognition ? StringTransformation.TransformEnumerable(t.ClanTags) : t.ClanTags).Any(n => regex.IsMatch(n));
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
        clanTagFunc = (t) => (matchOptions.NearCharacterRecognition ? StringTransformation.TransformEnumerable(t.ClanTags) : t.ClanTags).Contains(query, comparion);
      }

      // Now the names.
      Func<Team, bool> teamNameFunc;

      if (matchOptions.QueryIsRegex)
      {
        try
        {
          Regex regex = matchOptions.IgnoreCase ? new Regex(query, RegexOptions.IgnoreCase) : new Regex(query);
          teamNameFunc = (t) => regex.IsMatch(matchOptions.NearCharacterRecognition ? t.Name.TransformString() : t.Name);
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
        teamNameFunc = (t) => (matchOptions.NearCharacterRecognition ? t.Name.TransformString() : t.Name).Contains(query, comparion);
      }

      // Return matches,
      // but we want clan tag matches to be first in the ordering.
      return
        teams.Values
          .Where(t => clanTagFunc(t))
          .Concat(teams.Values.Where(t => teamNameFunc(t)))
          .Distinct()
          .ToArray();
    }

    public Player CreatePlayer(string source)
    {
      Player p = new Player
      {
        Id = players.Keys.LastOrDefault() + 1,
        Sources = new string[] { source }
      };
      players.Add(p.Id, p);
      return p;
    }

    public Team CreateTeam(string source)
    {
      Team t = new Team
      {
        Id = teams.Keys.LastOrDefault() + 1,
        Sources = new string[] { source }
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