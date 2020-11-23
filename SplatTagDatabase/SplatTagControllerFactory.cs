using SplatTagCore;
using System;
using System.Diagnostics;
using System.IO;

namespace SplatTagDatabase
{
  public static class SplatTagControllerFactory
  {
    /// <summary>
    /// Create the <see cref="SplatTagController"/> and an optional <see cref="GenericFilesImporter"/>.
    /// </summary>
    /// <param name="forceLoad">Force generating a GenericFilesImporter and subsequent source loading.</param>
    /// <param name="saveFolder">Optional save folder. If null, defaults to %appdata%/SplatTag.</param>
    /// <returns>SplatTagController and GenericFilesImporter if one was created.</returns>
    public static (SplatTagController, GenericFilesImporter) CreateController(bool forceLoad = false, string saveFolder = null)
    {
      if (saveFolder == null)
      {
        saveFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SplatTag");
      }
      Directory.CreateDirectory(saveFolder);

      GenericFilesImporter sourcesImporter = null;
      SplatTagJsonSnapshotDatabase snapshotDatabase = new SplatTagJsonSnapshotDatabase(saveFolder);
      SplatTagController splatTagController = new SplatTagController(snapshotDatabase);

      // If not forced, try a load here.
      // If we were able to load from a snapshot then we don't need the other importers.
      // Otherwise, do the processing and record the snapshot.
      try
      {
        if (!forceLoad)
        {
          splatTagController.Initialise();
        }

        if (forceLoad || splatTagController.MatchPlayer(null).Length == 0)
        {
          (sourcesImporter, splatTagController) = GenerateNewDatabase(saveFolder, snapshotDatabase);
        }
      }
      catch (Exception ex)
      {
        string error = $"Unable to initialise the {nameof(SplatTagController)} because of an exception:\n {ex} \n";
        Console.Error.WriteLine(error);
        Trace.WriteLine(error);
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
    public static (GenericFilesImporter, SplatTagController) GenerateNewDatabase(string saveFolder = null, SplatTagJsonSnapshotDatabase snapshotDatabase = null)
    {
      if (saveFolder == null)
      {
        saveFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SplatTag");
      }
      Directory.CreateDirectory(saveFolder);

      GenericFilesImporter sourcesImporter = new GenericFilesImporter(saveFolder);
      MultiDatabase splatTagDatabase = new MultiDatabase(saveFolder, sourcesImporter);
      SplatTagController splatTagController = new SplatTagController(splatTagDatabase);
      Trace.WriteLine($"Full load of {sourcesImporter.Sources.Count} files...");
      splatTagController.Initialise();

      // Now that we've initialised, take a snapshot of everything.
      (snapshotDatabase ?? (new SplatTagJsonSnapshotDatabase(saveFolder)))
        .Save(splatTagController.MatchPlayer(null), splatTagController.MatchTeam(null));
      return (sourcesImporter, splatTagController);
    }
  }
}