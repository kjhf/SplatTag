using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using static SplatTagCore.JSONConverters;

namespace SplatTagCore
{
  public record Placement
  {
    public Placement(
      Dictionary<int, Guid[]>? playersByPlacement = null,
      Dictionary<int, Guid[]>? teamsByPlacement = null)
    {
      this.PlayersByPlacement = playersByPlacement ?? new Dictionary<int, Guid[]>();
      this.TeamsByPlacement = teamsByPlacement ?? new Dictionary<int, Guid[]>();
    }

    /// <summary>
    /// Players ordered by placement.
    /// </summary>
    [JsonPropertyName("PlayersByPlacement")]
    [JsonConverter(typeof(GuidArrayConverter))]
    public Dictionary<int, Guid[]> PlayersByPlacement { get; }

    /// <summary>
    /// Teams ordered by placement.
    /// </summary>
    [JsonPropertyName("TeamsByPlacement")]
    [JsonConverter(typeof(GuidArrayConverter))]
    public Dictionary<int, Guid[]> TeamsByPlacement { get; }

    /// <summary>
    /// Get if this Placements object has placements.
    /// </summary>
    [JsonIgnore]
    public bool HasPlacements => PlayersByPlacement.Count > 0 && TeamsByPlacement.Count > 0;
  }
}