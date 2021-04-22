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
    /// <returns>Any work done (true, should loop) or No work done (false, can stop)</returns>
    public static bool FinalisePlayers(List<Player> playersToMutate, TextWriter? logger = null)
    {
      bool workDone = false;
      if (playersToMutate == null) return workDone;
      var indexesToRemove = new List<int>();

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
            // Quick check that the player is definitely older
            if (olderPlayerRecord.CompareToBySourceChronology(newerPlayerRecord) == 1)
            {
              // Swap the instances round
              logger?.WriteLine($"Newer player is not newer, swapping {newerPlayerRecord} with {olderPlayerRecord}.");
              (newerPlayerRecord, olderPlayerRecord) = (playersToMutate[i], playersToMutate[j]) = (olderPlayerRecord, newerPlayerRecord);
            }

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
              // Quick check that the player is definitely older
              if (olderPlayerRecord.CompareToBySourceChronology(newerPlayerRecord) == 1)
              {
                // Swap the instances round
                logger?.WriteLine($"Newer player is not newer, swapping {newerPlayerRecord} with {olderPlayerRecord}.");
                (newerPlayerRecord, olderPlayerRecord) = (playersToMutate[i], playersToMutate[j]) = (olderPlayerRecord, newerPlayerRecord);
              }

              foundPlayer = olderPlayerRecord;
              break;
            }
          }
        }

        // If a player has now been found, merge it.
        if (foundPlayer != null)
        {
          logger?.WriteLine($"Merging player {newerPlayerRecord} from sources [{string.Join(", ", newerPlayerRecord.Sources)}] into {foundPlayer} from sources [{string.Join(", ", foundPlayer.Sources)}].");
          foundPlayer.Merge(newerPlayerRecord);
          indexesToRemove.Add(i);
          workDone = true;
        }

        string progressBar = Util.GetProgressBar(playersToMutate.Count - i, playersToMutate.Count, 100);
        if (!progressBar.Equals(lastProgressBar))
        {
          if (logger != null)
          {
            logger.WriteLine(progressBar);
          }
          else
          {
            Console.WriteLine(progressBar);
          }
          lastProgressBar = progressBar;
        }
      }

      logger?.WriteLine($"{nameof(FinalisePlayers)}: Remaking players list {playersToMutate.Count} --> {playersToMutate.Count - indexesToRemove.Count} entries.");
      playersToMutate.RemoveAtRange(indexesToRemove);

      logMessage = $"Finished {nameof(FinalisePlayers)} with {playersToMutate.Count} entries. Work done: {workDone}";
      logger?.WriteLine(logMessage);
      return workDone;
    }

    /// <summary>
    /// Final time-consuming call to look at all team entries and merge where appropriate.
    /// </summary>
    /// <returns>
    /// A dictionary of merged team ids keyed by the newest id to become the value of the pre-known id.
    /// Empty dictionary = no work done.
    /// </returns>
    public static IDictionary<Guid, Guid> FinaliseTeams(IReadOnlyCollection<Player> allPlayers, List<Team> teamsToMutate, TextWriter? logger = null)
    {
      var mergeResult = new Dictionary<Guid, Guid>();
      var indexesToRemove = new List<int>();

      logger?.WriteLine($"Beginning {nameof(FinaliseTeams)} on {teamsToMutate.Count} entries.");

      string lastProgressBar = "";
      for (int i = teamsToMutate.Count - 1; i >= 0; --i)
      {
        var newerTeamRecord = teamsToMutate[i];

        // Try match teams.
        Team? foundOlderTeamRecord = null;
        for (int j = 0; j < i; ++j)
        {
          var olderTeamRecord = teamsToMutate[j];

          if (Matcher.TeamsMatch(allPlayers, olderTeamRecord, newerTeamRecord, logger))
          {
            // Quick check that the team is definitely older
            if (olderTeamRecord.CompareToBySourceChronology(newerTeamRecord) == 1)
            {
              // Swap the instances round
              logger?.WriteLine($"Newer team is not newer, swapping {newerTeamRecord} with {olderTeamRecord}.");
              (newerTeamRecord, olderTeamRecord) = (teamsToMutate[i], teamsToMutate[j]) = (olderTeamRecord, newerTeamRecord);
            }

            foundOlderTeamRecord = olderTeamRecord;
            break;
          }
        }

        // If an older team was found, merge it.
        if (foundOlderTeamRecord != null)
        {
          logger?.WriteLine($"Merging newer team {newerTeamRecord} into {foundOlderTeamRecord} and deleting index [{i}].");
          MergeExistingTeam(mergeResult, newerTeamRecord, foundOlderTeamRecord);

          // Remove the newer record (the older record persists)
          indexesToRemove.Add(i);
          logger?.WriteLine($"Resultant team: {foundOlderTeamRecord}.");
        }

        string progressBar = Util.GetProgressBar(teamsToMutate.Count - i, teamsToMutate.Count, 100);
        if (!progressBar.Equals(lastProgressBar))
        {
          if (logger != null)
          {
            logger.WriteLine(progressBar);
          }
          else
          {
            Console.WriteLine(progressBar);
          }
          lastProgressBar = progressBar;
        }
      }

      logger?.WriteLine($"{nameof(FinaliseTeams)}: Remaking teams list {teamsToMutate.Count} --> {teamsToMutate.Count - indexesToRemove.Count} entries.");
      teamsToMutate.RemoveAtRange(indexesToRemove);

      logger?.WriteLine($"Finished {nameof(FinaliseTeams)} with {teamsToMutate.Count} entries.");
      return mergeResult;
    }

    /// <summary>
    /// Remove multiple indicies
    /// With thanks to https://stackoverflow.com/questions/63495264/how-can-i-efficiently-remove-elements-by-index-from-a-very-large-list
    /// </summary>
    internal static void RemoveAtRange<T>(this List<T> values, List<int> indicies)
    {
      if (indicies.Count == 0)
      {
        return;
      }
      else if (indicies.Count == 1)
      {
        values.RemoveAt(indicies[0]);
      }
      else
      {
        indicies.Sort();

        int sourceStartIndex = 0;
        int skipCount = 0;

        int destStartIndex;
        int spanLength;

        // Copy items up to last index to be skipped
        foreach (var skipIndex in indicies)
        {
          spanLength = skipIndex - sourceStartIndex;
          destStartIndex = sourceStartIndex - skipCount;

          for (int i = sourceStartIndex; i < sourceStartIndex + spanLength; i++)
          {
            values[destStartIndex] = values[i];
            destStartIndex++;
          }

          sourceStartIndex = skipIndex + 1;
          skipCount++;
        }

        // Copy remaining items (between last index to be skipped and end of list)
        spanLength = values.Count - sourceStartIndex;
        destStartIndex = sourceStartIndex - skipCount;

        for (int i = sourceStartIndex; i < sourceStartIndex + spanLength; i++)
        {
          values[destStartIndex] = values[i];
          destStartIndex++;
        }

        values.RemoveRange(destStartIndex, indicies.Count);
      }
    }

    /// <summary>
    /// Merge the loaded players into the current players list.
    /// </summary>
    internal static void MergePlayers(List<Player> playersToMutate, IEnumerable<Player> incomingPlayers, TextWriter? logger = null)
    {
      if (playersToMutate == null || incomingPlayers == null) return;

      // Add if the player is new (by name) and assign them with a new id
      // Otherwise, match the found team with its id, based on name.
      ConcurrentBag<Player> concurrentPlayersToAdd = new ConcurrentBag<Player>();
      ConcurrentBag<(Player, Player)> concurrentPlayersToMerge = new ConcurrentBag<(Player, Player)>();

      Parallel.ForEach(incomingPlayers, (importPlayer) =>
      {
        try
        {
          // First, try match through persistent information only.
          // If that doesn't work, try and match a name and same team.
          Player foundPlayer =
            playersToMutate.Find(p => Matcher.PlayersMatch(importPlayer, p, FilterOptions.Persistent, logger))
            ?? playersToMutate.Find(p => Matcher.PlayersMatch(importPlayer, p, FilterOptions.Name, logger));

          if (foundPlayer == null)
          {
            concurrentPlayersToAdd.Add(importPlayer);
          }
          else
          {
            concurrentPlayersToMerge.Add((foundPlayer, importPlayer));
          }
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine($"Error in importing player {importPlayer}: {ex}");
        }
      });

      playersToMutate.AddRange(concurrentPlayersToAdd);

      foreach (var (foundPlayer, importPlayer) in concurrentPlayersToMerge)
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
    public static IDictionary<Guid, Guid> MergeTeamsByPersistentIds(List<Team> teamsToMutate, IEnumerable<Team> incomingTeams)
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
        ConcurrentBag<Team> concurrentTeamsToAdd = new ConcurrentBag<Team>();

        // Merge teams based on the Battlefy Persistent Id.
        Parallel.ForEach(incomingTeams, importTeam =>
        {
          if (importTeam.BattlefyPersistentTeamId != null)
          {
            var foundTeam = teamsToMutate.Find(t => importTeam.BattlefyPersistentTeamId.Value.Equals(t?.BattlefyPersistentTeamId?.Value));
            if (foundTeam != null)
            {
              MergeExistingTeam(mergeResult, importTeam, foundTeam);
            }
            else
            {
              concurrentTeamsToAdd.Add(importTeam);
            }
          }
          else
          {
            concurrentTeamsToAdd.Add(importTeam);
          }
        });

        teamsToMutate.AddRange(concurrentTeamsToAdd);
      }
      else
      {
        // No merge required, just take as-is.
        teamsToMutate.AddRange(incomingTeams);
      }

      return mergeResult;
    }

    /// <summary>
    /// Merge two existing teams and add to the merge result dictionary.
    /// </summary>
    private static void MergeExistingTeam(IDictionary<Guid, Guid> mergeResult, Team newerTeam, Team olderTeam)
    {
      // Newer id to become the older team's id
      mergeResult.Add(newerTeam.Id, olderTeam.Id);
      olderTeam.Merge(newerTeam);

      // If the merge result is already pointing to the newer team, we need to update it to the new older team
      foreach (var pair in mergeResult.Where(p => p.Value == newerTeam.Id).ToArray())
      {
        mergeResult[pair.Key] = olderTeam.Id;
      }
    }
  }
}