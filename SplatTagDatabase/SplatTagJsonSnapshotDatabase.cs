using Newtonsoft.Json;
using NLog;
using SplatTagCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SplatTagDatabase
{
  public class SplatTagJsonSnapshotDatabase : ISplatTagDatabase
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static readonly HashSet<string> errorMessagesReported = new();

    public static readonly Func<JsonSerializerSettings> JsonConvertDefaultSettings =
      () =>
        new JsonSerializerSettings
        {
          DefaultValueHandling = DefaultValueHandling.Ignore,
          Error = (sender, args) =>
          {
            string m = args.ErrorContext.Error.Message;
            if (!errorMessagesReported.Contains(m))
            {
              logger.Error(m);
              errorMessagesReported.Add(m);
            }
            args.ErrorContext.Handled = true;
          },
          TypeNameHandling = TypeNameHandling.Auto
        };

    private const string SNAPSHOT_FORMAT = "Snapshot-*.json";

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
        JsonConvert.DefaultSettings ??= JsonConvertDefaultSettings;
        var settings = JsonConvert.DefaultSettings();

        logger.Info($"[{DateTime.Now:HH:mm:ss.fffffff}] Loading sourcesSnapshotFile from {sourcesSnapshotFile}... ");
        Source[] sources = LoadSnapshot<Source>(sourcesSnapshotFile, settings, capacityHint: 1024);
        var lookup = sources.ToDictionary(s => s.Id, s => s);
        logger.Info($"[{DateTime.Now:HH:mm:ss.fffffff}] {lookup.Count} sources transformed.");
        GC.Collect();
        GC.WaitForPendingFinalizers();

        logger.Info($"[{DateTime.Now:HH:mm:ss.fffffff}] Loading playersSnapshotFile from {playersSnapshotFile}... ");
        settings.Context = new StreamingContext(StreamingContextStates.All, new Source.SourceStringConverter(lookup));
        Player[] players = LoadSnapshot<Player>(playersSnapshotFile, settings, capacityHint: 65536);
        logger.Info($"[{DateTime.Now:HH:mm:ss.fffffff}] {players.Length} players loaded.");

        logger.Info($"[{DateTime.Now:HH:mm:ss.fffffff}] Loading teamsSnapshotFile from {teamsSnapshotFile}... ");
        Team[] teams = LoadSnapshot<Team>(teamsSnapshotFile, settings, capacityHint: 16384);
        logger.Info($"[{DateTime.Now:HH:mm:ss.fffffff}] {teams.Length} teams loaded.");

        logger.Info($"[{DateTime.Now:HH:mm:ss.fffffff}] Load done... ");
        return (players, teams, lookup);
      }
      catch (Exception ex)
      {
        logger.Error(ex, "An error occurred in loading the Snapshot Database.");
        throw;
      }
      finally
      {
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.Default;
        WinApi.TryTimeEndPeriod(1);
      }
    }

    private static T[] LoadSnapshot<T>(string filePath, JsonSerializerSettings settings, int capacityHint) where T : class
    {
      List<T> result = new List<T>(capacityHint);
      var serializer = JsonSerializer.Create(settings);
      using (StreamReader file = File.OpenText(filePath))
      using (JsonTextReader reader = new JsonTextReader(file))
      {
        if (file.BaseStream.Length <= 2) // Empty file or [] or {}
        {
          logger.Warn("Nothing to load from this file.");
        }

        // reader.SupportMultipleContent = true;
        while (reader.Read())
        {
          if (reader.TokenType == JsonToken.StartObject)
          {
            try
            {
              var toAdd = serializer.Deserialize<T>(reader);
              if (toAdd == null)
              {
                throw new ArgumentNullException("Should not have deserialized null.");
              }
              result.Add(toAdd);
            }
            catch (Exception ex)
            {
              logger.Error($"[{DateTime.Now:HH:mm:ss.fffffff}] Could not parse {typeof(T)} from line " + reader.LineNumber + ".");
              logger.Error($"[{DateTime.Now:HH:mm:ss.fffffff}] " + ex);
              logger.Error($"[{DateTime.Now:HH:mm:ss.fffffff}] " + reader);
            }
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
          await file.WriteLineAsync(JsonConvert.SerializeObject(savePlayers, Formatting.None)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
          string error = $"Unable to save the {nameof(SplatTagJsonSnapshotDatabase)} players because of an exception: {ex}";
          logger.Error(ex, error);
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
          await file.WriteLineAsync(JsonConvert.SerializeObject(saveTeams, Formatting.None)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
          string error = $"Unable to save the {nameof(SplatTagJsonSnapshotDatabase)} teams because of an exception: {ex}";
          logger.Error(ex, error);
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
          await file.WriteLineAsync(JsonConvert.SerializeObject(saveSources, Formatting.None)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
          string error = $"Unable to save the {nameof(SplatTagJsonSnapshotDatabase)} sources because of an exception: {ex}";
          logger.Error(ex, error);
        }
      });

      Task.WaitAll(savePlayersTask, saveTeamsTask, saveSourcesTask);
      return this;
    }
  }
}