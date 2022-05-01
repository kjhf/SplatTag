using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  /// <summary>
  /// Base class for all handlers that have data that can be matched, merged, and serialized.
  /// </summary>
  public abstract class BaseHandler : IMatchable, IMergable
  {
    protected BaseHandler()
    {
    }

    /// <summary>
    /// Serialize the handler into JSON data by populating the <see cref="SerializationInfo"/>.
    /// </summary>
    public abstract void GetObjectData(SerializationInfo info, StreamingContext context);

    /// <inheritdoc/>
    public abstract FilterOptions MatchWithReason(IMatchable other);

    /// <inheritdoc/>
    public abstract void Merge(IMergable other);

    /// <summary>
    /// Get if the handler has data that needs serializing (true), or if it can be skipped (false).
    /// </summary>
    public abstract bool HasDataToSerialize { get; }
  }
}