namespace SplatTagCore
{
  public interface IMatchable<in T>
  {
    /// <summary>
    /// Match the object with other instance and return the <see cref="FilterOptions"/> that describe how equal it is.
    /// Returns <see cref="FilterOptions.None"/> if unrelated or a FilterOption is not relevant here.
    /// </summary>
    FilterOptions MatchWithReason(T? other);
  }
}