using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SplatTagCore
{
  [Serializable]
  public class Score
  {
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

    [JsonProperty]
    /// <summary>
    /// Score points, indexed by team.
    /// </summary>
    public IList<int> Points { get; set; }
  }
}