using NLog;
using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagDatabase
{
  [Serializable]
  internal class MergeRecordHandler : ISerializable
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private List<ISplatTagCoreObject>? addedItems;
    private List<ISplatTagCoreObject> AddedItems => addedItems ??= new List<ISplatTagCoreObject>();

    public MergeRecordHandler()
    {
    }

    /// <summary>
    /// Flag if any work was done (things added or merged).
    /// </summary>
    public bool WorkDone => addedItems?.Count > 0 || records.Count > 0;

    /// <summary>
    /// All records of objects that have been merged.
    /// </summary>
    private readonly IList<MergeRecord> records = new List<MergeRecord>();

    /// <summary>
    /// The merged records' ids, keyed by the kept id, value is the merged id(s).
    /// </summary>
    private readonly Dictionary<Guid, List<Guid>> mergedIdsLookup = new();

    /// <summary>
    /// Get the merged id results, keyed by old id that must be changed, and values of the id it must change to.
    /// </summary>
    public IReadOnlyDictionary<Guid, Guid> FinalMergedIds
    {
      get
      {
        /// Keyed by old id, Value is its new id.
        Dictionary<Guid, Guid> result = new();
        foreach (var (keptId, mergedIds) in mergedIdsLookup)
        {
          foreach (var oldId in mergedIds)
          {
            if (keptId == oldId) continue;
            result[oldId] = keptId;
          }
        }
        return result;
      }
    }

    /// <summary>
    /// Get the final items that have been merged.
    /// </summary>
    public IEnumerable<ISplatTagCoreObject> FinalItems
    {
      get
      {
        Dictionary<Guid, ISplatTagCoreObject> finalItems =
          addedItems?.Count > 0 ?
          AddedItems.ToDictionary(i => i.Id, i => i) :
          new();

        foreach (var item in records.Where(item => mergedIdsLookup.ContainsKey(item.KeptId)))
        {
          finalItems.TryAdd(item.KeptId, item.itemKept);
        }

        return finalItems.Values;
      }
    }

    /// <summary>
    /// Have the handler merge two objects together with a reason, and add to the records.
    /// </summary>
    public void AddMerge<T>(T toKeep, T toMerge) where T : class, ISplatTagCoreObject<T>
    {
      toKeep.Merge(toMerge);

      // Add the record and its ids
      Add(new MergeRecord(toKeep, toMerge));
    }

    /// <summary>
    /// Have the handler add an object without a merge for it.
    /// </summary>
    public void AddWithoutMerge<T>(T toKeep) where T : class, ISplatTagCoreObject<T>
    {
      AddedItems.Add(toKeep);
    }

    /// <summary>
    /// Have the handler add objects without a merge for them.
    /// </summary>
    public void AddWithoutMerge<T>(IEnumerable<T> toKeep) where T : class, ISplatTagCoreObject<T>
    {
      AddedItems.AddRange(toKeep);
    }

    /// <summary>
    /// Add a new record of objects that have been merged
    /// </summary>
    /// <param name="record"></param>
    public void Add(MergeRecord record)
    {
      // Add the record
      records.Add(record);
      AddMigrateId(record.KeptId, record.MergedId);
    }

    private void AddMigrateId(Guid keptId, Guid mergedId)
    {
      // Add the record's merged id to the kept id list
      mergedIdsLookup.AddOrAppend(keptId, mergedId);

      // Migrate the merged id records (if any)
      if (mergedIdsLookup.Remove(mergedId, out var toMigrate))
      {
        mergedIdsLookup.AddOrAppend(keptId, toMigrate);
      }
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
        logger.Log(level, record.itemMerged.ToString() + " merged into " + record.itemKept + " because it matched: " + record.mergeReason.ToString());
      }
    }

    public int Count => records.Count;

    /// <summary>
    /// Overridden ToString
    /// </summary>
    public override string ToString()
    {
      return $"{nameof(MergeRecordHandler)}: {Count} merge records (with {addedItems?.Count ?? 0} unmerged items)";
    }

    #region Serialization

    // Deserialize
    protected MergeRecordHandler(SerializationInfo info, StreamingContext context)
    {
      foreach (var record in info.GetValueOrDefault("Records", Array.Empty<MergeRecord>()))
      {
        // Add will also calculate the merged ids.
        Add(record);
      }
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