using System;

namespace SplatTagCore
{
  public class Placement
  {
    public Placement(Guid[]? players = null, Guid[]? teams = null)
    {
      this.PlayersByPlacement = players ?? Array.Empty<Guid>();
      this.TeamsByPlacement = teams ?? Array.Empty<Guid>();
    }

    /// <summary>
    /// Players ordered by placement.
    /// </summary>
    public Guid[] PlayersByPlacement { get; } = Array.Empty<Guid>();

    /// <summary>
    /// Teams ordered by placement.
    /// </summary>
    public Guid[] TeamsByPlacement { get; } = Array.Empty<Guid>();
  }
}