using NLog;
using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SplatTagDatabase.Merging
{
  [Serializable]
  public record CoreMergeResults : ISerializable
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// All records of objects.
    /// </summary>
    private readonly MergeRecord[] records;

    /// <summary>
    /// Incoming items that were added without merge
    /// </summary>
    public IEnumerable<ISplatTagCoreObject> AddedItems => AddedRecords.Select(r => r.ResultantItem).Distinct();

    /// <summary>
    /// Incoming item records that were added without merge
    /// </summary>
    public IEnumerable<MergeRecord> AddedRecords => records.Where(r => r.ChangeType == MergeChangeEnum.Added);

    /// <summary>
    /// Get an array that contains all of the resulting items.
    /// </summary>
    public IEnumerable<ISplatTagCoreObject> AllItems => records.Select(r => r.ResultantItem).Distinct();

    /// <summary>
    /// Get if any of the records are merges.
    /// </summary>
    public bool AnyMerged => records.Any(r => r.IsMerge);

    /// <summary>
    /// Count the total number of records.
    /// </summary>
    public int Count => records.Length;

    /// <summary>
    /// Items that have been merged into another and therefore are to be discarded.
    /// </summary>
    public IEnumerable<ISplatTagCoreObject> DiscardedItems => MergedRecords.Select(r => r.MergedItem!).Distinct();

    /// <summary>
    /// Get the final players.
    /// </summary>
    public IEnumerable<Player> ResultingPlayers => AllItems.OfType<Player>();

    /// <summary>
    /// Get the final teams.
    /// </summary>
    public IEnumerable<Team> ResultingTeams => AllItems.OfType<Team>();

    /// <summary>
    /// Items that have been updated by a merge.
    /// </summary>
    public IEnumerable<MergeRecord> MergedRecords => records.Where(r => r.ChangeType == MergeChangeEnum.Merged);

    /// <summary>
    /// Existing items that have been kept
    /// </summary>
    public IEnumerable<ISplatTagCoreObject> UnchangedItems => UnchangedRecords.Select(r => r.ResultantItem).Distinct();

    /// <summary>
    /// Existing object records that have been kept
    /// </summary>
    public IEnumerable<MergeRecord> UnchangedRecords => records.Where(r => r.ChangeType == MergeChangeEnum.Kept);

    /// <summary>
    /// Existing objects that have had incoming items merged in
    /// </summary>
    public IEnumerable<ISplatTagCoreObject> UpdatedItems => MergedRecords.Select(x => x.ResultantItem).Distinct();

    /// <summary>
    /// Construct a new instance of the <see cref="CoreMergeResults"/> class with its records.
    /// </summary>
    /// <param name="incoming"></param>
    public CoreMergeResults(IEnumerable<MergeRecord> incoming)
    {
      // Discard dupe (flipped) merges
      List<MergeRecord> result = new();
      foreach (var record in incoming)
      {
        // Switch on the change type
        switch (record.ChangeType)
        {
          case MergeChangeEnum.Added:
          case MergeChangeEnum.Kept:
            // Add the record
            result.Add(record);
            break;

          case MergeChangeEnum.Merged:
            // If the merged item is the same as the resultant item, then discard the merge
            if (record.MergedItem == record.ResultantItem)
            {
              logger.Warn($"Discarding duplicate merge record {record}");
            }
            else
            {
              // Check if the flipped merge is already in the list
              var found = result.Find(r => r.IsMerge && r.MergedItem == record.ResultantItem && r.ResultantItem == record.MergedItem);
              if (found != null)
              {
                logger.ConditionalDebug($"Discarding duplicate merge of {record.MergedItemId} ({record.MergedItem}) -> {record.ResultantItemId} ({record.ResultantItem}) already added ({found.ResultantItemId} kept)");
              }
              else
              {
                // Add the record
                result.Add(record);
              }
            }
            break;

          default:
            throw new NotImplementedException("Unknown change type: " + record.ChangeType + " for record: " + record);
        }
      }

      this.records = result.ToArray();
      logger.ConditionalDebug($"Constructed a {nameof(CoreMergeResults)} with {Count} records, containing {MergedRecords.Count()} merged items.");
    }

    /// <summary>
    /// Construct a new instance of the <see cref="CoreMergeResults"/> class with its records.
    /// </summary>
    /// <param name="records"></param>
    public CoreMergeResults(params CoreMergeResults?[] coreMergeResults)
    {
      records = coreMergeResults.Where(x => x != null).SelectMany(x => x!.records).ToArray();
      logger.ConditionalDebug($"Constructed a merged {nameof(CoreMergeResults)} with {Count} records, containing {MergedRecords.Count()} merged items.");
    }

    /// <summary>
    /// Output the records to the log.
    /// </summary>
    /// <param name="logLevel">The log level to output (info by default)</param>
    public void ToLog(LogLevel? logLevel = null)
    {
      var level = logLevel ?? LogLevel.Info;
      foreach (var record in records)
      {
        if (record.IsMerge)
        {
          logger.Log(level, $"Merged\t{record.MergedItemId}\t({record.MergedItem})\tinto\t{record.ResultantItemId}\t({record.ResultantItem})\tbecause it matched:\t{record.mergeReason}");
        }
        else
        {
          logger.Log(level, $"{record.ChangeType}\t{record.ResultantItemId}");
        }
      }
    }

    /// <summary>
    /// Output the records to a StringBuilder.
    /// </summary>
    public StringBuilder ToStringBuilder()
    {
      StringBuilder sb = new();
      foreach (var record in records)
      {
        if (record.IsMerge)
        {
          sb.Append("Merged\t").Append(record.MergedItemId).Append("\t(").Append(record.MergedItem).Append(")\tinto\t").Append(record.ResultantItemId).Append("\t(").Append(record.ResultantItem).Append(")\tbecause it matched:\t").Append(record.mergeReason).AppendLine();
        }
        else
        {
          sb.Append(record.ChangeType).Append('\t').Append(record.ResultantItemId).AppendLine();
        }
      }
      return sb;
    }
    /// <summary>
    /// Overridden ToString
    /// </summary>
    public override string ToString()
    {
      return $"{nameof(CoreMergeResults)}: {Count} merge records. Has {(AnyMerged ? "merged items." : "no merged items.")}";
    }

    #region Serialization

    // Deserialize
    protected CoreMergeResults(SerializationInfo info, StreamingContext context)
    {
      var result = new List<MergeRecord>();
      result.AddRange(info.GetValueOrDefault("Records", Array.Empty<MergeRecord>()));
      records = result.ToArray();
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (Count > 0)
        info.AddValue("Records", this.records);
    }

    #endregion Serialization
  }
}