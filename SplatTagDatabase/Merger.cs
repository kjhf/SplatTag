using SplatTagCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SplatTagDatabase
{
  /// <summary>
  /// Merging functions
  /// </summary>
  internal static class Merger
  {
    /// <summary>
    /// Adds a new player to the players list with respect to their team and its tag.
    /// Returns the player that was added (or null if <paramref name="playerName"/> was null or empty).
    /// </summary>
    /// <param name="playerName">The player's name on the roster</param>
    /// <param name="newTeam">The new team, including its tag</param>
    /// <param name="players">Players list to add the player to</param>
    /// <param name="source">Source</param>
    public static Player? AddPlayerFromTag(string? playerName, Team newTeam, List<Player> players, Source source)
    {
      if (playerName != null && !string.IsNullOrWhiteSpace(playerName))
      {
        playerName = playerName.Trim();
        playerName = newTeam.Tag?.StripFromPlayer(playerName) ?? playerName;

        var p = new Player(playerName, new[] { newTeam.Id }, source);
        players.Add(p);
        return p;
      }
      else
      {
        return null;
      }
    }

    public static void CorrectTeamIdsForPlayers(ICollection<Player> incomingPlayers, IDictionary<Guid, Guid> teamsMergeResult, TextWriter? logger = null)
    {
      if (incomingPlayers == null || teamsMergeResult == null || incomingPlayers.Count == 0 || teamsMergeResult.Count == 0) return;

      if (logger != null)
      {
        logger.Write(nameof(CorrectTeamIdsForPlayers));
        logger.Write(" called with ");
        logger.Write(incomingPlayers.Count);
        logger.WriteLine(" entries.");
        logger.WriteLine("Entries: ");
        foreach (var resultPair in teamsMergeResult)
        {
          logger.Write('[');
          logger.Write(resultPair.Key);
          logger.Write("] --> ");
          logger.Write(resultPair.Value);
          logger.WriteLine();
        }
      }

      // For each team, correct the id as specified.
      Parallel.ForEach(incomingPlayers, (importPlayer) =>
      {
        importPlayer.CorrectTeamIds(teamsMergeResult);
      });
    }

    /// <summary>
    /// Final time-consuming call to look at all player entries and merge where appropriate.
    /// </summary>
    /// <param name="playersToMutate"></param>
    public static void FinalisePlayers(IList<Player> playersToMutate, TextWriter? logger = null)
    {
      if (playersToMutate == null) return;

      string logMessage = $"Beginning {nameof(FinalisePlayers)} on {playersToMutate.Count} entries.";
      logger?.WriteLine(logMessage);

      string lastProgressBar = "";
      for (int i = playersToMutate.Count - 1; i >= 0; --i)
      {
        var newerPlayerRecord = playersToMutate[i];

        // First, try match through persistent information only.
        Player? foundPlayer = null;
        for (int j = 0; j < i; ++j)
        {
          var olderPlayerRecord = playersToMutate[j];
          if (Matcher.PlayersMatch(olderPlayerRecord, newerPlayerRecord, FilterOptions.Persistent, logger))
          {
            foundPlayer = olderPlayerRecord;
            break;
          }
        }

        // If that doesn't work, try and match a name and same team.
        if (foundPlayer == null)
        {
          for (int j = 0; j < i; ++j)
          {
            var olderPlayerRecord = playersToMutate[j];
            if (Matcher.PlayersMatch(olderPlayerRecord, newerPlayerRecord, FilterOptions.Name, logger))
            {
              foundPlayer = olderPlayerRecord;
              break;
            }
          }
        }

        // If a player has now been found, merge it.
        if (foundPlayer != null)
        {
          logger?.WriteLine($"Merging player {newerPlayerRecord} with teams [{string.Join(", ", newerPlayerRecord.Teams)}] into {foundPlayer} with teams [{string.Join(", ", foundPlayer.Teams)}].");
          foundPlayer.Merge(newerPlayerRecord);
          playersToMutate.RemoveAt(i); // remove the newer record
        }

        string progressBar = Util.GetProgressBar(playersToMutate.Count - i, playersToMutate.Count, 100);
        if (!progressBar.Equals(lastProgressBar))
        {
          logger?.WriteLine(progressBar);
          lastProgressBar = progressBar;
        }
      }

      logMessage = $"Finished {nameof(FinalisePlayers)} with {playersToMutate.Count} entries.";
      logger?.WriteLine(logMessage);
    }

    /// <summary>
    /// Final time-consuming call to look at all team entries and merge where appropriate.
    /// </summary>
    /// <returns>
    /// A dictionary of merged team ids keyed by initial with values of the new id.
    /// </returns>
    public static IDictionary<Guid, Guid> FinaliseTeams(IReadOnlyCollection<Player> allPlayers, IList<Team> teamsToMutate, TextWriter? logger = null)
    {
      ConcurrentDictionary<Guid, Guid> mergeResult = new ConcurrentDictionary<Guid, Guid>();

      logger?.WriteLine($"Beginning {nameof(FinaliseTeams)} on {teamsToMutate.Count} entries.");

      string lastProgressBar = "";
      for (int i = teamsToMutate.Count - 1; i >= 0; --i)
      {
        var newerTeamRecord = teamsToMutate[i];

        // Try match teams.
        Team? foundTeam = null;
        for (int j = 0; j < i; ++j)
        {
          var olderTeamRecord = teamsToMutate[j];
          if (Matcher.TeamsMatch(allPlayers, olderTeamRecord, newerTeamRecord, logger))
          {
            foundTeam = olderTeamRecord;
            break;
          }
        }

        // If a teams has now been found, merge it.
        if (foundTeam != null)
        {
          MergeExistingTeam(mergeResult, newerTeamRecord, foundTeam);
          teamsToMutate.RemoveAt(i);
        }

        string progressBar = Util.GetProgressBar(teamsToMutate.Count - i, teamsToMutate.Count, 100);
        if (!progressBar.Equals(lastProgressBar))
        {
          logger?.WriteLine(progressBar);
          lastProgressBar = progressBar;
        }
      }

      logger?.WriteLine($"Finished {nameof(FinaliseTeams)} with {teamsToMutate.Count} entries.");
      return mergeResult;
    }

    /// <summary>
    /// Merge the loaded players into the current players list.
    /// </summary>
    public static void MergePlayers(ICollection<Player> playersToMutate, IEnumerable<Player> incomingPlayers, TextWriter? logger = null)
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
            playersToMutate.FirstOrDefault(p => Matcher.PlayersMatch(importPlayer, p, FilterOptions.Persistent, logger))
            ?? playersToMutate.FirstOrDefault(p => Matcher.PlayersMatch(importPlayer, p, FilterOptions.Name, logger));

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
    /// Merge the loaded teams into the current teams list.
    /// </summary>
    /// <returns>
    /// A dictionary of merged team ids keyed by initial with values of the new id.
    /// </returns>
    public static IDictionary<Guid, Guid> MergeTeamsByPersistentIds(ICollection<Team> teamsToMutate, IEnumerable<Team> incomingTeams)
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

      if (teamsToMutate.Count > 0)
      {
        // Merge teams based on the Battlefy Persistent Id.
        foreach (Team importTeam in incomingTeams)
        {
          if (importTeam.BattlefyPersistentTeamId != null)
          {
            var foundTeam = teamsToMutate.FirstOrDefault(t => importTeam.BattlefyPersistentTeamId.Value.Equals(t?.BattlefyPersistentTeamId?.Value));
            if (foundTeam != null)
            {
              MergeExistingTeam(mergeResult, importTeam, foundTeam);
            }
            else
            {
              teamsToMutate.Add(importTeam);
            }
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

    /// <summary>
    /// Find a player that matches another instance through their persistent information.
    /// </summary>
    /// <param name="playersToMutate">The players to search</param>
    /// <param name="testPlayer">The player instance to try and find</param>
    /// <returns>The matched player, or null if new</returns>
    private static Player FindSamePlayerPersistent(IEnumerable<Player> playersToMutate, Player testPlayer, TextWriter? logger = null)
    {
      FilterOptions matchOptions = FilterOptions.None;
      if (testPlayer.FC != FriendCode.NO_FRIEND_CODE)
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
      if (testPlayer.Battlefy?.Count > 0)
      {
        matchOptions |= FilterOptions.BattlefySlugs;
        matchOptions |= FilterOptions.BattlefyUsername;
      }

      return playersToMutate.FirstOrDefault(p => Matcher.PlayersMatch(testPlayer, p, matchOptions, logger));
    }

    private static void MergeExistingTeam(ConcurrentDictionary<Guid, Guid> mergeResult, Team newerTeam, Team olderTeam)
    {
      mergeResult.TryAdd(newerTeam.Id, olderTeam.Id);
      olderTeam.Merge(newerTeam);
    }
  }
}