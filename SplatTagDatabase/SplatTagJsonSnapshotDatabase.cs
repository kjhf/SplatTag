using SplatTagCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static SplatTagCore.JSONConverters;

namespace SplatTagDatabase
{
  public class SplatTagJsonSnapshotDatabase : ISplatTagDatabase
  {
    private const string SNAPSHOT_FORMAT = "Snapshot-*.json";
    public static readonly JsonSerializerOptions jsonSerializerOptions = CreateJsonSerializerOptions();

    private static JsonSerializerOptions CreateJsonSerializerOptions()
    {
      var options = new JsonSerializerOptions
      {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
        PropertyNameCaseInsensitive = true
      };
      return options;
    }

    private static readonly HashSet<string> errorMessagesReported = new();

    private readonly string saveDirectory;
    private string? playersSnapshotFile = null;
    private string? teamsSnapshotFile = null;
    private string? sourcesSnapshotFile = null;

    private List<Player> _players = new();
    private Dictionary<Guid, Team> _teams = new();
    private Dictionary<string, Source> _sources = new();

    public IReadOnlyList<Player> Players => _players.Count == 0 ? Array.Empty<Player>() : _players;
    public IReadOnlyDictionary<Guid, Team> Teams => _teams;
    public IReadOnlyDictionary<string, Source> Sources => _sources;

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

    /// <summary>
    /// Loads the database and saves the results to the DB's fields.
    /// Returns if anything was loaded.
    /// </summary>
    public bool Load()
    {
      var (players, teams, importedSources) = LoadInline();
      _players = players.ToList();
      _teams = teams.ToDictionary(t => t.Id, t => t);
      _sources = importedSources;
      return players.Length != 0 || teams.Length != 0 || importedSources.Count != 0;
    }

    /// <summary>
    /// Loads the database but bypasses saving the fields, instead passes the result as return tuple.
    /// </summary>
    public (Player[], Team[], Dictionary<string, Source>) LoadInline()
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

    private static (Player[], Team[], Dictionary<string, Source>) Load(string? playersSnapshotFile, string? teamsSnapshotFile, string? sourcesSnapshotFile)
    {
      if (playersSnapshotFile == null || teamsSnapshotFile == null || sourcesSnapshotFile == null)
        return (Array.Empty<Player>(), Array.Empty<Team>(), new Dictionary<string, Source>());

      try
      {
        WinApi.TryTimeBeginPeriod(1);
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // Invoked from command line
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Loading sourcesSnapshotFile from {sourcesSnapshotFile}... ");
        var sources = LoadSnapshot<Source>(sourcesSnapshotFile, capacityHint: 1024).ToDictionary(s => s.Id, s => s);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] {sources.Count} sources transformed.");
        if (sources.Count > 0)
        {
          Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Starting from {sources.First()} to {sources.Last()}.");
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Loading playersSnapshotFile from {playersSnapshotFile}... ");
        _ = new GuidToSourceConverter(sources);  // Sets the instance.
        Player[] players = LoadSnapshot<Player>(playersSnapshotFile, capacityHint: 65536);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] {players.Length} players loaded.");
        if (players.Length > 0)
        {
          Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Starting from {players[0]} to {players[^1]}.");
        }

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Loading teamsSnapshotFile from {teamsSnapshotFile}... ");
        Team[] teams = LoadSnapshot<Team>(teamsSnapshotFile, capacityHint: 16384);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] {teams.Length} teams loaded.");
        if (teams.Length > 0)
        {
          Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Starting from {teams[0]} to {teams[^1]}.");
        }

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Load done... ");
        return (players, teams, sources);
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine("An error occurred in loading the Snapshot Database.");
        Console.Error.WriteLine(ex);
        throw;
      }
      finally
      {
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.Default;
        WinApi.TryTimeEndPeriod(1);
      }
    }

    /// <summary>
    /// Reads a file from path as ReadOnlyMemory UTF-8 bytes.
    /// </summary>
    public static ReadOnlyMemory<byte> ReadFileAsUtf8Bytes(string filePath)
    {
      byte[] utf8Bytes = File.ReadAllBytes(filePath);
      return new ReadOnlyMemory<byte>(utf8Bytes);
    }

    private static T[] LoadSnapshot<T>(string filePath, int capacityHint) where T : class
    {
      List<T> result = new(capacityHint);
      using (JsonDocument document = JsonDocument.Parse(ReadFileAsUtf8Bytes(filePath)))
      {
        foreach (JsonElement root in document.RootElement.EnumerateArray())
        {
          try
          {
            var toAdd = DeserializeWithErrorHandling<T>(root.ToString());
            if (toAdd != default)
            {
              result.Add(toAdd);
            }
          }
          catch (Exception ex)
          {
            ConsoleColor restore = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            var raw = root.GetRawText();
            var trim = raw.Length > 80 ? raw[0..80] + "…" : raw;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Could not parse {typeof(T)} ({root.ValueKind}).");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] with text: " + trim);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] " + ex);
            Console.ForegroundColor = restore;
          }
        }
      }
      return result.ToArray();
    }

    public SplatTagJsonSnapshotDatabase Save(IEnumerable<Player> savePlayers, IEnumerable<Team> saveTeams, IEnumerable<Source> saveSources)
    {
      Task savePlayersTask = Task.Run(async () =>
      {
        try
        {
          // Write players
          string filePath = Path.Combine(saveDirectory, "Snapshot-Players-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".json");
          Encoding outputEnc = new UTF8Encoding(false); // UTF-8 no BOM

          using TextWriter file = new StreamWriter(filePath, false, outputEnc);
          await file.WriteLineAsync(JsonSerializer.Serialize(savePlayers)).ConfigureAwait(false);
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
          await file.WriteLineAsync(JsonSerializer.Serialize(saveTeams)).ConfigureAwait(false);
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
          await file.WriteLineAsync(JsonSerializer.Serialize(saveSources)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
          string error = $"Unable to save the {nameof(SplatTagJsonSnapshotDatabase)} sources because of an exception: {ex}";
          Console.Error.WriteLine(error);
          Console.WriteLine(error);
        }
      });

      Task.WaitAll(savePlayersTask, saveTeamsTask, saveSourcesTask);
      return this;
    }

    private static T? DeserializeWithErrorHandling<T>(string json)
    {
      try
      {
        return JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
      }
      catch (JsonException ex)
      {
        string m = ex.Message;
        if (!errorMessagesReported.Contains(m))
        {
          ConsoleColor restore = Console.ForegroundColor;
          Console.ForegroundColor = ConsoleColor.Red;
          var trim = json.Length > 80 ? json[0..80] + "…" : json;
          Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Could not parse {typeof(T)}");
          Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] with text: " + trim);
          Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] " + ex);
          Console.ForegroundColor = restore;
          errorMessagesReported.Add(m);
        }
        return default;
      }
    }
  }
}