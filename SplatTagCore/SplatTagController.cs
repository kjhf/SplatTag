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
      else if (loadedPlayers.Length == 0)
      {
        Console.WriteLine("... nothing loaded.");
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
    /// Match a query to players with default options with all known players.
    /// </summary>
    public Player[] MatchPlayer(string query)
    {
      return MatchPlayer(query, new MatchOptions());
    }

    /// <summary>
    /// Match a query to players with given options with all known players.
    /// </summary>
    public Player[] MatchPlayer(string query, MatchOptions matchOptions)
    {
      return MatchPlayer(query, matchOptions, players.Values);
    }

    /// <summary>
    /// Match a query to players with given options from a set of players.
    /// </summary>
    public Player[] MatchPlayer(string query, MatchOptions matchOptions, ICollection<Player> playersToSearch)
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
              foreach (string slug in p.BattlefySlugs)
              {
                string toMatch = (matchOptions.NearCharacterRecognition) ? slug.TransformString() : slug;
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
              foreach (string name in p.Names)
              {
                string toMatch = (matchOptions.NearCharacterRecognition) ? name.TransformString() : name;
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

            if ((filterOptions & FilterOptions.Twitch) != 0 && p.Twitch != null)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? p.Twitch.TransformString() : p.Twitch;
              if (regex.IsMatch(toMatch))
              {
                relevance++;
              }
            }

            if ((filterOptions & FilterOptions.Twitter) != 0 && p.Twitter != null)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? p.Twitter.TransformString() : p.Twitter;
              if (regex.IsMatch(toMatch))
              {
                relevance++;
              }
            }

            if ((filterOptions & FilterOptions.Sources) != 0)
            {
              foreach (string name in p.Sources)
              {
                string toMatch = (matchOptions.NearCharacterRecognition) ? name.TransformString() : name;
                if (regex.IsMatch(toMatch))
                {
                  relevance++;
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
            foreach (string slug in p.BattlefySlugs)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? slug.TransformString() : slug;
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
            foreach (string name in p.Names)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? name.TransformString() : name;
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

          if ((filterOptions & FilterOptions.Twitch) != 0 && p.Twitch != null)
          {
            string toMatch = (matchOptions.NearCharacterRecognition) ? p.Twitch.TransformString() : p.Twitch;
            if (toMatch.Equals(query, comparion))
            {
              relevance += 10; // Give it more relevance
            }
            else if (toMatch.Contains(query, comparion))
            {
              relevance++;
            }
          }

          if ((filterOptions & FilterOptions.Twitter) != 0 && p.Twitter != null)
          {
            string toMatch = (matchOptions.NearCharacterRecognition) ? p.Twitter.TransformString() : p.Twitter;
            if (toMatch.Equals(query, comparion))
            {
              relevance += 10; // Give it more relevance
            }
            else if (toMatch.Contains(query, comparion))
            {
              relevance++;
            }
          }

          if ((filterOptions & FilterOptions.Sources) != 0)
          {
            foreach (string name in p.Sources)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? name.TransformString() : name;
              if (toMatch.Contains(query, comparion))
              {
                relevance++;
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
    public Team[] MatchTeam(string query)
    {
      return MatchTeam(query, new MatchOptions());
    }

    /// <summary>
    /// Match a query to teams with given options with all known teams.
    /// </summary>
    public Team[] MatchTeam(string query, MatchOptions matchOptions)
    {
      return MatchTeam(query, matchOptions, teams.Values);
    }

    /// <summary>
    /// Match a query to teams with given options with all known teams.
    /// </summary>
    public Team[] MatchTeam(string query, MatchOptions matchOptions, ICollection<Team> teamsToSearch)
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
              foreach (string tag in t.ClanTags)
              {
                string toMatch = (matchOptions.NearCharacterRecognition) ? tag.TransformString() : tag;
                if (regex.IsMatch(toMatch))
                {
                  // Put Clan Tag matches first
                  return int.MaxValue;
                }
              }
            }

            if ((filterOptions & FilterOptions.Name) != 0 && t.Name != null)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? t.SearchableName : t.Name;
              if (regex.IsMatch(toMatch))
              {
                relevance++;
              }
            }

            if ((filterOptions & FilterOptions.Sources) != 0 && t.Sources != null)
            {
              foreach (string name in t.Sources)
              {
                string toMatch = (matchOptions.NearCharacterRecognition) ? name.TransformString() : name;
                if (regex.IsMatch(toMatch))
                {
                  relevance++;
                }
              }
            }

            if ((filterOptions & FilterOptions.Twitter) != 0 && t.Twitter != null)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? t.Twitter.TransformString() : t.Twitter;
              if (regex.IsMatch(toMatch))
              {
                relevance++;
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
            foreach (string tag in t.ClanTags)
            {
              string toMatch = (matchOptions.NearCharacterRecognition) ? tag.TransformString() : tag;
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
            string toMatch = (matchOptions.NearCharacterRecognition) ? t.SearchableName : t.Name;
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
            foreach (string name in t.Sources)
            {
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

          if ((filterOptions & FilterOptions.Twitter) != 0 && t.Twitter != null)
          {
            string toMatch = (matchOptions.NearCharacterRecognition) ? t.Twitter.TransformString() : t.Twitter;
            if (toMatch.Equals(query, comparion))
            {
              relevance += 10; // Give it more relevance
            }
            else if (toMatch.Contains(query, comparion))
            {
              relevance++;
            }
          }

          return relevance;
        };
      }

      return teamsToSearch.Select(t => (t, func(t))).Where(pair => pair.Item2 > 0).OrderByDescending(pair => pair.Item2).Select(pair => pair.t).ToArray();
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
      if (id == Team.NoTeam.Id)
      {
        return Team.NoTeam;
      }
      bool matched = teams.TryGetValue(id, out Team found);
      return matched ? found : null;
    }

    /// <summary>Launch the team's Twitter account if it exists.</summary>
    public bool TryLaunchTwitter(Team t) => TryLaunchAddress(t.Twitter);

    /// <summary>Launch the Player's Twitter account if it exists.</summary>
    public bool TryLaunchTwitter(Player p) => TryLaunchAddress(p.Twitter);

    /// <summary> Launch an address in a separate internet browser. </summary>
    public bool TryLaunchAddress(string link)
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