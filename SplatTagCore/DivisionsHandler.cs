using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class DivisionsHandler : SourcedHandlerBase<Division>, ISerializable
  {
    public DivisionsHandler()
    {
    }

    /// <summary>
    /// Get the most recent division.
    /// </summary>
    public Division? CurrentDivision => mostRecentItem;

    /// <summary>
    /// Get all the divisions as an ordered list from most recent division to oldest.
    /// </summary>
    public IReadOnlyList<Division> GetDivisionsOrdered() => GetItemsOrdered();

    public IReadOnlyCollection<Division> GetDivisionsUnordered() => GetItemsUnordered();

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
      if (Count > 0)
      {
        info.AddValue("D", OrderedItems.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value.Select(s => s.Id)));
      }
    }

    #endregion Serialization
  }
}