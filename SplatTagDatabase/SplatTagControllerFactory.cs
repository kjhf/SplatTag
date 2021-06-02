using SplatTagCore;
using System;
using System.IO;

namespace SplatTagDatabase
{
  public static class SplatTagControllerFactory
  {
    /// <summary>
    /// Get the SplatTag application data folder
    /// </summary>
    /// <returns></returns>
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
              sourcesFile: sourcesFile,
              snapshotDatabase: snapshotDatabase);
        }
      }
      catch (Exception ex)
      {
        string error = $"Unable to initialise the {nameof(SplatTagController)} because of an exception:\n {ex} \n";
        Console.Error.WriteLine(error);
        Console.WriteLine(error);
      }

      return (splatTagController, sourcesImporter);
    }

    /// <summary>
    /// Generate a new Database. Optionally save.
    /// </summary>
    /// <param name="saveFolder">The working directory of the Controller. Set to null for default handling.</param>
    /// <param name="snapshotDatabase">The Snapshot Database. Set to null for default handling.</param>
    /// <returns>The Generic Importer and the Controller.</returns>
    /// <exception cref="Exception">This method may throw based on the Initialisation method.</exception>
    public static (GenericFilesToIImporters, SplatTagController) GenerateNewDatabase(
      string? saveFolder = null,
      string? sourcesFile = null,
      SplatTagJsonSnapshotDatabase? snapshotDatabase = null)
    {
      Console.WriteLine($"GenerateNewDatabase called with saveFolder={saveFolder}, sourcesFile={sourcesFile}, snapshotDatabase={snapshotDatabase}...");

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
        MultiDatabase splatTagDatabase = new MultiDatabase(saveFolder, sourcesImporter);
        SplatTagController splatTagController = new SplatTagController(splatTagDatabase);
        Console.WriteLine($"Full load of {sourcesImporter.Sources.Count} files from dir {Path.GetFullPath(saveFolder)}...");
        splatTagController.Initialise();

        // Now that we've initialised, take a snapshot of everything.
        if (snapshotDatabase == null)
        {
          SaveDatabase(splatTagController, saveFolder);
        }
        else
        {
          SaveDatabase(splatTagController, snapshotDatabase);
        }

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

    public static void SaveDatabase(SplatTagController splatTagController, string? saveFolder = null)
    {
      if (saveFolder == null)
      {
        saveFolder = GetDefaultPath();
      }
      new SplatTagJsonSnapshotDatabase(saveFolder).Save(splatTagController.MatchPlayer(null), splatTagController.MatchTeam(null), splatTagController.GetSources());
    }
  }
}