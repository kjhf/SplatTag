using SplatTagCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplatTagDatabase
{
  internal static class Merger
  {
    /// <summary>
    /// Logging string builder used when merging.
    /// </summary>
    private static readonly StringBuilder logger = new StringBuilder();

    public static void FinalisePlayers(IList<Player> playersToMutate)
    {
      if (playersToMutate == null) return;

      string logMessage = $"Beginning {nameof(FinalisePlayers)} on {playersToMutate.Count} entries.";
      logger.AppendLine(logMessage);
      Trace.WriteLine(logMessage);

      string lastProgressBar = "";
      for (int i = playersToMutate.Count - 1; i >= 0; --i)
      {
        var newerPlayerRecord = playersToMutate[i];

        // First, try match through persistent information only.
        Player foundPlayer = null;
        for (int j = 0; j < i; ++j)
        {
          var olderPlayerRecord = playersToMutate[j];
          if (PlayersMatch(olderPlayerRecord, newerPlayerRecord, FilterOptions.Persistent, logger))
          {
            foundPlayer = olderPlayerRecord;
          }
        }

        // If that doesn't work, try and match a name and same team.
        if (foundPlayer == null)
        {
          for (int j = 0; j < i; ++j)
          {
            var olderPlayerRecord = playersToMutate[j];
            if (PlayersMatch(olderPlayerRecord, newerPlayerRecord, FilterOptions.Name, logger))
            {
              foundPlayer = olderPlayerRecord;
            }
          }
        }

        // If a player has now been found, merge it.
        if (foundPlayer != null)
        {
          foundPlayer.Merge(newerPlayerRecord);
          playersToMutate.RemoveAt(i);
        }

        string progressBar = Util.GetProgressBar(playersToMutate.Count - i, playersToMutate.Count, 100);
        if (!progressBar.Equals(lastProgressBar))
        {
          Trace.WriteLine(progressBar);
          lastProgressBar = progressBar;
        }
      }

      logMessage = $"Finished {nameof(FinalisePlayers)} with {playersToMutate.Count} entries.";
      logger.AppendLine(logMessage);
      Trace.WriteLine(logMessage);
    }

    public static void DumpLogger()
    {
      string path = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "_MergeResult.txt";
      Trace.WriteLine("Writing merge result log to " + Path.GetFullPath(path));

      try
      {
        File.WriteAllText(path, logger.ToString(), Encoding.UTF8);
      }
      catch (Exception ex)
      {
        Trace.WriteLine("ERROR: Failed to write the merge log. " + ex);
      }
      logger.Clear();
    }

    public static void CorrectTeamIdsForPlayers(ICollection<Player> incomingPlayers, IDictionary<Guid, Guid> teamsMergeResult)
    {
      if (incomingPlayers == null || teamsMergeResult == null || incomingPlayers.Count == 0 || teamsMergeResult.Count == 0) return;

      logger.Append(nameof(CorrectTeamIdsForPlayers)).Append(" called with ").Append(incomingPlayers.Count).AppendLine(" entries.");
      logger.AppendLine("Entries: ");
      foreach (var resultPair in teamsMergeResult)
      {
        logger.Append('[').Append(resultPair.Key).Append("] --> ").Append(resultPair.Value).AppendLine();
      }

      // For each team, correct the id as specified.
      Parallel.ForEach(incomingPlayers, (importPlayer) =>
      {
        bool hasChanges = false;
        Guid[] teams = importPlayer.Teams.ToArray();
        for (int i = 0; i < teams.Length; i++)
        {
          var thisTeam = teams[i];
          if (teamsMergeResult.ContainsKey(thisTeam))
          {
            teams[i] = teamsMergeResult[thisTeam];
            hasChanges = true;
          }
        }

        if (hasChanges)
        {
          importPlayer.Teams = teams;
        }
      });
    }

    /// <summary>
    /// Merge the loaded players into the current players list.
    /// </summary>
    public static void MergePlayers(ICollection<Player> playersToMutate, IEnumerable<Player> incomingPlayers)
    {
      if (playersToMutate == null || incomingPlayers == null) return;

      // Add if the player is new (by name) and assign them with a new id
      // Otherwise, match the found team with its id, based on name.
      ConcurrentBag<Player> playersToAdd = new ConcurrentBag<Player>();
      ConcurrentBag<(Player, Player)> playersToMerge = new ConcurrentBag<(Player, Player)>();

      Parallel.ForEach(incomingPlayers, (importPlayer) =>
      {
        try
        {
          // First, try match through persistent information only.
          // If that doesn't work, try and match a name and same team.
          Player foundPlayer =
            playersToMutate.FirstOrDefault(p => PlayersMatch(importPlayer, p, FilterOptions.Persistent, logger))
            ?? playersToMutate.FirstOrDefault(p => PlayersMatch(importPlayer, p, FilterOptions.Name, logger));

          if (foundPlayer == null)
          {
            playersToAdd.Add(importPlayer);
          }
          else
          {
            playersToMerge.Add((foundPlayer, importPlayer));
          }
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine($"Error in importing player {importPlayer}: {ex}");
        }
      });

      foreach (Player importPlayer in playersToAdd)
      {
        playersToMutate.Add(importPlayer);
      }

      foreach (var (foundPlayer, importPlayer) in playersToMerge)
      {
        foundPlayer.Merge(importPlayer);
      }
    }

    /// <summary>
    /// Find a player that matches another instance through their persistent information.
    /// </summary>
    /// <param name="playersToMutate">The players to search</param>
    /// <param name="testPlayer">The player instance to try and find</param>
    /// <returns>The matched player, or null if new</returns>
    private static Player FindSamePlayerPersistent(IEnumerable<Player> playersToMutate, Player testPlayer)
    {
      FilterOptions matchOptions = FilterOptions.None;
      if (!string.IsNullOrEmpty(testPlayer.FriendCode))
      {
        matchOptions |= FilterOptions.FriendCode;
      }
      if (!string.IsNullOrEmpty(testPlayer.DiscordName))
      {
        matchOptions |= FilterOptions.DiscordName;
      }
      if (testPlayer.DiscordId != null)
      {
        matchOptions |= FilterOptions.DiscordId;
      }
      if (testPlayer.Twitch != null)
      {
        matchOptions |= FilterOptions.Twitch;
      }
      if (testPlayer.Twitter != null)
      {
        matchOptions |= FilterOptions.Twitter;
      }
      if (testPlayer.BattlefySlugs?.Count > 0)
      {
        matchOptions |= FilterOptions.BattlefySlugs;
      }
      if (testPlayer.BattlefyUsername != null)
      {
        matchOptions |= FilterOptions.BattlefyUsername;
      }

      return playersToMutate.FirstOrDefault(p => PlayersMatch(testPlayer, p, matchOptions, logger));
    }

    /// <summary>
    /// Merge the loaded teams into the current teams list.
    /// </summary>
    /// <returns>
    /// A dictionary of merged team ids keyed by initial with values of the new id.
    /// </returns>
    public static IDictionary<Guid, Guid> MergeTeams(ICollection<Team> teamsToMutate, IEnumerable<Team> incomingTeams)
    {
      ConcurrentDictionary<Guid, Guid> mergeResult = new ConcurrentDictionary<Guid, Guid>();

      if (incomingTeams == null)
      {
        return mergeResult;
      }

      if (teamsToMutate == null)
      {
        teamsToMutate = new List<Team>();
      }

      if (teamsToMutate?.Count > 0)
      {
        // Construct a SearchableNames lookup of teams.
        Dictionary<string, Team> transformedTeamNames = new Dictionary<string, Team>();
        foreach (var importTeam in teamsToMutate)
        {
          try
          {
            transformedTeamNames.Add(importTeam.SearchableName, importTeam);
          }
          catch (ArgumentException)
          {
            // If this team name already exists, merge the teams.
            var existingTeam = transformedTeamNames[importTeam.SearchableName];
            MergeExistingTeam(mergeResult, importTeam, existingTeam);
          }
        }

        // Add if the team is new (by name) and assign them with a new id
        // Otherwise, match the found team with its id, based on name.
        foreach (Team importTeam in incomingTeams)
        {
          // Replace spaces because people adding tags or starting with space messes up same-name detection.
          // Also transform the team name.
          if (transformedTeamNames.TryGetValue(importTeam.SearchableName, out Team existingTeam))
          {
            MergeExistingTeam(mergeResult, importTeam, existingTeam);
          }
          else
          {
            teamsToMutate.Add(importTeam);
          }
        }
      }
      else
      {
        // No merge required, just take as-is.
        foreach (Team importTeam in incomingTeams)
        {
          teamsToMutate.Add(importTeam);
        }
      }

      return mergeResult;
    }

    private static void MergeExistingTeam(ConcurrentDictionary<Guid, Guid> mergeResult, Team importTeam, Team existingTeam)
    {
      mergeResult.TryAdd(importTeam.Id, existingTeam.Id);
      existingTeam.Merge(importTeam);
    }

    /// <summary>
    /// Get if two Players match.
    /// </summary>
    /// <param name="first">First Player to match</param>
    /// <param name="second">Second Player to match</param>
    /// <param name="matchOptions">How to match</param>
    /// <param name="logger">Logger to write to (or null to not write)</param>
    /// <returns>Players are equal based on the match options</returns>
    public static bool PlayersMatch(Player first, Player second, FilterOptions matchOptions, StringBuilder logger = null)
    {
      // Test if the Discord Ids match.
      if ((matchOptions & FilterOptions.DiscordId) != 0 && second.DiscordId?.Equals(first.DiscordId) == true)
      {
        // They do.
        logger?.Append(nameof(PlayersMatch))
              .Append(": Matched player ")
              .Append(first.ToString())
              .Append(" (Id ")
              .Append(first.Id)
              .Append(") with Discord Id ")
              .Append(second.DiscordId)
              .Append(" from player ")
              .Append(second)
              .Append(" (Id ")
              .Append(second.Id)
              .AppendLine(").");
        return true;
      }

      // Test if the Battlefy Usernames match.
      if ((matchOptions & FilterOptions.BattlefyUsername) != 0 && second.BattlefyUsername?.Equals(first.BattlefyUsername) == true)
      {
        // They do.
        logger?.Append(nameof(PlayersMatch))
              .Append(": Matched player ")
              .Append(first.ToString())
              .Append(" (Id ")
              .Append(first.Id)
              .Append(") with BattlefyUsername ")
              .Append(first.BattlefyUsername)
              .Append(" from player ")
              .Append(second)
              .Append(" (Id ")
              .Append(second.Id)
              .AppendLine(").");
        return true;
      }

      // Test if any of the Battlefy Slugs match.
      if ((matchOptions & FilterOptions.BattlefySlugs) != 0 && second.BattlefySlugs.Intersect(first.BattlefySlugs).Any())
      {
        // They do.
        logger?.Append(nameof(PlayersMatch))
              .Append(": Matched player ")
              .Append(first.ToString())
              .Append(" (Id ")
              .Append(first.Id)
              .Append(") with test BattlefySlug(s) e.g. ")
              .Append(first.BattlefySlugs.FirstOrDefault())
              .Append(" with player ")
              .Append(second)
              .Append(" (Id ")
              .Append(second.Id)
              .AppendLine(").");
        return true;
      }

      // Test if the Switch FC's match.
      if ((matchOptions & FilterOptions.FriendCode) != 0 && second.FriendCode?.Equals(first.FriendCode) == true)
      {
        // They do.
        logger?.Append(nameof(PlayersMatch))
              .Append(": Matched player ")
              .Append(first.ToString())
              .Append(" (Id ")
              .Append(first.Id)
              .Append(") with FC ")
              .Append(first.FriendCode)
              .Append(" from player ")
              .Append(second)
              .Append(" (Id ")
              .Append(second.Id)
              .AppendLine(").");
        return true;
      }

      // Test if the Twitches match.
      if ((matchOptions & FilterOptions.Twitch) != 0 && second.Twitch?.Equals(first.Twitch) == true)
      {
        // They do.
        logger?.Append(nameof(PlayersMatch))
              .Append(": Matched player ")
              .Append(first.ToString())
              .Append(" (Id ")
              .Append(first.Id)
              .Append(") with Twitch ")
              .Append(first.Twitch)
              .Append(" from player ")
              .Append(second)
              .Append(" (Id ")
              .Append(second.Id)
              .AppendLine(").");
        return true;
      }

      // Test if the Twitters match.
      if ((matchOptions & FilterOptions.Twitter) != 0 && second.Twitter?.Equals(first.Twitter) == true)
      {
        // They do.
        logger?.Append(nameof(PlayersMatch))
              .Append(": Matched player ")
              .Append(first.ToString())
              .Append(" (Id ")
              .Append(first.Id)
              .Append(") with Twitter ")
              .Append(first.Twitter)
              .Append(" from player ")
              .Append(second)
              .Append(" (Id ")
              .Append(second.Id)
              .AppendLine(").");
        return true;
      }

      // Test if the Discord names match.
      if ((matchOptions & FilterOptions.DiscordName) != 0 && second.DiscordName?.Equals(first.DiscordName) == true)
      {
        // They do.
        logger?.Append(nameof(PlayersMatch))
              .Append(": Matched player ")
              .Append(first.ToString())
              .Append(" (Id ")
              .Append(first.Id)
              .Append(") with Discord name ")
              .Append(first.DiscordName)
              .Append(" from player ")
              .Append(second)
              .Append(" (Id ")
              .Append(second.Id)
              .AppendLine(").");
        return true;
      }

      if ((matchOptions & FilterOptions.Name) != 0
        && first.Teams.Intersect(second.Teams).Any()
        && first.TransformedNames.Intersect(second.TransformedNames).Any())
      {
        logger.Append(nameof(PlayersMatch))
              .Append(": Matched player ")
              .Append(first.ToString())
              .Append(" (Id ")
              .Append(first.Id)
              .Append(") with transformed names [")
              .Append(string.Join(", ", first.TransformedNames))
              .Append("] from player ")
              .Append(second)
              .Append(" (Id ")
              .Append(second.Id)
              .Append(") with transformed names [")
              .Append(string.Join(", ", second.TransformedNames))
              .AppendLine("].");
        return true;
      }

      return false;
    }
  }
}