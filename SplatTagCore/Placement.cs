using System;
using System.Collections.Generic;

namespace SplatTagCore
{
  [Serializable]
  public class Placement
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
  }
}