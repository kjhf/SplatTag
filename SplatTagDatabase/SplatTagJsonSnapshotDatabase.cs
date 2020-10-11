using Newtonsoft.Json;
using SplatTagCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SplatTagDatabase
{
  public class SplatTagJsonSnapshotDatabase : ISplatTagDatabase
  {
    private readonly string saveDirectory;
    private const string SNAPSHOT_FORMAT = "Snapshot-*.json";
    private string playersSnapshotFile = null;
    private string teamsSnapshotFile = null;

    public SplatTagJsonSnapshotDatabase(string saveDirectory)
    {
      this.saveDirectory = saveDirectory ?? throw new ArgumentNullException(nameof(saveDirectory));
      Directory.CreateDirectory(saveDirectory);
    }

    public SplatTagJsonSnapshotDatabase(string playersSnapshotFile, string teamsSnapshotFile)
    {
      this.playersSnapshotFile = playersSnapshotFile;
      this.teamsSnapshotFile = teamsSnapshotFile;
    }

    public (Player[], Team[]) Load()
    {
      if (playersSnapshotFile != null && teamsSnapshotFile != null)
      {
        return Load(playersSnapshotFile, teamsSnapshotFile);
      }

      // Check in the save directory for the latest snapshot.
      var directory = new DirectoryInfo(saveDirectory);
      var snapshots = directory.GetFiles(SNAPSHOT_FORMAT)
        .OrderByDescending(f => f.LastWriteTime);

      foreach (var snapshot in snapshots)
      {
        if (playersSnapshotFile == null && snapshot.Name.Contains("Players"))
        {
          playersSnapshotFile = snapshot.FullName;
        }

        if (teamsSnapshotFile == null && snapshot.Name.Contains("Teams"))
        {
          teamsSnapshotFile = snapshot.FullName;
        }

        if (playersSnapshotFile != null && teamsSnapshotFile != null)
        {
          break;
        }
      }

      return Load(playersSnapshotFile, teamsSnapshotFile);
    }

    private static (Player[], Team[]) Load(string playersSnapshotFile, string teamsSnapshotFile)
    {
      if (playersSnapshotFile == null || teamsSnapshotFile == null) return (new Player[0], new Team[0]);

      var players = JsonConvert.DeserializeObject<Player[]>(File.ReadAllText(playersSnapshotFile));
      var teams = JsonConvert.DeserializeObject<Team[]>(File.ReadAllText(teamsSnapshotFile));
      return (players, teams);
    }

    public void Save(IEnumerable<Player> savePlayers, IEnumerable<Team> saveTeams)
    {
      // Write players
      File.WriteAllText(Path.Combine(saveDirectory, "Snapshot-Players-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".json"), JsonConvert.SerializeObject(savePlayers));

      // Write teams
      File.WriteAllText(Path.Combine(saveDirectory, "Snapshot-Teams-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".json"), JsonConvert.SerializeObject(saveTeams));
    }
  }
}