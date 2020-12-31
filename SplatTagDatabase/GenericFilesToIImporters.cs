using SplatTagCore;
using SplatTagDatabase.Importers;
using System;
using System.Collections.Generic;
using System.IO;

namespace SplatTagDatabase
{
  public class GenericFilesToIImporters
  {
    public const string SourcesFileName = "sources.yaml";
    private readonly List<string> paths = new List<string>();
    private readonly string? sourcesFile;
    private readonly string? saveDirectory;

    public IReadOnlyCollection<string> Sources => paths;

    public GenericFilesToIImporters(string saveDirectory)
    {
      this.saveDirectory = saveDirectory ?? throw new ArgumentNullException(nameof(saveDirectory));

      this.sourcesFile = Path.Combine(saveDirectory, SourcesFileName);
      Directory.CreateDirectory(saveDirectory);
      if (File.Exists(sourcesFile))
      {
        paths = new List<string>(File.ReadAllLines(sourcesFile));
      }
    }

    public GenericFilesToIImporters(IEnumerable<string> loadedSources)
    {
      paths = new List<string>(loadedSources);
    }

    public IImporter[] Load()
    {
      List<IImporter> importers = new List<IImporter>();
      // Make sure that relative paths are correctly defined.
      var currentDirectory = Directory.GetCurrentDirectory();
      Directory.SetCurrentDirectory(this.saveDirectory);

      for (int i = 0; i < paths.Count; i++)
      {
        string file = paths[i];
        importers.AddRange(PathToIImporter(file));
      }

      // Re-set the current directory.
      Directory.SetCurrentDirectory(currentDirectory);

      return importers.ToArray();
    }

    public void SetSingleSource(string source)
    {
      paths.Clear();
      paths.Add(source);
    }

    /// <summary>
    /// Save the new sources collection.
    /// </summary>
    /// <param name="contents"></param>
    public void SaveSources(string[] contents)
    {
      paths.Clear();
      paths.AddRange(contents);

      if (this.sourcesFile != null)
      {
        File.WriteAllLines(sourcesFile, contents);
      }
    }

    private static IEnumerable<IImporter> PathToIImporter(string input)
    {
      // Remove preceding and seceding quotes from path.
      input = input.TrimStart('"').TrimEnd('"');

      // Correct paths
      if (!Path.IsPathRooted(input))
      {
        input = Path.GetFullPath(input);
      }

      if (Directory.Exists(input) && input.Contains(Path.DirectorySeparatorChar + "statink"))
      {
        foreach (var file in Directory.EnumerateFiles(input))
        {
          if (StatInkReader.AcceptsInput(file))
          {
            yield return new StatInkReader(file);
          }
        }
      }
      else if (!File.Exists(input))
      {
        Console.WriteLine($"Input does not exist on disk. Remote is not currently supported ({input}).");
      }
      else if (TwitterReader.AcceptsInput(input))
      {
        yield return new TwitterReader(input);
      }
      else if (SendouReader.AcceptsInput(input))
      {
        yield return new SendouReader(input);
      }
      else if (TSVReader.AcceptsInput(input))
      {
        yield return new TSVReader(input);
      }
      else if (LUTIJsonReader.AcceptsInput(input))
      {
        yield return new LUTIJsonReader(input);
      }
      else if (BattlefyJsonReader.AcceptsInput(input))
      {
        yield return new BattlefyJsonReader(input);
      }
      else
      {
        throw new NotImplementedException("File extension not recognised or supported.");
      }
    }
  }
}