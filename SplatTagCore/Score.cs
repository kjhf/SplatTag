using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SplatTagCore
{
  public record Score
  {
    public Score(IList<int>? points = null)
    {
      this.Points = points ?? Array.Empty<int>();
    }

    [JsonIgnore]
    /// <summary>
    /// Displayable description of the score, e.g. 3-2
    /// </summary>
    public string Description => string.Join("-", Points);

    [JsonIgnore]
    /// <summary>
    /// Sum the points total to find the number of games played.
    /// </summary>
    public int GamesPlayed => Points.Sum();

    [JsonPropertyName("Points")]
    /// <summary>
    /// Score points, indexed by team.
    /// </summary>
    public IList<int> Points { get; }
  }
}