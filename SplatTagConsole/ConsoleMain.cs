using Newtonsoft.Json;
using NLog;
using SplatTagCore;
using SplatTagDatabase;
using System;
using System.Collections.Generic;
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
    private static readonly JsonSerializer serializer;
    private static readonly HashSet<string> errorMessagesReported;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static ConsoleMain()
    {
      // Set Console to UTF-8
      Console.OutputEncoding = Encoding.UTF8;
      errorMessagesReported = new HashSet<string>();

      // Invoked from command line
      JsonConvert.DefaultSettings ??= SplatTagJsonSnapshotDatabase.JsonConvertDefaultSettings;
      serializer = JsonSerializer.Create(JsonConvert.DefaultSettings());

      splatTagController = SplatTagControllerFactory.CreateControllerNoLoad();
    }

    public static async Task Main(string[] args)
    {
      AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
      {
        if (e.ExceptionObject is Exception exception)
        {
          logger.Error(exception, "Unhandled exception: " + exception);
          PathUtils.DumpLogSafely("crash", () => new[] { exception.ToString() });
        }
        else
        {
          logger.Error("Unhandled exception: " + e.ExceptionObject);
          PathUtils.DumpLogSafely("crash", () => new[] { e.ExceptionObject?.ToString() ?? "null" });
        }
      };

      if (!args.Contains("--rebuild") && !args.Contains("--patch"))
      {
        SplatTagControllerFactory.EnsureInitialised(splatTagController);
      }
      // Otherwise we'll handle the rebuild shortly

      if (args.Length > 0)
      {
        var rootCommand = new RootCommand();
        var command = new Command("Slapp", "Slapp Query");
        Array.ForEach(GetOptions(), o => command.AddGlobalOption(o));
        rootCommand.Add(command);

        // Note that the parameter names must match the --option name
        command.Handler = CommandHandler.Create((CommandLineIn obj) =>
        {
          logger.Trace("HandleCommandLineQuery: Handler invoked.");
          HandleCommandLineQuery(
            obj.B64,
            obj.Query,
            obj.SlappId,
            obj.ExactCase,
            obj.ExactCharacterRecognition,
            obj.Rebuild,
            obj.Patch,
            obj.KeepOpen,
            obj.Limit,
            obj.Verbose,
            obj.QueryIsRegex,
            obj.QueryIsClanTag,
            obj.QueryIsTeam,
            obj.QueryIsPlayer);
          return 0;
        });

        logger.Trace("Main: parsing argument...");
        var parseResult = command.Parse(args);
        bool keepOpen = parseResult.Tokens.Any(t => t.Value.Contains("keepOpen"));
        logger.Trace($"Main: arguments parsed, keepOpen={keepOpen}, parseResult={parseResult}...");

        logger.Trace("Main: Invoking...");
        parseResult.Invoke();
        logger.Trace("Main: Out of Invoke...");

        while (keepOpen)
        {
          try
          {
            logger.Trace("Main (keepOpen): Waiting on stdin data...");
            string? line = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(line))
            {
              // If the following queries after keepOpen omit the --query, just add it back in.
              // If the query contains -- then... gloves are off. Use --b64.
              if (!line.Contains("--"))
              {
                line = "--query " + line;
              }

              logger.Trace("Main (keepOpen): Invoking parse with the line:");
              logger.Trace(line);
              command.Parse(line).Invoke();
            }

            // Allow stdin to flush the data
            await Task.Delay(10).ConfigureAwait(false);
          }
          catch (ObjectDisposedException odex)
          {
            logger.Info($"Sleeping (input was disposed - {odex.Message}).");
            await Task.Delay(500).ConfigureAwait(false);
          }
          catch (Exception ex)
          {
            logger.Error(ex, $"Exception in keepOpen: {ex.Message}");
          }
          // Loop
        }

        logger.Debug($"Returning from args: [{string.Join(", ", args)}]");
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

    private static void HandleCommandLineQuery(
      string? b64,
      string? query,
      string? slappId,
      bool exactCase,
      bool exactCharacterRecognition,
      string? rebuild,
      string? patch,
      bool keepOpen,
      int limit,
      bool verbose,
      bool queryIsRegex,
      bool queryIsClanTag,
      bool queryIsTeam,
      bool queryIsPlayer)
    {
      try
      {
        if (verbose)
        {
          SplatTagControllerFactory.SetNLogLevel();
        }
        else
        {
          SplatTagControllerFactory.SetNLogLevel(LogLevel.Info);
        }

        var options = new MatchOptions
        {
          IgnoreCase = !exactCase,
          NearCharacterRecognition = !exactCharacterRecognition,
          QueryIsRegex = queryIsRegex,
          Limit = limit
        };

        if (queryIsPlayer)
        {
          options.FilterOptions = FilterOptions.Player;
        }
        else if (queryIsTeam)
        {
          options.FilterOptions = FilterOptions.Team;
        }
        else if (queryIsClanTag)
        {
          options.FilterOptions = FilterOptions.ClanTag;
        }
        else
        {
          options.FilterOptions = FilterOptions.Default;
        }

        var result = new CommandLineResult
        {
          Message = "OK",
          Options = options
        };

        string?[] inputs = new string?[] { b64, query, slappId };

        if (rebuild != null)
        {
          if (rebuild.Equals(string.Empty))
          {
            SplatTagControllerFactory.GenerateNewDatabase();
            result.Message = "Database rebuilt from default sources!";
          }
          else if (File.Exists(rebuild))
          {
            string? saveFolder = Directory.GetParent(rebuild)?.FullName;
            string sourcesFile = Path.GetFileName(rebuild);
            SplatTagControllerFactory.GenerateNewDatabase(saveFolder: saveFolder, sourcesFile: sourcesFile);
            result.Message = $"Database rebuilt from {rebuild}!";
          }
          else
          {
            result.Message = $"Rebuild specified but the sources file specified `{rebuild}` from `{Directory.GetCurrentDirectory()}` does not exist. Aborting.";
          }
        }
        else if (patch != null)
        {
          if (patch.Equals(string.Empty))
          {
            result.Message = "Patch specified but no patch sources file specified. Aborting.";
          }
          else if (patch == "merge")
          {
            // Merge only
            var dir = SplatTagControllerFactory.GetDefaultPath();
            SplatTagControllerFactory.MergeJSONDatabase(dir);
          }
          else if (File.Exists(patch))
          {
            SplatTagControllerFactory.GeneratePatchedDatabaseFromFile(patchFile: patch);
            result.Message = $"Database patched from {patch}!";
          }
          else
          {
            result.Message = $"Patch specified but the sources file specified `{patch}` from `{Directory.GetCurrentDirectory()}` does not exist. Aborting.";
          }
        }
        else if (inputs.All(s => string.IsNullOrEmpty(s)))
        {
          if (keepOpen)
          {
            // First run to open
            result.Message = $"Connection established. {splatTagController.MatchPlayer(null).Length} players and {splatTagController.MatchTeam(null).Length} teams loaded.";
          }
          else
          {
            result.Message = "Nothing to search!";
          }
        }
        else
        {
          try
          {
            if (new[] { b64, query, slappId }.Count(s => !string.IsNullOrEmpty(s)) > 1)
            {
              result.Message = "Warning: Multiple inputs defined. YMMV.";
            }

            if (!string.IsNullOrEmpty(b64))
            {
              result.Query = query = Encoding.UTF8.GetString(Convert.FromBase64String(b64));
            }

            if (!string.IsNullOrEmpty(slappId))
            {
              result.Query = slappId;
              options.FilterOptions = FilterOptions.SlappId;
              result.Players = splatTagController.MatchPlayer(slappId, options);
              result.Teams = splatTagController.MatchTeam(slappId, options);
            }
            else
            {
              Console.WriteLine($"Building result for query={query}");
              result.Query = query ?? string.Empty;
              result.Players = splatTagController.MatchPlayer(query, options);
              result.Teams = splatTagController.MatchTeam(query, options);
            }

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
                .ToDictionary(t => t.Id, t => splatTagController.GetPlayersForTeam(t).ToArray());

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

              var sources = new HashSet<string>();
              foreach (var s in result.Players.SelectMany(p => p.Sources))
              {
                sources.Add(s.Name);
              }
              foreach (var s in result.Teams.SelectMany(t => t.Sources))
              {
                sources.Add(s.Name);
              }
              foreach (var s in result.AdditionalTeams.Values.SelectMany(t => t.Sources))
              {
                sources.Add(s.Name);
              }
              foreach (var s in result.PlayersForTeams.Values.SelectMany(tupleArray => tupleArray.SelectMany(p => p.Item1.Sources)))
              {
                sources.Add(s.Name);
              }
              sources.Add(Builtins.BuiltinSource.Name);
              sources.Add(Builtins.ManualSource.Name);
              result.Sources = sources.ToArray();

              try
              {
                result.PlacementsForPlayers = new Dictionary<Guid, Dictionary<string, Bracket[]>>();
                foreach (var player in result.Players)
                {
                  result.PlacementsForPlayers[player.Id] = new Dictionary<string, Bracket[]>();
                  foreach (var source in player.Sources)
                  {
                    result.PlacementsForPlayers[player.Id][source.Name] = source.Brackets;
                  }
                }
              }
              catch (OutOfMemoryException oom)
              {
                const string message = "ERROR: OutOfMemoryException on PlacementsForPlayers. Will continue anyway.";
                logger.Warn(message, oom);
                result.PlacementsForPlayers = new Dictionary<Guid, Dictionary<string, Bracket[]>>();
              }
            }
          }
          catch (Exception ex)
          {
            string message = $"ERROR: {ex.GetType().Name} while compiling data for serialization...";
            logger.Error(ex, message);

            string q = result.Query;
            result = new CommandLineResult
            {
              Query = q,
              Options = options,
              Message = message,
            };
          }
        }

        string messageToSend;

        try
        {
          StringWriter sw = new StringWriter();
          serializer.Serialize(sw, result);
          messageToSend = sw.ToString();
        }
        catch (Exception ex)
        {
          string message = $"ERROR: {ex.GetType().Name} while serializing result...";
          logger.Error(ex, message);
          logger.Error(ex, ex.ToString());

          string q = result.Query;
          result = new CommandLineResult
          {
            Query = q,
            Options = options,
            Message = message
          };

          // Attempt to send the message as a different serialized error
          StringWriter sw = new StringWriter();
          serializer.Serialize(sw, result);
          messageToSend = sw.ToString();
        }

        // Send back as a b64 string
        Console.WriteLine(Convert.ToBase64String(Encoding.UTF8.GetBytes(messageToSend)));
      }
      catch (Exception ex)
      {
        logger.Error(ex, $"ERROR: Outer Exception handler, caught a {ex.GetType().Name}...");
        logger.Error(ex, ex.ToString());
      }
      finally
      {
        errorMessagesReported.Clear();
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
          foreach (Player p in splatTagController.MatchPlayer(input))
          {
            Console.WriteLine($"{(p.Country != null ? ":flag_" + p.Country + ": " : "")}{(p.Top500 ? "👑 " : "")}{p.Name}, Plays for: {splatTagController.GetTeamById(p.CurrentTeam)}");
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
      Console.WriteLine("TBD!");
    }

    private static Option[] GetOptions()
    {
      List<Option> options = new List<Option>();
      foreach (var (optionType, flagName, description, getDefaultValue) in ConsoleOptions.GetOptionsAsTuple())
      {
        var arg = new Argument { ArgumentType = optionType };
        if (getDefaultValue != null)
        {
          arg.SetDefaultValueFactory(getDefaultValue);
        }
        var option = new Option(flagName, description) { Argument = arg };
        options.Add(option);
      }
      return options.ToArray();
    }
  }
}