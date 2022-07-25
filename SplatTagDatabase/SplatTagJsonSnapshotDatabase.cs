using Newtonsoft.Json;
using NLog;
using SplatTagCore;
using SplatTagCore.Extensions;
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
    private const string SNAPSHOT_FORMAT = "Snapshot-*.json";
    private static readonly HashSet<string> errorMessagesReported = new();
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private readonly string saveDirectory;
    private List<Player> _players = new();
    private Dictionary<string, Source> _sources = new();
    private Dictionary<Guid, Team> _teams = new();
    private string? playersSnapshotFile = null;
    private string? sourcesSnapshotFile = null;
    private string? teamsSnapshotFile = null;

    public bool Loaded => _sources.Count > 0;
    public IReadOnlyList<Player> Players => _players.Count == 0 ? Array.Empty<Player>() : _players;
    public IReadOnlyDictionary<string, Source> Sources => _sources;
    public IReadOnlyDictionary<Guid, Team> Teams => _teams;

    public static readonly Func<JsonSerializerSettings> JsonConvertDefaultSettings =
      () =>
        new JsonSerializerSettings
        {
          DefaultValueHandling = DefaultValueHandling.Ignore,
          Error = (sender, args) =>
          {
            string m = args.ErrorContext.Error.Message;
            object? original = args.ErrorContext.OriginalObject;
            if (!errorMessagesReported.Contains(m))
            {
              logger.Error(m);
              errorMessagesReported.Add(m);
            }
            args.ErrorContext.Handled = true;
          },
          // TypeNameHandling = TypeNameHandling.Auto
        };

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
    /// Save snapshot files of Players, Teams, and Sources to the given directory.
    /// </summary>
    public static void SaveSnapshots(string saveDirectory, IEnumerable<Player>? players, IEnumerable<Team>? teams, IEnumerable<Source>? sources)
    {
      Task savePlayersTask = Task.Run(async () => await SaveSnapshotAsync("Players", saveDirectory, players));
      Task saveTeamsTask = Task.Run(async () => await SaveSnapshotAsync("Teams", saveDirectory, teams));
      Task saveSourcesTask = Task.Run(async () => await SaveSnapshotAsync("Sources", saveDirectory, sources));
      Task.WaitAll(savePlayersTask, saveTeamsTask, saveSourcesTask);
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

    /// <summary>
    /// Save the current state of the database to its save directory.
    /// </summary>
    public SplatTagJsonSnapshotDatabase Save(IEnumerable<Player> savePlayers, IEnumerable<Team> saveTeams, IEnumerable<Source> saveSources)
    {
      SaveSnapshots(saveDirectory, savePlayers, saveTeams, saveSources);
      return this;
    }

    /// <summary>
    /// Saves the database using its internal player, team, and source values.
    /// </summary>
    internal SplatTagJsonSnapshotDatabase SaveInternal()
    {
      if (!Loaded)
      {
        throw new InvalidOperationException("Cannot save an unloaded database.");
      }
      return Save(_players, _teams.Values, _sources.Values);
    }

    private static (Player[], Team[], Dictionary<string, Source>) Load(string? playersSnapshotFile, string? teamsSnapshotFile, string? sourcesSnapshotFile)
    {
      if (playersSnapshotFile == null || teamsSnapshotFile == null || sourcesSnapshotFile == null)
        return (Array.Empty<Player>(), Array.Empty<Team>(), new Dictionary<string, Source>());

      try
      {
        WinApi.TryTimeBeginPeriod();
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
        WinApi.TryTimeEndPeriod();
      }
    }

    private static T[] LoadSnapshot<T>(string filePath, JsonSerializerSettings settings, int capacityHint) where T : class
    {
      var result = new List<T>(capacityHint);
      var serializer = JsonSerializer.Create(settings);
      using (StreamReader file = File.OpenText(filePath))
      using (var reader = new JsonTextReader(file))
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

    private static async Task SaveSnapshotAsync(string title, string saveDirectory, IEnumerable<object>? contents)
    {
      if (contents == null)
      {
        return;
      }

      try
      {
        string filePath = Path.Combine(saveDirectory, "Snapshot-" + title + "-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".json");
        Encoding outputEnc = new UTF8Encoding(false); // UTF-8 no BOM

        using TextWriter file = new StreamWriter(filePath, false, outputEnc);
        await file.WriteLineAsync(JsonConvert.SerializeObject(contents, Formatting.None)).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
        string error = $"Unable to save the {nameof(SplatTagJsonSnapshotDatabase)} {title} because of an exception: {ex}";
        logger.Error(ex, error);
      }
    }

    public bool GetTeamById(Guid id, out Team team)
    {
      team = GetTeamById(id);
      return !team.Equals(Team.UnknownTeam);
    }

    public Team GetTeamById(Guid id) => _teams.Get(id, Team.UnknownTeam);

    public IReadOnlyList<(Player player, bool mostRecent)> GetPlayersForTeam(Team t)
    {
      return t.GetPlayers(Players)
          .AsParallel()
          .Select(p => (p, p.CurrentTeam == t.Id))
          .ToArray();
    }
  }
}