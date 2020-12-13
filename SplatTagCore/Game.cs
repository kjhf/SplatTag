using System;
using System.Collections.Generic;

namespace SplatTagCore
{
  [Serializable]
  public class Game
  {
    /// <summary>
    /// The players that have played this match.
    /// </summary>
    public IList<Guid>? Players { get; set; }

    /// <summary>
    /// The final score of this match.
    /// </summary>
    public Score? Score { get; set; }

    /// <summary>
    /// The teams that have played this match. Often two teams (alpha vs bravo).
    /// </summary>
    public IList<Guid>? Teams { get; set; }
  }
}