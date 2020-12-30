using System;
using System.Collections.Generic;

namespace SplatTagCore
{
  [Serializable]
  public class Game
  {
    public Game(Score? score = null, IList<Guid>? players = null, IList<Guid>? teams = null)
    {
      this.Score = score ?? new Score();
      this.Players = players ?? Array.Empty<Guid>();
      this.Teams = teams ?? Array.Empty<Guid>();
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
    public IList<Guid> Teams { get; set; }
  }
}