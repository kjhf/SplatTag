using System.Collections.Generic;

namespace SplatTagCore
{
  /// <summary>
  /// Base class for Handler classes that define the functionality of sorting and ordering of sources.
  /// </summary>
  public abstract class SourcedHandlerBase : IReadonlySourceable
  {
    /// <summary>
    /// Back-store for quick access to the most recent source.
    /// </summary>
    protected Source? mostRecentSource = default;

    /// <summary>
    /// Get the most recent source.
    /// </summary>
    public Source? MostRecentSource => mostRecentSource;

    public abstract IReadOnlyList<Source> Sources { get; }
  }
}