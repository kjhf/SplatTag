using Newtonsoft.Json;
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

namespace SplatTagConsole
{
  public static class ConsoleMain
  {
    private static readonly SplatTagController splatTagController;
    private static readonly GenericFilesImporter? importer;

    static ConsoleMain()
    {
      // Set Console to UTF-8
      Console.OutputEncoding = Encoding.UTF8;
      (splatTagController, importer) = SplatTagControllerFactory.CreateController();
    }

    public static int Main(string[]? args)
    {
      if (args?.Length > 0)
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
            CommandLineResult result = new CommandLineResult
            {
              Message = "OK"
            };

            if (rebuild)
            {
              SplatTagControllerFactory.GenerateNewDatabase();
              result.Message = "Database rebuilt!";
              result.Players = new Player[0];
              result.Teams = new Team[0];
              result.AdditionalTeams = new Dictionary<Guid, Team>();
              result.PlayersForTeams = new Dictionary<Guid, (Player, bool)[]>();
            }
            else if (string.IsNullOrWhiteSpace(query))
            {
              result.Message = "Nothing to search!";
              result.Players = new Player[0];
              result.Teams = new Team[0];
              result.AdditionalTeams = new Dictionary<Guid, Team>();
              result.PlayersForTeams = new Dictionary<Guid, (Player, bool)[]>();
            }
            else
            {
              result.Players =
                splatTagController.MatchPlayer(query,
                  new MatchOptions
                  {
                    IgnoreCase = !exactCase,
                    NearCharacterRecognition = !exactCharacterRecognition,
                    QueryIsRegex = queryIsRegex
                  }
                );

              result.Teams =
                splatTagController.MatchTeam(query,
                  new MatchOptions
                  {
                    IgnoreCase = !exactCase,
                    NearCharacterRecognition = !exactCharacterRecognition,
                    QueryIsRegex = queryIsRegex
                  }
                );

              result.AdditionalTeams =
                result.Players.SelectMany(p => p.Teams.Select(id => splatTagController.GetTeamById(id)))
                .Distinct()
                .ToDictionary(t => t.Id, t => t);

              result.PlayersForTeams =
                result.Teams.ToDictionary(t => t.Id, t => splatTagController.GetPlayersForTeam(t));

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
            }

            StringWriter sw = new StringWriter();
            serializer.Serialize(sw, result);
            Console.WriteLine(sw.ToString());
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
            string? line = Console.ReadLine();
            if (line == null)
            {
              // Exited.
              break;
            }
            else
            {
              parsed = rootCommand.Parse(line);
            }
          }
        }
        while (keepOpen);
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
      sb.AppendLine("M: Manual entry");
      sb.AppendLine("P: Match a player");
      sb.AppendLine("S: Save/overwrite local database");
      sb.AppendLine("T: Match a team");

      return sb.ToString();
    }

    private static void DoCommand(char c)
    {
      switch (char.ToLowerInvariant(c))
      {
        case 'f': BeginFetch(); break;
        case 'l': splatTagController.LoadDatabase(); break;
        case 's': splatTagController.SaveDatabase(); break;

        case 'm':
          {
            Console.WriteLine("Team or player? [t/p]");
            char tp = char.ToLowerInvariant(Console.ReadKey().KeyChar);
            Console.WriteLine();
            if (tp == 't')
            {
              // Manual entry for Team
              Team t = splatTagController.CreateTeam();

              Console.WriteLine("Name of team?");
              t.Name = Console.ReadLine() ?? "";

              Console.WriteLine("Clan tag?");
              t.ClanTags = new string[1] { Console.ReadLine() ?? "" };

              Console.WriteLine("Where does the clan tag go?");
              foreach (TagOption option in Enum.GetValues(typeof(TagOption)))
              {
                Console.WriteLine($"{(int)option}. {option}");
              }
              if (Enum.TryParse(Console.ReadLine(), out TagOption temp))
              {
                t.ClanTagOption = temp;
              }

              Console.WriteLine("Div?");
              t.Div = new Division(Console.ReadLine() ?? "");

              splatTagController.SaveDatabase();
            }
            else if (tp == 'p')
            {
              // Manual entry for Player
              Player p = splatTagController.CreatePlayer();

              Console.WriteLine("Name of player?");
              p.Names = new string[1] { Console.ReadLine() ?? "" };

              Console.WriteLine("Player's current team? (Or leave blank)");
              string? input = Console.ReadLine();
              if (!string.IsNullOrWhiteSpace(input))
              {
                Team[] matchedTeams = splatTagController.MatchTeam(input);
                switch (matchedTeams.Length)
                {
                  case 0:
                    {
                      Console.WriteLine("Team not found with that name or tag. Create the team first.");
                      break;
                    }

                  case 1:
                    {
                      p.Teams = new Guid[1] { matchedTeams[0].Id };
                      Console.WriteLine("Successfully matched.");
                      break;
                    }

                  default:
                    {
                      Console.WriteLine($"More than one team matched. Assuming the first one ({matchedTeams[0].Name}).");
                      p.Teams = new Guid[1] { matchedTeams[0].Id };
                      break;
                    }
                }
              }
              splatTagController.SaveDatabase();
            }
            // else do nothing

            break;
          }

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