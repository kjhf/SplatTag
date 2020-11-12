using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
      List<Player> players = new List<Player>();
      List<Team> teams = new List<Team>();
      foreach (var db in importers)
      {
        Player[] loadedPlayers;
        Team[] loadedTeams;
        try
        {
          (loadedPlayers, loadedTeams) = db.Load();
        }
        catch (Exception ex)
        {
          Trace.WriteLine($"ERROR: Importer {db} failed. Discarding result and continuing. {ex}");
          continue;
        }

        try
        {
          var mergeResult = Merger.MergeTeams(teams, loadedTeams);
          Merger.MergePlayers(players, loadedPlayers);
          Merger.CorrectTeamIdsForPlayers(players, mergeResult);
        }
        catch (Exception ex)
        {
          Trace.WriteLine($"ERROR: Failed to merge during import of {db}. Discarding result and continuing. {ex}");
        }
      }

      // Perform a final merge.
      try
      {
        Merger.FinalisePlayers(players);
        Merger.DumpLogger();
      }
      catch (Exception ex)
      {
        Trace.WriteLine($"Warning: Failed {nameof(Merger.FinalisePlayers)}. Continuing anyway. {ex}");
      }
      return (players.ToArray(), teams.ToArray());
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