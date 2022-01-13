using System;
using System.Collections.Generic;

namespace SplatTagCore
{
  public interface ISplatTagDatabase
  {
    /// <summary>
    /// Load the database and return the merged Players, merged Teams, and Sources.
    /// </summary>
    (Player[], Team[], Dictionary<string, Source>) Load();
  }
}