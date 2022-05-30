using NLog;
using SplatTagCore;
using SplatTagCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplatTagDatabase.Merging
{
  internal record IdMigrationHandler
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The merged records' ids, keyed by the kept id, value is the merged id(s).
    /// </summary>
    private readonly Dictionary<Guid, List<Guid>> mergedIdsLookup = new();

    /// <summary>
    /// Get the merged id results, keyed by old id that must be changed, and values of the id it must change to.
    /// </summary>
    private IReadOnlyDictionary<Guid, Guid> FinalMergedIds
    {
      get
      {
        /// Keyed by old id, Value is its new id.
        Dictionary<Guid, Guid> result = new();
        foreach (var (keptId, mergedIds) in mergedIdsLookup)
        {
          foreach (var oldId in mergedIds)
          {
            if (keptId == oldId)
            {
              logger.Warn($"Old id {oldId} in merged ids is the same as the kept id {keptId}");
              continue;
            }

            if (result.ContainsKey(oldId))
            {
              logger.Warn($"Old id {oldId} in merged ids is already mapped to {result[oldId]} and will be overwritten with {keptId}");
            }

            result[oldId] = keptId;
          }
        }
        return result;
      }
    }

    public void AddMigration(MergeRecord record)
    {
      if (record.IsMerge)
      {
        AddMigrateId(record.ResultantItemId, record.MergedItemId!.Value);
      }
    }

    private void AddMigrateId(Guid keptId, Guid mergedId)
    {
      if (keptId == mergedId)
      {
        logger.Warn($"Merged id {mergedId} is the same as the kept id {keptId}");
        return;
      }

      // Add the record's merged id to the kept id list
      mergedIdsLookup.AddOrAppend(keptId, mergedId);

      // Migrate the merged id records (if any)
      if (mergedIdsLookup.Remove(mergedId, out var toMigrate))
      {
        mergedIdsLookup.AddOrAppend(keptId, toMigrate);
      }
    }

    public bool PerformMigration(IReadOnlyCollection<Player> players)
    {
      bool workDone = CorrectTeamIdsForPlayers(players, FinalMergedIds);
      mergedIdsLookup.Clear();
      return workDone;
    }

    private static bool CorrectTeamIdsForPlayers(IReadOnlyCollection<Player> referencePlayers, IReadOnlyDictionary<Guid, Guid> teamsMergeResult)
    {
      if (referencePlayers == null || teamsMergeResult == null || referencePlayers.Count == 0 || teamsMergeResult.Count == 0)
      {
        logger.ConditionalDebug($"{nameof(CorrectTeamIdsForPlayers)}: Nothing to do.");
        return false;
      }

      if (logger.IsDebugEnabled)
      {
        logger.ConditionalDebug($"{nameof(CorrectTeamIdsForPlayers)}: {referencePlayers.Count} entries: ");
        foreach (var resultPair in teamsMergeResult)
        {
          logger.ConditionalDebug($"[{resultPair.Key}] --> {resultPair.Value}");
        }
      }

      // For each team, correct the id as specified.
      // If we have a lot of work, fire up a parallel for-each, otherwise just do a regular one.
      void CorrectTeamIdsAction(Player importPlayer) => importPlayer.CorrectTeamIds(teamsMergeResult);
      if (referencePlayers.Count > Builtins.PARALLEL_THRESHOLD)
      {
        Parallel.ForEach(referencePlayers, CorrectTeamIdsAction);
      }
      else
      {
        referencePlayers.ForEach(CorrectTeamIdsAction);
      }
      return true;
    }

    public static string IdsToString(IReadOnlyDictionary<Guid, Guid> ids)
    {
      return new StringBuilder()
        .AppendJoin("\n", ids.Select(resultPair => $"[{resultPair.Key}] --> {resultPair.Value}"))
        .AppendLine()
        .ToString();
    }

    internal void PrepMigrations(CoreMergeResults prep)
    {
      foreach (var record in prep.MergedRecords)
      {
        AddMigration(record);
      }

      // Correct the ids of merges based off of the merged destinations (i.e. resolve the merge chain of A-->B-->C-->D should be A-->D)
      foreach (var record in prep.MergedRecords)
      {
        if (FinalMergedIds.TryGetValue(record.ResultantItemId, out var newResultId))
        {
          var newResult = prep.AllKnownItems.FirstOrDefault(item => item.Id == newResultId);
          if (newResult != null)
          {
            record.Migrate(newResult);
          }
        }
      }
    }
  }
}