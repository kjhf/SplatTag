using System.Collections.Generic;

namespace SplatTagCore
{
  public interface ISplatTagDatabase
  {
    (Player[], Team[]) Load();

    void Save(IEnumerable<Player> players, IEnumerable<Team> teams);
  }
}