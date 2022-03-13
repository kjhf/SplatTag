using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class DivisionsHandler : SourcedItemHandler<Division>, ISerializable
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

    public static Team? GetBestByDiv(IEnumerable<Team> teams)
    {
      return (teams?.Any() != true)
        ? null
        : teams.OrderBy(t => t.GetBestDiv() ?? Division.Unknown).FirstOrDefault();
    }

    /// <summary>
    /// Get the best division, or null if not known.
    /// </summary>
    /// <param name="lastNDivisions">Limit the search to this many divisions in most-recent chronological order (-1 = no limit)</param>
    public Division? GetBestDiv(int lastNDivisions = -1)
    {
      if (CurrentDivision == null)
      {
        return null;
      }
      // else
      IEnumerable<Division> divs =
        (lastNDivisions == -1) ?
          this.GetDivisionsUnordered() :
          // Take is a limit operation (does not throw if limit > count)
          this.GetDivisionsOrdered().Take(lastNDivisions);

      var bestDiv = divs.Min();
      return bestDiv.IsUnknown ? null : bestDiv;
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
      if (Count > 0)
      {
        info.AddValue("D", OrderedItems.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value.Select(s => s.Id)));
      }
    }

    #endregion Serialization
  }
}