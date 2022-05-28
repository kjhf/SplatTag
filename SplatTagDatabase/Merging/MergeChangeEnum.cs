namespace SplatTagDatabase.Merging
{
  public enum MergeChangeEnum
  {
    /// <summary> The item has no change association </summary>
    Unknown,

    /// <summary> The item has been added from incoming into the reference </summary>
    Added,

    /// <summary> The item incoming has been merged into the reference </summary>
    Merged,

    /// <summary> The item has been kept in the reference with no merges </summary>
    Kept
  }
}