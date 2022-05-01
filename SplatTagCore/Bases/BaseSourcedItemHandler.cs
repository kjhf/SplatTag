using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  /// <summary>
  /// Base class for <see cref="BaseHandlerSourced{T}"/> classes that also have classes that are sourced against it.
  /// </summary>
  public abstract class BaseSourcedItemHandler<T> :
    BaseHandlerSourced<BaseSourcedItemHandler<T>>
    where T : notnull
  {
    /// <summary>
    /// Back-store for the items this Handler handles.
    /// </summary>
    /// <remarks>
    /// Though a HashSet may seem more performant for the sources list, for collections with
    /// a small number of elements (under 20), List is actually better
    /// https://stackoverflow.com/questions/150750/hashset-vs-list-performance
    /// </remarks>
    protected readonly Dictionary<T, List<Source>> items = new();

    /// <summary>
    /// Back-store for quick access to the most recent item (current).
    /// </summary>
    protected T? mostRecentItem = default;

    private const string SerializedItemsName = "Items";
    private const string SerializedSourceName = "S";
    private const string SerializedItemName = "V";

    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    protected BaseSourcedItemHandler()
    {
    }

    /// <summary>
    /// Get the number of this sourced entity.
    /// </summary>
    public int Count => items.Count;

    /// <inheritdoc/>
    public override bool HasDataToSerialize => items.Count > 0;

    /// <summary>
    /// Get the most recent item.
    /// </summary>
    public T? MostRecent => mostRecentItem;

    /// <summary>
    /// Get the name to serialize the items under.
    /// </summary>
    public abstract string SerializedHandlerName { get; }

    public override IReadOnlyList<Source> Sources => items.Values.SelectMany(x => x).Distinct().ToList();

    /// <summary>
    /// Get all the items and their sources as an ordered enumerable from most recent item to oldest.
    /// </summary>
    protected IOrderedEnumerable<KeyValuePair<T, List<Source>>> OrderedItems => items.OrderByDescending(pair => pair.Value.Max());

    /// <summary>
    /// Add an item and its one source to this handler.
    /// </summary>
    /// <param name="incoming">Item to add</param>
    /// <param name="source">The source this item comes from</param>
    public virtual void Add(T incoming, Source source)
      => Add(new[] { incoming }, source);

    /// <summary>
    /// Add an item and its sources to this handler.
    /// Will not add if there is no source.
    /// Handles sources' times.
    /// </summary>
    /// <param name="incoming">The item to add</param>
    /// <param name="sources">The sources this incoming item comes from</param>
    public virtual void Add(T incoming, IEnumerable<Source> sources)
    {
      switch (sources.Count())
      {
        case 0:
          return;

        case 1:
          Add(new[] { incoming }, sources.ToList(), sources.First());
          break;

        default:
        {
          var latestSource = sources.Max();
          Add(new[] { incoming }, sources.ToList(), latestSource);
          break;
        }
      }
    }

    /// <summary>
    /// Add items to this handler.
    /// </summary>
    /// <param name="incoming">Items to add</param>
    /// <param name="source">The source these item come from</param>
    public virtual void Add(IList<T> incoming, Source source)
      => Add(incoming, new List<Source> { source }, source);

    /// <summary>
    /// Get if the handler has this item.
    /// </summary>
    public bool Contains(T? item) => item != null && items.ContainsKey(item);

    /// <summary>
    /// Get all the items as an ordered list from most recent item to oldest.
    /// </summary>
    public IReadOnlyList<T> GetItemsOrdered() => OrderedItems.Select(pair => pair.Key).ToArray();

    /// <summary>
    /// Get all the items as an ordered array from most recent item to oldest.
    /// </summary>
    public KeyValuePair<T, ReadOnlyCollection<Source>>[] GetItemsSourcedOrdered()
      => OrderedItems.Select(pair => KeyValuePair.Create(pair.Key, pair.Value.AsReadOnly())).ToArray();

    /// <summary>
    /// Get all the items and their sources in an unordered collection.
    /// </summary>
    public IReadOnlyDictionary<T, IReadOnlyList<Source>> GetItemsSourcedUnordered()
    {
      if (mostRecentItem == null) return new Dictionary<T, IReadOnlyList<Source>>();
      return items.ToDictionary(pair => pair.Key, pair => (IReadOnlyList<Source>)pair.Value.AsReadOnly());
    }

    /// <summary>
    /// Get all the items in an unordered collection.
    /// </summary>
    public IReadOnlyCollection<T> GetItemsUnordered()
    {
      if (mostRecentItem == null) return Array.Empty<T>();
      return items.Keys;
    }

    /// <summary>
    /// If the Sourced Item Handler generic matches in the <see cref="MatchWithReason(object)"/> function, get the reason why.
    /// </summary>
    public abstract FilterOptions GetMatchReason();

    /// <summary>
    /// Get a collection of old items which excludes the current, unordered.
    /// </summary>
    public IReadOnlyCollection<T> GetOldItemsUnordered()
    {
      if (mostRecentItem == null) return Array.Empty<T>();
      var hashSet = new HashSet<T>(items.Keys);
      hashSet.Remove(mostRecentItem);
      return hashSet;
    }

    /// <summary>
    /// Get the sources for the specified item.
    /// </summary>
    public IReadOnlyList<Source> GetSourcesForItem(T item)
    {
      if (items.TryGetValue(item, out List<Source> sources))
      {
        return sources;
      }
      return Array.Empty<Source>();
    }

    /// <summary>
    /// Return if this handler matches another.
    /// </summary>
    public virtual bool Match(BaseSourcedItemHandler<T> other)
      => GetItemsUnordered().GenericMatch(other.GetItemsUnordered());

    /// <summary>
    /// Get if this handler matches another with a Filter Reason specified by <see cref="GetMatchReason"/> or <see cref="FilterOptions.None"/>
    /// </summary>
    public override FilterOptions MatchWithReason(BaseSourcedItemHandler<T> other)
    {
      if (other == null)
      {
        logger.Error("Attempted to match a null handler. Returning None. " + ToString());
        return FilterOptions.None;
      }

      if (ReferenceEquals(other, this)) return (FilterOptions)(-1); // Literally same
      return Match(other) ? GetMatchReason() : FilterOptions.None;
    }

    /// <summary>
    /// Merge this team handler with another.
    /// Handles sources and timing.
    /// </summary>
    public override void Merge(BaseSourcedItemHandler<T> other) => Merge(other.items);

    public override string ToString()
    {
      return $"{Count} items{(MostRecent != null ? $", current: {MostRecent}" : "")}";
    }

    /// <summary>
    /// Merge this team handler with another.
    /// Handles sources and timing.
    /// </summary>
    protected void Merge(Dictionary<T, List<Source>> incoming)
    {
      foreach (var pair in incoming)
      {
        Add(pair.Key, pair.Value);
      }
    }

    /// <summary>
    /// Internal working to the add system where the latest source needs to be saved and the items added to the dictionary.
    /// </summary>
    /// <param name="incoming"></param>
    /// <param name="sources"></param>
    /// <param name="latestSource"></param>
    private void Add(IList<T> incoming, List<Source> sources, Source latestSource)
    {
      if (incoming.Count == 0) return;
      if (mostRecentSource == null || mostRecentItem == null || latestSource.CompareTo(mostRecentSource) > 0)
      {
        mostRecentSource = latestSource;
        mostRecentItem = incoming[0];
      }

      foreach (var teamToAdd in incoming)
      {
        if (items.ContainsKey(teamToAdd))
        {
          items[teamToAdd].AddRange(sources);
        }
        else
        {
          items[teamToAdd] = sources;
        }
      }
    }

    #region Serialization

    protected virtual void DeserializeBaseSourcedItems(SerializationInfo info, StreamingContext context)
    {
      Source.SourceStringConverter? converter = context.Context as Source.SourceStringConverter ?? new Source.SourceStringConverter();

      var val = info.GetValueOrDefault(SerializedItemsName, Array.Empty<Dictionary<string, object>>());
      if (val.Length > 0)
      {
        foreach (var pair in val)
        {
          var item = pair[SerializedItemName];
          if (item != null)
          {
            var sourceStrings = (string[])pair.Get(SerializedSourceName, Array.Empty<string>());
            var sources = converter.Convert(sourceStrings.Where(s => !string.IsNullOrWhiteSpace(s)));
            if (item is T t)
            {
              Add(t, sources);
            }
            else if (item is string str)
            {
              if (typeof(T) == typeof(Guid))
              {
                Add((T)(object)Guid.Parse(str), sources);
              }
              else
              {
                string error = "Cannot handle string " + str + " from T type " + typeof(T).Name;
                logger.Error(error);
                throw new NotImplementedException(error);
              }
            }
            else
            {
              string error = "Cannot handle item type " + item.GetType().Name + " from T type " + typeof(T).Name;
              logger.Error(error);
              throw new NotImplementedException(error);
            }
          }
        }
      }
    }

    /// <summary>
    /// Reads the handler name that's been serialized. For most cases, this won't be needed as the name is constant.
    /// But for handlers that are not-abstract but multi-instance, this may be required to deserialize the handler.
    /// </summary>
    protected string ReadSerializedHandlerName(SerializationInfo info) => info.AsKeyValuePairs().FirstOrDefault(pair => pair.Key != SerializedSourceName).Key;

    protected void SerializeBaseSourcedItems(SerializationInfo info, StreamingContext _)
    {
      if (HasDataToSerialize)
      {
        // e.g.
        // Items { [
        //    {
        //        V: {
        //          (item property)
        //        }
        //        S: [
        //          "id1", "id2", "id3" etc
        //        ]
        //    }
        // ]}
        var serializable = GetItemsSourcedUnordered().Select(pair => new Dictionary<string, object> {
            { SerializedItemName, pair.Key },
            { SerializedSourceName, pair.Value.Select(s => s.Id).ToArray() }
          }
        ).ToArray();
        if (logger.IsErrorEnabled && serializable.Length == 0)
        {
          logger.Error("Unexpected empty return from GetItemsSourcedUnordered. " + ToString());
        }
        info.AddValue(SerializedItemsName, serializable);
      }
    }

    #endregion Serialization
  }
}