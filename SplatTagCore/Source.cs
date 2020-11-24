using System;
using System.Collections.Generic;

namespace SplatTagCore
{
  [Serializable]
  public class Source
  {
    /// <summary>
    /// The brackets that make up the source.
    /// </summary>
    public IList<Bracket> Brackets { get; set; }

    /// <summary>
    /// The source identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The friendly name for the source
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Final placements for teams and players
    /// </summary>
    public IList<Placements> Placements { get; set; }

    /// <summary>
    /// The players that this source represents
    /// e.g. all players that have signed up to this tournament
    /// </summary>
    public IList<Player> Players { get; set; }

    /// <summary>
    /// The teams that this source represents
    /// e.g. all teams that have signed up to this tournament
    /// </summary>
    public IList<Team> Teams { get; set; }

    /// <summary>
    /// Relevant URI for the source
    /// </summary>
    public Uri Uri { get; set; }
  }
}