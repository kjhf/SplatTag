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
      if (!forceLoad)
      {
        splatTagController.Initialise();
      }

      if (forceLoad || splatTagController.MatchPlayer(null).Length == 0)
      {
        sourcesImporter = new GenericFilesImporter(saveFolder);
        MultiDatabase splatTagDatabase = new MultiDatabase(saveFolder, sourcesImporter);
        splatTagController = new SplatTagController(splatTagDatabase);
        Trace.WriteLine($"Full load of {sourcesImporter.Sources.Count} files...");
        splatTagController.Initialise();

        // Now that we've initialised, take a snapshot of everything.
        snapshotDatabase.Save(splatTagController.MatchPlayer(null), splatTagController.MatchTeam(null));
      }

      return (splatTagController, sourcesImporter);
    }
  }
}