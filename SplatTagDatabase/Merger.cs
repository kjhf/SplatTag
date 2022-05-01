using NLog;
using SplatTagCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace SplatTagDatabase
{
  /// <summary>
  /// Merging functions for the Core Types.
  /// </summary>
  internal static class Merger
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public static void CorrectTeamIdsForPlayers(ICollection<Player> incomingPlayers, IReadOnlyDictionary<Guid, Guid> teamsMergeResult)
    {
      if (incomingPlayers == null || teamsMergeResult == null || incomingPlayers.Count == 0 || teamsMergeResult.Count == 0) return;

      if (logger.IsDebugEnabled)
      {
        logger.ConditionalDebug($"{nameof(CorrectTeamIdsForPlayers)}: {incomingPlayers.Count} entries: ");
        foreach (var resultPair in teamsMergeResult)
        {
          logger.ConditionalDebug($"[{resultPair.Key}] --> {resultPair.Value}");
        }
      }

      // For each team, correct the id as specified.
      // If we have a lot of work, fire up a parallel for-each, otherwise just do a regular one.
      void action(Player importPlayer) => importPlayer.TeamInformation.CorrectTeamIds(teamsMergeResult);
      if (incomingPlayers.Count > 15)
      {
        Parallel.ForEach(incomingPlayers, action);
      }
      else
      {
        incomingPlayers.ForEach(action);
      }
    }

    /// <summary>
    /// Perform the final merge.
    /// </summary>
    public static (Player[], Team[]) MergeAllInParallel(IReadOnlyList<Player> players, IReadOnlyList<Team> teams)
    {
      try
      {
        bool workDone = true;
        List<Player> finalPlayers = new();
        List<Team> finalTeams = new();

        for (int iteration = 1; workDone && iteration < 20; iteration++)
        {
          logger.Info($"Performing final merge (iteration #{iteration})...");
          MergeRecordHandler playerMergeResult = MergeIntoList(players, ref finalPlayers, finalPlayers);
          MergeRecordHandler teamMergeResult = MergeIntoList(teams, ref finalTeams, finalPlayers);
          CorrectTeamIdsForPlayers(finalPlayers, teamMergeResult.FinalMergedIds);
          workDone |= (playerMergeResult.WorkDone || teamMergeResult.WorkDone);
        }
        return (finalPlayers.ToArray(), finalTeams.ToArray());
      }
      catch (Exception ex)
      {
        logger.Warn(ex, $"ERROR: Failed {nameof(MergeAllInParallel)}. Continuing anyway. {ex}");
      }
      return (Array.Empty<Player>(), Array.Empty<Team>());
    }

    [return: NotNullIfNotNull("incoming")]
    [return: NotNullIfNotNull("toMutate")]
    public static MergeRecordHandler? MergeIntoList<CoreType>(IReadOnlyCollection<CoreType> incoming, ref List<CoreType> toMutate, IReadOnlyCollection<Player>? knownPlayers)
      where CoreType : BaseSplatTagCoreObject<CoreType>
    {
      if (toMutate == null || incoming == null) return null;

      if (incoming.Count > 15)
      {
        logger.ConditionalDebug($"Merging {incoming.Count} {typeof(CoreType).Name}s in parallel.");
        return MergeParallelInPlace(incoming, ref toMutate, knownPlayers);
      }
      else
      {
        logger.ConditionalDebug($"Merging {incoming.Count} {typeof(CoreType).Name}s in series.");
        return MergeSerialInPlace(incoming, ref toMutate, knownPlayers);
      }
    }

    public static void MergeSource(List<Player> players, List<Team> teams, IReadOnlyList<Player> sourcePlayers, IReadOnlyCollection<Team> sourceTeams)
    {
      var teamMergeResult = MergeIntoList(sourceTeams, ref teams, players);
      _ = MergeIntoList(sourcePlayers, ref players, players);
      CorrectTeamIdsForPlayers(players, teamMergeResult.FinalMergedIds);
    }

    internal static MergeRecordHandler MergeParallelInPlace<CoreType>(IReadOnlyCollection<CoreType> incoming, ref List<CoreType> toMutate, IReadOnlyCollection<Player>? knownPlayers)
              where CoreType : BaseSplatTagCoreObject<CoreType>
    {
      var (toAdd, toMerge) = PrepareMergeParallel(incoming, toMutate, knownPlayers);
      MergeRecordHandler mergeRecordHandler = new();
      foreach (var (itemToKeep, itemToMerge) in toMerge)
      {
        mergeRecordHandler.AddMerge(itemToKeep, itemToMerge);
      }
      mergeRecordHandler.AddWithoutMerge(toAdd);
      var result = mergeRecordHandler.FinalItems.Cast<CoreType>().ToList();
      logger.ConditionalDebug($"Setting toMutate ({toMutate.Count} items) to value with {result.Count} items.");
      toMutate = result;
      logger.ConditionalDebug($"Returning parallel merge of {typeof(CoreType).Name}: {mergeRecordHandler}.");
      return mergeRecordHandler;
    }

    internal static MergeRecordHandler MergeSerialInPlace<CoreType>(IReadOnlyCollection<CoreType> incoming, ref List<CoreType> toMutate, IReadOnlyCollection<Player>? knownPlayers)
      where CoreType : BaseSplatTagCoreObject<CoreType>
    {
      var (toAdd, toMerge) = PrepareMergeSerial(incoming, toMutate, knownPlayers);

      MergeRecordHandler mergeRecordHandler = new();
      foreach (var (itemToKeep, itemToMerge) in toMerge)
      {
        mergeRecordHandler.AddMerge(itemToKeep, itemToMerge);
      }
      mergeRecordHandler.AddWithoutMerge(toAdd);
      var result = mergeRecordHandler.FinalItems.Cast<CoreType>().ToList();
      logger.ConditionalDebug($"Setting toMutate ({toMutate.Count} items) to value with {result.Count} items.");
      toMutate = result;
      logger.ConditionalDebug($"Returning serial merge of {typeof(CoreType).Name}: {mergeRecordHandler}.");
      return mergeRecordHandler;
    }

    internal static (IList<CoreType> toAdd, IList<(CoreType, CoreType)> toMerge) PrepareMergeParallel<CoreType>(
      IReadOnlyCollection<CoreType> incoming, IReadOnlyCollection<CoreType> reference, IReadOnlyCollection<Player>? knownPlayers)
      where CoreType : BaseSplatTagCoreObject<CoreType>
    {
      ConcurrentBag<CoreType> concurrentToAdd = new();
      ConcurrentBag<(CoreType, CoreType)> concurrentToMerge = new();

      Parallel.ForEach(incoming, incomingItem =>
      {
        try
        {
          CoreType? found = TryFindExisting(incomingItem, reference, knownPlayers);

          if (found == null)
          {
            concurrentToAdd.Add(incomingItem);
          }
          else
          {
            concurrentToMerge.Add((found, incomingItem));
          }
        }
        catch (Exception ex)
        {
          logger.Error(ex, $"Error in preparing {incomingItem}: {ex}");
        }
      });

      var toAdd = concurrentToAdd.ToArray();
      var toMerge = concurrentToMerge.ToList();

      // Make sure we don't merge both ways (it should be fine as all handlers have duplication checks, but it's just silly to do)
      // Flip the itemToKeep and itemToMerge then attempt a removal. Returns false if not found (shrug)
      foreach (var (itemToKeep, itemToMerge) in concurrentToMerge)
      {
        toMerge.Remove((itemToMerge, itemToKeep));
      }

      return (toAdd, toMerge);
    }

    internal static (List<CoreType> toAdd, List<(CoreType, CoreType)> toMerge) PrepareMergeSerial<CoreType>(
      IReadOnlyCollection<CoreType> incoming, IReadOnlyCollection<CoreType> reference, IReadOnlyCollection<Player>? knownPlayers)
      where CoreType : BaseSplatTagCoreObject<CoreType>
    {
      List<CoreType> toAdd = new();
      List<(CoreType, CoreType)> toMerge = new();

      foreach (var incomingItem in incoming)
      {
        try
        {
          CoreType? found = TryFindExisting(incomingItem, reference, knownPlayers);

          if (found == null)
          {
            toAdd.Add(incomingItem);
          }
          else
          {
            toMerge.Add((found, incomingItem));
          }
        }
        catch (Exception ex)
        {
          logger.Error(ex, $"Error in preparing {incomingItem}: {ex}");
        }
      }
      return (toAdd, toMerge);
    }

    private static CoreType? TryFindExisting<CoreType>(CoreType incomingItem, IReadOnlyCollection<CoreType> reference, IReadOnlyCollection<Player>? knownPlayers)
      where CoreType : BaseSplatTagCoreObject<CoreType>
    {
      var orderedResults = reference.Where(i => incomingItem.Id != i.Id).GroupBy(i => i.MatchWithReason(incomingItem).ToWeight()).OrderByDescending(group => group.Key);
      var bestResultGroup = orderedResults.FirstOrDefault();
      if (bestResultGroup != null)
      {
        var weighting = bestResultGroup.Key;
        foreach (var bestResult in bestResultGroup)
        {
          if (weighting > FilterOptionWeights.MergableThresholdWeight)
          {
            return bestResult;
          }

          var bestFilterOption = bestResult.MatchWithReason(incomingItem);
          if (knownPlayers != null && bestFilterOption.HasFlag(FilterOptions.TeamName) && incomingItem is Team t1 && bestResult is Team t2)
          {
            bool matched = Matcher.TeamsMatch(knownPlayers, t1, t2);
            if (matched)
            {
              return bestResult;
            }
            // else loop
          }
        }
      }
      return null;
    }
  }
}