using SplatTagCore;
using SplatTagDatabase.Importers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SplatTagDatabase
{
  public class GenericFilesImporter : IImporter
  {
    public const string SourcesFileName = "sources.yaml";
    private readonly List<string> sources = new List<string>();
    private readonly string? sourcesFile;
    private readonly string? saveDirectory;

    public IReadOnlyCollection<string> Sources => sources;

    public GenericFilesImporter(string saveDirectory)
    {
      this.saveDirectory = saveDirectory ?? throw new ArgumentNullException(nameof(saveDirectory));

      this.sourcesFile = Path.Combine(saveDirectory, SourcesFileName);
      Directory.CreateDirectory(saveDirectory);
      if (File.Exists(sourcesFile))
      {
        sources = new List<string>(File.ReadAllLines(sourcesFile));
      }
    }

    public GenericFilesImporter(IEnumerable<string> loadedSources)
    {
      sources = new List<string>(loadedSources);
    }

    public (Player[], Team[]) Load()
    {
      List<Player> players = new List<Player>();
      List<Team> teams = new List<Team>();

      // Make sure that relative paths are correctly defined.
      var currentDirectory = Directory.GetCurrentDirectory();
      Directory.SetCurrentDirectory(this.saveDirectory);

      // Iterate through the sources to load them.
      string lastProgressBar = "";
      for (int i = 0; i < sources.Count; i++)
      {
        string file = sources[i];

        try
        {
          var (loadedPlayers, loadedTeams) = TryImportFromPath(file);
          var teamsMergeResult = Merger.MergeTeamsByPersistentIds(teams, loadedTeams);
          Merger.MergePlayers(players, loadedPlayers);
          Merger.CorrectTeamIdsForPlayers(players, teamsMergeResult);
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine(ex);
        }

        string progressBar = Util.GetProgressBar(i, sources.Count, 100);
        if (!progressBar.Equals(lastProgressBar))
        {
          Trace.WriteLine(progressBar);
          lastProgressBar = progressBar;
        }
      }

      // Re-set the current directory.
      Directory.SetCurrentDirectory(currentDirectory);

      // And return the loaded players and teams.
      return (players.ToArray(), teams.ToArray());
    }

    public void SetSingleSource(string source)
    {
      sources.Clear();
      sources.Add(source);
    }

    /// <summary>
    /// Save the new sources collection.
    /// </summary>
    /// <param name="contents"></param>
    public void SaveSources(string[] contents)
    {
      sources.Clear();
      sources.AddRange(contents);

      if (this.sourcesFile != null)
      {
        File.WriteAllLines(sourcesFile, contents);
      }
    }

    private static (Player[], Team[]) TryImportFromPath(string input)
    {
      // Remove preceding and seceding quotes from path.
      input = input.TrimStart('"').TrimEnd('"');

      if (Directory.Exists(input) && input.Contains("/statink"))
      {
        List<Player> players = new List<Player>();
        List<Team> teams = new List<Team>();
        foreach (var file in Directory.EnumerateFiles(input))
        {
          if (StatInkReader.AcceptsInput(file))
          {
            try
            {
              StatInkReader reader = new StatInkReader(file);
              var (loadedPlayers, loadedTeams) = reader.Load();
              players.AddRange(loadedPlayers);
              teams.AddRange(loadedTeams);
            }
            catch (Exception ex)
            {
              Trace.WriteLine(ex);
              Trace.WriteLine($"Failed to read Stat Ink JSON input {input}: {ex.Message}");
            }
          }
        }
        return (players.ToArray(), teams.ToArray());
      }
      else if (!File.Exists(input))
      {
        throw new InvalidOperationException($"Input does not exist on disk. Remote is not currently supported ({input}).");
      }
      else if (TwitterReader.AcceptsInput(input))
      {
        try
        {
          TwitterReader twitterReader = new TwitterReader(input);
          return twitterReader.Load();
        }
        catch (Exception ex)
        {
          Trace.WriteLine(ex);
          throw new InvalidOperationException($"Failed to read Twitter input {input}: {ex.Message}", ex);
        }
      }
      else if (SendouReader.AcceptsInput(input))
      {
        try
        {
          SendouReader sendouReader = new SendouReader(input);
          return sendouReader.Load();
        }
        catch (Exception ex)
        {
          Trace.WriteLine(ex);
          throw new InvalidOperationException($"Failed to read Sendou input {input}: {ex.Message}", ex);
        }
      }
      else if (TSVReader.AcceptsInput(input))
      {
        try
        {
          TSVReader tsvReader = new TSVReader(input);
          return tsvReader.Load();
        }
        catch (Exception ex)
        {
          Trace.WriteLine(ex);
          throw new InvalidOperationException($"Failed to read TSV input {input}: {ex.Message}", ex);
        }
      }
      else if (LUTIJsonReader.AcceptsInput(input))
      {
        try
        {
          LUTIJsonReader lutiReader = new LUTIJsonReader(input);
          return lutiReader.Load();
        }
        catch (Exception ex)
        {
          Trace.WriteLine(ex);
          throw new InvalidOperationException($"Failed to read LUTI JSON input {input}: {ex.Message}");
        }
      }
      else if (BattlefyJsonReader.AcceptsInput(input))
      {
        try
        {
          BattlefyJsonReader battlefyReader = new BattlefyJsonReader(input);
          return battlefyReader.Load();
        }
        catch (Exception ex)
        {
          Trace.WriteLine(ex);
          throw new InvalidOperationException($"Failed to read Battlefy JSON input {input}: {ex.Message}", ex);
        }
      }
      else
      {
        throw new NotImplementedException("File extension not recognised or supported.");
      }
    }
  }
}