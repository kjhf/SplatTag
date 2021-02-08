using SplatTagCore.Social;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("SplatTagUnitTests")]

namespace SplatTagCore
{
  public class SplatTagController : ITeamResolver
  {
    private readonly ISplatTagDatabase database;
    private Player[] players;
    private Dictionary<Guid, Team> teams;
    private Dictionary<Guid, Source> sources;
    private Task? cachingTask;
    private readonly ConcurrentDictionary<Team, (Player, bool)[]> playersForTeam;

    public SplatTagController(ISplatTagDatabase database)
    {
      this.database = database;
      this.players = Array.Empty<Player>();
      this.teams = new Dictionary<Guid, Team>();
      this.sources = new Dictionary<Guid, Source>();
      this.playersForTeam = new ConcurrentDictionary<Team, (Player, bool)[]>();
    }

    public void Initialise()
    {
      LoadDatabase();
    }

    public void LoadDatabase()
    {
      Console.WriteLine("Loading Database... ");
      var start = DateTime.Now;
      var (loadedPlayers, loadedTeams, loadedSources) = database.Load();
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
        teams = loadedTeams.AsParallel().ToDictionary(t => t.Id, t => t);
        sources = loadedSources;

        cachingTask = Task.Run(() =>
        {
          // Cache the players and their teams.
          Parallel.ForEach(teams.Values, t =>
          {
            var teamPlayers =
              t.GetPlayers(players)
              .Select(p => (p, p.CurrentTeam == t.Id))
              .ToArray();
            playersForTeam.TryAdd(t, teamPlayers);
          });
          Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Caching task done.");
        });

        var diff = DateTime.Now - start;
        Console.WriteLine("Database loaded successfully.");
        Console.WriteLine($"...Done in {diff.TotalSeconds} seconds.");
      }
    }

    /// <summary>
    /// Get all players associated with a team.
    /// The tuple represents the Player and if they are a current player of the team (true) or not (false).
    /// </summary>
    public (Player, bool)[] GetPlayersForTeam(Team t)
    {
      if (cachingTask == null || !cachingTask.IsCompleted)
      {
        return t.GetPlayers(players)
          .Select(p => (p, p.CurrentTeam == t.Id))
          .ToArray();
      }
      else
      {
        return playersForTeam[t];
      }
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
            if ((filterOptions & FilterOptions.FriendCode) != 0)
            {
              // If FC matches, return top match.
              foreach (FriendCode code in p.FriendCodes)
              {
                string toMatch = code.ToString();
                if (regex.IsMatch(toMatch))
                {
                  return int.MaxValue;
                }
              }
            }

            if ((filterOptions & FilterOptions.BattlefySlugs) != 0)
            {
              // If the battlefy slugs match, return top match.
              foreach (Name bf in p.Battlefy.Slugs)
              {
                string toMatch = (matchOptions.NearCharacterRecognition) ? bf.Transformed : bf.Value;
                if (regex.IsMatch(toMatch))
                {
                  return int.MaxValue;
                }
              }
            }

            if ((filterOptions & FilterOptions.BattlefyPersistentIds) != 0)
            {
              // If the battlefy persistent ids match, return top match.
              foreach (Name bf in p.Battlefy.PersistentIds)
              {
                string toMatch = (matchOptions.NearCharacterRecognition) ? bf.Transformed : bf.Value;
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
                string toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value;
                if (regex.IsMatch(toMatch))
                {
                  relevance++;
                }
              }
            }

            if ((filterOptions & FilterOptions.BattlefyUsername) != 0)
            {
              // Look through the battlefy usernames.
              foreach (Name bf in p.Battlefy.Usernames)
              {
                string toMatch = (matchOptions.NearCharacterRecognition) ? bf.Transformed : bf.Value;
                if (regex.IsMatch(toMatch))
                {
                  relevance += 5;
                }
              }
            }

            if ((filterOptions & FilterOptions.DiscordName) != 0)
            {
              foreach (Name name in p.DiscordNames)
              {
                string toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value;
                if (regex.IsMatch(toMatch))
                {
                  relevance++;
                }
              }
            }

            if ((filterOptions & FilterOptions.DiscordId) != 0)
            {
              foreach (Name name in p.DiscordIds)
              {
                string toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value;
                if (regex.IsMatch(toMatch))
                {
                  relevance++;
                }
              }
            }

            if ((filterOptions & FilterOptions.Twitch) != 0)
            {
              foreach (Twitch twitch in p.Twitch)
              {
                string toMatch = (matchOptions.NearCharacterRecognition) ? twitch.Transformed : twitch.Value;
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
                string toMatch = (matchOptions.NearCharacterRecognition) ? twitter.Transformed : twitter.Value;
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
          return Array.Empty<Player>();
        }
      }
      else
      {
        // Standard query
        StringComparison comparison = matchOptions.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        func = (p) =>
        {
          int relevance = 0;

          if ((filterOptions & FilterOptions.SlappId) != 0)
          {
            // If internal id matches, return top match.
            string toMatch = p.Id.ToString();
            if (toMatch.Equals(query, comparison))
            {
              return int.MaxValue;
            }
          }

          if ((filterOptions & FilterOptions.FriendCode) != 0)
          {
            // If FC matches, return top match.
            foreach (FriendCode code in p.FriendCodes)
            {
              string toMatch = code.ToString();
              if (toMatch.Equals(query, comparison))
              {
                return int.MaxValue;
              }
              else if (toMatch.Contains(query, comparison))
              {
                relevance++;
              }
            }
          }

          if ((filterOptions & FilterOptions.BattlefySlugs) != 0)
          {
            // If the battlefy slugs match, return top match.
            foreach (Name bf in p.Battlefy.Slugs)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? bf.Transformed : bf.Value;
              if (toMatch.Equals(query, comparison))
              {
                return int.MaxValue;
              }
              else if (toMatch.Contains(query, comparison))
              {
                relevance++;
              }
            }
          }

          if ((filterOptions & FilterOptions.BattlefyPersistentIds) != 0)
          {
            // If the battlefy persistent ids match, return top match.
            foreach (Name bf in p.Battlefy.PersistentIds)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? bf.Transformed : bf.Value;
              if (toMatch.Equals(query, comparison))
              {
                return int.MaxValue;
              }
              else if (toMatch.Contains(query, comparison))
              {
                relevance++;
              }
            }
          }

          if ((filterOptions & FilterOptions.BattlefyUsername) != 0)
          {
            // Look through the battlefy usernames.
            foreach (Name bf in p.Battlefy.Usernames)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? bf.Transformed : bf.Value;
              AdjustRelevanceForStringComparison(ref relevance, toMatch, query, comparison);
            }
          }

          if ((filterOptions & FilterOptions.Name) != 0)
          {
            foreach (Name name in p.Names)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value;
              AdjustRelevanceForStringComparison(ref relevance, toMatch, query, comparison);
            }
          }

          if ((filterOptions & FilterOptions.DiscordName) != 0)
          {
            foreach (Name name in p.DiscordNames)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value;
              AdjustRelevanceForStringComparison(ref relevance, toMatch, query, comparison);
            }
          }

          if ((filterOptions & FilterOptions.DiscordId) != 0)
          {
            foreach (Name name in p.DiscordIds)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value;
              AdjustRelevanceForStringComparison(ref relevance, toMatch, query, comparison);
            }
          }

          if ((filterOptions & FilterOptions.Twitch) != 0)
          {
            foreach (Name name in p.Twitch)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value;
              AdjustRelevanceForStringComparison(ref relevance, toMatch, query, comparison);
            }
          }

          if ((filterOptions & FilterOptions.Twitter) != 0)
          {
            foreach (Name name in p.Twitter)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value;
              AdjustRelevanceForStringComparison(ref relevance, toMatch, query, comparison);
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
                if (toMatch.Contains(query, comparison))
                {
                  relevance++;
                }
              }
            }
          }
          return relevance;
        };
      }

      return playersToSearch
        .AsParallel()
        .Select(p => (p, func(p)))
        .Where(pair => pair.Item2 > 0)
        .OrderByDescending(pair => pair.Item2)
        .Select(pair => pair.p)
        .ToArray();
    }

    private static void AdjustRelevanceForStringComparison(ref int relevance, string toMatch, string query, StringComparison comparison)
    {
      if (toMatch.Equals(query, comparison))
      {
        relevance += 50; // Give it more relevance
      }
      else if (toMatch.StartsWith(query, comparison))
      {
        relevance += 10;
      }
      else if (toMatch.Contains(query, comparison))
      {
        relevance += 2;
      }
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
      return MatchTeam(query, matchOptions, teams.Values);
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
                string toMatch = (matchOptions.NearCharacterRecognition) ? tag.Transformed : tag.Value;
                if (regex.IsMatch(toMatch))
                {
                  // Put Clan Tag matches first
                  return int.MaxValue;
                }
              }
            }

            if ((filterOptions & FilterOptions.Name) != 0 && t.Name != null)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? t.Name.Transformed : t.Name.Value;
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
                string toMatch = (matchOptions.NearCharacterRecognition) ? twitter.Transformed : twitter.Value;
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
          return Array.Empty<Team>();
        }
      }
      else
      {
        // Standard query
        StringComparison comparison = matchOptions.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        func = (t) =>
        {
          int relevance = 0;
          if ((filterOptions & FilterOptions.SlappId) != 0)
          {
            // If internal id matches, return top match.
            string toMatch = t.Id.ToString();
            if (toMatch.Equals(query, comparison))
            {
              return int.MaxValue;
            }
          }

          if ((filterOptions & FilterOptions.ClanTag) != 0 && t.ClanTags != null)
          {
            foreach (ClanTag tag in t.ClanTags)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? tag.Transformed : tag.Value;
              if (toMatch.Equals(query, comparison))
              {
                return int.MaxValue; // Clan tag perfect match
              }
              else if (toMatch.Contains(query, comparison))
              {
                relevance++;
              }
            }
          }

          if ((filterOptions & FilterOptions.BattlefyPersistentIds) != 0 && t.BattlefyPersistentTeamId != null)
          {
            // If the battlefy persistent ids match, return top match.
            foreach (Name bf in t.BattlefyPersistentTeamIds)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? bf.Transformed : bf.Value;
              if (toMatch.Equals(query, comparison))
              {
                return int.MaxValue; // Id perfect match
              }
            }
          }

          if ((filterOptions & FilterOptions.Name) != 0 && t.Name != null)
          {
            string toMatch = (matchOptions.NearCharacterRecognition) ? t.Name.Transformed : t.Name.Value;
            AdjustRelevanceForStringComparison(ref relevance, toMatch, query, comparison);
          }

          if ((filterOptions & FilterOptions.Sources) != 0 && t.Sources != null)
          {
            foreach (Source source in t.Sources)
            {
              if (source?.Name != null)
              {
                string name = source.Name;
                string toMatch = (matchOptions.NearCharacterRecognition) ? name.TransformString() : name;
                if (toMatch.Equals(query, comparison))
                {
                  relevance += 10; // Give it more relevance
                }
                else if (toMatch.Contains(query, comparison))
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
              string toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value;
              AdjustRelevanceForStringComparison(ref relevance, toMatch, query, comparison);
            }
          }
          return relevance;
        };
      }

      return teamsToSearch
        .AsParallel()
        .Select(t => (t, func(t)))
        .Where(pair => pair.Item2 > 0)
        .OrderByDescending(pair => pair.Item2)
        .Select(pair => pair.t)
        .ToArray();
    }

    public Source[] GetSources()
    {
      return sources.Values.ToArray();
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
    /// Match a <see cref="Team"/> by its id.
    /// Returns <see cref="Team.UnlinkedTeam"/> if not found.
    /// </summary>
    public Team GetTeamById(Guid id)
    {
      if (id == Team.NoTeam.Id)
      {
        return Team.NoTeam;
      }
      return teams.ContainsKey(id) ? teams[id] : Team.UnlinkedTeam;
    }

    /// <summary>
    /// Match a <see cref="Source"/> by its id.
    /// Returns <see cref="Builtins.BuiltinSource"/> if not found.
    /// </summary>
    public Source GetSourceById(Guid id)
    {
      if (id == Guid.Empty)
      {
        return Builtins.BuiltinSource;
      }
      return sources.ContainsKey(id) ? sources[id] : Builtins.BuiltinSource;
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