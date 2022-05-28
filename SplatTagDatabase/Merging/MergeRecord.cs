using NLog;
using SplatTagCore;
using System;
using System.Runtime.Serialization;

namespace SplatTagDatabase.Merging
{
  [Serializable]
  public record MergeRecord : ISerializable
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The kept item, i.e. the destination of the merged item.
    /// </summary>
    private ISplatTagCoreObject? referenceItemKept;

    /// <summary>
    /// The merged item, i.e. the source of the merge.
    /// </summary>
    private readonly ISplatTagCoreObject? incomingItemMergedOrAdded;

    /// <summary>
    /// Get if the record indicates a merge took place.
    /// </summary>
    public bool IsMerge => ChangeType == MergeChangeEnum.Merged;

    /// <summary>
    /// The item that was unchanged/kept or added.
    /// Note that the discarded item won't be gotten through this property because in a merge then referenceItemKept will be returned first.
    /// </summary>
    public ISplatTagCoreObject ResultantItem => referenceItemKept ?? incomingItemMergedOrAdded ?? throw new InvalidOperationException("No item was kept or added.");

    /// <summary>
    /// The item that was unchanged/kept or added's id.
    /// </summary>
    public Guid ResultantItemId => ResultantItem.Id;

    /// <summary>
    /// The item that was merged; null if this isn't a merge.
    /// </summary>
    public ISplatTagCoreObject? MergedItem => incomingItemMergedOrAdded;

    /// <summary>
    /// The item that was merged's id; null if this isn't a merge.
    /// </summary>
    public Guid? MergedItemId => MergedItem?.Id;

    public MergeChangeEnum ChangeType
    {
      get
      {
        if (referenceItemKept != null && incomingItemMergedOrAdded != null)
        {
          return MergeChangeEnum.Merged;
        }
        else if (referenceItemKept != null)
        {
          return MergeChangeEnum.Kept;
        }
        else if (incomingItemMergedOrAdded != null)
        {
          return MergeChangeEnum.Added;
        }
        else
        {
          throw new InvalidOperationException("No item was kept or added.");
        }
      }
    }

    internal void PerformMerge()
    {
      if (referenceItemKept == null)
      {
        throw new InvalidOperationException("No item was kept.");
      }

      if (incomingItemMergedOrAdded == null)
      {
        throw new InvalidOperationException("No item was added.");
      }

      referenceItemKept.Merge(incomingItemMergedOrAdded);
    }

    /// <summary>
    /// The reason for the merge
    /// </summary>
    public MergeReason? mergeReason;

    private MergeRecord(ISplatTagCoreObject? itemKept = null, ISplatTagCoreObject? itemMerged = null, MergeReason? reason = null)
    {
      if (itemKept == null && itemMerged == null) throw new ArgumentNullException("itemKept and itemMerged cannot both be null.");

      if (itemKept != null)
      {
        referenceItemKept = itemKept;
      }

      if (itemMerged != null)
      {
        incomingItemMergedOrAdded = itemMerged;
        mergeReason = reason ?? new MergeReason(itemKept?.MatchWithReason(itemMerged) ?? FilterOptions.None);
      }
    }

    public static MergeRecord CreateMergeRecordForAddedItem(ISplatTagCoreObject itemAdded) => new(null, itemAdded);
    public static MergeRecord CreateMergeRecordForKeptItem(ISplatTagCoreObject itemKept) => new(itemKept, null);
    public static MergeRecord CreateMergeRecordForMergedItems(ISplatTagCoreObject referenceItemKept, ISplatTagCoreObject incomingMergedItem, FilterOptions reason)
      => new(referenceItemKept, incomingMergedItem, new MergeReason(reason));

    protected MergeRecord() { }

    // Deserialize
    protected MergeRecord(SerializationInfo info, StreamingContext context)
    {
      referenceItemKept = ((ISplatTagCoreObject?)
        info.GetValueOrDefault("K", (Player?)null)
        ?? info.GetValueOrDefault("K", (Team?)null)
        ?? throw new ArgumentNullException("Kept", "itemKept cannot be null."))!;

      incomingItemMergedOrAdded = (ISplatTagCoreObject?)
        info.GetValueOrDefault("M", (Player?)null)
        ?? info.GetValueOrDefault("M", (Team?)null);

      mergeReason = new MergeReason(info.GetValueOrDefault("R", FilterOptions.None));
    }

    internal void Migrate(ISplatTagCoreObject? newMergeResult)
    {
      if (!IsMerge) throw new InvalidOperationException("This is not a merge.");
      if (newMergeResult == null) throw new ArgumentNullException(nameof(newMergeResult));
      if (ReferenceEquals(newMergeResult, referenceItemKept))
      {
        logger.Warn("Tried to migrate a merge record to the same item.");
      }
      else
      {
        logger.ConditionalDebug($"Migrating kept merge record: was {referenceItemKept?.Id} now {newMergeResult.Id}");
        referenceItemKept = newMergeResult;
      }
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("K", referenceItemKept);
      if (IsMerge)
      {
        info.AddValue("M", incomingItemMergedOrAdded);
        info.AddValue("R", mergeReason!.Flags);
      }
    }
  }
}