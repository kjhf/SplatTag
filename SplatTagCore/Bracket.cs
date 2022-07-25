using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public record Bracket : ICoreObject, IEquatable<Bracket>
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

    public string GetDisplayValue() => Name;

    public bool Equals(ICoreObject other) => Equals(other as Bracket);

    #region Serialization

    // Deserialize
    protected Bracket(SerializationInfo info, StreamingContext _)
    {
      this.Name = info.GetValueOrDefault("Name", Builtins.UNKNOWN_BRACKET);
      this.Matches = info.GetValueOrDefault("Matches", Array.Empty<Game>());
      this.Players = info.GetValueOrDefault("Players", Array.Empty<Guid>());
      this.Teams = info.GetValueOrDefault("Teams", Array.Empty<Guid>());
      this.Placements = info.GetValueOrDefault("Placements", new Placement());
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext _)
    {
      info.AddValue("Name", this.Name);

      if (this.Matches.Count > 0)
        info.AddValue("Matches", this.Matches);

      if (this.Players.Count > 0)
        info.AddValue("Players", this.Players);

      if (this.Teams.Count > 0)
        info.AddValue("Teams", this.Teams);

      if (this.Placements.HasPlacements)
        info.AddValue("Placements", this.Placements);
    }

    #endregion Serialization
  }
}