using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class TeamsHandler : ISerializable
  {
    /// <summary>
    /// Back-store for the teams guids
    /// </summary>
    private readonly Dictionary<Guid, List<Source>> teams = new();

    /// <summary>
    /// Back-store for quick access to the most recent source.
    /// </summary>
    private Source? mostRecentSource;

    /// <summary>
    /// Back-store for quick access to the most recent team (current team).
    /// </summary>
    private Guid? mostRecentTeam;

    public TeamsHandler()
    {
    }

    /// <summary>
    /// Get the number of teams.
    /// </summary>
    public int Count => teams.Count;

    /// <summary>
    /// Get the most recent team.
    /// </summary>
    public Guid? CurrentTeam => mostRecentTeam;

    /// <summary>
    /// Get all the teams and their sources as an ordered enumerable from most recent team to oldest.
    /// </summary>
    public IOrderedEnumerable<KeyValuePair<Guid, List<Source>>> OrderedTeams => teams.OrderByDescending(pair => pair.Value.Max());

    /// <summary>
    /// Add a team to this handler.
    /// </summary>
    /// <param name="incoming">Team to add guid</param>
    /// <param name="source">The source this team comes from</param>
    public void Add(Guid incoming, Source source) => Add(new[] { incoming }, source);

    /// <summary>
    /// Add a team and its sources to this handler.
    /// Will not add if there is no source.
    /// </summary>
    /// <param name="incoming">Team to add guid</param>
    /// <param name="sources">The sources this team comes from</param>
    public void Add(Guid incoming, IList<Source> sources)
    {
      if (sources.Count == 0) return;
      var latestSource = sources.Count == 1 ? sources[0] : sources.Max();

      if (mostRecentSource == null || latestSource.CompareTo(mostRecentSource) > 0)
      {
        mostRecentSource = latestSource;
        mostRecentTeam = incoming;
      }

      if (teams.ContainsKey(incoming))
      {
        teams[incoming].AddRange(sources);
      }
      else
      {
        teams[incoming] = sources.ToList();
      }
    }

    /// <summary>
    /// Add teams to this handler.
    /// </summary>
    /// <param name="incoming">Guids of teams to add</param>
    /// <param name="source">The source these teams come from</param>
    public void Add(IList<Guid> incoming, Source source)
    {
      if (incoming.Count == 0) return;
      if (mostRecentSource == null || source.CompareTo(mostRecentSource) > 0)
      {
        mostRecentSource = source;
        mostRecentTeam = incoming[0];
      }

      foreach (var teamToAdd in incoming)
      {
        if (teams.ContainsKey(teamToAdd))
        {
          teams[teamToAdd].Add(source);
        }
        else
        {
          teams[teamToAdd] = new List<Source> { source };
        }
      }
    }

    /// <summary>
    /// Get if the handler has this team.
    /// </summary>
    public bool Contains(Guid? team) => team != null && teams.ContainsKey(team.Value);

    /// <summary>
    /// Correct the team ids for this player given a merge result (containing old id --> the replacement id)
    /// Returns if any work was done.
    /// </summary>
    public bool CorrectTeamIds(IDictionary<Guid, Guid> teamsMergeResult)
    {
      // Quick out for 0 count
      if (teams.Count == 0 || teamsMergeResult.Count == 0 || mostRecentTeam == null)
      {
        return false;
      }
      // else
      bool workDone = false;

      // Correct the most recent reference.
      if (teamsMergeResult.TryGetValue(mostRecentTeam.Value, out Guid newRecentId))
      {
        mostRecentTeam = newRecentId;
      }

      // Correct the teams.
      foreach (var pair in teams.ToArray())
      {
        // If the merge result has this id changed, update the id.
        if (teamsMergeResult.TryGetValue(pair.Key, out Guid newId))
        {
          teams.Remove(pair.Key);
          teams[newId] = pair.Value;
          workDone = true;
        }
      }

      return workDone;
    }

    /// <summary>
    /// Get a collection of old teams, unordered.
    /// </summary>
    public IReadOnlyCollection<Guid> GetOldTeamsUnordered()
    {
      if (mostRecentTeam == null) return Array.Empty<Guid>();
      var hashSet = new HashSet<Guid>(teams.Keys);
      hashSet.Remove((Guid)mostRecentTeam);
      return hashSet;
    }

    /// <summary>
    /// Get the sources for the specified team.
    /// </summary>
    public IReadOnlyList<Source> GetSourcesForTeam(Guid team)
    {
      if (teams.TryGetValue(team, out List<Source> sources))
      {
        return sources;
      }
      return Array.Empty<Source>();
    }

    /// <summary>
    /// Get all the teams as an ordered list from most recent team to oldest.
    /// </summary>
    public IReadOnlyList<Guid> GetTeamsOrdered() => OrderedTeams.Select(pair => pair.Key).ToArray();

    /// <summary>
    /// Get all the teams and their sources in an unordered collection.
    /// </summary>
    public IReadOnlyDictionary<Guid, IReadOnlyList<Source>> GetTeamsSourcedUnordered()
    {
      if (mostRecentTeam == null) return new Dictionary<Guid, IReadOnlyList<Source>>();
      return teams.ToDictionary(pair => pair.Key, pair => (IReadOnlyList<Source>)pair.Value.AsReadOnly());
    }

    /// <summary>
    /// Get all the teams in an unordered collection.
    /// </summary>
    public IReadOnlyCollection<Guid> GetTeamsUnordered()
    {
      if (mostRecentTeam == null) return Array.Empty<Guid>();
      return teams.Keys;
    }

    /// <summary>
    /// Return if this handler matches another.
    /// </summary>
    public bool Match(TeamsHandler other) => GetTeamsUnordered().GenericMatch(other.GetTeamsUnordered());

    /// <summary>
    /// Merge this team handler with another.
    /// </summary>
    internal void Merge(TeamsHandler teamInformation) => Merge(teamInformation.teams);

    /// <summary>
    /// Merge this team handler with another.
    /// </summary>
    private void Merge(Dictionary<Guid, List<Source>> incoming)
    {
      foreach (var pair in incoming)
      {
        Add(pair.Key, pair.Value);
      }
    }

    public override string ToString()
    {
      return $"{Count} teams{(CurrentTeam != null ? $", current: {CurrentTeam}" : "")}";
    }

    #region Serialization

    // Deserialize
    protected TeamsHandler(SerializationInfo info, StreamingContext context)
    {
      Source.GuidToSourceConverter? converter = context.Context as Source.GuidToSourceConverter;
      var val = info.GetValueOrDefault("T", new Dictionary<Guid, List<string>>());
      Merge(val.ToDictionary(pair => pair.Key, pair => (converter?.Convert(pair.Value) ?? pair.Value.Select(s => new Source(s))).ToList()));
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext _)
    {
      if (this.teams.Count > 0)
      {
        info.AddValue("T", this.OrderedTeams.ToDictionary(pair => pair.Key, pair => pair.Value.Select(s => s.Id)));
      }
    }

    #endregion Serialization
  }
}