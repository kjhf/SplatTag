using SplatTagCore;
using SplatTagDatabase;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace SplatTagConsole
{
  internal static class ProgramMain
  {
    private static readonly SplatTagController splatTagController;
    private static readonly GenericFilesImporter importer;
    private static readonly ISplatTagDatabase database;
    private static readonly string splatTagFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SplatTag");

    static ProgramMain()
    {
      importer = new GenericFilesImporter(splatTagFolder);
      database = new MultiDatabase(splatTagFolder, importer);
      splatTagController = new SplatTagController(database);
    }

    private static void Main(string[] args)
    {
      splatTagController.Initialise(args);

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
            t.Name = Console.ReadLine();

            Console.WriteLine("Clan tag?");
            t.ClanTags = new string[1] { Console.ReadLine() };

            Console.WriteLine("Where does the clan tag go?");
            foreach (TagOption option in Enum.GetValues(typeof(TagOption)))
            {
              Console.WriteLine($"{(int)option}. {option.ToString()}");
            }
            Enum.TryParse(Console.ReadLine(), out TagOption temp);
            t.ClanTagOption = temp;

            Console.WriteLine("Div?");
            t.Div = new Division(Console.ReadLine());

            splatTagController.SaveDatabase();
          }
          else if (tp == 'p')
          {
            // Manual entry for Player
            Player p = splatTagController.CreatePlayer();

            Console.WriteLine("Name of player?");
            p.Names = new string[1] { Console.ReadLine() };

            Console.WriteLine("Player's current team? (Or leave blank)");
            string input = Console.ReadLine();
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
                  p.Teams = new Team[1] { matchedTeams[0] };
                  Console.WriteLine("Successfully matched.");
                  break;
                }

                default:
                {
                  Console.WriteLine($"More than one team matched. Assuming the first one ({matchedTeams[0].Name}).");
                  p.Teams = new Team[1] { matchedTeams[0] };
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
          string input = Console.ReadLine();
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
          string input = Console.ReadLine();
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
      Console.WriteLine("File or site to import?");
      string input = Console.ReadLine();
      importer.SetSingleSource(input);
      splatTagController.LoadDatabase();
    }
  }
}