﻿using SplatTagCore;
using SplatTagDatabase.Importers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SplatTagDatabase
{
  public class GenericFilesImporter : IImporter
  {
    public const string SourcesFileName = "sources.yaml";
    private readonly List<string> sources = new List<string>();
    private readonly SortedDictionary<uint, Player> players = new SortedDictionary<uint, Player>();
    private readonly SortedDictionary<long, Team> teams = new SortedDictionary<long, Team>();
    private readonly string sourcesFile;
    private readonly string saveDirectory;

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
      // Make sure that relative paths are correctly defined.
      var currentDirectory = Directory.GetCurrentDirectory();
      Directory.SetCurrentDirectory(this.saveDirectory);

      // Iterate through the sources to load them.
      for (int i = 0; i < sources.Count; i++)
      {
        string file = sources[i];
        string error = TryImportFromPath(file);
        if (error != string.Empty)
        {
          Console.Error.WriteLine(error);
        }
        Trace.WriteLine(GetProgressBar(i, sources.Count, 100));
      }

      // Re-set the current directory.
      Directory.SetCurrentDirectory(currentDirectory);

      // And return the loaded players and teams.
      return (players.Values.ToArray(), teams.Values.ToArray());
    }

    private static string GetProgressBar(int value, int capacity, int width = 10)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("[");
      int bars = Math.Min(width - 1, (int)(((value + 1) * width) / (double)capacity));
      for (int i = 0; i < bars; i++)
      {
        sb.Append("=");
      }
      sb.Append(">");
      for (int i = bars; i < (width - 1); i++)
      {
        sb.Append(" ");
      }
      sb.Append("]");
      return sb.ToString();
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
        return $"Input does not exist on disk. Remote is not currently supported ({input}).";
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