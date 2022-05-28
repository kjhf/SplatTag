using SplatTagCore;

namespace SplatTagDatabase.Merging
{
  public interface ICoreMergeHandler
  {
    /// <summary>
    /// This merges incoming data into the database.
    /// </summary>
    /// <param name="source">Incoming source data</param>
    /// <returns>Results detailing the merge</returns>
    CoreMergeResults MergeSource(Source source);

    /// <summary>
    /// This finalises the merging process by merging all players and teams, and corrects team ids.
    /// </summary>
    /// <returns>Results detailing the merge</returns>
    CoreMergeResults[] MergeKnown();
  }
}