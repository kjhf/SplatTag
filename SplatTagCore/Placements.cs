using System;
using System.Collections.Generic;

namespace SplatTagCore
{
  public class Placements
  {
    /// <summary>
    /// Players ordered by placement.
    /// </summary>
    public IList<Guid>? PlayersByPlacement { get; set; }

    /// <summary>
    /// Teams ordered by placement.
    /// </summary>
    public IList<Guid>? TeamsByPlacement { get; set; }
  }
}