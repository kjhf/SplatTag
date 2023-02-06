using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SplatTagCore
{
  public record Bracket
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
    [JsonPropertyName("Matches")]
    public IList<Game> Matches { get; } = Array.Empty<Game>();

    /// <summary>
    /// Name of the bracket if specified
    /// </summary>
    /// <example>Top Cut</example>
    /// <example>Alpha</example>
    /// <example>Swiss</example>
    [JsonPropertyName("Name")]
    public string Name { get; } = Builtins.UNKNOWN_BRACKET;

    /// <summary>
    /// The players that have played in the bracket
    /// </summary>
    [JsonPropertyName("Players")]
    public IList<Guid> Players { get; } = Array.Empty<Guid>();

    /// <summary>
    /// The teams that have played in the bracket
    /// </summary>
    [JsonPropertyName("Teams")]
    public IList<Guid> Teams { get; } = Array.Empty<Guid>();

    /// <summary>
    /// Final placements for teams and players
    /// </summary>
    [JsonPropertyName("Placements")]
    public Placement Placements { get; } = new Placement();
  }
}