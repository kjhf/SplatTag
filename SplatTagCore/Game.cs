using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public record Game : ICoreObject
  {
    public Game(Score? score = null, IList<Guid>? players = null, IList<TeamId>? teams = null)
    {
      this.Score = score ?? Score.Empty;
      this.Players = players ?? Array.Empty<Guid>();
      this.Teams = teams ?? Array.Empty<TeamId>();
    }

    /// <summary>
    /// The final score of this match.
    /// </summary>
    public Score Score { get; set; }

    /// <summary>
    /// The players that have played this match.
    /// </summary>
    public IList<Guid> Players { get; set; }

    /// <summary>
    /// The teams that have played this match. Often two teams (alpha vs bravo).
    /// </summary>
    public IList<TeamId> Teams { get; set; }

    /// <summary>
    /// Overridden string representation of the game's result in form "Team1Id X-Y Team2Id"
    /// </summary>
    public override string ToString() => $"Game: {Teams.ElementAtOrDefault(0)} {Score.Description} {Teams.ElementAtOrDefault(1)}";

    public bool Equals(ICoreObject other) => Equals(other as Game);

    public string GetDisplayValue() => ToString();

    #region Serialization

    // Deserialize
    protected Game(SerializationInfo info, StreamingContext context)
      : base()
    {
      Score = info.GetValueOrDefault(nameof(Score), Score.Empty);
      Players = info.GetValueOrDefault(nameof(Players), Array.Empty<Guid>());
      Teams = info.GetValueOrDefault(nameof(Teams), Array.Empty<TeamId>());
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (Score != default && Score != Score.Empty)
      {
        info.AddValue(nameof(Score), Score);
      }

      if (Players.Count > 0)
      {
        info.AddValue(nameof(Players), Players);
      }

      if (Teams.Count > 0)
      {
        info.AddValue(nameof(Teams), Teams);
      }
    }

    #endregion Serialization
  }
}