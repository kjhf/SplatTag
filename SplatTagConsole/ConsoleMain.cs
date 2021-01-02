using Newtonsoft.Json;
using SplatTagCore;
using SplatTagDatabase;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace SplatTagConsole
{
  public static class ConsoleMain
  {
    private static readonly SplatTagController splatTagController;
    private static readonly GenericFilesToIImporters? importer;

    static ConsoleMain()
    {
      // Set Console to UTF-8
      Console.OutputEncoding = Encoding.UTF8;
      (splatTagController, importer) = SplatTagControllerFactory.CreateController();
    }

    public static int Main(string[]? args = null)
    {
      if (args == null)
      {
        args = Array.Empty<string>();
      }
      Console.WriteLine($"Slapp called with: [{string.Join(", ", args)}]");

      if (args.Length > 0)
      {
        // Invoked from command line
        if (JsonConvert.DefaultSettings == null)
        {
          JsonConvert.DefaultSettings = () => new JsonSerializerSettings();
        }
        var settings = JsonConvert.DefaultSettings();
        settings.DefaultValueHandling = DefaultValueHandling.Ignore;
        var serializer = JsonSerializer.Create(settings);

        var exactCaseOption = new Option<bool>("--exactCase", () => false, "Exact Case?");
        var exactCharacterRecognitionOption = new Option<bool>("--exactCharacterRecognition", () => false, "Exact Character Recognition?");
        var queryIsRegexOption = new Option<bool>("--queryIsRegex", () => false, "Exact Character Recognition?");
        var keepOpenOption = new Option<bool>("--keepOpen", () => false, "Keep the console open?");
        var rebuildDatabaseOption = new Option<bool>("--rebuild", () => false, "Rebuilds the database");
        var queryArgument = new Argument<string>("query", "The team, tag, or player query");
        var rootCommand = new RootCommand
        {
          queryArgument,
          exactCaseOption,
          exactCharacterRecognitionOption,
          queryIsRegexOption,
          rebuildDatabaseOption,
          keepOpenOption,
        };

        rootCommand.Handler = CommandHandler.Create(
          // Note that the parameter names must match the --option name
          (string query, bool exactCase, bool exactCharacterRecognition, bool queryIsRegex, bool rebuild, bool _) =>
          {
            try
            {
              var options = new MatchOptions
              {
                IgnoreCase = !exactCase,
                NearCharacterRecognition = !exactCharacterRecognition,
                QueryIsRegex = queryIsRegex
              };

              CommandLineResult result = new CommandLineResult
              {
                Message = "OK",
                Query = query,
                Options = options
              };

              if (rebuild)
              {
                SplatTagControllerFactory.GenerateNewDatabase();
                result.Message = "Database rebuilt!";
              }
              else if (string.IsNullOrWhiteSpace(query))
              {
                result.Message = "Nothing to search!";
              }
              else
              {
                try
                {
                  Console.WriteLine("Building result...");
                  result.Players = splatTagController.MatchPlayer(query, options);
                  result.Teams = splatTagController.MatchTeam(query, options);

                  result.AdditionalTeams =
                    result.Players
                    .SelectMany(p => p.Teams.Select(id => splatTagController.GetTeamById(id)))
                    .Distinct()
                    .ToDictionary(t => t.Id, t => t);
                  result.AdditionalTeams[Team.NoTeam.Id] = Team.NoTeam;
                  result.AdditionalTeams[Team.UnlinkedTeam.Id] = Team.UnlinkedTeam;

                  result.PlayersForTeams =
                    result.Teams
                    .ToDictionary(t => t.Id, t => splatTagController.GetPlayersForTeam(t));

                  foreach (var pair in result.PlayersForTeams)
                  {
                    foreach ((Player, bool) tuple in pair.Value)
                    {
                      foreach (Guid t in tuple.Item1.Teams)
                      {
                        result.AdditionalTeams.TryAdd(t, splatTagController.GetTeamById(t));
                      }
                    }
                  }

                  result.Sources =
                    result.Players.SelectMany(p => p.Sources)
                    .Concat(result.Teams.SelectMany(t => t.Sources))
                    //.Concat(result.AdditionalTeams.Values.AsParallel().SelectMany(t => t.Sources))
                    //.Concat(result.PlayersForTeams.Values.AsParallel().SelectMany(tupleArray => tupleArray.SelectMany(p => p.Item1.Sources)))
                    .Distinct()
                    .ToDictionary(s => s.Id, s => s.Name);
                }
                catch (Exception ex)
                {
                  Console.WriteLine("ERROR: Exception while compiling data for serialization...");
                  Console.WriteLine(ex.ToString());
                }
              }

              try
              {
                StringWriter sw = new StringWriter();
                serializer.Serialize(sw, result);
                Console.WriteLine(sw.ToString());
              }
              catch (Exception ex)
              {
                Console.WriteLine("ERROR: Exception while Serializing result...");
                Console.WriteLine(ex.ToString());
              }
            }
            catch (Exception ex)
            {
              Console.WriteLine("ERROR: Outer Exception handler...");
              Console.WriteLine(ex.ToString());
            }
          }
        );

        int mainResult = 0;
        var parsed = rootCommand.Parse(args);
        bool keepOpen = parsed.Tokens.Any(t => t.Value.Contains("keepOpen"));

        do
        {
          mainResult = parsed.Invoke();

          if (keepOpen)
          {
            try
            {
              string? line = Console.ReadLine();
              if (!string.IsNullOrWhiteSpace(line))
              {
                parsed = rootCommand.Parse(line);
                continue;
              }
              else
              {
                Console.WriteLine("Warning: line is null or spaces only.");
              }

              Console.WriteLine("Looping until input can be seeked.");
              SpinWait.SpinUntil(() => Console.OpenStandardInput().CanSeek);
            }
            catch (ObjectDisposedException odex)
            {
              Console.WriteLine($"Sleeping (input was disposed - {odex.Message}).");
            }
            catch (Exception ex)
            {
              Console.WriteLine($"Exception in keepOpen: {ex.Message}");
            }
            // Loop
          }
        }
        while (keepOpen);

        Console.WriteLine($"Returning from args: [{string.Join(", ", args)}]");
        return mainResult;
      }
      else
      {
        for (; ; )
        {
          Console.WriteLine();
          Console.WriteLine("Choose a function:");
          Console.WriteLine(GetCommandsString());
          char c = Console.ReadKey().KeyChar;
          Console.WriteLine();
          DoCommand(c);
        }
      }
    }

    private static string GetCommandsString()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("F: Fetch data from site or file");
      sb.AppendLine("L: (Re)load local database");
      sb.AppendLine("P: Match a player");
      sb.AppendLine("T: Match a team");

      return sb.ToString();
    }

    private static void DoCommand(char c)
    {
      switch (char.ToLowerInvariant(c))
      {
        case 'f': BeginFetch(); break;
        case 'l': splatTagController.LoadDatabase(); break;
        case 'p':
        {
          Console.WriteLine("Player name?");
          string input = Console.ReadLine() ?? "";
          foreach (var p in splatTagController.MatchPlayer(input))
          {
            Console.WriteLine(p);
          }
          break;
        }

        case 't':
        {
          Console.WriteLine();
          Console.WriteLine("Team name?");
          string input = Console.ReadLine() ?? "";
          foreach (var t in splatTagController.MatchTeam(input))
          {
            Console.WriteLine(t);
            Console.WriteLine("Players: " + string.Join(", ", splatTagController.GetPlayersForTeam(t).Select(tuple => tuple.Item1.Name + " " + (tuple.Item2 ? "(Most recent)" : "(Ex)"))));
            Console.WriteLine("-----");
          }
          break;
        }
      }
    }

    private static void BeginFetch()
    {
      if (importer == null)
      {
        Console.WriteLine("No can do. Working in Snapshot mode.");
      }
      else
      {
        Console.WriteLine("File or site to import?");
        string input = Console.ReadLine() ?? "";
        if (!string.IsNullOrEmpty(input))
        {
          importer.SetSingleSource(input);
          splatTagController.LoadDatabase();
        }
      }
    }
  }
}