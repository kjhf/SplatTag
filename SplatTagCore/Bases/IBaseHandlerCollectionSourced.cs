using System;
using System.Collections.Generic;

namespace SplatTagCore
{
  /// <summary>
  /// Interface for <see cref="BaseHandlerCollectionSourced"/>
  /// </summary>
  /// <remarks>Do NOT implement/inherit Dictionary, it will seriously screw up serialization. Use the property only.</remarks>
  public interface IBaseHandlerCollectionSourced :
    ISelfMatchable,
    ISelfMergable,
    IReadonlySourceable
  {
    /// <summary>
    /// Dictionary of handlers, keyed by its serialization name.
    /// </summary>
    internal protected IReadOnlyDictionary<string, BaseHandler> Handlers { get; }

    /// <summary>
    /// Get if the handler has data that needs serializing (true), or if it can be skipped (false).
    /// </summary>
    public bool HasDataToSerialize { get; }

    /// <summary>
    /// Get the handlers that are supported by this collection, in form of a serialization name to handler type and function on how to construct it.
    /// </summary>
    /// <example>
    /// { HandlerSerialization, (typeof(NamesHandler{Name}), () => new NamesHandler{Name}(FilterOptions.Filter, HandlerSerialization)) },
    /// </example>
    public IReadOnlyDictionary<string, (Type, Func<BaseHandler>)> SupportedHandlers { get; }

    /// <summary>
    /// Get if the handler collection contains the specified handler key.
    /// </summary>
    public bool ContainsKey(string key);
  }
}