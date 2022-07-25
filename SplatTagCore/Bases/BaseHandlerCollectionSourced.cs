using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using SplatTagCore;
using SplatTagCore.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  /// <summary>
  /// Base class for a group of handler classes that can iterate through their children.
  /// </summary>
  /// <remarks>To use the underlying dictionary, call <see cref="Handlers"/> which is implemented as part of the public interface, <see cref="IBaseHandlerCollectionSourced"/>.</remarks>
  public abstract class BaseHandlerCollectionSourced :
    BaseHandler,
    IBaseHandlerCollectionSourced
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly object _handlerLock = new();

    /// <summary>
    /// Dictionary of handlers, keyed by its serialization name.
    /// </summary>
    private readonly Dictionary<string, BaseHandler> _handlers = new();

    protected BaseHandlerCollectionSourced()
    {
      logger.Trace($"{nameof(BaseHandlerCollectionSourced)} constructor called.");
    }

    public IReadOnlyDictionary<string, BaseHandler> Handlers => _handlers;

    /// <inheritdoc/>
    public override bool HasDataToSerialize => Values.Any(h => h.HasDataToSerialize);

    /// <summary>
    /// Get how this object matches another, or <see cref="FilterOptions.None"/> if they do not.
    /// </summary>
    public IReadOnlyList<Source> Sources
    {
      get
      {
        var sources = new HashSet<Source>();
        // A filter OfType<IReadonlySourceable> is needed here as BaseHandlerSourced implements this interface but BaseHandler does not.
        // Any handlers that are not properly sourced are therefore not counted here. Likely their information is contained in other handlers.
        foreach (var handler in ReadOnlyHandlers.OfType<IReadonlySourceable>())
        {
          sources.UnionWith(handler.Sources);
        }
        return sources.Distinct().ToList();
      }
    }

    IReadOnlyDictionary<string, (Type, Func<BaseHandler>)> IBaseHandlerCollectionSourced.SupportedHandlers => SupportedHandlers;

    /// <summary>
    /// Get the handlers that are supported by this collection, in form of a serialization name to handler type and function on how to construct it.
    /// </summary>
    /// <example>
    /// { HandlerSerialization, (typeof(NamesHandler{Name}), () => new NamesHandler{Name}(FilterOptions.Filter, HandlerSerialization)) },
    /// </example>
    protected abstract IReadOnlyDictionary<string, (Type, Func<BaseHandler>)> SupportedHandlers { get; }

    /// <summary>
    /// Get all initialised handlers as a read-only collection.
    /// </summary>
    private IReadOnlyList<BaseHandler> ReadOnlyHandlers => _handlers.Values.ToArray();

    /// <summary>
    /// Get how this object matches another, or <see cref="FilterOptions.None"/> if they do not.
    /// </summary>
    public FilterOptions MatchWithReason(IBaseHandlerCollectionSourced? other)
    {
      if (other is null) return FilterOptions.None;
      // if (Count == 0 || other.Count == 0) return FilterOptions.None; // Waste of a check - handlers will always have Id and name

      FilterOptions options = FilterOptions.None;
      foreach (var (name, thisChildHandler) in _handlers.ToArray())
      {
        if (other.Handlers.TryGetValue(name, out var otherChildHandler))
        {
          options |= thisChildHandler.MatchWithReason(otherChildHandler);
        }
      }
      return options;
    }

    public override FilterOptions MatchWithReason(BaseHandler other) => MatchWithReason((IBaseHandlerCollectionSourced)other);

    /// <summary>
    /// Merge all the handlers in this collection.
    /// Handles Sources and timings.
    /// </summary>
    public override void Merge(ISelfMergable other) => Merge((IBaseHandlerCollectionSourced)other);

    /// <summary>
    /// Merge all the handlers in this collection.
    /// Handles Sources and timings.
    /// </summary>
    public void Merge(IBaseHandlerCollectionSourced other)
    {
      if (ReferenceEquals(this, other))
      {
        logger.Error("Attempting to merge the same object. Returning early. " + ToString());
        return;
      }

      // Merge existing
      foreach (var handler in _handlers)
      {
        var otherHandler = other.Handlers.Get(handler.Key);
        if (otherHandler != null)
        {
          handler.Value.Merge(otherHandler);
        }
      }

      // Add missing
      foreach (var handler in other.Handlers)
      {
        if (!ContainsKey(handler.Key))
        {
          Add(handler.Key, handler.Value);
        }
      }
    }

    public override object ToSerializedObject()
    {
      logger.Trace($"{this.GetType()} {nameof(ToSerializedObject)} (serialize) called.");
      return SerializeHandlers();
    }

    public override string ToString() => $"{nameof(BaseHandlerCollectionSourced)} ({this.GetType()}): [{string.Join(", ", ReadOnlyHandlers)}]";

    /// <summary>
    /// Get or create the handler with the specified name.
    /// </summary>
    /// <exception cref="InvalidCastException"></exception>
    protected internal DerivedType GetHandler<DerivedType>(string serializationName)
      where DerivedType : BaseHandler
    {
      var entry = SupportedHandlers[serializationName];
      lock (_handlerLock)
      {
        return (DerivedType)_handlers.GetOrAdd(serializationName, entry.Item2);
      }
    }

    /// <summary>
    /// Get the handler with the specified name if it exists, else null.
    /// </summary>
    protected internal DerivedType? GetHandlerNoCreate<DerivedType>(string serializationName) where DerivedType : BaseHandler
      => TryGetValue(serializationName, out var value) ? (DerivedType)value : null;

    protected internal bool MatchByHandlerName(string name, BaseHandlerCollectionSourced? other)
    {
      if (other == null) return false;

      var thisHandler = GetHandlerNoCreate<BaseHandler>(name);
      var otherHandler = other.GetHandlerNoCreate<BaseHandler>(name);
      return thisHandler != null && otherHandler != null && thisHandler.MatchWithReason(otherHandler) != FilterOptions.None;
    }

    protected virtual void DeserializeHandlers(SerializationInfo info, StreamingContext context)
    {
      logger.Trace($"{nameof(DeserializeHandlers)} called for {GetType()}");
      if (info.Contains(SerializedHandlerName))
      {
        DeserializeHandlers(info.GetValueOrDefault(SerializedHandlerName, new Dictionary<string, object>()));
      }
      else
      {
        DeserializeHandlers(new Dictionary<string, object>(info.AsKeyValuePairs()));
      }
    }

    protected virtual void DeserializeHandlers(Dictionary<string, object> deserializedDict)
    {
      if (deserializedDict.Count == 0)
      {
        logger.Warn($"{nameof(DeserializeHandlers)} not attempting deserialization under {SerializedHandlerName} ({GetType()}) as the deserializedDict is empty.");
        return;
      }

      foreach (var handlerInfo in SupportedHandlers)
      {
        if (deserializedDict.TryGetValue(handlerInfo.Key, out var deserializedObj))
        {
          if (deserializedObj is JToken token)
          {
            logger.Trace($"{nameof(DeserializeHandlers)} instantiating object type {handlerInfo.Value.Item1} for {handlerInfo.Key}.");
            deserializedObj = token.ToObject(handlerInfo.Value.Item1);
          }

          if (deserializedObj is BaseHandler deserializedHandler)
          {
            // If found, merge. Else add as-is.
            if (TryGetValue(handlerInfo.Key, out var value))
            {
              value.Merge(deserializedHandler);
              logger.Trace($"{nameof(DeserializeHandlers)} merged {handlerInfo.Key}: {deserializedHandler}.");
            }
            else
            {
              Add(handlerInfo.Key, deserializedHandler);
              logger.Trace($"{nameof(DeserializeHandlers)} added {handlerInfo.Key}: {deserializedHandler}.");
            }
          }
          else
          {
            string err = $"{nameof(DeserializeHandlers)} Cannot deserialize the type {deserializedObj?.GetType()} as it's not a BaseHandler";
            logger.Error(err);
            throw new ArgumentException(err);
          }
        }
        else
        {
          logger.Trace($"{nameof(DeserializeHandlers)} {handlerInfo.Key} not found.");
        }
        // otherwise we can leave and it won't be needlessly added to the handlers list
      }
      logger.Trace($"{nameof(DeserializeHandlers)} end for {GetType()}");

      if (_handlers.Count == 0)
      {
        logger.Warn($"{nameof(DeserializeHandlers)} didn't deserialize anything in type {GetType()}. The read dictionary contains {deserializedDict.Count} entries: {string.Join(",", deserializedDict.Keys)}");
      }
    }

    protected object SerializeHandlers()
    {
      Dictionary<string, object> result = new();
      logger.Trace(nameof(SerializeHandlers) + " called for " + ToString());
      lock (_handlerLock)
      {
        foreach (var handler in _handlers.Where(h => h.Value.HasDataToSerialize))
        {
          result.Add(handler.Key, handler.Value.ToSerializedObject());
        }
      }
      return result;
    }

    // public so derived classes can implement ISerializable
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (HasDataToSerialize)
      {
        info.AddValue(SerializedHandlerName, ToSerializedObject());
      }
    }

    #region IDictionary overrides

    public int Count => _handlers.Count;

    public ICollection<string> Keys
      => _handlers.Keys;

    public ICollection<BaseHandler> Values
          => _handlers.Values;

    public void Add(string key, BaseHandler value)
    {
      lock (_handlerLock)
      {
        _handlers.Add(key, value);
      }
    }

    public void Add(KeyValuePair<string, BaseHandler> item)
    {
      lock (_handlerLock)
      {
        _handlers.Add(item.Key, item.Value);
      }
    }

    public void Clear()
    {
      lock (_handlerLock)
      {
        _handlers.Clear();
      }
    }

    public bool Contains(KeyValuePair<string, BaseHandler> item)
    {
      lock (_handlerLock)
      {
        return _handlers.Contains(item);
      }
    }

    public bool ContainsKey(string key)
    {
      lock (_handlerLock)
      {
        return _handlers.ContainsKey(key);
      }
    }

    public bool Remove(string key)
    {
      lock (_handlerLock)
      {
        return _handlers.Remove(key);
      }
    }

    public bool Remove(KeyValuePair<string, BaseHandler> item)
    {
      lock (_handlerLock)
      {
        return _handlers.Remove(item.Key);
      }
    }

    public bool TryGetValue(string key, out BaseHandler value)
    {
      lock (_handlerLock)
      {
        return _handlers.TryGetValue(key, out value);
      }
    }

    #endregion IDictionary overrides
  }
}