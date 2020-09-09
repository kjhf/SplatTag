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

      TwitterReader twitterReader = new TwitterReader(input);
      if (twitterReader.AcceptsInput(input))
      {
        try
        {
          var (_, loadedTeams) = twitterReader.Load();
          Merger.MergeTeams(teams, loadedTeams);
          return "";
        }
        catch (Exception ex)
        {
          return ex.Message;
        }
      }
      else
      {
        LUTIJsonReader lutiReader = new LUTIJsonReader(input);
        if (lutiReader.AcceptsInput(input))
        {
          try
          {
            var (loadedPlayers, loadedTeams) = lutiReader.Load();
            var teamDictionaryPreMerge = loadedTeams.ToDictionary(team => team.Id, team => team);
            Merger.MergeTeams(teams, loadedTeams);
            Merger.MergePlayers(players, loadedPlayers, teamDictionaryPreMerge);
            return "";
          }
          catch (Exception ex)
          {
            return ex.Message;
          }
        }
        else
        {
          BattlefyJsonReader battlefyReader = new BattlefyJsonReader(input);
          if (battlefyReader.AcceptsInput(input))
          {
            try
            {
              var (loadedPlayers, loadedTeams) = battlefyReader.Load();
              var teamDictionaryPreMerge = loadedTeams.ToDictionary(team => team.Id, team => team);
              Merger.MergeTeams(teams, loadedTeams);
              Merger.MergePlayers(players, loadedPlayers, teamDictionaryPreMerge);
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
  }
}