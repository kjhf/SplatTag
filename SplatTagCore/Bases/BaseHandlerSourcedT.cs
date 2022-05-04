using System;
using System.Collections.Generic;

namespace SplatTagCore
{
  /// <summary>
  /// Base class for <see cref="BaseHandler{T}"/> classes that are also sourced.
  /// </summary>
  public abstract class BaseHandlerSourced<T> : BaseHandler<T>, IReadonlySourceable
  {
    protected BaseHandlerSourced()
    { }

    /// <summary>
    /// Back-store for quick access to the most recent source.
    /// </summary>
    protected Source? mostRecentSource = default;

    /// <summary>
    /// Get the most recent source.
    /// </summary>
    public Source? MostRecentSource => mostRecentSource;

    public abstract IReadOnlyList<Source> Sources { get; }

    public override FilterOptions MatchWithReason(IMatchable other) => MatchWithReason((T)other);

    public override void Merge(IMergable other) => Merge((T)other);
  }
}