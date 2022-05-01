using SplatTagCore;
using System;
using System.Runtime.Serialization;

namespace SplatTagDatabase
{
  [Serializable]
  public record MergeRecord : ISerializable
  {
    /// <summary>
    /// The kept item, i.e. the destination of the merge.
    /// </summary>
    public ISplatTagCoreObject itemKept;

    /// <summary>
    /// The merged item, i.e. the source of the merge.
    /// </summary>
    public ISplatTagCoreObject itemMerged;

    /// <summary>
    /// The kept item's id
    /// </summary>
    public Guid KeptId => itemKept.Id;

    /// <summary>
    /// The merged item's id
    /// </summary>
    public Guid MergedId => itemMerged.Id;

    /// <summary>
    /// The reason for the merge (as a <see cref="FilterOptions"/> object)
    /// </summary>
    public FilterOptions mergeReason;

    public MergeRecord(ISplatTagCoreObject itemKept, ISplatTagCoreObject itemMerged)
    {
      this.itemKept = itemKept ?? throw new ArgumentNullException(nameof(itemKept));
      this.itemMerged = itemMerged ?? throw new ArgumentNullException(nameof(itemMerged));
      this.mergeReason = itemKept.MatchWithReason(itemMerged);
    }

    #region Serialization

    // Deserialize
    protected MergeRecord(SerializationInfo info, StreamingContext context)
    {
      this.itemKept = ((ISplatTagCoreObject?)
        info.GetValueOrDefault("K", (Player?)null)
        ?? info.GetValueOrDefault("K", (Team?)null)
        ?? throw new ArgumentNullException("Kept", "itemKept cannot be null."))!;

      this.itemMerged = ((ISplatTagCoreObject?)
        info.GetValueOrDefault("M", (Player?)null)
        ?? info.GetValueOrDefault("M", (Team?)null)
        ?? throw new ArgumentNullException("Merged", "itemMerged cannot be null."))!;

      this.mergeReason = info.GetValueOrDefault("R", FilterOptions.None);
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("K", this.itemKept);
      info.AddValue("M", this.itemMerged);
      info.AddValue("R", this.mergeReason);
    }

    #endregion Serialization
  }
}