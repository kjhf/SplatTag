using System;
using System.Collections.Generic;

namespace SplatTagCore
{
  internal interface IBaseHandlerCollectionSourced : IReadonlySourceable, IDictionary<string, BaseHandler>
  {
    /// <summary>
    /// Dictionary of handlers, keyed by its serialization name.
    /// </summary>
    internal IDictionary<string, BaseHandler> Handlers { get; }

    /// <summary>
    /// Get if the handler has data that needs serializing (true), or if it can be skipped (false).
    /// </summary>
    bool HasDataToSerialize { get; }

    /// <summary>
    /// Get the handlers that are supported by this collection, in form of a serialization name to handler type and function on how to construct it.
    /// </summary>
    /// <example>
    /// { HandlerSerialization, (typeof(NamesHandler{Name}), () => new NamesHandler{Name}(FilterOptions.Filter, HandlerSerialization)) },
    /// </example>
    internal IReadOnlyDictionary<string, (Type, Func<BaseHandler>)> SupportedHandlers { get; }
  }
}