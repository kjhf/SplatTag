namespace SplatTagCore
{
  public interface IMatchable
  {
    /// <summary>
    /// Match the object with other instance and return the <see cref="FilterOptions"/> that describe how equal it is.
    /// Returns <see cref="FilterOptions.None"/> if unrelated or a FilterOption is not relevant here.
    /// </summary>
    FilterOptions MatchWithReason(IMatchable other);

    /// <summary>
    /// Match the object with other instance and return true/false representing if any results match the <paramref name="matchOptions"/> set.
    /// </summary>
    bool Matches(IMatchable other, FilterOptions matchOptions) => (MatchWithReason(other) & matchOptions) != 0;
  }
}