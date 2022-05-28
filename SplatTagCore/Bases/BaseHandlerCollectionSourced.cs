using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  /// <summary>
  /// Base class for a group of handler classes that can iterate through their children.
  /// </summary>
  public abstract class BaseHandlerCollectionSourced<T> :
    BaseHandlerSourced<T>,
    IBaseHandlerCollectionSourced
    where T : BaseHandlerCollectionSourced<T>
  {
    /// <summary>
    /// Dictionary of handlers, keyed by its serialization name.
    /// </summary>
    protected internal readonly Dictionary<string, BaseHandler> handlers = new();

    /// <inheritdoc/>
    IDictionary<string, BaseHandler> IBaseHandlerCollectionSourced.Handlers => handlers;

    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    protected BaseHandlerCollectionSourced()
    {
    }

    /// <inheritdoc/>
    public override bool HasDataToSerialize => handlers.Values.Any(h => h.HasDataToSerialize);

    /// <summary>
    /// Get how this object matches another, or <see cref="FilterOptions.None"/> if they do not.
    /// </summary>
    public override IReadOnlyList<Source> Sources
    {
      get
      {
        var sources = new HashSet<Source>();
        // A filter OfType<IReadonlySourceable> is needed here as BaseHandlerSourced implements this interface but BaseHandler does not.
        // Any handlers that are not properly sourced are therefore not counted here. Likely their information is contained in other handlers.
        foreach (var handler in handlers.Values.OfType<IReadonlySourceable>())
        {
          sources.UnionWith(handler.Sources);
        }
        return sources.ToList();
      }
    }

    /// <summary>
    /// Get the handlers that are supported by this collection, in form of a serialization name to handler type and function on how to construct it.
    /// </summary>
    /// <example>
    /// { HandlerSerialization, (typeof(NamesHandler{Name}), () => new NamesHandler{Name}(FilterOptions.Filter, HandlerSerialization)) },
    /// </example>
    protected abstract IReadOnlyDictionary<string, (Type, Func<BaseHandler>)> SupportedHandlers { get; }

    IReadOnlyDictionary<string, (Type, Func<BaseHandler>)> IBaseHandlerCollectionSourced.SupportedHandlers => SupportedHandlers;

    /// <summary>
    /// Get how this object matches another, or <see cref="FilterOptions.None"/> if they do not.
    /// </summary>
    public override FilterOptions MatchWithReason(T? other)
    {
      if (other is null) return FilterOptions.None;

      FilterOptions options = FilterOptions.None;
      foreach (var handlerPair in handlers)
      {
        if (other.handlers.TryGetValue(handlerPair.Key, out var otherHandler))
        {
          options |= handlerPair.Value.MatchWithReason(otherHandler);
        }
      }
      return options;
    }

    /// <summary>
    /// Merge all the handlers in this collection.
    /// Handles Sources and timings.
    /// </summary>
    public override void Merge(T? other)
    {
      if (other == null)
      {
        return;
      }
      if (ReferenceEquals(this, other))
      {
        logger.Error("Attempting to merge the same object. Returning early. " + ToString());
        return;
      }

      // Merge existing
      foreach (var handler in handlers)
      {
        if (other.ContainsKey(handler.Key))
        {
          handler.Value.Merge(other[handler.Key]);
        }
      }

      // Add missing
      foreach (var handler in other.handlers)
      {
        if (!handlers.ContainsKey(handler.Key))
        {
          handlers.Add(handler.Key, handler.Value);
        }
      }
    }

    public override string ToString() => string.Join(", ", handlers.Values);

    /// <summary>
    /// Get or create the handler with the specified name.
    /// </summary>
    /// <exception cref="InvalidCastException"></exception>
    protected internal DerivedType GetHandler<DerivedType>(string serializationName) where DerivedType : BaseHandler
    {
      var entry = SupportedHandlers[serializationName];
      if (typeof(DerivedType) != entry.Item1) throw new InvalidCastException($"DerivedType ({typeof(DerivedType)}) != the handler's type of {entry.Item1} ");

      return (DerivedType)handlers.GetOrAdd(serializationName, entry.Item2);
    }

    /// <summary>
    /// Get the handler with the specified name if it exists, else null.
    /// </summary>
    protected internal DerivedType? GetHandlerNoCreate<DerivedType>(string serializationName) where DerivedType : BaseHandler
      => handlers.TryGetValue(serializationName, out var value) ? (DerivedType)value : null;

    protected internal bool MatchByHandlerName(string name, T? other)
    {
      if (other == null) return false;

      var thisHandler = GetHandlerNoCreate<BaseHandler>(name);
      var otherHandler = other.GetHandlerNoCreate<BaseHandler>(name);
      return thisHandler != null && otherHandler != null && thisHandler.MatchWithReason(otherHandler) != FilterOptions.None;
    }

    protected virtual void DeserializeHandlers(SerializationInfo info, StreamingContext context)
    {
      logger.Debug(nameof(DeserializeHandlers) + " called for " + ToString());
      foreach (var handlerInfo in SupportedHandlers)
      {
        BaseHandler? deserializedHandler = (BaseHandler?)info.GetValueOrDefault(handlerInfo.Key, handlerInfo.Value.Item1);
        if (deserializedHandler != null)
        {
          // If found, merge. Else add as-is.
          if (handlers.TryGetValue(handlerInfo.Key, out var value))
          {
            value.Merge(deserializedHandler);
          }
          else
          {
            handlers.Add(handlerInfo.Key, deserializedHandler);
          }
        }
        // otherwise we can leave and it won't be needlessly added to the handlers list
      }
      logger.Debug(nameof(DeserializeHandlers) + " end for " + ToString());
    }

    protected void SerializeHandlers(SerializationInfo info, StreamingContext context)
    {
      logger.Debug(nameof(SerializeHandlers) + " called for " + ToString());
      foreach (var handlerPair in handlers.Where(h => h.Value.HasDataToSerialize))
      {
        handlerPair.Value.GetObjectData(info, context);
      }
    }

    #region IDictionary overrides

    public int Count => handlers.Count;
    public bool IsReadOnly => false;

    public ICollection<string> Keys
      => handlers.Keys;

    public ICollection<BaseHandler> Values
      => handlers.Values;

    public BaseHandler this[string key] { get => handlers[key]; set => handlers[key] = value; }

    public void Add(string key, BaseHandler value)
      => handlers.Add(key, value);

    public void Add(KeyValuePair<string, BaseHandler> item)
      => handlers.Add(item.Key, item.Value);

    public void Clear()
      => handlers.Clear();

    public bool Contains(KeyValuePair<string, BaseHandler> item)
      => handlers.Contains(item);

    public bool ContainsKey(string key)
      => handlers.ContainsKey(key);

    public void CopyTo(KeyValuePair<string, BaseHandler>[] array, int arrayIndex)
      => ((ICollection<KeyValuePair<string, BaseHandler>>)handlers).CopyTo(array, arrayIndex);

    public IEnumerator<KeyValuePair<string, BaseHandler>> GetEnumerator()
      => handlers.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
      => handlers.GetEnumerator();

    public bool Remove(string key)
      => handlers.Remove(key);

    public bool Remove(KeyValuePair<string, BaseHandler> item)
      => handlers.Remove(item.Key);

    public bool TryGetValue(string key, out BaseHandler value)
      => handlers.TryGetValue(key, out value);

    #endregion IDictionary overrides
  }
}