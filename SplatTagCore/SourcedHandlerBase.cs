using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SplatTagCore
{
  /// <summary>
  /// Base class for Handler classes that define the functionality of sorting and ordering sourced data.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public abstract class SourcedHandlerBase<T> : IReadonlySourceable where T : notnull
  {
    /// <summary>
    /// Back-store for the items this Handler handles.
    /// </summary>
    protected readonly Dictionary<T, List<Source>> items = new();

    /// <summary>
    /// Back-store for quick access to the most recent item (current).
    /// </summary>
    protected T? mostRecentItem = default;

    /// <summary>
    /// Back-store for quick access to the most recent source.
    /// </summary>
    protected Source? mostRecentSource = default;

    /// <summary>
    /// Get the number of this sourced entity.
    /// </summary>
    public int Count => items.Count;

    /// <summary>
    /// Get the most recent item.
    /// </summary>
    public T? MostRecent => mostRecentItem;

    /// <summary>
    /// Get the most recent source.
    /// </summary>
    public Source? MostRecentSource => mostRecentSource;

    public IReadOnlyList<Source> Sources => items.Values.SelectMany(x => x).Distinct().ToList();

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
    public virtual void Add(T incoming, IList<Source> sources)
    {
      switch (sources.Count)
      {
        case 0:
          return;

        case 1:
          Add(new[] { incoming }, sources.ToList(), sources[0]);
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
    /// Internal working to the add system where the latest source needs to be saved and the items added to the dictionary.
    /// </summary>
    /// <param name="incoming"></param>
    /// <param name="sources"></param>
    /// <param name="latestSource"></param>
    private void Add(IList<T> incoming, List<Source> sources, Source latestSource)
    {
      if (incoming.Count == 0) return;
      if (mostRecentSource == null || latestSource.CompareTo(mostRecentSource) > 0)
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
    public virtual bool Match(SourcedHandlerBase<T> other) => GetItemsUnordered().GenericMatch(other.GetItemsUnordered());

    public override string ToString()
    {
      return $"{Count} items{(MostRecent != null ? $", current: {MostRecent}" : "")}";
    }

    /// <summary>
    /// Merge this team handler with another.
    /// </summary>
    internal void Merge(SourcedHandlerBase<T> other) => Merge(other.items);

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
  }
}