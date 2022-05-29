using NLog;
using SplatTagCore;
using SplatTagDatabase.Merging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SplatTagDatabase
{
  public class MultiDatabase : ISplatTagDatabase
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private HashSet<IImporter> importers = new();
    private SplatTagJsonSnapshotDatabase? jsonDatabase;
    private Player[] _players = Array.Empty<Player>();
    private Dictionary<Guid, Team> _teams = new();
    private Dictionary<string, Source> _sources = new();

    public IReadOnlyList<Player> Players => _players.Length == 0 ? Array.Empty<Player>() : _players;
    public IReadOnlyDictionary<Guid, Team> Teams => _teams;
    public IReadOnlyDictionary<string, Source> Sources => _sources;

    public bool Loaded => _sources.Count > 0;

    public MultiDatabase With(params IImporter[] importers)
    {
      this.importers = importers.ToHashSet();
      return this;
    }

    public MultiDatabase With(GenericFilesToIImporters converter)
    {
      StageSourcesFile(converter);
      return this;
    }

    public MultiDatabase With(SplatTagJsonSnapshotDatabase database)
    {
      this.jsonDatabase = database;
      return this;
    }

    public bool Load()
    {
      // If we need to do our conversion first, do so now.
      IImporter[] toLoad = importers.ToArray();

      if (toLoad.Length == 0 && jsonDatabase == null)
      {
        logger.Info("Nothing to load.");
        return false;
      }

      logger.Debug($"{nameof(MultiDatabase)}.{nameof(Load)} toLoad.Length={toLoad.Length}");

      var databaseSources = new Dictionary<string, Source>();
      var players = new List<Player>();
      var teams = new List<Team>();

      if (jsonDatabase != null)
      {
        var (newPlayers, newTeams, newSources) = jsonDatabase.LoadInline();
        players.AddRange(newPlayers);
        teams.AddRange(newTeams);
        databaseSources = newSources;

        if (toLoad.Length == 0)
        {
          logger.Debug("MultiDatabase: Loaded JSON Database only.");
          return true;
        }
      }

      // Load each importer into a Source
      // Create the destination imported sources array.
      // Offset by the sources that we've already loaded from the current database.
      Source[] importedSources = databaseSources.Values.ToArray();
      int offset = importedSources.Length;
      Array.Resize(ref importedSources, offset + toLoad.Length);

      logger.Debug($"Reading {toLoad.Length} additional sources, " +
        $"{importedSources.Length} total sources to merge from {players.Count} players, {teams.Count} teams, {databaseSources.Count} sources pre-known...");
      bool filterSourcesForNull = false;
      Parallel.For(0, toLoad.Length, i =>
      {
        try
        {
          importedSources[offset + i] = toLoad[i].Load();
        }
        catch (Exception ex)
        {
          logger.Error($"Importer {toLoad[offset + i]} failed. Discarding result and continuing. {ex}");
          filterSourcesForNull = true;
        }
      });

      if (filterSourcesForNull)
      {
        importedSources = importedSources.Where(s => s != null).ToArray();
      }

      // Merge each Source into our global Players and Teams list.
      // (But start from the sources not yet merged).
      logger.Debug($"Merging {importedSources.Length - offset} sources beginning with {importedSources[offset]} and ending with {importedSources[^1].Name}...");

      CoreMergeHandler mergeHandler = new();
      mergeHandler.AddPlayers(players);
      mergeHandler.AddTeams(teams);
      logger.Debug($"Beginning merging with {players.Count} players, {teams.Count} teams pre-merged.");

      int lastProgressBars = -1;
      for (int i = offset; i < importedSources.Length; i++)
      {
        try
        {
          Source source = importedSources[i];
          logger.Debug($"Merging {source.Name}...");
          mergeHandler.MergeSource(source);
          logger.Debug($"Source {source.Name} merged. Source had {source.Players.Length} players and {source.Teams.Length} teams incoming. ");
        }
        catch (Exception ex)
        {
          logger.Warn(ex, $"Failed to merge during import of {importedSources[i]}. Discarding result and continuing. {ex}");
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
          int progressBars = ProgressBar.CalculateProgressBars(i, importedSources.Length, 100);
          if (progressBars != lastProgressBars)
          {
            string progressBar = ProgressBar.GetProgressBar(progressBars, 100, rightToLeft: true) + " " + (i + 1) + "/" + importedSources.Length;
            logger.Debug(progressBar);
            lastProgressBars = progressBars;
          }
        }
      }

      var mergeResults = mergeHandler.MergeKnown();
      if (mergeResults.Length == 0)
      {
        logger.Warn("Nothing merged!");
        return false;
      }

      try
      {
        string filePath = Path.Combine(SplatTagControllerFactory.GetDefaultPath(), "MergeLog-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".log");
        logger.Trace("Saving to " + filePath);

        var creationWaitTask = Task.Run(() =>
        {
          // Wait until the log is written before continuing so the program has finished writing before exiting.
          PathUtils.WaitForFileCreatedAndReady(filePath);
        });

        var savingTask = Task.Run(() =>
        {
          // Write merge log
          StringBuilder sb = new();
          sb.AppendJoin('\n', mergeResults.Select(r => r.ToStringBuilder()));
          File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(false)); // UTF-8 no BOM
        });

        Task.WaitAll(creationWaitTask, savingTask);
        logger.Trace("creationWaitTask: " + creationWaitTask.Status + ", savingTask: " + savingTask.Status);
      }
      catch (Exception ex)
      {
        string error = $"Unable to save the {nameof(MultiDatabase)} merge log because of an exception: {ex}";
        logger.Error(ex, error);
      }

      var last = mergeResults.Last();
      _players = last.ResultingPlayers.ToArray();
      _teams = last.ResultingTeams.ToDictionary(t => t.Id);
      _sources = importedSources.ToDictionary(s => s.Id, s => s);
      return true;
    }

    /// <summary>
    /// Takes a sources file and stages its importer files contents ready for a load.
    /// Returns the old and new count.
    /// </summary>
    public (int oldCount, int newCount) StageSourcesFile(string saveDirectory, string sourcesFile = "sources.yaml")
      => StageSourcesFile(new GenericFilesToIImporters(saveDirectory, sourcesFile));

    /// <summary>
    /// Takes a GenericFilesToIImporters and stages its importer files contents ready for a load.
    /// Returns the old and new count.
    /// </summary>
    public (int oldCount, int newCount) StageSourcesFile(GenericFilesToIImporters converter)
    {
      var newList = converter.Load();
      int oldCount = importers.Count;
      int newCount = newList.Length;
      if (oldCount != newCount)
      {
        importers.UnionWith(newList);
      }
      return (oldCount, newCount);
    }

    /// <summary>
    /// Saves the database using its internal player, team, and source values.
    /// </summary>
    internal MultiDatabase SaveInternal(string saveDirectory)
    {
      if (!Loaded)
      {
        throw new InvalidOperationException("Cannot save an unloaded database.");
      }

      SplatTagJsonSnapshotDatabase.SaveSnapshots(saveDirectory, _players, _teams.Values, _sources.Values);
      return this;
    }
  }
}