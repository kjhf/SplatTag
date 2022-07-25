using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class TeamsHandler :
    BaseSourcedItemHandler<TeamId>,
    ISerializable
  {
    public const string SerializationName = "TIds";

    public TeamsHandler()
    {
    }

    public TeamId? CurrentTeam => mostRecentItem == default ? null : mostRecentItem;

    public override string SerializedHandlerName => SerializationName;

    /// <summary>
    /// Correct the item ids for this player given a merge result (containing old id --> the replacement id)
    /// Returns if any work was done.
    /// </summary>
    public bool CorrectTeamIds(IReadOnlyDictionary<TeamId, TeamId> teamsMergeResult)
    {
      // Quick out for 0 count
      if (items.Count == 0 || teamsMergeResult.Count == 0 || mostRecentItem == default)
      {
        return false;
      }
      // else
      bool workDone = false;

      // Correct the most recent reference.
      if (teamsMergeResult.TryGetValue(mostRecentItem, out TeamId newRecentId))
      {
        mostRecentItem = newRecentId;
      }

      // Correct the teams.
      foreach (var pair in items.ToArray())
      {
        // If the merge result has this id changed, update the id.
        if (teamsMergeResult.TryGetValue(pair.Key, out TeamId newId))
        {
          items.Remove(pair.Key);
          items[newId] = pair.Value;
          workDone = true;
        }
      }

      return workDone;
    }

    /// <summary>
    /// Get a collection of all teams, sourced and ordered.
    /// </summary>
    public KeyValuePair<TeamId, ReadOnlyCollection<Source>>[] GetAllTeamsOrdered()
      => GetItemsSourcedOrdered();

    /// <summary>
    /// Get a collection of all teams, unordered.
    /// </summary>
    public IReadOnlyCollection<TeamId> GetAllTeamsUnordered()
      => GetItemsUnordered();

    public override FilterOptions GetMatchReason() => FilterOptions.TeamName;

    /// <summary>
    /// Get all the teams and their sources in an unordered collection.
    /// </summary>
    public IReadOnlyDictionary<TeamId, IReadOnlyList<Source>> GetTeamsSourcedUnordered()
      => GetItemsSourcedUnordered();

    #region Serialization

    // Deserialize
    protected TeamsHandler(SerializationInfo info, StreamingContext context)
    {
      DeserializeBaseSourcedItems(info, context);
    }

    /// <summary>Serialize</summary>
    /// <remarks>Handled in <see cref="BaseSourcedItemHandler{T}.GetObjectData(SerializationInfo, StreamingContext)"/>.</remarks>

    #endregion Serialization
  }
}