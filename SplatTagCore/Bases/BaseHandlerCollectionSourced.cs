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
  public abstract class BaseHandlerCollectionSourced<T> : BaseHandlerSourced<T>, IDictionary<string, BaseHandler>
    where T : BaseHandlerCollectionSourced<T>
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Dictionary of handlers, keyed by its serialization name.
    /// </summary>
    protected internal readonly Dictionary<string, BaseHandler> handlers = new();

    protected BaseHandlerCollectionSourced()
    {
      InitialiseHandlers();
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
    /// Get how this object matches another, or <see cref="FilterOptions.None"/> if they do not.
    /// </summary>
    public override FilterOptions MatchWithReason(T other)
    {
      FilterOptions options = FilterOptions.None;
      foreach (var handlerPair in handlers)
      {
        options |= handlerPair.Value.MatchWithReason(other.handlers[handlerPair.Key]);
      }
      return options;
    }

    /// <summary>
    /// Merge all the handlers in this collection.
    /// Handles Sources and timings.
    /// </summary>
    public override void Merge(T other)
    {
      if (other == null)
      {
        logger.Error("Attempted to merge a null handler. Returning early. " + ToString());
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

    protected virtual void DeserializeHandlers(SerializationInfo info, StreamingContext _)
    {
      foreach (var handler in handlers)
      {
        BaseHandler? baseHandler = info.GetValueOrDefault<BaseHandler>(handler.Key);
        if (baseHandler != null)
        {
          handler.Value.Merge(baseHandler);
        }
      }
    }

    protected abstract void InitialiseHandlers();

    protected void SerializeHandlers(SerializationInfo info, StreamingContext context)
    {
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