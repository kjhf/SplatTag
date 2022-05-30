using System;

namespace SplatTagDatabase.Merging
{
  public record MergeLogEntry
  {
    public MergeChangeEnum ChangeType { get; }
    public Guid FinalItemId { get; }
    public Guid? MergedItemId { get; }
    public string? Reasons { get; }
    public int? SimilarityScore { get; }

    public MergeLogEntry(string line)
    {
      string[] parts = line.Split('\t');
      if (parts.Length != 2 && parts.Length != 10)
      {
        throw new Exception($"Invalid line (unexpected length {parts.Length}): {line}");
      }
      ChangeType = Enum.Parse<MergeChangeEnum>(parts[0], false);
      if (ChangeType == MergeChangeEnum.Merged)
      {
        FinalItemId = Guid.Parse(parts[1]);

        // parts[2] is MergedItem's name

        if (parts[3] != CoreMergeResults.INTO_CONSTANT)
          throw new Exception($"Invalid line (failed [3]): {line}");

        MergedItemId = Guid.Parse(parts[4]);

        // parts[5] is ResultantItem's name

        if (parts[6] != CoreMergeResults.BECAUSE_IT_MATCHED_CONSTANT)
          throw new Exception($"Invalid line (failed [6]): {line}");

        Reasons = parts[7];

        if (parts[8] != CoreMergeResults.SIMILARITY_CONSTANT)
          throw new Exception($"Invalid line (failed [8]): {line}");

        SimilarityScore = int.Parse(parts[9]);
      }
      else
      {
        MergedItemId = Guid.Parse(parts[1]);
      }
    }

    public MergeLogEntry(MergeChangeEnum changeType, Guid finalItemId, Guid? mergedItemId = null, string? reasons = null, int? similarityScore = null)
    {
      ChangeType = changeType;
      FinalItemId = finalItemId;
      MergedItemId = mergedItemId;
      Reasons = reasons;
      SimilarityScore = similarityScore;
    }

    public override string ToString()
    {
      if (ChangeType == MergeChangeEnum.Merged)
      {
        return $"{ChangeType}\t{FinalItemId}\t\t{CoreMergeResults.INTO_CONSTANT}\t{MergedItemId}\t\t{CoreMergeResults.BECAUSE_IT_MATCHED_CONSTANT}\t{Reasons}\t{CoreMergeResults.SIMILARITY_CONSTANT}\t{SimilarityScore}";
      }
      else
      {
        return $"{ChangeType}\t{MergedItemId}";
      }
    }
  }
}