using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class DivisionsHandler : ISerializable
  {
    /// <summary>
    /// Back-store for the divisions
    /// </summary>
    private readonly Dictionary<Division, List<Source>> divisions = new();

    /// <summary>
    /// Back-store for quick access to the most recent source.
    /// </summary>
    private Source? mostRecentSource;

    /// <summary>
    /// Back-store for quick access to the most recent division (current division).
    /// </summary>
    private Division? mostRecentDivision;

    public DivisionsHandler()
    {
    }

    /// <summary>
    /// Get the number of divisions.
    /// </summary>
    public int Count => divisions.Count;

    /// <summary>
    /// Get the most recent division.
    /// </summary>
    public Division? CurrentDivision => mostRecentDivision;

    /// <summary>
    /// Get all the divisions and their sources as an ordered enumerable from most recent division to oldest.
    /// </summary>
    public IOrderedEnumerable<KeyValuePair<Division, List<Source>>> OrderedDivisions => divisions.OrderByDescending(pair => pair.Value.Max());

    /// <summary>
    /// Add a division to this handler.
    /// </summary>
    /// <param name="incoming">Division to add guid</param>
    /// <param name="source">The source this division comes from</param>
    public void Add(Division incoming, Source source) => Add(new[] { incoming }, source);

    /// <summary>
    /// Add a division and its sources to this handler.
    /// Will not add if there is no source.
    /// </summary>
    /// <param name="incoming">Division to add</param>
    /// <param name="sources">The sources this division comes from</param>
    public void Add(Division incoming, IList<Source> sources)
    {
      if (sources.Count == 0) return;
      if (incoming.IsUnknown) return;
      var latestSource = sources.Count == 1 ? sources[0] : sources.Max();

      if (mostRecentSource == null || latestSource.CompareTo(mostRecentSource) > 0)
      {
        mostRecentSource = latestSource;
        mostRecentDivision = incoming;
      }

      if (divisions.ContainsKey(incoming))
      {
        divisions[incoming].AddRange(sources);
      }
      else
      {
        divisions[incoming] = sources.ToList();
      }
    }

    /// <summary>
    /// Add divisions to this handler.
    /// </summary>
    /// <param name="incoming">Divisions of divisions to add</param>
    /// <param name="source">The source these divisions come from</param>
    public void Add(IList<Division> incoming, Source source)
    {
      if (incoming.Count == 0) return;
      if (mostRecentSource == null || source.CompareTo(mostRecentSource) > 0)
      {
        mostRecentSource = source;
        mostRecentDivision = incoming[0];
      }

      foreach (var divisionToAdd in incoming)
      {
        if (divisions.ContainsKey(divisionToAdd))
        {
          divisions[divisionToAdd].Add(source);
        }
        else
        {
          divisions[divisionToAdd] = new List<Source> { source };
        }
      }
    }

    /// <summary>
    /// Get if the handler has this division.
    /// </summary>
    public bool Contains(Division division) => divisions.ContainsKey(division);

    /// <summary>
    /// Get a collection of old divisions, unordered.
    /// </summary>
    public IReadOnlyCollection<Division> GetOldDivisionsUnordered()
    {
      if (mostRecentDivision == null) return Array.Empty<Division>();
      var hashSet = new HashSet<Division>(divisions.Keys);
      hashSet.Remove((Division)mostRecentDivision);
      return hashSet;
    }

    /// <summary>
    /// Get all the divisions as an ordered list from most recent division to oldest.
    /// </summary>
    public IReadOnlyList<Division> GetDivisionsOrdered() => OrderedDivisions.Select(pair => pair.Key).ToArray();

    /// <summary>
    /// Get all the divisions and their sources in an unordered collection.
    /// </summary>
    public IReadOnlyDictionary<Division, IReadOnlyList<Source>> GetDivisionsSourcedUnordered()
    {
      if (mostRecentDivision == null) return new Dictionary<Division, IReadOnlyList<Source>>();
      return (IReadOnlyDictionary<Division, IReadOnlyList<Source>>)divisions.ToDictionary(pair => pair.Key, pair => pair.Value.AsReadOnly());
    }

    /// <summary>
    /// Get all the divisions in an unordered collection.
    /// </summary>
    public IReadOnlyCollection<Division> GetDivisionsUnordered()
    {
      if (mostRecentDivision == null) return Array.Empty<Division>();
      return divisions.Keys;
    }

    /// <summary>
    /// Return if this handler matches another.
    /// </summary>
    public bool Match(DivisionsHandler other) => GetDivisionsUnordered().GenericMatch(other.GetDivisionsUnordered());

    /// <summary>
    /// Merge this division handler with another.
    /// </summary>
    internal void Merge(DivisionsHandler divisionInformation) => Merge(divisionInformation.divisions);

    /// <summary>
    /// Merge this division handler with another.
    /// </summary>
    private void Merge(Dictionary<Division, List<Source>> incoming)
    {
      foreach (var pair in incoming)
      {
        Add(pair.Key, pair.Value);
      }
    }

    public override string ToString()
    {
      return $"{Count} divisions{(CurrentDivision != null ? $", current: {CurrentDivision}" : "")}";
    }

    #region Serialization

    // Deserialize
    protected DivisionsHandler(SerializationInfo info, StreamingContext context)
    {
      Source.GuidToSourceConverter? converter = context.Context as Source.GuidToSourceConverter;
      var val = info.GetValueOrDefault("D", new Dictionary<string, List<string>>());
      Merge(val.ToDictionary(pair => new Division(pair.Key), pair => (converter?.Convert(pair.Value) ?? pair.Value.Select(s => new Source(s))).ToList()));
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext _)
    {
      if (this.divisions.Count > 0)
      {
        info.AddValue("D", this.OrderedDivisions.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value.Select(s => s.Id)));
      }
    }

    #endregion Serialization
  }
}