namespace SplatTagCore
{
  /// <summary>
  /// Objects that can be merged in a chronology-safe manner.
  /// </summary>
  public interface IMergable
  {
    /// <summary>
    /// Merge another instance into this one.
    /// Should correctly handle sources and chronology.
    /// </summary>
    public void Merge(IMergable other);
  }
}