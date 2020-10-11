using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SplatTagDatabase
{
  public class MultiDatabase : ISplatTagDatabase
  {
    private readonly List<IImporter> importers;
    private readonly string saveDirectory;

    public MultiDatabase(string saveDirectory, params IImporter[] importers)
    {
      this.importers = new List<IImporter>(importers ?? throw new ArgumentNullException(nameof(importers)));
      this.saveDirectory = saveDirectory ?? throw new ArgumentNullException(nameof(saveDirectory));
    }

    public (Player[], Team[]) Load()
    {
      Dictionary<uint, Player> players = new Dictionary<uint, Player>();
      Dictionary<long, Team> teams = new Dictionary<long, Team>();
      foreach (var db in importers)
      {
        Dictionary<long, Team> teamDictionaryPreMerge = new Dictionary<long, Team>();
        var (loadedPlayers, loadedTeams) = db.Load();
        if (loadedTeams != null)
        {
          teamDictionaryPreMerge = loadedTeams.ToDictionary(team => team.Id, team => team);
          Merger.MergeTeams(teams, loadedTeams);
        }
        Merger.MergePlayers(players, loadedPlayers, teamDictionaryPreMerge);
      }

      // Perform a final merge.
      Merger.FinalisePlayers(players);
      return (players.Values.ToArray(), teams.Values.ToArray());
    }

    public void Save(IEnumerable<Player> players, IEnumerable<Team> teams)
    {
      // Offload to a Json database instead.
      SplatTagJsonSnapshotDatabase jsonDb = new SplatTagJsonSnapshotDatabase(saveDirectory);
      jsonDb.Save(players, teams);
      importers.Clear();
      importers.Add(jsonDb);
    }
  }
}