using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;
using static SplatTagCore.JSONConverters;

namespace SplatTagCore
{
  [Serializable]
  public class TeamsHandler : SourcedItemHandler<Guid>
  {
    public TeamsHandler()
    {
    }

    [JsonPropertyName("T")]
    protected Dictionary<string, string[]> Model
    {
      get => OrderedItems.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value.Select(s => s.Id).ToArray());
      set
      {
        if (GuidToSourceConverter.Instance != null)
        {
          Merge(value.ToDictionary(pair => Guid.Parse(pair.Key), pair => GuidToSourceConverter.Instance.Convert(pair.Value).ToList()));
        }
        else
        {
          Merge(value.ToDictionary(pair => Guid.Parse(pair.Key), pair => pair.Value.Select(s => new Source(s)).ToList()));
        }
      }
    }

    [JsonIgnore]
    public Guid? CurrentTeam => mostRecentItem == default ? null : mostRecentItem;

    /// <summary>
    /// Get a collection of all teams, unordered.
    /// </summary>
    public IReadOnlyCollection<Guid> GetAllTeamsUnordered()
      => GetItemsUnordered();

    /// <summary>
    /// Get a collection of all teams, sourced and ordered.
    /// </summary>
    public KeyValuePair<Guid, ReadOnlyCollection<Source>>[] GetAllTeamsOrdered()
      => GetItemsSourcedOrdered();

    /// <summary>
    /// Get all the teams and their sources in an unordered collection.
    /// </summary>
    public IReadOnlyDictionary<Guid, IReadOnlyList<Source>> GetTeamsSourcedUnordered()
      => GetItemsSourcedUnordered();

    /// <summary>
    /// Correct the item ids for this player given a merge result (containing old id --> the replacement id)
    /// Returns if any work was done.
    /// </summary>
    public bool CorrectTeamIds(IDictionary<Guid, Guid> teamsMergeResult)
    {
      // Quick out for 0 count
      if (items.Count == 0 || teamsMergeResult.Count == 0 || mostRecentItem == default)
      {
        return false;
      }
      // else
      bool workDone = false;

      // Correct the most recent reference.
      if (teamsMergeResult.TryGetValue(MostRecent, out Guid newRecentId))
      {
        mostRecentItem = newRecentId;
      }

      // Correct the teams.
      foreach (var pair in items.ToArray())
      {
        // If the merge result has this id changed, update the id.
        if (teamsMergeResult.TryGetValue(pair.Key, out Guid newId))
        {
          items.Remove(pair.Key);
          items[newId] = pair.Value;
          workDone = true;
        }
      }

      return workDone;
    }
  }
}