using NLog;
using SplatTagCore;
using System;
using System.IO;

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
    public static string GetDefaultPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SplatTag");

    /// <summary>
    /// Create the <see cref="SplatTagController"/> and an optional <see cref="GenericFilesToIImporters"/>.
    /// </summary>
    /// <param name="suppressLoad">Suppress the initialisation of the database. Can be used for a rebuild.</param>
    /// <param name="saveFolder">Optional save folder. If null, defaults to %appdata%/SplatTag.</param>
    /// <param name="sourcesFile">Optional sources file. If null, defaults to default sources.yaml</param>
    /// <returns>SplatTagController and GenericFilesImporter if one was created.</returns>
    public static (SplatTagController, GenericFilesToIImporters?) CreateController(bool suppressLoad = false, string? saveFolder = null, string? sourcesFile = null)
    {
      if (saveFolder == null)
      {
        saveFolder = GetDefaultPath();
      }
      Directory.CreateDirectory(saveFolder);

      GenericFilesToIImporters? sourcesImporter = null;
      SplatTagJsonSnapshotDatabase snapshotDatabase = new SplatTagJsonSnapshotDatabase(saveFolder);
      SplatTagController splatTagController = new SplatTagController(snapshotDatabase);

      if (suppressLoad)
      {
        return (splatTagController, sourcesImporter);
      }

      // Try a load here.
      // If we were able to load from a snapshot then we don't need the other importers.
      // Otherwise, do the processing and record the snapshot.
      try
      {
        splatTagController.Initialise();
        if (splatTagController.MatchPlayer(null).Length == 0)
        {
          (sourcesImporter, splatTagController) =
            GenerateNewDatabase(
              saveFolder: saveFolder,
              sourcesFile: sourcesFile);
        }
      }
      catch (Exception ex)
      {
        logger.Error(ex, $"Unable to initialise the {nameof(SplatTagController)} because of an exception");
      }

      return (splatTagController, sourcesImporter);
    }

    /// <summary>
    /// Patch the current Database. Optionally save.
    /// </summary>
    /// <param name="saveFolder">The working directory of the Controller. Set to null for default handling.</param>
    /// <returns>The Generic Importer and the Controller.</returns>
    /// <exception cref="Exception">This method may throw based on the Initialisation method.</exception>
    public static void GenerateDatabasePatch(
      string patchFile,
      string? saveFolder = null)
    {
      logger.Info($"GenerateDatabasePatch called with patchFile={patchFile}, saveFolder={saveFolder}...");

      if (saveFolder == null)
      {
        saveFolder = Directory.GetParent(patchFile).FullName;
      }

      try
      {
        WinApi.TryTimeBeginPeriod(1);
        Directory.CreateDirectory(saveFolder);

        // Load.
        SplatTagJsonSnapshotDatabase splatTagJsonSnapshotDatabase = new SplatTagJsonSnapshotDatabase(saveFolder);
        GenericFilesToIImporters iImporters = new GenericFilesToIImporters(saveFolder, patchFile);
        MultiDatabase database = new MultiDatabase()
          .With(splatTagJsonSnapshotDatabase)
          .With(iImporters);

        SplatTagController splatTagController = new SplatTagController(database);
        splatTagController.Initialise();

        // Now that we've initialised, take a snapshot of everything.
        SaveDatabase(splatTagController, splatTagJsonSnapshotDatabase);
      }
      finally
      {
        WinApi.TryTimeEndPeriod(1);
      }
    }

    /// <summary>
    /// Generate a new Database. Optionally save.
    /// </summary>
    /// <param name="saveFolder">The working directory of the Controller. Set to null for default handling.</param>
    /// <returns>The Generic Importer and the Controller.</returns>
    /// <exception cref="Exception">This method may throw based on the Initialisation method.</exception>
    public static (GenericFilesToIImporters, SplatTagController) GenerateNewDatabase(
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
        WinApi.TryTimeBeginPeriod(1);

        // Directories created in GenericFilesToIImporters
        GenericFilesToIImporters sourcesImporter = new GenericFilesToIImporters(saveFolder, sourcesFile);
        MultiDatabase splatTagDatabase = new MultiDatabase().With(sourcesImporter);
        SplatTagController splatTagController = new SplatTagController(splatTagDatabase);
        logger.Info($"Full load of {sourcesImporter.Sources.Count} files from dir {Path.GetFullPath(saveFolder)}...");
        splatTagController.Initialise();

        // Now that we've initialised, take a snapshot of everything.
        SaveDatabase(splatTagController, saveFolder);
        return (sourcesImporter, splatTagController);
      }
      finally
      {
        WinApi.TryTimeEndPeriod(1);
      }
    }

    public static void SaveDatabase(SplatTagController splatTagController, SplatTagJsonSnapshotDatabase snapshotDatabase)
    {
      snapshotDatabase.Save(splatTagController.MatchPlayer(null), splatTagController.MatchTeam(null), splatTagController.GetSources());
    }

    public static SplatTagJsonSnapshotDatabase SaveDatabase(SplatTagController splatTagController, string? saveFolder = null)
    {
      if (saveFolder == null)
      {
        saveFolder = GetDefaultPath();
      }
      return new SplatTagJsonSnapshotDatabase(saveFolder).Save(splatTagController.MatchPlayer(null), splatTagController.MatchTeam(null), splatTagController.GetSources());
    }
  }
}