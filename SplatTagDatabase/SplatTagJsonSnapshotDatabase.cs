using Newtonsoft.Json;
using SplatTagCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SplatTagDatabase
{
  public class SplatTagJsonSnapshotDatabase : ISplatTagDatabase
  {
    private const string SNAPSHOT_FORMAT = "Snapshot-*.json";
    private static readonly HashSet<string> errorMessagesReported = new HashSet<string>();

    private readonly string saveDirectory;
    private string? playersSnapshotFile = null;
    private string? teamsSnapshotFile = null;
    private string? sourcesSnapshotFile = null;

    public SplatTagJsonSnapshotDatabase(string saveDirectory)
    {
      this.saveDirectory = saveDirectory ?? throw new ArgumentNullException(nameof(saveDirectory));
      Directory.CreateDirectory(saveDirectory);
    }

    public SplatTagJsonSnapshotDatabase(string playersSnapshotFile, string teamsSnapshotFile, string sourcesSnapshotFile)
    {
      this.playersSnapshotFile = playersSnapshotFile ?? throw new ArgumentNullException(nameof(playersSnapshotFile));
      this.teamsSnapshotFile = teamsSnapshotFile ?? throw new ArgumentNullException(nameof(teamsSnapshotFile));
      this.sourcesSnapshotFile = sourcesSnapshotFile ?? throw new ArgumentNullException(nameof(sourcesSnapshotFile));

      this.saveDirectory = Directory.GetParent(playersSnapshotFile).FullName;
    }

    public static IOrderedEnumerable<FileInfo> GetSnapshots(string dir)
    {
      // Check in the save directory for the latest snapshot.
      return new DirectoryInfo(dir).GetFiles(SNAPSHOT_FORMAT).OrderByDescending(f => f.LastWriteTime);
    }

    public (Player[], Team[], Dictionary<string, Source>) Load()
    {
      if (playersSnapshotFile != null && teamsSnapshotFile != null && sourcesSnapshotFile != null)
      {
        return Load(playersSnapshotFile, teamsSnapshotFile, sourcesSnapshotFile);
      }

      // Check in the save directory for the latest snapshot.
      foreach (var snapshot in GetSnapshots(saveDirectory))
      {
        if (playersSnapshotFile == null && snapshot.Name.Contains("Players"))
        {
          playersSnapshotFile = snapshot.FullName;
        }

        if (teamsSnapshotFile == null && snapshot.Name.Contains("Teams"))
        {
          teamsSnapshotFile = snapshot.FullName;
        }

        if (sourcesSnapshotFile == null && snapshot.Name.Contains("Sources"))
        {
          sourcesSnapshotFile = snapshot.FullName;
        }

        if (playersSnapshotFile != null && teamsSnapshotFile != null && sourcesSnapshotFile != null)
        {
          break;
        }
      }

      return Load(playersSnapshotFile, teamsSnapshotFile, sourcesSnapshotFile);
    }

    private static (Player[], Team[], Dictionary<string, Source>) Load(string? playersSnapshotFile, string? teamsSnapshotFile, string? sourcesSnapshotFile)
    {
      if (playersSnapshotFile == null || teamsSnapshotFile == null || sourcesSnapshotFile == null)
        return (Array.Empty<Player>(), Array.Empty<Team>(), new Dictionary<string, Source>());

      try
      {
        WinApi.TryTimeBeginPeriod(1);

        // Invoked from command line
        if (JsonConvert.DefaultSettings == null)
        {
          JsonConvert.DefaultSettings = () => new JsonSerializerSettings
          {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Error = (sender, args) =>
            {
              string m = args.ErrorContext.Error.Message;
              if (!errorMessagesReported.Contains(m))
              {
                Console.Error.WriteLine(m);
                errorMessagesReported.Add(m);
              }
              args.ErrorContext.Handled = true;
            }
          };
        }
        var settings = JsonConvert.DefaultSettings();

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Loading sourcesSnapshotFile from {sourcesSnapshotFile}... ");
        Source[] sources = LoadSnapshot<Source>(sourcesSnapshotFile, settings);
        var lookup = sources.ToDictionary(s => s.Id, s => s);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] {lookup.Count} sources transformed. ");
        GC.Collect();

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Loading playersSnapshotFile from {playersSnapshotFile}... ");
        settings.Context = new StreamingContext(StreamingContextStates.All, new Source.GuidToSourceConverter(lookup));
        Player[] players = LoadSnapshot<Player>(playersSnapshotFile, settings);

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Loading teamsSnapshotFile from {teamsSnapshotFile}... ");
        Team[] teams = LoadSnapshot<Team>(teamsSnapshotFile, settings);

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Load done... ");
        return (players, teams, lookup);
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine("An error occurred in loading the Snapshot Database.");
        Console.Error.WriteLine(ex);
        throw;
      }
      finally
      {
        WinApi.TryTimeEndPeriod(1);
      }
    }

    private static T[] LoadSnapshot<T>(string filePath, JsonSerializerSettings settings) where T : class
    {
      List<T> result = new List<T>();
      using (StreamReader file = File.OpenText(filePath))
      using (JsonTextReader reader = new JsonTextReader(file))
      {
        reader.SupportMultipleContent = true;

        var serializer = JsonSerializer.Create(settings);
        while (reader.Read())
        {
          if (reader.TokenType == JsonToken.StartObject)
          {
            try
            {
              var toAdd = serializer.Deserialize<T>(reader);
              if (toAdd == null)
              {
                throw new ArgumentNullException("Should not have deserialized null.");
              }
              result.Add(toAdd);
            }
            catch (Exception ex)
            {
              var restore = Console.BackgroundColor;
              Console.BackgroundColor = ConsoleColor.Red;
              Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] Could not parse {typeof(T)} from line " + reader.LineNumber + ".");
              Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] " + ex);
              Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fffffff}] " + reader);
              Console.BackgroundColor = restore;
            }
          }
        }
      }

      return result.ToArray();
    }

    public SplatTagJsonSnapshotDatabase Save(IEnumerable<Player> savePlayers, IEnumerable<Team> saveTeams, IEnumerable<Source> saveSources)
    {
      Task savePlayersTask = Task.Run(async () =>
      {
        try
        {
          // Write players
          string filePath = Path.Combine(saveDirectory, "Snapshot-Players-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".json");
          Encoding outputEnc = new UTF8Encoding(false); // UTF-8 no BOM

          using TextWriter file = new StreamWriter(filePath, false, outputEnc);
          await file.WriteLineAsync(JsonConvert.SerializeObject(savePlayers, Formatting.None)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
          string error = $"Unable to save the {nameof(SplatTagJsonSnapshotDatabase)} players because of an exception: {ex}";
          Console.Error.WriteLine(error);
          Console.WriteLine(error);
        }
      });

      Task saveTeamsTask = Task.Run(async () =>
      {
        try
        {
          // Write teams
          string filePath = Path.Combine(saveDirectory, "Snapshot-Teams-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".json");
          Encoding outputEnc = new UTF8Encoding(false); // UTF-8 no BOM

          using TextWriter file = new StreamWriter(filePath, false, outputEnc);
          await file.WriteLineAsync(JsonConvert.SerializeObject(saveTeams, Formatting.None)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
          string error = $"Unable to save the {nameof(SplatTagJsonSnapshotDatabase)} teams because of an exception: {ex}";
          Console.Error.WriteLine(error);
          Console.WriteLine(error);
        }
      });

      Task saveSourcesTask = Task.Run(async () =>
      {
        try
        {
          // Write sources
          string filePath = Path.Combine(saveDirectory, "Snapshot-Sources-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".json");
          Encoding outputEnc = new UTF8Encoding(false); // UTF-8 no BOM

          using TextWriter file = new StreamWriter(filePath, false, outputEnc);
          await file.WriteLineAsync(JsonConvert.SerializeObject(saveSources, Formatting.None)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
          string error = $"Unable to save the {nameof(SplatTagJsonSnapshotDatabase)} sources because of an exception: {ex}";
          Console.Error.WriteLine(error);
          Console.WriteLine(error);
        }
      });

      Task.WaitAll(savePlayersTask, saveTeamsTask, saveSourcesTask);
      return this;
    }
  }
}