namespace SplatTagCore
{
  /// <summary>
  /// Objects that can be merged in a chronology-safe manner.
  /// </summary>
  public interface IMergable<in T>
  {
    /// <summary>
    /// Merge another instance into this one.
    /// Should correctly handle sources and chronology.
    /// </summary>
    void Merge(T other);
  }
}