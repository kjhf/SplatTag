using Newtonsoft.Json;
using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Timers;

namespace SplatTagDatabase
{
  public class SplatTagJsonSnapshotDatabase : ISplatTagDatabase
  {
    private readonly string? saveDirectory;
    private const string SNAPSHOT_FORMAT = "Snapshot-*.json";
    private string? playersSnapshotFile = null;
    private string? teamsSnapshotFile = null;
    private string? sourcesSnapshotFile = null;

    public SplatTagJsonSnapshotDatabase(string saveDirectory)
    {
      this.saveDirectory = saveDirectory ?? throw new ArgumentNullException(nameof(saveDirectory));
      Directory.CreateDirectory(saveDirectory);
    }

    public SplatTagJsonSnapshotDatabase(string playersSnapshotFile, string teamsSnapshotFile, string sourcesSnapshotFile)
    {
      this.playersSnapshotFile = playersSnapshotFile;
      this.teamsSnapshotFile = teamsSnapshotFile;
      this.sourcesSnapshotFile = sourcesSnapshotFile;
    }

    public (Player[], Team[], Dictionary<Guid, Source>) Load()
    {
      if (playersSnapshotFile != null && teamsSnapshotFile != null && sourcesSnapshotFile != null)
      {
        return Load(playersSnapshotFile, teamsSnapshotFile, sourcesSnapshotFile);
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

        if (sourcesSnapshotFile == null && snapshot.Name.Contains("Sources"))
        {
          sourcesSnapshotFile = snapshot.FullName;
        }

        if (playersSnapshotFile != null && teamsSnapshotFile != null && sourcesSnapshotFile != null)
        {
          break;
        }
      }

      return Load(playersSnapshotFile, teamsSnapshotFile, sourcesSnapshotFile);
    }

    private static (Player[], Team[], Dictionary<Guid, Source>) Load(string? playersSnapshotFile, string? teamsSnapshotFile, string? sourcesSnapshotFile)
    {
      if (playersSnapshotFile == null || teamsSnapshotFile == null || sourcesSnapshotFile == null)
        return (Array.Empty<Player>(), Array.Empty<Team>(), new Dictionary<Guid, Source>());

      Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Load sourcesSnapshotFile... ");
      Stopwatch t = new Stopwatch();
      t.Start();
      string json = File.ReadAllText(sourcesSnapshotFile);
      t.Stop();
      Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Took {t.ElapsedMilliseconds}ms (1/2)");
      t.Restart();
      var sources = JsonConvert.DeserializeObject<Source[]>(json);
      t.Stop();
      Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Took {t.ElapsedMilliseconds}ms (2/2)");
      Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Transforming sources... ");
      var lookup = sources.AsParallel().ToDictionary(s => s.Id, s => s);

      Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Load playersSnapshotFile... ");
      var settings = new JsonSerializerSettings
      {
        DefaultValueHandling = DefaultValueHandling.Ignore
      };
      settings.Context = new StreamingContext(StreamingContextStates.All, new Source.GuidToSourceConverter(lookup));
      var players = JsonConvert.DeserializeObject<Player[]>(File.ReadAllText(playersSnapshotFile), settings);

      Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Load teamsSnapshotFile... ");
      var teams = JsonConvert.DeserializeObject<Team[]>(File.ReadAllText(teamsSnapshotFile), settings);

      Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Load done... ");
      return (players, teams, lookup);
    }

    public void Save(IEnumerable<Player> savePlayers, IEnumerable<Team> saveTeams, IEnumerable<Source> saveSources)
    {
      try
      {
        // Write players
        File.WriteAllText(
          Path.Combine(saveDirectory, "Snapshot-Players-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".json"),
          JsonConvert.SerializeObject(savePlayers, Formatting.Indented),
          Encoding.UTF8);
      }
      catch (Exception ex)
      {
        string error = $"Unable to save the {nameof(SplatTagJsonSnapshotDatabase)} players because of an exception: {ex}";
        Console.Error.WriteLine(error);
        Console.WriteLine(error);
      }

      try
      {
        // Write teams
        File.WriteAllText(
          Path.Combine(saveDirectory, "Snapshot-Teams-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".json"),
          JsonConvert.SerializeObject(saveTeams, Formatting.Indented),
          Encoding.UTF8);
      }
      catch (Exception ex)
      {
        string error = $"Unable to save the {nameof(SplatTagJsonSnapshotDatabase)} teams because of an exception: {ex}";
        Console.Error.WriteLine(error);
        Console.WriteLine(error);
      }

      try
      {
        // Write sources
        File.WriteAllText(
          Path.Combine(saveDirectory, "Snapshot-Sources-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".json"),
          JsonConvert.SerializeObject(saveSources, Formatting.Indented),
          Encoding.UTF8);
      }
      catch (Exception ex)
      {
        string error = $"Unable to save the {nameof(SplatTagJsonSnapshotDatabase)} sources because of an exception: {ex}";
        Console.Error.WriteLine(error);
        Console.WriteLine(error);
      }
    }
  }
}