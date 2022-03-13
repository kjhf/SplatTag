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
    /// <summary>
    /// Verbose flag
    /// </summary>
    public static bool Verbose { get; set; }

    private readonly ISplatTagDatabase? database;
    private IReadOnlyList<Player> Players => database?.Players ?? Array.Empty<Player>();
    private IReadOnlyDictionary<Guid, Team> Teams => database?.Teams ?? new Dictionary<Guid, Team>();
    private IReadOnlyDictionary<string, Source> Sources => database?.Sources ?? new Dictionary<string, Source>();

    private Task? cachingTask;
    private readonly ConcurrentDictionary<Team, (Player, bool)[]> playersForTeam;
    public bool CachingDone => cachingTask?.IsCompleted == true;

    public SplatTagController(ISplatTagDatabase? database = null)
    {
      Console.WriteLine("Creating SplatTagController. Debugger.IsAttached=" + Debugger.IsAttached);
#if DEBUG
      Console.WriteLine("Running in DEBUG.");
#else
      Console.WriteLine("Running in Release.");
#endif // DEBUG

      this.database = database;
      this.playersForTeam = new ConcurrentDictionary<Team, (Player, bool)[]>();
    }

    public void Initialise()
    {
      LoadDatabase();
    }

    public void LoadDatabase()
    {
      Console.WriteLine("Loading Database... ");

      if (database == null)
      {
        Console.Error.WriteLine($"ERROR: No database attached to this {nameof(SplatTagController)}.");
        Console.WriteLine("... nothing loaded.");
        return;
      }

      var start = DateTime.Now;
      bool loaded = database.Load();
      if (!loaded)
      {
        Console.WriteLine("... nothing loaded.");
      }
      else
      {
        cachingTask = Task.Run(() =>
        {
          // Cache the players and their teams.
          Parallel.ForEach(Teams.Values.Where(t => t != null), t =>
          {
            var teamPlayers =
                t.GetPlayers(Players)
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
    /// Get the players that played on team <paramref name="t"/>, as a list of tuples, containing the player and if
    /// that player still plays for the team (true) or is no longer the most recent team (false).
    /// </summary>
    public IReadOnlyList<(Player player, bool mostRecent)> GetPlayersForTeam(Team t)
    {
      if (cachingTask?.IsCompleted == true)
      {
        return playersForTeam[t];
      }
      else
      {
        return t.GetPlayers(Players)
          .AsParallel()
          .Select(p => (p, p.CurrentTeam == t.Id))
          .ToArray();
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
    /// <param name="limit">Limit number of results, or -1 to not set.</param>
    public Player[] MatchPlayer(string? query, MatchOptions matchOptions)
    {
      return MatchPlayer(query, matchOptions, Players);
    }

    /// <summary>
    /// Match a query to players with given options from a set of players.
    /// </summary>
    public Player[] MatchPlayer(string? query, MatchOptions matchOptions, IReadOnlyCollection<Player> playersToSearch)
    {
      if (string.IsNullOrEmpty(query))
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
              foreach (var toMatch in from FriendCode code in p.FCInformation.GetCodesUnordered()
                                      let toMatch = code.ToString()
                                      select toMatch)
              {
                if (regex.IsMatch(toMatch))
                {
                  return int.MaxValue;
                }
              }
            }

            if ((filterOptions & FilterOptions.BattlefySlugs) != 0)
            {
              // If the battlefy slugs match, return top match.
              foreach (var toMatch in from Name name in p.Battlefy.Slugs
                                      let toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value
                                      select toMatch)
              {
                if (regex.IsMatch(toMatch))
                {
                  return int.MaxValue;
                }
              }
            }

            if ((filterOptions & FilterOptions.BattlefyPersistentIds) != 0)
            {
              // If the battlefy persistent ids match, return top match.
              foreach (var toMatch in from Name name in p.Battlefy.PersistentIds
                                      let toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value
                                      select toMatch)
              {
                if (regex.IsMatch(toMatch))
                {
                  return int.MaxValue;
                }
              }
            }

            // Otherwise ...
            int relevance = 0;
            if ((filterOptions & FilterOptions.PlayerName) != 0)
            {
              foreach (var toMatch in from Name name in p.AllKnownNames
                                      let toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value
                                      select toMatch)
              {
                if (regex.IsMatch(toMatch))
                {
                  relevance++;
                }
              }
            }

            if ((filterOptions & FilterOptions.BattlefyUsername) != 0)
            {
              // Look through the battlefy usernames.
              foreach (var toMatch in from Name name in p.Battlefy.Usernames
                                      let toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value
                                      select toMatch)
              {
                if (regex.IsMatch(toMatch))
                {
                  relevance += 5;
                }
              }
            }

            if ((filterOptions & FilterOptions.DiscordName) != 0)
            {
              foreach (var toMatch in from Name name in p.DiscordNames
                                      let toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value
                                      select toMatch)
              {
                if (regex.IsMatch(toMatch))
                {
                  relevance++;
                }
              }
            }

            if ((filterOptions & FilterOptions.DiscordId) != 0)
            {
              foreach (var toMatch in from Name name in p.DiscordIds
                                      let toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value
                                      select toMatch)
              {
                if (regex.IsMatch(toMatch))
                {
                  relevance++;
                }
              }
            }

            if ((filterOptions & FilterOptions.Twitch) != 0)
            {
              foreach (var toMatch in from Twitch twitch in p.TwitchProfiles
                                      let toMatch = (matchOptions.NearCharacterRecognition) ? twitch.Transformed : twitch.Value
                                      select toMatch)
              {
                if (regex.IsMatch(toMatch))
                {
                  return int.MaxValue;
                }
              }
            }

            if ((filterOptions & FilterOptions.Twitter) != 0)
            {
              foreach (var toMatch in from Twitter twitter in p.TwitterProfiles
                                      let toMatch = (matchOptions.NearCharacterRecognition) ? twitter.Transformed : twitter.Value
                                      select toMatch)
              {
                if (regex.IsMatch(toMatch))
                {
                  relevance += 20;
                }
              }
            }

            if ((filterOptions & FilterOptions.Sources) != 0)
            {
              relevance += (from Source source in p.Sources
                            where source?.Name != null
                            let name = source.Name
                            let toMatch = (matchOptions.NearCharacterRecognition) ? name.TransformString() : name
                            where regex.IsMatch(toMatch)
                            select source).Count();
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
            foreach (var toMatch in from FriendCode code in p.FCInformation.GetCodesUnordered()
                                    let toMatch = code.ToString()
                                    select toMatch)
            {
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
            foreach (var toMatch in from Name name in p.Battlefy.Slugs
                                    let toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value
                                    select toMatch)
            {
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
            foreach (var toMatch in from Name name in p.Battlefy.PersistentIds
                                    let toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value
                                    select toMatch)
            {
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
            foreach (var toMatch in from Name name in p.Battlefy.Usernames
                                    let toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value
                                    select toMatch)
            {
              AdjustRelevanceForStringComparison(ref relevance, toMatch, query, comparison);
            }
          }

          if ((filterOptions & FilterOptions.PlayerName) != 0)
          {
            foreach (var toMatch in from Name name in p.AllKnownNames
                                    let toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value
                                    select toMatch)
            {
              AdjustRelevanceForStringComparison(ref relevance, toMatch, query, comparison);
            }
          }

          if ((filterOptions & FilterOptions.DiscordName) != 0)
          {
            foreach (var toMatch in from Name name in p.DiscordNames
                                    let toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value
                                    select toMatch)
            {
              AdjustRelevanceForStringComparison(ref relevance, toMatch, query, comparison);
            }
          }

          if ((filterOptions & FilterOptions.DiscordId) != 0)
          {
            foreach (var toMatch in from Name name in p.DiscordIds
                                    let toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value
                                    select toMatch)
            {
              AdjustRelevanceForStringComparison(ref relevance, toMatch, query, comparison);
            }
          }

          if ((filterOptions & FilterOptions.Twitch) != 0)
          {
            foreach (var toMatch in from Name name in p.TwitchProfiles
                                    let toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value
                                    select toMatch)
            {
              AdjustRelevanceForStringComparison(ref relevance, toMatch, query, comparison);
            }
          }

          if ((filterOptions & FilterOptions.Twitter) != 0)
          {
            foreach (var toMatch in from Name name in p.TwitterProfiles
                                    let toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value
                                    select toMatch)
            {
              AdjustRelevanceForStringComparison(ref relevance, toMatch, query, comparison);
            }
          }

          if ((filterOptions & FilterOptions.Sources) != 0)
          {
            relevance += (from Source source in p.Sources
                          where source?.Name != null
                          let name = source.Name
                          let toMatch = (matchOptions.NearCharacterRecognition) ? name.TransformString() : name
                          where toMatch.Contains(query, comparison)
                          select source).Count();
          }
          return relevance;
        };
      }

      var r = playersToSearch
        .AsParallel()
        .Select(p => (p, func(p)))
        .Where(pair => pair.Item2 > 0)
        .OrderByDescending(pair => pair.Item2)
        .ThenBy(pair => pair.p, IReadonlySourceableExtensions.GetMostRecentComparer())  // Most recent first
        .Select(pair => pair.p);

      // Take is a limit operation (does not throw if limit > count)
      return matchOptions.Limit == -1 ? r.ToArray() : r.Take(matchOptions.Limit).ToArray();
    }

    private static void AdjustRelevanceForStringComparison(ref int relevance, string toMatch, string query, StringComparison comparison, int containsScore = 1)
    {
      if (toMatch.Equals(query, comparison))
      {
        relevance += 50; // Give it more relevance
      }
      else if (toMatch.StartsWith(query, comparison))
      {
        relevance += 10;
      }
      else if ((containsScore != 0) && toMatch.Contains(query, comparison))
      {
        relevance += containsScore;
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
      return MatchTeam(query, matchOptions, Teams.Values);
    }

    /// <summary>
    /// Match a query to teams with given options with all known teams.
    /// </summary>
    public Team[] MatchTeam(string? query, MatchOptions matchOptions, IEnumerable<Team> teamsToSearch)
    {
      if (string.IsNullOrEmpty(query))
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
              foreach (var toMatch in from ClanTag tag in t.ClanTags
                                      let toMatch = (matchOptions.NearCharacterRecognition) ? tag.Transformed : tag.Value
                                      select toMatch)
              {
                if (regex.IsMatch(toMatch))
                {
                  return int.MaxValue;
                }
              }
            }

            if ((filterOptions & FilterOptions.TeamName) != 0 && t.Name != null)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? t.Name.Transformed : t.Name.Value;
              if (regex.IsMatch(toMatch))
              {
                relevance++;
              }
            }

            if ((filterOptions & FilterOptions.Sources) != 0 && t.Sources != null)
            {
              relevance += (from Source source in t.Sources
                            where source?.Name != null
                            let name = source.Name
                            let toMatch = (matchOptions.NearCharacterRecognition) ? name.TransformString() : name
                            where regex.IsMatch(toMatch)
                            select source).Count();
            }

            if ((filterOptions & FilterOptions.Twitter) != 0)
            {
              foreach (var toMatch in from Twitter twitter in t.TwitterProfiles
                                      let toMatch = (matchOptions.NearCharacterRecognition) ? twitter.Transformed : twitter.Value
                                      select toMatch)
              {
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
            foreach (var toMatch in from ClanTag tag in t.ClanTags
                                    let toMatch = (matchOptions.NearCharacterRecognition) ? tag.Transformed : tag.Value
                                    select toMatch)
            {
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

          if ((filterOptions & FilterOptions.BattlefyPersistentIds) != 0 && t.BattlefyPersistentTeamId != null)
          {
            // If the battlefy persistent ids match, return top match.
            foreach (var toMatch in from Name name in t.BattlefyPersistentTeamIds
                                    let toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value
                                    select toMatch)
            {
              if (toMatch.Equals(query, comparison))
              {
                return int.MaxValue;
              }
            }
          }

          if ((filterOptions & FilterOptions.TeamName) != 0 && t.Name != null)
          {
            string toMatch = (matchOptions.NearCharacterRecognition) ? t.Name.Transformed : t.Name.Value;
            AdjustRelevanceForStringComparison(ref relevance, toMatch, query, comparison);
          }

          if ((filterOptions & FilterOptions.Sources) != 0 && t.Sources != null)
          {
            foreach (var toMatch in from Source source in t.Sources
                                    where source?.Name != null
                                    let name = source.Name
                                    let toMatch = (matchOptions.NearCharacterRecognition) ? name.TransformString() : name
                                    select toMatch)
            {
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

          if ((filterOptions & FilterOptions.Twitter) != 0)
          {
            foreach (var toMatch in from Name name in t.TwitterProfiles
                                    let toMatch = (matchOptions.NearCharacterRecognition) ? name.Transformed : name.Value
                                    select toMatch)
            {
              AdjustRelevanceForStringComparison(ref relevance, toMatch, query, comparison);
            }
          }
          return relevance;
        };
      }

      var r = teamsToSearch
        .AsParallel()
        .Select(t => (t, func(t)))
        .Where(pair => pair.Item2 > 0)
        .OrderByDescending(pair => pair.Item2)
        .ThenBy(pair => pair.t, IReadonlySourceableExtensions.GetMostRecentComparer())  // Most recent first
        .Select(pair => pair.t);

      // Take is a limit operation (does not throw if limit > count)
      return matchOptions.Limit == -1 ? r.ToArray() : r.Take(matchOptions.Limit).ToArray();
    }

    public Source[] GetSources()
    {
      return Sources.Values.ToArray();
    }

    /// <summary>
    /// Match a Team by its id.
    /// </summary>
    /// <returns>
    /// Non-null team, which defaults to <see cref="Team.UnlinkedTeam"/> if not found.
    /// </returns>
    public Team GetTeamById(Guid id)
    {
      return (id == Team.NoTeam.Id) ? Team.NoTeam :
        (Teams.TryGetValue(id, out Team team) ? team :
        Team.UnlinkedTeam);
    }

    /// <summary>
    /// Match a <see cref="Team"/> by its id.
    /// Sets <paramref name="team"/> to <see cref="Team.UnlinkedTeam"/> if not found.
    /// Returns if team returns is not the unlinked team (was found).
    /// </summary>
    public bool GetTeamById(Guid id, out Team team)
    {
      team = GetTeamById(id);
      return !team.Equals(Team.UnlinkedTeam);
    }

    /// <summary>
    /// Launch an address in a separate internet browser.
    /// </summary>
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