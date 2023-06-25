using System.Collections.Generic;
using System.Text.Json.Serialization;

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
    [JsonIgnore]
    protected Source? mostRecentSource = default;

    /// <summary>
    /// Get the most recent source.
    /// </summary>
    [JsonIgnore]
    public Source? MostRecentSource => mostRecentSource;

    [JsonIgnore]
    public abstract IReadOnlyList<Source> Sources { get; }
  }
}