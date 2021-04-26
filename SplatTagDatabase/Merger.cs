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
    /// Perform the final merge.
    /// </summary>
    internal static void FinalMerge(List<Player> players, List<Team> teams, TextWriter? logger = null)
    {
      try
      {
        bool workDone = true;
        for (int iteration = 1; workDone && iteration < 20; iteration++)
        {
          Console.WriteLine($"Performing final merge (iteration #{iteration})...");
          workDone = FinalisePlayers(players, logger);
          var mergeResult = FinaliseTeams(players, teams, logger);
          CorrectTeamIdsForPlayers(players, mergeResult, logger);
          workDone |= mergeResult.Count != 0;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"ERROR: Failed {nameof(Merger.FinalisePlayers)}. Continuing anyway. {ex}");
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
      // If we have a lot of work, fire up a parallel for-each, otherwise just do a regular one.
      if (incomingPlayers.Count > 15)
      {
        Parallel.ForEach(incomingPlayers, (importPlayer) => importPlayer.CorrectTeamIds(teamsMergeResult));
      }
      else
      {
        foreach (var importPlayer in incomingPlayers)
        {
          importPlayer.CorrectTeamIds(teamsMergeResult);
        }
      }
    }

    /// <summary>
    /// Final time-consuming call to look at all player entries and merge where appropriate.
    /// </summary>
    /// <param name="playersToMutate">List of players to change</param>
    /// <returns>Any work done (true, should loop) or No work done (false, can stop)</returns>
    public static bool FinalisePlayers(List<Player> playersToMutate, TextWriter? logger = null)
    {
      bool workDone = false;
      if (playersToMutate == null) return workDone;

      var indexesToRemove = new List<int>();

      string logMessage = $"Beginning {nameof(FinalisePlayers)} on {playersToMutate.Count} entries.";
      logger?.WriteLine(logMessage);

      int lastProgressBars = -1;
      int length = playersToMutate.Count - 1;
      for (int i = length; i >= 0; --i)
      {
        // This is the player record that we are modifying
        var newerPlayerRecord = playersToMutate[i];

        // First, try match through persistent information only.
        Player? foundOlderPlayerRecord = null;

        for (int j = 0; j < i; ++j)
        {
          // This is the player record that we are checking against.
          var olderPlayerRecord = playersToMutate[j];

          if (Matcher.PlayersMatch(olderPlayerRecord, newerPlayerRecord, FilterOptions.Persistent, logger))
          {
            // Quick check that the player is definitely older
            // This happens when a merge has already happened, but the dates then go out of order.
            if (olderPlayerRecord.CompareToBySourceChronology(newerPlayerRecord) == 1)
            {
              // Swap the instances round
              logger?.WriteLine($"Newer player is not newer, swapping {newerPlayerRecord} with {olderPlayerRecord}.");
              (newerPlayerRecord, olderPlayerRecord) = (playersToMutate[i], playersToMutate[j]) = (olderPlayerRecord, newerPlayerRecord);
            }

            foundOlderPlayerRecord = olderPlayerRecord;
            break;
          }
          // else
          // If that doesn't work, try and match a name and same team.
          if (foundOlderPlayerRecord == null && Matcher.PlayersMatch(olderPlayerRecord, newerPlayerRecord, FilterOptions.Name, logger))
          {
            if (olderPlayerRecord.CompareToBySourceChronology(newerPlayerRecord) == 1)
            {
              logger?.WriteLine($"Newer player is not newer, swapping {newerPlayerRecord} with {olderPlayerRecord}.");
              (newerPlayerRecord, olderPlayerRecord) = (playersToMutate[i], playersToMutate[j]) = (olderPlayerRecord, newerPlayerRecord);
            }

            foundOlderPlayerRecord = olderPlayerRecord;
            // no break -- we want to continue searching all Persistent records first
            // however by assigning foundPlayer, we've earmarked this entry so we don't have to iterate again
          }
        }

        // If a player has now been found, merge it.
        if (foundOlderPlayerRecord != null)
        {
          logger?.WriteLine($"Merging player {newerPlayerRecord} from sources [{string.Join(", ", newerPlayerRecord.Sources)}] into {foundOlderPlayerRecord} from sources [{string.Join(", ", foundOlderPlayerRecord.Sources)}].");
          foundOlderPlayerRecord.Merge(newerPlayerRecord);
          indexesToRemove.Add(i);
          workDone = true;
        }

        // -i because we're counting backwards
        int progressBars = ProgressBar.CalculateProgressBars(playersToMutate.Count - i, playersToMutate.Count, 100);
        if (progressBars != lastProgressBars)
        {
          string progressBar = ProgressBar.GetProgressBar(progressBars, 100, false) + " " + i + "/" + playersToMutate.Count;
          if (logger != null)
          {
            logger.WriteLine(progressBar);
          }
          else
          {
            Console.WriteLine(progressBar);
          }
          lastProgressBars = progressBars;
        }
      }

      logger?.WriteLine($"{nameof(FinalisePlayers)}: Remaking players list {playersToMutate.Count} --> {playersToMutate.Count - indexesToRemove.Count} entries.");
      playersToMutate.RemoveAtRange(indexesToRemove);
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

      int lastProgressBars = -1;
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

        int progressBars = ProgressBar.CalculateProgressBars(teamsToMutate.Count - i, teamsToMutate.Count, 100);
        if (progressBars != lastProgressBars)
        {
          string progressBar = ProgressBar.GetProgressBar(progressBars, 100, false) + " " + i + "/" + teamsToMutate.Count;
          if (logger != null)
          {
            logger.WriteLine(progressBar);
          }
          else
          {
            Console.WriteLine(progressBar);
          }
          lastProgressBars = progressBars;
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

      if (indicies.Count == 1)
      {
        values.RemoveAt(indicies[0]);
        return;
      }

      // Otherwise
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

    /// <summary>
    /// Merge the loaded players into the current players list.
    /// </summary>
    internal static void MergePlayers(List<Player> playersToMutate, IReadOnlyCollection<Player> incomingPlayers, TextWriter? logger = null)
    {
      if (playersToMutate == null || incomingPlayers == null) return;

      // Add if the player is new (by name) and assign them with a new id
      // Otherwise, match the found team with its id, based on name.
      if (incomingPlayers.Count > 15)
      {
        logger?.WriteLine($"Merging {incomingPlayers.Count} Players in parallel.");
        MergePlayersParallel(playersToMutate, incomingPlayers, logger);
      }
      else
      {
        logger?.WriteLine($"Merging {incomingPlayers.Count} Players in series.");
        MergePlayersSerial(playersToMutate, incomingPlayers, logger);
      }
    }

    private static void MergePlayersParallel(List<Player> playersToMutate, IReadOnlyCollection<Player> incomingPlayers, TextWriter? logger)
    {
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

    private static void MergePlayersSerial(List<Player> playersToMutate, IReadOnlyCollection<Player> incomingPlayers, TextWriter? logger)
    {
      List<Player> playersToAdd = new List<Player>();
      List<(Player, Player)> playersToMerge = new List<(Player, Player)>();

      foreach (var importPlayer in incomingPlayers)
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
      }

      playersToMutate.AddRange(playersToAdd);

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
    public static IDictionary<Guid, Guid> MergeTeamsByPersistentIds(List<Team> teamsToMutate, IList<Team> incomingTeams)
    {
      ConcurrentDictionary<Guid, Guid> mergeResult = new ConcurrentDictionary<Guid, Guid>();

      if (incomingTeams == null || incomingTeams.Count == 0)
      {
        return mergeResult;
      }

      teamsToMutate ??= new List<Team>();

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