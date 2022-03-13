namespace SplatTagCore
{
  public interface IMergable<T>
  {
    /// <summary>
    /// Merge another instance into this one.
    /// Should correctly handle sources and chronology.
    /// </summary>
    public void Merge(T other);
  }
}