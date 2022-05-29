using SplatTagCore;

namespace SplatTagDatabase.Merging
{
  public record MergeReason
  {
    /// <summary> The reason for the merge. </summary>
    public FilterOptions Flags { get; }

    /// <summary> The similarity score of the two items being merged. </summary>
    public int Similarity => Flags.ToSummedWeight();

    /// <summary> Get if the <see cref="Similarity"/> was high enough for the merge threshold </summary>
    public bool MergedByThreshold => Similarity > FilterOptionWeights.MergableThresholdWeight;

    /// <summary>
    /// Overridden ToString. Returns the <see cref="Flags"/> and <see cref="Similarity"/>.
    /// </summary>
    public override string ToString() => $"{Flags}\twith similarity score\t{Similarity}";

    /// <summary>
    /// Create a new <see cref="MergeReason"/> with the given <see cref="FilterOptions"/>.
    /// </summary>
    /// <param name="flags"></param>
    public MergeReason(FilterOptions flags)
      => Flags = flags;
  }
}