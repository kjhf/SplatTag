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
    public static bool FinalisePlayers(IList<Player> playersToMutate, TextWriter? logger = null)
    {
      bool workDone = false;
      if (playersToMutate == null) return workDone;

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

      logMessage = $"Finished {nameof(FinalisePlayers)} with {playersToMutate.Count} entries. Work done: {workDone}";
      logger?.WriteLine(logMessage);
      return workDone;
    }

    /// <summary>
    /// Final time-consuming call to look at all team entries and merge where appropriate.
    /// </summary>
    /// <returns>
    /// A dictionary of merged team ids keyed by initial with values of the new id.
    /// Empty dictionary = no work done.
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
        Team? olderFoundTeam = null;
        for (int j = 0; j < i; ++j)
        {
          var olderTeamRecord = teamsToMutate[j];
          if (Matcher.TeamsMatch(allPlayers, olderTeamRecord, newerTeamRecord, logger))
          {
            olderFoundTeam = olderTeamRecord;
            break;
          }
        }

        // If a teams has now been found, merge it.
        if (olderFoundTeam != null)
        {
          logger?.WriteLine($"Merging newer team {newerTeamRecord} into {olderFoundTeam} and deleting index [{i}].");
          MergeExistingTeam(mergeResult, newerTeamRecord, olderFoundTeam);

          // Remove the newer record (the older record persists)
          teamsToMutate.RemoveAt(i);
          logger?.WriteLine($"Resultant team: {olderFoundTeam}.");
        }

        string progressBar = Util.GetProgressBar(teamsToMutate.Count - i, teamsToMutate.Count, 100);
        if (!progressBar.Equals(lastProgressBar))
        {
          logger?.WriteLine(progressBar);
          Console.WriteLine(progressBar);
          lastProgressBar = progressBar;
        }
      }

      logger?.WriteLine($"Finished {nameof(FinaliseTeams)} with {teamsToMutate.Count} entries.");
      return mergeResult;
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
            playersToMutate.AsParallel().FirstOrDefault(p => Matcher.PlayersMatch(importPlayer, p, FilterOptions.Persistent, logger))
            ?? playersToMutate.AsParallel().FirstOrDefault(p => Matcher.PlayersMatch(importPlayer, p, FilterOptions.Name, logger));

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

    private static void MergeExistingTeam(ConcurrentDictionary<Guid, Guid> mergeResult, Team newerTeam, Team olderTeam)
    {
      mergeResult.TryAdd(newerTeam.Id, olderTeam.Id);
      olderTeam.Merge(newerTeam);
    }
  }
}