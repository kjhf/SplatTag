namespace SplatTagCore
{
  public interface IMatchable<in T> : IMatchable
  {
    /// <summary>
    /// Get how this object matches another, or <see cref="FilterOptions.None"/> if they do not.
    /// </summary>
    FilterOptions MatchWithReason(T other);

    bool Matches(T other, FilterOptions matchOptions) => (MatchWithReason(other) & matchOptions) != 0;

    /// <summary>
    /// Get how this object matches another, or <see cref="FilterOptions.None"/> if they do not.
    /// </summary>
    new FilterOptions MatchWithReason(IMatchable other) => MatchWithReason((T)other);

    new bool Matches(IMatchable other, FilterOptions matchOptions) => (MatchWithReason((T)other) & matchOptions) != 0;
  }
}