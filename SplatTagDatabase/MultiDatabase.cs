using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SplatTagDatabase
{
  public class MultiDatabase : ISplatTagDatabase
  {
    private readonly string saveDirectory;
    private IImporter[] importers;
    private readonly GenericFilesToIImporters? converter;

    public MultiDatabase(string saveDirectory, GenericFilesToIImporters converter)
    {
      this.saveDirectory = saveDirectory ?? throw new ArgumentNullException(nameof(saveDirectory));
      this.converter = converter;
      this.importers = Array.Empty<IImporter>();
    }

    public MultiDatabase(string saveDirectory, params IImporter[] importers)
    {
      this.importers = (importers ?? throw new ArgumentNullException(nameof(importers)));
      this.saveDirectory = saveDirectory ?? throw new ArgumentNullException(nameof(saveDirectory));
    }

    public (Player[], Team[], Dictionary<Guid, Source>) Load()
    {
      TextWriter? logger = SplatTagController.Verbose ? Console.Out : null;

      // If we need to do our conversion first, do so now.
      if (converter != null)
      {
        importers = converter.Load();
      }

      if (importers.Length == 0)
      {
        Console.WriteLine("No importers.");
        return (Array.Empty<Player>(), Array.Empty<Team>(), new Dictionary<Guid, Source>());
      }

      // Load each importer into a Source
      Console.WriteLine($"Reading {importers.Length} sources...");
      Source[] sources = new Source[importers.Length];

      Parallel.For(0, importers.Length, i =>
      {
        try
        {
          sources[i] = importers[i].Load();
        }
        catch (Exception ex)
        {
          Console.WriteLine($"ERROR: Importer {importers[i]} failed. Discarding result and continuing. {ex}");
        }
      });

      // Merge each Source into our global Players and Teams list.
      Console.WriteLine($"Merging {sources.Length} sources beginning with {sources[0]} and ending with {sources[sources.Length - 1].Name}...");
      List<Player> players = new List<Player>();
      List<Team> teams = new List<Team>();

      string lastProgressBar = "";
      for (int i = 0; i < sources.Length; i++)
      {
        Source source = sources[i];
        Console.WriteLine($"Merging {source.Name}...");
        try
        {
          var mergeResult = Merger.MergeTeamsByPersistentIds(teams, source.Teams);
          Merger.MergePlayers(players, source.Players, logger);
          Merger.CorrectTeamIdsForPlayers(players, mergeResult, logger);
        }
        catch (Exception ex)
        {
          Console.WriteLine($"ERROR: Failed to merge during import of {source}. Discarding result and continuing. {ex}");
        }

        string progressBar = Util.GetProgressBar(sources.Length - i, sources.Length, 100);
        if (!progressBar.Equals(lastProgressBar))
        {
          Console.WriteLine(progressBar);
          lastProgressBar = progressBar;
        }
      }

      // Perform a final merge.
      Console.WriteLine("Performing final merge...");
      try
      {
        bool workDone = true;
        while (workDone)
        {
          workDone = Merger.FinalisePlayers(players, logger);
          var mergeResult = Merger.FinaliseTeams(players, teams, logger);
          Merger.CorrectTeamIdsForPlayers(players, mergeResult, logger);
          workDone |= (mergeResult.Count != 0);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"ERROR: Failed {nameof(Merger.FinalisePlayers)}. Continuing anyway. {ex}");
      }

      return (players.ToArray(), teams.ToArray(), sources.AsParallel().ToDictionary(s => s.Id, s => s));
    }
  }
}