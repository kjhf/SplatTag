namespace SplatTagCore
{
  /// <summary>
  /// Objects that can be merged in a chronology-safe manner.
  /// </summary>
  public interface IMergable<in T> : IMergable
  {
    /// <summary>
    /// Merge another instance into this one.
    /// Should correctly handle sources and chronology.
    /// </summary>
    public void Merge(T other);

    new public void Merge(IMergable other) => Merge((T)other);
  }
}