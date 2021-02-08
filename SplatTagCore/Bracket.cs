using System;
using System.Collections.Generic;

namespace SplatTagCore
{
  [Serializable]
  public class Bracket
  {
    public Bracket(string? name = null, IList<Game>? matches = null, IList<Guid>? players = null, IList<Guid>? teams = null, Placement? placements = null)
    {
      this.Name = name ?? Builtins.UNKNOWN_BRACKET;
      this.Matches = matches ?? Array.Empty<Game>();
      this.Players = players ?? Array.Empty<Guid>();
      this.Teams = teams ?? Array.Empty<Guid>();
      this.Placements = placements ?? new Placement();
    }

    /// <summary>
    /// The matches that make up the bracket
    /// </summary>
    public IList<Game> Matches { get; }

    /// <summary>
    /// Name of the bracket if specified
    /// </summary>
    /// <example>Top Cut</example>
    /// <example>Alpha</example>
    /// <example>Swiss</example>
    public string Name { get; }

    /// <summary>
    /// The players that have played in the bracket
    /// </summary>
    public IList<Guid> Players { get; }

    /// <summary>
    /// The teams that have played in the bracket
    /// </summary>
    public IList<Guid> Teams { get; }

    /// <summary>
    /// Final placements for teams and players
    /// </summary>
    public Placement Placements { get; }
  }
}