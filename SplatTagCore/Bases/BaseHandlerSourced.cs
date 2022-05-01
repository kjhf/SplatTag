using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  /// <summary>
  /// Base class for <see cref="BaseHandler"/> classes that are also sourced.
  /// </summary>
  public abstract class BaseHandlerSourced : BaseHandler, IReadonlySourceable
  {
    /// <summary>
    /// Back-store for quick access to the most recent source.
    /// </summary>
    protected Source? mostRecentSource = default;

    protected BaseHandlerSourced()
    {
    }

    /// <summary>
    /// Get the most recent source.
    /// </summary>
    public Source? MostRecentSource => mostRecentSource;

    public abstract IReadOnlyList<Source> Sources { get; }
  }
}