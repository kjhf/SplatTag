using SplatTagCore;
using SplatTagDatabase.Importers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SplatTagDatabase
{
  public class GenericFilesImporter : IImporter
  {
    private readonly List<string> sources = new List<string>();
    private readonly string saveDirectory;

    private readonly SortedDictionary<uint, Player> players = new SortedDictionary<uint, Player>();
    private readonly SortedDictionary<uint, Team> teams = new SortedDictionary<uint, Team>();
    private string SourcesFile => Path.Combine(saveDirectory, "sources.yaml");

    public string[] Sources => sources.ToArray();

    public GenericFilesImporter(string saveDirectory)
    {
      this.saveDirectory = saveDirectory ?? throw new ArgumentNullException(nameof(saveDirectory));
      Directory.CreateDirectory(saveDirectory);
      if (File.Exists(SourcesFile))
      {
        sources = new List<string>(File.ReadAllLines(SourcesFile));
      }
    }

    public (Player[], Team[]) Load()
    {
      foreach (string file in Sources)
      {
        string error = TryImportFromPath(file);
        if (error != string.Empty)
        {
          Console.WriteLine(error);
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
      File.WriteAllLines(SourcesFile, contents);
    }

    private string TryImportFromPath(string input)
    {
      // Remove preceding and seceding quotes from path.
      input = input.TrimStart('"').TrimEnd('"');

      // TODO --
      // A local file should be a readable format, in priority order: json, html, database (misp), xls, xml
      // A site should simply download the file or an html contents rep, and load the contents as if it were a local file.
      // We should be mindful about loading files that come from the internet though: always validate first.
      //
      // In future, we can respell this as an array and iterate over known importers.
      if (!File.Exists(input))
      {
        return "Input does not exist on disk. Remote is not currently supported.";
      }

      LUTIJsonReader reader = new LUTIJsonReader(input);
      if (reader.AcceptsInput(input))
      {
        try
        {
          var (loadedPlayers, loadedTeams) = reader.Load();
          Merger.MergeTeams(teams, loadedTeams);
          Merger.MergePlayers(players, loadedPlayers);
          return "";
        }
        catch (Exception ex)
        {
          return ex.Message;
        }
      }
      else
      {
        return "File extension not recognised or supported.";
      }
    }
  }
}