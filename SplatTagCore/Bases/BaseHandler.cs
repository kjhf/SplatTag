using NLog;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  /// <summary>
  /// Base class for all handlers that have data that can be matched and merged.
  /// </summary>
  public abstract class BaseHandler :
    ISelfMatchable,
    ISelfMergable
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    protected BaseHandler()
    {
      logger.Trace($"{nameof(BaseHandler)} base constructor called for {this.GetType()}.");
    }

    public abstract string SerializedHandlerName { get; }

    /// <summary>
    /// Serialize the handler into JSON data.
    /// Does not populate the <see cref="SerializationInfo"/>, see <see cref="ISerializable.GetObjectData(SerializationInfo, StreamingContext)"/> for this.
    /// </summary>
    public abstract object ToSerializedObject();

    /// <inheritdoc/>
    public virtual FilterOptions MatchWithReason(ISelfMatchable? other) => other is BaseHandler handler ? MatchWithReason(handler) : FilterOptions.None;

    /// <summary>
    /// Match the object with other instance and return the <see cref="FilterOptions"/> that describe how equal it is.
    /// Returns <see cref="FilterOptions.None"/> if unrelated or a FilterOption is not relevant here.
    /// </summary>
    public abstract FilterOptions MatchWithReason(BaseHandler other);

    /// <inheritdoc/>
    public abstract void Merge(ISelfMergable other);

    /// <summary>
    /// Get if the handler has data that needs serializing (true), or if it can be skipped (false).
    /// </summary>
    public abstract bool HasDataToSerialize { get; }
  }
}