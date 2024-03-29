﻿using SplatTagCore;
using SplatTagDatabase.Importers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SplatTagDatabase
{
  public class GenericFilesToIImporters
  {
    public const string DefaultSourcesFileName = "sources.yaml";
    private readonly List<string> paths = new List<string>();
    private readonly string? sourcesFile;
    private readonly string? saveDirectory;

    public IReadOnlyCollection<string> Sources => paths;

    public GenericFilesToIImporters(string saveDirectory, string sourcesFile = DefaultSourcesFileName)
    {
      this.saveDirectory = saveDirectory ?? throw new ArgumentNullException(nameof(saveDirectory));
      Directory.CreateDirectory(saveDirectory);

      // If the sources file is an absolute path, take as-is.
      // Otherwise, combine with the save directory to find that file.
      if (Path.IsPathRooted(sourcesFile))
      {
        this.sourcesFile = sourcesFile;
      }
      else
      {
        this.sourcesFile = Path.Combine(saveDirectory, sourcesFile);
      }

      if (File.Exists(this.sourcesFile))
      {
        paths = new List<string>(File.ReadAllLines(this.sourcesFile).Where(s => !string.IsNullOrWhiteSpace(s)));
      }
      else
      {
        Console.WriteLine($"Sources file doesn't exist `{this.sourcesFile}`.");
      }
    }

    public GenericFilesToIImporters(IEnumerable<string> loadedSources)
    {
      paths = new List<string>(loadedSources.Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    public IImporter[] Load()
    {
      // Make sure that relative paths are correctly defined.
      var currentDirectory = Directory.GetCurrentDirectory();
      Directory.SetCurrentDirectory(this.saveDirectory);

      // Convert each path to an importer
      IImporter[] importers = paths.SelectMany(file => PathToIImporter(file)).ToArray();

      // Re-set the current directory.
      Directory.SetCurrentDirectory(currentDirectory);

      return importers;
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

      if (string.IsNullOrWhiteSpace(input))
      {
        throw new ArgumentException("Input path cannot be null or whitespace.", nameof(input));
      }

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