using SplatTagCore.Social;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("SplatTagUnitTests")]

namespace SplatTagCore
{
  public class SplatTagController
  {
    private readonly ISplatTagDatabase database;
    private Player[] players;
    private Team[] teams;
    private Task? cachingTask;
    private readonly Dictionary<Team, (Player, bool)[]> playersForTeam;

    public SplatTagController(ISplatTagDatabase database)
    {
      this.database = database;
      this.players = new Player[0];
      this.teams = new Team[0];
      this.playersForTeam = new Dictionary<Team, (Player, bool)[]>();
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
      else if (loadedPlayers.Length == 0 && loadedTeams.Length == 0)
      {
        Console.WriteLine("... nothing loaded.");
      }
      else
      {
        players = loadedPlayers;
        teams = loadedTeams;

        cachingTask = Task.Run(() =>
        {
          // Cache the players and their teams.
          foreach (var t in teams)
          {
            var teamPlayers =
              t.GetPlayers(players)
              .Select(p => (p, p.CurrentTeam == t.Id))
              .ToArray();
            playersForTeam.Add(t, teamPlayers);
          }
        });

        var diff = DateTime.Now - start;
        Console.WriteLine("Database loaded successfully.");
        Console.WriteLine($"...Done in {diff.TotalSeconds} seconds.");
      }
    }

    public void SaveDatabase()
    {
      database.Save(players, teams);
    }

    /// <summary>
    /// Get all players associated with a team.
    /// The tuple represents the Player and if they are a current player of the team (true) or not (false).
    /// </summary>
    public (Player, bool)[] GetPlayersForTeam(Team t)
    {
      if (cachingTask != null && !cachingTask.IsCompleted)
      {
        cachingTask.Wait();
      }
      return playersForTeam[t];
    }

    /// <summary>
    /// Match a query to players with default options with all known players.
    /// </summary>
    public Player[] MatchPlayer(string? query)
    {
      return MatchPlayer(query, new MatchOptions());
    }

    /// <summary>
    /// Match a query to players with given options with all known players.
    /// </summary>
    public Player[] MatchPlayer(string? query, MatchOptions matchOptions)
    {
      return MatchPlayer(query, matchOptions, players);
    }

    /// <summary>
    /// Match a query to players with given options from a set of players.
    /// </summary>
    public Player[] MatchPlayer(string? query, MatchOptions matchOptions, ICollection<Player> playersToSearch)
    {
      if (query == null || query == string.Empty)
      {
        return playersToSearch.ToArray();
      }

      if (matchOptions.NearCharacterRecognition)
      {
        query = query.TransformString();
      }

      var filterOptions = matchOptions.FilterOptions;

      // Function of Player in and relevance out, where 0 is no match.
      Func<Player, int> func;
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
            if ((filterOptions & FilterOptions.FriendCode) != 0 && p.FriendCode != null)
            {
              // If FC matches, return top match.
              if (regex.IsMatch(p.FriendCode))
              {
                return int.MaxValue;
              }
            }

            if ((filterOptions & FilterOptions.BattlefySlugs) != 0 && p.BattlefySlugs != null)
            {
              // If the battle slugs match, return top match.
              foreach (Name slug in p.BattlefySlugs)
              {
                string toMatch = (matchOptions.NearCharacterRecognition) ? slug.TransformedName : slug.Value;
                if (regex.IsMatch(toMatch))
                {
                  return int.MaxValue;
                }
              }
            }

            // Otherwise ...
            int relevance = 0;
            if ((filterOptions & FilterOptions.Name) != 0)
            {
              foreach (Name name in p.Names)
              {
                string toMatch = (matchOptions.NearCharacterRecognition) ? name.TransformedName : name.Value;
                if (regex.IsMatch(toMatch))
                {
                  relevance++;
                }
              }
            }

            if ((filterOptions & FilterOptions.DiscordName) != 0 && p.DiscordName != null)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? p.DiscordName.TransformString() : p.DiscordName;
              if (regex.IsMatch(toMatch))
              {
                relevance++;
              }
            }

            if ((filterOptions & FilterOptions.Twitch) != 0)
            {
              foreach (Twitch twitch in p.Twitch)
              {
                string toMatch = (matchOptions.NearCharacterRecognition) ? twitch.TransformedName : twitch.Value;
                if (regex.IsMatch(toMatch))
                {
                  return int.MaxValue;
                }
              }
            }

            if ((filterOptions & FilterOptions.Twitter) != 0)
            {
              foreach (Twitter twitter in p.Twitter)
              {
                string toMatch = (matchOptions.NearCharacterRecognition) ? twitter.TransformedName : twitter.Value;
                if (regex.IsMatch(toMatch))
                {
                  relevance += 20;
                }
              }
            }

            if ((filterOptions & FilterOptions.Sources) != 0)
            {
              foreach (Source source in p.Sources)
              {
                if (source?.Name != null)
                {
                  string name = source.Name;
                  string toMatch = (matchOptions.NearCharacterRecognition) ? name.TransformString() : name;
                  if (regex.IsMatch(toMatch))
                  {
                    relevance++;
                  }
                }
              }
            }
            return relevance;
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
          int relevance = 0;
          if ((filterOptions & FilterOptions.FriendCode) != 0 && p.FriendCode != null)
          {
            // If FC matches, return top match.
            if (p.FriendCode.Equals(query, comparion))
            {
              return int.MaxValue;
            }
            else if (p.FriendCode.Contains(query, comparion))
            {
              relevance++;
            }
          }

          if ((filterOptions & FilterOptions.BattlefySlugs) != 0 && p.BattlefySlugs != null)
          {
            foreach (Name slug in p.BattlefySlugs)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? slug.TransformedName : slug.Value;
              if (toMatch.Equals(query, comparion))
              {
                return int.MaxValue;
              }
              else if (toMatch.Contains(query, comparion))
              {
                relevance++;
              }
            }
          }

          if ((filterOptions & FilterOptions.Name) != 0)
          {
            foreach (Name name in p.Names)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? name.TransformedName : name.Value;
              if (toMatch.Equals(query, comparion))
              {
                relevance += 50; // Give it more relevance
              }
              else if (toMatch.StartsWith(query, comparion))
              {
                relevance += 10;
              }
              else if (toMatch.Contains(query, comparion))
              {
                relevance += 2;
              }
            }
          }

          if ((filterOptions & FilterOptions.DiscordName) != 0 && p.DiscordName != null)
          {
            string toMatch = (matchOptions.NearCharacterRecognition) ? p.DiscordName.TransformString() : p.DiscordName;
            if (toMatch.Equals(query, comparion))
            {
              relevance += 10; // Give it more relevance
            }
            else if (toMatch.Contains(query, comparion))
            {
              relevance++;
            }
          }

          if ((filterOptions & FilterOptions.Twitch) != 0)
          {
            foreach (Name name in p.Twitch)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? name.TransformedName : name.Value;
              if (toMatch.Equals(query, comparion))
              {
                relevance += 50; // Give it more relevance
              }
              else if (toMatch.StartsWith(query, comparion))
              {
                relevance += 10;
              }
              else if (toMatch.Contains(query, comparion))
              {
                relevance += 2;
              }
            }
          }

          if ((filterOptions & FilterOptions.Twitter) != 0)
          {
            foreach (Name name in p.Twitter)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? name.TransformedName : name.Value;
              if (toMatch.Equals(query, comparion))
              {
                relevance += 50; // Give it more relevance
              }
              else if (toMatch.StartsWith(query, comparion))
              {
                relevance += 10;
              }
              else if (toMatch.Contains(query, comparion))
              {
                relevance += 2;
              }
            }
          }

          if ((filterOptions & FilterOptions.Sources) != 0)
          {
            foreach (Source source in p.Sources)
            {
              if (source?.Name != null)
              {
                string name = source.Name;
                string toMatch = (matchOptions.NearCharacterRecognition) ? name.TransformString() : name;
                if (toMatch.Contains(query, comparion))
                {
                  relevance++;
                }
              }
            }
          }
          return relevance;
        };
      }

      return playersToSearch.Select(p => (p, func(p))).Where(pair => pair.Item2 > 0).OrderByDescending(pair => pair.Item2).Select(pair => pair.p).ToArray();
    }

    /// <summary>
    /// Match a query to teams with default options.
    /// </summary>
    public Team[] MatchTeam(string? query)
    {
      return MatchTeam(query, new MatchOptions());
    }

    /// <summary>
    /// Match a query to teams with given options with all known teams.
    /// </summary>
    public Team[] MatchTeam(string? query, MatchOptions matchOptions)
    {
      return MatchTeam(query, matchOptions, teams);
    }

    /// <summary>
    /// Match a query to teams with given options with all known teams.
    /// </summary>
    public Team[] MatchTeam(string? query, MatchOptions matchOptions, ICollection<Team> teamsToSearch)
    {
      if (query == null || query == string.Empty)
      {
        return teamsToSearch.ToArray();
      }

      if (matchOptions.NearCharacterRecognition)
      {
        query = query.TransformString();
      }

      var filterOptions = matchOptions.FilterOptions;

      // Function of Team in and relevance out, where 0 is no match.
      Func<Team, int> func;
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

          func = (t) =>
          {
            int relevance = 0;
            if ((filterOptions & FilterOptions.ClanTag) != 0 && t.ClanTags != null)
            {
              foreach (ClanTag tag in t.ClanTags)
              {
                string toMatch = (matchOptions.NearCharacterRecognition) ? tag.TransformedName : tag.Value;
                if (regex.IsMatch(toMatch))
                {
                  // Put Clan Tag matches first
                  return int.MaxValue;
                }
              }
            }

            if ((filterOptions & FilterOptions.Name) != 0 && t.Name != null)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? t.Name.TransformedName : t.Name.Value;
              if (regex.IsMatch(toMatch))
              {
                relevance++;
              }
            }

            if ((filterOptions & FilterOptions.Sources) != 0 && t.Sources != null)
            {
              foreach (Source source in t.Sources)
              {
                if (source?.Name != null)
                {
                  string name = source.Name;
                  string toMatch = (matchOptions.NearCharacterRecognition) ? name.TransformString() : name;
                  if (regex.IsMatch(toMatch))
                  {
                    relevance++;
                  }
                }
              }
            }

            if ((filterOptions & FilterOptions.Twitter) != 0)
            {
              foreach (Twitter twitter in t.Twitter)
              {
                string toMatch = (matchOptions.NearCharacterRecognition) ? twitter.TransformedName : twitter.Value;
                if (regex.IsMatch(toMatch))
                {
                  relevance += 20;
                }
              }
            }

            return relevance;
          };
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
        func = (t) =>
        {
          int relevance = 0;
          if ((filterOptions & FilterOptions.ClanTag) != 0 && t.ClanTags != null)
          {
            foreach (ClanTag tag in t.ClanTags)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? tag.TransformedName : tag.Value;
              if (toMatch.Equals(query, comparion))
              {
                return int.MaxValue; // Clan tag perfect match
              }
              else if (toMatch.Contains(query, comparion))
              {
                relevance++;
              }
            }
          }

          if ((filterOptions & FilterOptions.Name) != 0 && t.Name != null)
          {
            string toMatch = (matchOptions.NearCharacterRecognition) ? t.Name.TransformedName : t.Name.Value;
            if (toMatch.Equals(query, comparion))
            {
              relevance += 10; // Give it more relevance
            }
            else if (toMatch.Contains(query, comparion))
            {
              relevance++;
            }
          }

          if ((filterOptions & FilterOptions.Sources) != 0 && t.Sources != null)
          {
            foreach (Source source in t.Sources)
            {
              if (source?.Name != null)
              {
                string name = source.Name;
                string toMatch = (matchOptions.NearCharacterRecognition) ? name.TransformString() : name;
                if (toMatch.Equals(query, comparion))
                {
                  relevance += 10; // Give it more relevance
                }
                else if (toMatch.Contains(query, comparion))
                {
                  relevance++;
                }
              }
            }
          }

          if ((filterOptions & FilterOptions.Twitter) != 0)
          {
            foreach (Name name in t.Twitter)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? name.TransformedName : name.Value;
              if (toMatch.Equals(query, comparion))
              {
                relevance += 50; // Give it more relevance
              }
              else if (toMatch.StartsWith(query, comparion))
              {
                relevance += 10;
              }
              else if (toMatch.Contains(query, comparion))
              {
                relevance += 2;
              }
            }
          }
          return relevance;
        };
      }

      return teamsToSearch.Select(t => (t, func(t))).Where(pair => pair.Item2 > 0).OrderByDescending(pair => pair.Item2).Select(pair => pair.t).ToArray();
    }

    /// <summary>
    /// Create a new Player object.
    /// This does NOT save to a database.
    /// </summary>
    /// <param name="source">Specified source of the addition, else null to default to Manual add</param>
    /// <returns></returns>
    public Player CreatePlayer(string ign, Source? source = null)
    {
      return new Player(ign, source ?? Builtins.ManualSource);
    }

    /// <summary>
    /// Create a new Team object.
    /// This does NOT save to a database.
    /// </summary>
    /// <param name="source">Specified source of the addition, else null to default to Manual add</param>
    /// <returns></returns>
    public Team CreateTeam(string name, Source? source = null)
    {
      return new Team(name, source ?? Builtins.ManualSource);
    }

    /// <summary>
    /// Match a Team by its id.
    /// Never returns null.
    /// </summary>
    public Team GetTeamById(Guid id)
    {
      if (id == Team.NoTeam.Id)
      {
        return Team.NoTeam;
      }
      return Array.Find(teams, t => t.Id.Equals(id)) ?? Team.UnlinkedTeam;
    }

    /// <summary> Launch an address in a separate internet browser. </summary>
    public bool TryLaunchAddress(string? link)
    {
      if (!string.IsNullOrEmpty(link))
      {
        try
        {
          var ps = new ProcessStartInfo(link)
          {
            UseShellExecute = true,
            Verb = "open"
          };
          Process.Start(ps);
          return true;
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Can't start the address at {link}: {ex.Message}");
        }
      }
      return false;
    }
  }
}