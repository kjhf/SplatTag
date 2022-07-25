using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public record Placement : ICoreObject
  {
    public Placement(Dictionary<int, Guid[]>? players = null, Dictionary<int, Guid[]>? teams = null)
    {
      this.PlayersByPlacement = players ?? new Dictionary<int, Guid[]>();
      this.TeamsByPlacement = teams ?? new Dictionary<int, Guid[]>();
    }

    /// <summary>
    /// Players ordered by placement.
    /// </summary>
    public Dictionary<int, Guid[]> PlayersByPlacement { get; }

    /// <summary>
    /// Teams ordered by placement.
    /// </summary>
    public Dictionary<int, Guid[]> TeamsByPlacement { get; }

    /// <summary>
    /// Get if this Placements object has placements.
    /// </summary>
    public bool HasPlacements => PlayersByPlacement.Count > 0 && TeamsByPlacement.Count > 0;

    /// <summary>
    /// Overridden ToString in form "Placement: playerCount players and teamCount teams"
    /// </summary>
    public override string ToString() => $"Placement: {PlayersByPlacement.Count} players and {TeamsByPlacement} teams";

    public string GetDisplayValue() => ToString();

    public bool Equals(ICoreObject other) => Equals(other as Placement);

    #region Serialization

    // Deserialize
    protected Placement(SerializationInfo info, StreamingContext _)
    {
      this.PlayersByPlacement =
        info.GetValueOrDefault("PlayersByPlacement", new Dictionary<string, Guid[]>())
        .Where(pair => pair.Value.Length > 0)
        .ToDictionary(pair => { int.TryParse(pair.Key, out var place); return place; }, pair => pair.Value); // If not parsed, this uses the default 0

      this.TeamsByPlacement =
        info.GetValueOrDefault("TeamsByPlacement", new Dictionary<string, Guid[]>())
        .Where(pair => pair.Value.Length > 0)
        .ToDictionary(pair => { int.TryParse(pair.Key, out var place); return place; }, pair => pair.Value);
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext _)
    {
      if (this.PlayersByPlacement.Count > 0)
        info.AddValue("PlayersByPlacement", this.PlayersByPlacement.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value));

      if (this.TeamsByPlacement.Count > 0)
        info.AddValue("TeamsByPlacement", this.TeamsByPlacement.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value));
    }

    #endregion Serialization
  }
}