using NLog;
using SplatTagCore;
using SplatTagDatabase.Merging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SplatTagDatabase
{
  public static class SplatTagControllerFactory
  {
    static SplatTagControllerFactory()
    {
      LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(PathUtils.FindFileUpToRoot("nlog.config"));
    }

    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Get the SplatTag application data folder
    /// </summary>
    public static string GetDefaultPath()
    {
      string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SplatTag");
      Directory.CreateDirectory(dir);
      return dir;
    }

    public static string GetDumpPath()
    {
      string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SplatTag", "Dumps");
      Directory.CreateDirectory(dir);
      return dir;
    }

    /// <summary>
    /// Create the <see cref="SplatTagController"/> and an optional <see cref="GenericFilesToIImporters"/>.
    /// </summary>
    /// <param name="saveFolder">Optional save folder. If null, defaults to %appdata%/SplatTag.</param>
    /// <returns>SplatTagController and GenericFilesImporter if one was created.</returns>
    public static SplatTagController CreateControllerNoLoad(string? saveFolder = null)
    {
      if (saveFolder == null)
      {
        saveFolder = GetDefaultPath();
      }

      Directory.CreateDirectory(saveFolder);
      return new SplatTagController(new SplatTagJsonSnapshotDatabase(saveFolder));
    }

    /// <summary>
    /// Initialise the SplatTagController.
    /// If the database is not found, it will be created and initialised.
    /// </summary>
    /// <param name="saveFolder">Optional save folder. If null, defaults to %appdata%/SplatTag.</param>
    public static void EnsureInitialised(SplatTagController splatTagController, string? saveFolder = null, string? sourcesFile = null)
    {
      try
      {
        if (!splatTagController.Initialise())
        {
          var database = GenerateNewDatabase(saveFolder, sourcesFile);
          splatTagController.SetDatabase(database);  // This will kick off another load.
        }
      }
      catch (Exception ex)
      {
        logger.Error(ex, $"Unable to initialise the {nameof(SplatTagController)} because of an exception");
      }
    }

    /// <summary>
    /// Create the <see cref="SplatTagController"/> and an optional <see cref="GenericFilesToIImporters"/>.
    /// </summary>
    /// <param name="suppressLoad">Suppress the initialisation of the database. Can be used for a rebuild.</param>
    /// <param name="saveFolder">Optional save folder. If null, defaults to %appdata%/SplatTag.</param>
    /// <param name="sourcesFile">Optional sources file. If null, defaults to default sources.yaml</param>
    /// <returns>SplatTagController and GenericFilesImporter if one was created.</returns>
    [Obsolete("Use CreateControllerNoLoad and optionally call EnsureInitialised instead")]
    public static (SplatTagController, GenericFilesToIImporters?) CreateController(bool suppressLoad = false, string? saveFolder = null, string? sourcesFile = null)
    {
      var splatTagController = CreateControllerNoLoad(saveFolder);
      if (!suppressLoad)
      {
        EnsureInitialised(splatTagController, saveFolder, sourcesFile);
        return (splatTagController, null);
      }
      return (splatTagController, null);
    }

    /// <summary>
    /// Load the current database and re-perform a merge on it.
    /// </summary>
    public static bool MergeJSONDatabase(string saveFolder)
    {
      logger.Info($"{nameof(MergeJSONDatabase)} called with saveFolder={saveFolder}...");
      var database = new SplatTagJsonSnapshotDatabase(saveFolder);
      database.Load();

      CoreMergeHandler mergeHandler = new();
      mergeHandler.SetInternalNoMerge(database.Players, database.Teams.Values);

      var mergeResults = mergeHandler.MergeKnown();
      if (mergeResults.Length == 0)
      {
        logger.Warn("Nothing merged!");
        return false;
      }

      PathUtils.DumpLogSafely("MergeLog", () => CoreMergeHandler.GetMergeLogContents(mergeResults));
      var last = mergeResults.Last();
      SplatTagJsonSnapshotDatabase.SaveSnapshots(saveFolder, last.ResultingPlayers, last.ResultingTeams, null);
      return true;
    }

    /// <summary>
    /// Patch the current Database with a new sources file.
    /// </summary>
    public static void GeneratePatchedDatabaseFromFile(
    string patchFile,
    string? saveFolder = null)
    {
      logger.Info($"{nameof(GeneratePatchedDatabaseFromFile)} called with patchFile={patchFile}, saveFolder={saveFolder}...");

      if (saveFolder == null)
      {
        saveFolder = Directory.GetParent(patchFile).FullName;
      }

      GeneratePatchedDatabase(new GenericFilesToIImporters(saveFolder, patchFile), saveFolder);
    }

    /// <summary>
    /// Patch the current Database with a new sources file.
    /// </summary>
    public static void GeneratePatchedDatabaseFromNewSource(
      string source,
      string saveFolder)
    {
      logger.Info($"{nameof(GeneratePatchedDatabaseFromNewSource)} called with source={source}, saveFolder={saveFolder}...");
      var importer = new GenericFilesToIImporters(saveFolder).SetSingleSource(source);
      GeneratePatchedDatabase(importer, saveFolder);

      // TODO should also handle http sources
    }

    /// <summary>
    /// Patch the current Database with a specified importer to merge from.
    /// </summary>
    private static void GeneratePatchedDatabase(
      GenericFilesToIImporters importer,
      string saveFolder)
    {
      logger.Info($"{nameof(GeneratePatchedDatabase)} called, saveFolder={saveFolder}...");

      try
      {
        WinApi.TryTimeBeginPeriod();
        Directory.CreateDirectory(saveFolder);

        // Create the things.
        var splatTagJsonSnapshotDatabase = new SplatTagJsonSnapshotDatabase(saveFolder);
        MultiDatabase database = new MultiDatabase()
          .With(splatTagJsonSnapshotDatabase)
          .With(importer);

        // Load the database (and generate)
        var splatTagController = new SplatTagController(database);
        splatTagController.Initialise();

        // Now that we've initialised, take a snapshot of everything.
        database.SaveInternal(saveFolder);
      }
      finally
      {
        WinApi.TryTimeEndPeriod();
      }
    }

    /// <summary>
    /// Generate a new Database. Optionally save.
    /// </summary>
    /// <param name="saveFolder">The working directory of the Controller. Set to null for default handling.</param>
    /// <returns>The Generic Importer and the Controller.</returns>
    /// <exception cref="Exception">This method may throw based on the Initialisation method.</exception>
    public static MultiDatabase GenerateNewDatabase(
      string? saveFolder = null,
      string? sourcesFile = null)
    {
      logger.Info($"GenerateNewDatabase called with saveFolder={saveFolder}, sourcesFile={sourcesFile}...");

      if (sourcesFile == null)
      {
        sourcesFile = GenericFilesToIImporters.DefaultSourcesFileName;

        if (saveFolder == null)
        {
          saveFolder = GetDefaultPath();
        }
      }
      else if (saveFolder == null)
      {
        saveFolder = Directory.GetParent(sourcesFile).FullName;
      }

      try
      {
        WinApi.TryTimeBeginPeriod();

        // Directories created in GenericFilesToIImporters
        var sourcesImporter = new GenericFilesToIImporters(saveFolder, sourcesFile);
        var splatTagDatabase = new MultiDatabase().With(sourcesImporter);

        logger.Info($"Full load of {sourcesImporter.Sources.Count} files from dir {Path.GetFullPath(saveFolder)}...");
        bool loaded = splatTagDatabase.Load();

        // Now that we've initialised, take a snapshot of everything.
        if (loaded)
        {
          splatTagDatabase.SaveInternal(saveFolder);
        }
        else
        {
          logger.Error($"Unable to load the database from {saveFolder}");
        }
        return splatTagDatabase;
      }
      finally
      {
        WinApi.TryTimeEndPeriod();
      }
    }

    /// <summary>
    /// Save the database to a snapshot. Calls load if the database is not loaded.
    /// </summary>
    /// <param name="database"></param>
    /// <param name="saveFolder"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public static void SaveDatabase(ISplatTagDatabase database, string? saveFolder = null)
    {
      if (saveFolder == null)
      {
        saveFolder = GetDefaultPath();
      }

      bool loaded = database.Loaded;
      if (!loaded)
      {
        loaded = database.Load();
      }

      if (loaded)
      {
        if (database is SplatTagJsonSnapshotDatabase splatTagJsonSnapshotDatabase)
        {
          splatTagJsonSnapshotDatabase.SaveInternal();
        }
        else if (database is MultiDatabase multiDatabase)
        {
          multiDatabase.SaveInternal(saveFolder);
        }
        else
        {
          throw new NotImplementedException($"Please implement save on {database.GetType()}");
        }
      }
      else
      {
        logger.Error("Nothing was loaded from the database so cannot save it. If a merge occurred, likely it has errored.");
      }
    }

    /// <summary>
    /// Set the NLog level for all logging, from minLevel to maxLevel.
    /// If minLevel is null, it will be set to the lowest level (Trace).
    /// If maxLevel is null, it will be set to Fatal (i.e. minLevel and above excluding "Off").
    /// </summary>
    /// <param name="minLevel"></param>
    /// <param name="maxLevel"></param>
    public static void SetNLogLevel(LogLevel? minLevel = null, LogLevel? maxLevel = null)
    {
      if (minLevel == null)
      {
        minLevel = LogLevel.Trace;
      }

      if (maxLevel == null)
      {
        maxLevel = LogLevel.Fatal;
      }

      foreach (var rule in LogManager.Configuration.LoggingRules)
      {
        rule.SetLoggingLevels(minLevel, maxLevel);
      }
      LogManager.ReconfigExistingLoggers();
    }
  }
}