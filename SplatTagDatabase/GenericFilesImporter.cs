using SplatTagCore;
using SplatTagDatabase.Importers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SplatTagDatabase
{
  public class GenericFilesImporter : IImporter
  {
    public const string SourcesFileName = "sources.yaml";
    private readonly List<string> sources = new List<string>();
    private readonly SortedDictionary<uint, Player> players = new SortedDictionary<uint, Player>();
    private readonly SortedDictionary<long, Team> teams = new SortedDictionary<long, Team>();
    private readonly string sourcesFile;

    public IReadOnlyCollection<string> Sources => sources;

    public GenericFilesImporter(string saveDirectory)
    {
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
      foreach (string file in Sources)
      {
        string error = TryImportFromPath(file);
        if (error != string.Empty)
        {
          Console.Error.WriteLine(error);
        }
      }

      return (players.Values.ToArray(), teams.Values.ToArray());
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

    private string TryImportFromPath(string input)
    {
      // Remove preceding and seceding quotes from path.
      input = input.TrimStart('"').TrimEnd('"');

      if (!File.Exists(input))
      {
        return "Input does not exist on disk. Remote is not currently supported.";
      }

      if (TwitterReader.AcceptsInput(input))
      {
        try
        {
          TwitterReader twitterReader = new TwitterReader(input);
          var (_, loadedTeams) = twitterReader.Load();
          Merger.MergeTeams(teams, loadedTeams);
          return "";
        }
        catch (Exception ex)
        {
          Trace.WriteLine(ex);
          return $"Failed to read Twitter input {input}: {ex.Message}";
        }
      }
      else if (SendouReader.AcceptsInput(input))
      {
        try
        {
          SendouReader sendouReader = new SendouReader(input);
          var (loadedPlayers, _) = sendouReader.Load();
          Merger.MergePlayers(players, loadedPlayers);
          return "";
        }
        catch (Exception ex)
        {
          Trace.WriteLine(ex);
          return $"Failed to read Sendou input {input}: {ex.Message}";
        }
      }
      else if (TSVReader.AcceptsInput(input))
      {
        try
        {
          TSVReader tsvReader = new TSVReader(input);
          var (loadedPlayers, loadedTeams) = tsvReader.Load();
          var mergeResult = Merger.MergeTeams(teams, loadedTeams);
          Merger.CorrectPlayerIds(loadedPlayers, mergeResult);
          Merger.MergePlayers(players, loadedPlayers);
          return "";
        }
        catch (Exception ex)
        {
          Trace.WriteLine(ex);
          return $"Failed to read TSV input {input}: {ex.Message}";
        }
      }
      else if (LUTIJsonReader.AcceptsInput(input))
      {
        try
        {
          LUTIJsonReader lutiReader = new LUTIJsonReader(input);
          var (loadedPlayers, loadedTeams) = lutiReader.Load();
          var mergeResult = Merger.MergeTeams(teams, loadedTeams);
          Merger.CorrectPlayerIds(loadedPlayers, mergeResult);
          Merger.MergePlayers(players, loadedPlayers);
          return "";
        }
        catch (Exception ex)
        {
          Trace.WriteLine(ex);
          return $"Failed to read LUTI JSON input {input}: {ex.Message}";
        }
      }
      else if (BattlefyJsonReader.AcceptsInput(input))
      {
        try
        {
          BattlefyJsonReader battlefyReader = new BattlefyJsonReader(input);
          var (loadedPlayers, loadedTeams) = battlefyReader.Load();
          var mergeResult = Merger.MergeTeams(teams, loadedTeams);
          Merger.CorrectPlayerIds(loadedPlayers, mergeResult);
          Merger.MergePlayers(players, loadedPlayers);
          return "";
        }
        catch (Exception ex)
        {
          Trace.WriteLine(ex);
          return $"Failed to read Battlefy JSON input {input}: {ex.Message}";
        }
      }
      else
      {
        return "File extension not recognised or supported.";
      }
    }
  }
}