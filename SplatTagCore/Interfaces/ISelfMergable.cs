namespace SplatTagCore
{
  /// <summary>
  /// Objects that can be merged in a chronology-safe manner.
  /// </summary>
  public interface ISelfMergable : IMergable<ISelfMergable>
  {
  }
}