using SplatTagCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SplatTagDatabase
{
  public class MultiDatabase : ISplatTagDatabase
  {
    private IImporter[] importers = Array.Empty<IImporter>();
    private GenericFilesToIImporters? converter;
    private SplatTagJsonSnapshotDatabase? jsonDatabase;

    public MultiDatabase With(params IImporter[] importers)
    {
      this.importers = importers;
      return this;
    }

    public MultiDatabase With(GenericFilesToIImporters converter)
    {
      this.converter = converter;
      return this;
    }

    public MultiDatabase With(SplatTagJsonSnapshotDatabase database)
    {
      this.jsonDatabase = database;
      return this;
    }

    public (Player[], Team[], Dictionary<Guid, Source>) Load()
    {
      // If we need to do our conversion first, do so now.
      List<IImporter> toLoad = new List<IImporter>();
      if (converter != null)
      {
        toLoad.AddRange(converter.Load());
      }
      toLoad.AddRange(importers);

      if (toLoad.Count == 0)
      {
        if (jsonDatabase != null)
        {
          Console.WriteLine("MultiDatabase: Loading JSON Database only.");
          return jsonDatabase.Load();
        }
        else
        {
          Console.WriteLine("Nothing to load.");
          return (Array.Empty<Player>(), Array.Empty<Team>(), new Dictionary<Guid, Source>());
        }
      }

      Console.WriteLine($"{nameof(MultiDatabase)}.{nameof(Load)} toLoad.Count={toLoad.Count}");

      Dictionary<Guid, Source> databaseSources = new Dictionary<Guid, Source>();
      List<Player> players = new List<Player>();
      List<Team> teams = new List<Team>();

      if (jsonDatabase != null)
      {
        var (newPlayers, newTeams, newSources) = jsonDatabase.Load();
        players.AddRange(newPlayers);
        teams.AddRange(newTeams);
        databaseSources = newSources;
      }

      // Load each importer into a Source
      // Create the destination imported sources array.
      // Offset by the sources that we've already loaded from the current database.
      Source[] importedSources = databaseSources.Values.ToArray();
      int offset = importedSources.Length;
      Array.Resize(ref importedSources, offset + toLoad.Count);

      Console.WriteLine($"Reading {toLoad.Count} additional sources, " +
        $"{importedSources.Length} total sources to merge from {players.Count} players, {teams.Count} teams, {databaseSources.Count} sources pre-known...");
      bool filterSourcesForNull = false;
      Parallel.For(0, toLoad.Count, i =>
      {
        try
        {
          importedSources[offset + i] = toLoad[i].Load();
        }
        catch (Exception ex)
        {
          Console.WriteLine($"ERROR: Importer {toLoad[offset + i]} failed. Discarding result and continuing. {ex}");
          filterSourcesForNull = true;
        }
      });

      if (filterSourcesForNull)
      {
        importedSources = importedSources.Where(s => s != null).ToArray();
      }

      // Merge each Source into our global Players and Teams list.
      // (But start from the sources not yet merged).
      TextWriter? logger = SplatTagController.Verbose ? Console.Out : null;

      Console.WriteLine($"Merging {importedSources.Length - offset} sources beginning with {importedSources[offset]} and ending with {importedSources[importedSources.Length - 1].Name}...");
      int lastProgressBars = -1;
      for (int i = offset; i < importedSources.Length; i++)
      {
        try
        {
          Source source = importedSources[i];
          Console.WriteLine($"Merging {source.Name}...");

          var mergeResult = Merger.MergeTeamsByPersistentIds(teams, source.Teams);
          Merger.MergePlayers(players, source.Players, logger);
          Merger.CorrectTeamIdsForPlayers(players, mergeResult, logger);
        }
        catch (Exception ex)
        {
          Console.WriteLine($"ERROR: Failed to merge during import of {importedSources[i]}. Discarding result and continuing. {ex}");
        }

        int progressBars = ProgressBar.CalculateProgressBars(i, importedSources.Length, 100);
        if (progressBars != lastProgressBars)
        {
          string progressBar = ProgressBar.GetProgressBar(progressBars, 100, true) + " " + i + "/" + importedSources.Length;
          Console.WriteLine(progressBar);
          lastProgressBars = progressBars;
        }
      }

      Merger.FinalMerge(players, teams, logger);
      return (players.ToArray(), teams.ToArray(), importedSources.ToDictionary(s => s.Id, s => s));
    }
  }
}