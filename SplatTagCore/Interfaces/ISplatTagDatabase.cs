using System;
using System.Collections.Generic;

namespace SplatTagCore
{
  public interface ISplatTagDatabase
  {
    /// <summary>
    /// Load the database. Returns if anything was loaded.
    /// </summary>
    bool Load();

    /// <summary>
    /// Get the loaded players.
    /// </summary>
    IReadOnlyList<Player> Players { get; }

    /// <summary>
    /// Get the loaded teams.
    /// </summary>
    IReadOnlyDictionary<Guid, Team> Teams { get; }

    /// <summary>
    /// Get the loaded sources.
    /// </summary>
    IReadOnlyDictionary<string, Source> Sources { get; }

    /// <summary>
    /// Get if the database is loaded.
    /// </summary>
    bool Loaded { get; }
  }
}