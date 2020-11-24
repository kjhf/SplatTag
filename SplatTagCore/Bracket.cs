using System;
using System.Collections.Generic;

namespace SplatTagCore
{
  [Serializable]
  public class Bracket
  {
    /// <summary>
    /// The matches that make up the bracket
    /// </summary>
    public IList<Match> Matches { get; set; }

    /// <summary>
    /// Name of the bracket if specified
    /// </summary>
    /// <example>Top Cut</example>
    /// <example>Alpha</example>
    /// <example>Swiss</example>
    public string Name { get; set; }

    /// <summary>
    /// The players that have played in the bracket
    /// </summary>
    public IList<Guid> Players { get; set; }

    /// <summary>
    /// The teams that have played in the bracket
    /// </summary>
    public IList<Guid> Teams { get; set; }
  }
}