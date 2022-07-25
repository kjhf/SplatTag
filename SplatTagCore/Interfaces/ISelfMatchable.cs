namespace SplatTagCore
{
  public interface ISelfMatchable : IMatchable<ISelfMatchable>
  {
  }

  public static class ISelfMatchableExtensions
  {
    /// <summary>
    /// Match the object with other instance and return true/false representing if any results match the <paramref name="matchOptions"/> set.
    /// </summary>
    public static bool Matches(this ISelfMatchable instance, ISelfMatchable? other, FilterOptions matchOptions) => (instance.MatchWithReason(other) & matchOptions) != 0;
  }
}