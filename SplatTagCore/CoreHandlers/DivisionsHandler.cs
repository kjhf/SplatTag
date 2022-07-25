using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class DivisionsHandler :
    BaseSourcedItemHandler<Division>,
    ISerializable
  {
    public const string SerializationName = "Divs";

    public override string SerializedHandlerName => SerializationName;

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

    /// <summary>
    /// If the Sourced Item Handler generic matches in the <see cref="BaseSourcedItemHandler{T}.MatchWithReason(object)"/> function, get the reason why.
    /// </summary>
    public override FilterOptions GetMatchReason()
    {
      // Matching by division has no bearing on the equality
      return FilterOptions.None;
    }

    #region Serialization

    // Deserialize
    protected DivisionsHandler(SerializationInfo info, StreamingContext context)
    {
      DeserializeBaseSourcedItems(info, context);
    }

    /// <summary>Serialize</summary>
    /// <remarks>Handled in <see cref="BaseSourcedItemHandler{T}.GetObjectData(SerializationInfo, StreamingContext)"/>.</remarks>

    #endregion Serialization
  }
}