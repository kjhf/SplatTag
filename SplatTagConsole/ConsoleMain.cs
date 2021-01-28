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
using System.Threading.Tasks;

namespace SplatTagConsole
{
  public static class ConsoleMain
  {
    private static readonly SplatTagController splatTagController;
    private static readonly GenericFilesToIImporters? importer;
    private static readonly JsonSerializer serializer;

    static ConsoleMain()
    {
      // Set Console to UTF-8
      Console.OutputEncoding = Encoding.UTF8;

      // Invoked from command line
      if (JsonConvert.DefaultSettings == null)
      {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
          DefaultValueHandling = DefaultValueHandling.Ignore,
          Error = (sender, args) =>
          {
            Console.Error.WriteLine(args.ErrorContext.Error.Message);
            args.ErrorContext.Handled = true;
          }
        };
      }
      serializer = JsonSerializer.Create(JsonConvert.DefaultSettings());

      (splatTagController, importer) = SplatTagControllerFactory.CreateController();
    }

    public static async Task Main(string[]? args)
    {
      if (args == null)
      {
        args = Array.Empty<string>();
      }
      // Console.WriteLine($"Slapp called with: [{string.Join(", ", args)}]");

      if (args.Length > 0)
      {
        var rootCommand = new RootCommand();
        var command = new Command("Slapp", "Slapp Query")
        {
          new Option<string>("--b64", "The team, tag, or player query as a base 64 query"),
          new Option<string>("--query", "The team, tag, or player query"),
          new Option<bool>("--exactCase", () => false, "Exact Case?"),
          new Option<bool>("--exactCharacterRecognition", () => false, "Exact Character Recognition?"),
          new Option<bool>("--queryIsRegex", () => false, "Exact Character Recognition?"),
          new Option<bool>("--rebuild", () => false, "Rebuilds the database"),
          new Option<bool>("--keepOpen", () => false, "Keep the console open?"),
        };
        rootCommand.Add(command);

        // Note that the parameter names must match the --option name
        command.Handler = CommandHandler.Create((CommandLineIn obj) =>
        {
          // Console.WriteLine("HandleCommandLineQuery: Handler invoked.");
          HandleCommandLineQuery(
            obj.B64,
            obj.Query,
            obj.ExactCase,
            obj.ExactCharacterRecognition,
            obj.QueryIsRegex,
            obj.Rebuild,
            obj.KeepOpen);
          return 0;
        });

        // Console.WriteLine("Main: parsing argument...");
        var parseResult = command.Parse(args);
        bool keepOpen = parseResult.Tokens.Any(t => t.Value.Contains("keepOpen"));
        // Console.WriteLine($"Main: arguments parsed, keepOpen={keepOpen}, parseResult={parseResult}...");

        // Console.WriteLine($"Main: Invoking...");
        parseResult.Invoke();
        // Console.WriteLine($"Main: Out of Invoke...");

        while (keepOpen)
        {
          try
          {
            // Console.WriteLine($"Main (keepOpen): Waiting on stdin data...");
            string? line = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(line))
            {
              // If the following queries after keepOpen omit the --query, just add it back in.
              // If the query contains -- then... gloves are off. Use --b64.
              if (!line.Contains("--"))
              {
                line = "--query " + line;
              }

              // Console.WriteLine($"Main (keepOpen): Invoking parse with the line:");
              // Console.WriteLine(line);
              command.Parse(line).Invoke();
            }

            // Allow stdin to flush the data
            await Task.Delay(10).ConfigureAwait(false);
          }
          catch (ObjectDisposedException odex)
          {
            Console.WriteLine($"Sleeping (input was disposed - {odex.Message}).");
            await Task.Delay(100).ConfigureAwait(false);
          }
          catch (Exception ex)
          {
            Console.WriteLine($"Exception in keepOpen: {ex.Message}");
          }
          // Loop
        }

        // Console.WriteLine($"Returning from args: [{string.Join(", ", args)}]");
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

    private static void HandleCommandLineQuery(string? b64, string? query, bool exactCase, bool exactCharacterRecognition, bool queryIsRegex, bool rebuild, bool _)
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
          Query = query ?? string.Empty,
          Options = options
        };

        if (rebuild)
        {
          SplatTagControllerFactory.GenerateNewDatabase();
          result.Message = "Database rebuilt!";
        }
        else if (string.IsNullOrWhiteSpace(query) && string.IsNullOrEmpty(b64))
        {
          result.Message = "Nothing to search!";
        }
        else
        {
          try
          {
            if (!string.IsNullOrEmpty(query) && !string.IsNullOrEmpty(b64))
            {
              result.Message = "Warning: Both b64 and query specified. Using b64.";
            }
            if (!string.IsNullOrEmpty(b64))
            {
              query = Encoding.UTF8.GetString(Convert.FromBase64String(b64));
            }

            Console.WriteLine($"Building result for query={query}");
            result.Players = splatTagController.MatchPlayer(query, options);
            result.Teams = splatTagController.MatchTeam(query, options);

            if (result.Players.Length > 0 || result.Teams.Length > 0)
            {
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

              result.PlacementsForPlayers =
                result.Players
                .ToDictionary(p => p.Id, p => p.Sources
                .ToDictionary(s => s.Id, s => s.Brackets));
            }
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

          // Send back as a b64 string
          Console.WriteLine(Convert.ToBase64String(Encoding.UTF8.GetBytes(sw.ToString())));
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