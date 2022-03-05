using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class FriendCodesHandler : ISerializable, IReadonlySourceable
  {
    /// <summary>
    /// Back-store for the friend codes
    /// </summary>
    private readonly Dictionary<FriendCode, List<Source>> codes = new();

    public IReadOnlyList<Source> Sources => codes.Values.SelectMany(x => x).Distinct().ToList();

    public FriendCodesHandler()
    {
    }

    /// <summary>
    /// Get the number of codes.
    /// </summary>
    public int Count => codes.Count;

    /// <summary>
    /// Get all the codes and their sources as an ordered enumerable from most recent code to oldest.
    /// </summary>
    public IOrderedEnumerable<KeyValuePair<FriendCode, List<Source>>> OrderedCodes => codes.OrderByDescending(pair => pair.Value.Max());

    /// <summary>
    /// Add a code to this handler.
    /// </summary>
    /// <param name="incoming">Code to add</param>
    /// <param name="source">The source this code comes from</param>
    public void Add(FriendCode incoming, Source source) => Add(new[] { incoming }, source);

    /// <summary>
    /// Add a code and its sources to this handler.
    /// Will not add if there is no source.
    /// </summary>
    /// <param name="incoming">Code to add</param>
    /// <param name="sources">The sources this code comes from</param>
    public void Add(FriendCode incoming, IList<Source> sources)
    {
      if (incoming.NoCode || sources.Count == 0)
        return;

      if (codes.ContainsKey(incoming))
      {
        codes[incoming].AddRange(sources);
      }
      else
      {
        codes[incoming] = sources.ToList();
      }
    }

    /// <summary>
    /// Add codes to this handler.
    /// </summary>
    /// <param name="incoming">Codes to add</param>
    /// <param name="source">The source these codes come from</param>
    public void Add(IList<FriendCode> incoming, Source source)
    {
      if (incoming.Count == 0) return;

      foreach (var fcToAdd in incoming)
      {
        if (fcToAdd.NoCode)
        {
          continue;
        }

        if (codes.ContainsKey(fcToAdd))
        {
          codes[fcToAdd].Add(source);
        }
        else
        {
          codes[fcToAdd] = new List<Source> { source };
        }
      }
    }

    /// <summary>
    /// Get if the handler has this code.
    /// </summary>
    public bool Contains(FriendCode code) => codes.ContainsKey(code);

    /// <summary>
    /// Get the sources for the specified code.
    /// </summary>
    public IReadOnlyList<Source> GetSourcesForCode(FriendCode code)
    {
      if (codes.TryGetValue(code, out List<Source> sources))
      {
        return sources;
      }
      return Array.Empty<Source>();
    }

    /// <summary>
    /// Get all the codes as an ordered list from most recent code to oldest.
    /// </summary>
    public IReadOnlyList<FriendCode> GetCodesOrdered() => OrderedCodes.Select(pair => pair.Key).ToArray();

    /// <summary>
    /// Get all the codes and their sources in an unordered collection.
    /// </summary>
    public IReadOnlyDictionary<FriendCode, IReadOnlyList<Source>> GetCodesSourcedUnordered() =>
      (IReadOnlyDictionary<FriendCode, IReadOnlyList<Source>>)codes.ToDictionary(pair => pair.Key, pair => pair.Value.AsReadOnly());

    /// <summary>
    /// Get all the codes in an unordered collection.
    /// </summary>
    public IReadOnlyCollection<FriendCode> GetCodesUnordered() => codes.Keys;

    /// <summary>
    /// Return if this handler matches another.
    /// </summary>
    public bool Match(FriendCodesHandler other) => GetCodesUnordered().GenericMatch(other.GetCodesUnordered());

    /// <summary>
    /// Merge this code handler with another.
    /// </summary>
    internal void Merge(FriendCodesHandler codeInformation) => Merge(codeInformation.codes);

    /// <summary>
    /// Merge this code handler with another.
    /// </summary>
    private void Merge(Dictionary<FriendCode, List<Source>> incoming)
    {
      foreach (var pair in incoming)
      {
        Add(pair.Key, pair.Value);
      }
    }

    public override string ToString()
    {
      return $"{Count} FCs{(Count > 0 ? $", {codes.First()}" : "")}";
    }

    #region Serialization

    // Deserialize
    protected FriendCodesHandler(SerializationInfo info, StreamingContext context)
    {
      Source.GuidToSourceConverter? converter = context.Context as Source.GuidToSourceConverter;
      var val = info.GetValueOrDefault("C", new Dictionary<string, List<string>>());
      Merge(val.ToDictionary(pair => new FriendCode(ulong.Parse(pair.Key)), pair => (converter?.Convert(pair.Value) ?? pair.Value.Select(s => new Source(s))).ToList()));
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext _)
    {
      if (this.codes.Count > 0)
      {
        info.AddValue("C", this.OrderedCodes.ToDictionary(pair => pair.Key.ToULong(), pair => pair.Value.Select(s => s.Id)));
      }
    }

    #endregion Serialization
  }
}