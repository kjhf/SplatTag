using System.Collections.Generic;

namespace SplatTagCore
{
  public interface ISplatTagDatabase : IImporter
  {
    // (Player[], Team[]) Load(); // From IImporter

    /// <summary>
    /// Save the database with the given players and teams.
    /// </summary>
    /// <param name="players"></param>
    /// <param name="teams"></param>
    void Save(IEnumerable<Player> players, IEnumerable<Team> teams);
  }
}