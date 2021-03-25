using Newtonsoft.Json;
using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SplatTagDatabase
{
  public class SplatTagJsonSnapshotDatabase : ISplatTagDatabase
  {
    private const string SNAPSHOT_FORMAT = "Snapshot-*.json";

    private readonly string saveDirectory;
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
      this.playersSnapshotFile = playersSnapshotFile ?? throw new ArgumentNullException(nameof(playersSnapshotFile));
      this.teamsSnapshotFile = teamsSnapshotFile ?? throw new ArgumentNullException(nameof(teamsSnapshotFile));
      this.sourcesSnapshotFile = sourcesSnapshotFile ?? throw new ArgumentNullException(nameof(sourcesSnapshotFile));

      this.saveDirectory = Directory.GetParent(playersSnapshotFile).FullName;
    }

    public static IOrderedEnumerable<FileInfo> GetSnapshots(string dir)
    {
      // Check in the save directory for the latest snapshot.
      return new DirectoryInfo(dir).GetFiles(SNAPSHOT_FORMAT).OrderByDescending(f => f.LastWriteTime);
    }

    public (Player[], Team[], Dictionary<Guid, Source>) Load()
    {
      if (playersSnapshotFile != null && teamsSnapshotFile != null && sourcesSnapshotFile != null)
      {
        return Load(playersSnapshotFile, teamsSnapshotFile, sourcesSnapshotFile);
      }

      // Check in the save directory for the latest snapshot.
      foreach (var snapshot in GetSnapshots(saveDirectory))
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

      Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Load sourcesSnapshotFile from {sourcesSnapshotFile}... ");
      Stopwatch t = new Stopwatch();
      t.Start();
      string json = File.ReadAllText(sourcesSnapshotFile);
      t.Stop();
      Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Took {t.ElapsedMilliseconds}ms (1/2)");
      t.Restart();

      var settings = JsonConvert.DefaultSettings();
      List<Source> sources = LoadSnapshot<Source>(json, settings);

      t.Stop();
      Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Took {t.ElapsedMilliseconds}ms (2/2)");
      Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Transforming sources... ");
      var lookup = sources.AsParallel().ToDictionary(s => s.Id, s => s);

      Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Load playersSnapshotFile from {playersSnapshotFile}... ");
      settings.Context = new StreamingContext(StreamingContextStates.All, new Source.GuidToSourceConverter(lookup));
      json = File.ReadAllText(playersSnapshotFile);
      List<Player> players = LoadSnapshot<Player>(json, settings);

      Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Load teamsSnapshotFile from {teamsSnapshotFile}... ");
      json = File.ReadAllText(teamsSnapshotFile);
      List<Team> teams = LoadSnapshot<Team>(json, settings);

      Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Load done... ");
      return (players.ToArray(), teams.ToArray(), lookup);
    }

    private static List<T> LoadSnapshot<T>(string json, JsonSerializerSettings settings) where T : class
    {
      List<T> result = new List<T>();
      using (JsonTextReader reader = new JsonTextReader(new StringReader(json)))
      {
        reader.SupportMultipleContent = true;

        var serializer = JsonSerializer.Create(settings);
        while (reader.Read())
        {
          if (reader.TokenType == JsonToken.StartObject)
          {
            try
            {
              result.Add(serializer.Deserialize<T>(reader));
            }
            catch (Exception ex)
            {
              Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Could not parse {typeof(T)} from line " + reader.LineNumber + ".");
              Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] " + ex);
              Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] " + reader);
            }
          }
        }
      }

      return result;
    }

    public void Save(IEnumerable<Player> savePlayers, IEnumerable<Team> saveTeams, IEnumerable<Source> saveSources)
    {
      Task savePlayersTask = Task.Run(async () =>
      {
        try
        {
          // Write players
          string filePath = Path.Combine(saveDirectory, "Snapshot-Players-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".json");
          Encoding outputEnc = new UTF8Encoding(false); // UTF-8 no BOM

          using TextWriter file = new StreamWriter(filePath, false, outputEnc);
          await file.WriteLineAsync(JsonConvert.SerializeObject(savePlayers, Formatting.Indented)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
          string error = $"Unable to save the {nameof(SplatTagJsonSnapshotDatabase)} players because of an exception: {ex}";
          Console.Error.WriteLine(error);
          Console.WriteLine(error);
        }
      });

      Task saveTeamsTask = Task.Run(async () =>
      {
        try
        {
          // Write teams
          string filePath = Path.Combine(saveDirectory, "Snapshot-Teams-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".json");
          Encoding outputEnc = new UTF8Encoding(false); // UTF-8 no BOM

          using TextWriter file = new StreamWriter(filePath, false, outputEnc);
          await file.WriteLineAsync(JsonConvert.SerializeObject(saveTeams, Formatting.Indented)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
          string error = $"Unable to save the {nameof(SplatTagJsonSnapshotDatabase)} teams because of an exception: {ex}";
          Console.Error.WriteLine(error);
          Console.WriteLine(error);
        }
      });

      Task saveSourcesTask = Task.Run(async () =>
      {
        try
        {
          // Write sources
          string filePath = Path.Combine(saveDirectory, "Snapshot-Sources-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".json");
          Encoding outputEnc = new UTF8Encoding(false); // UTF-8 no BOM

          using TextWriter file = new StreamWriter(filePath, false, outputEnc);
          await file.WriteLineAsync(JsonConvert.SerializeObject(saveSources, Formatting.Indented)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
          string error = $"Unable to save the {nameof(SplatTagJsonSnapshotDatabase)} sources because of an exception: {ex}";
          Console.Error.WriteLine(error);
          Console.WriteLine(error);
        }
      });

      Task.WaitAll(savePlayersTask, saveTeamsTask, saveSourcesTask);
    }
  }
}