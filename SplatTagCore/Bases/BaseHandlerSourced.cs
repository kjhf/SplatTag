using NLog;
using System.Collections.Generic;

namespace SplatTagCore
{
  /// <summary>
  /// Base class for <see cref="BaseHandler"/> classes that are also sourced.
  /// </summary>
  public abstract class BaseHandlerSourced :
    BaseHandler,
    IReadonlySourceable
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    protected BaseHandlerSourced()
    {
      logger.Trace($"{nameof(BaseHandlerSourced)} constructor in {this.GetType()} called.");
    }

    /// <summary>
    /// Back-store for quick access to the most recent source.
    /// </summary>
    protected Source? mostRecentSource = default;

    /// <summary>
    /// Get the most recent source.
    /// </summary>
    public Source? MostRecentSource => mostRecentSource;

    public abstract IReadOnlyList<Source> Sources { get; }

    /// <inheritdoc/>
    public override FilterOptions MatchWithReason(BaseHandler other) => MatchWithReason((BaseHandlerSourced)other);
  }
}