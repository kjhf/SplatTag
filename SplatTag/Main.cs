using SplatTagCore;
using SplatTagDatabase;
using System;
using System.IO;
using System.Text;

namespace SplatTag
{
  internal static class ProgramMain
  {
    private static readonly SplatTagController splatTagController;
    private static readonly SplatTagJsonDatabase splatTagDatabase;
    private static readonly string jsonFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SplatTag");

    static ProgramMain()
    {
      splatTagDatabase = new SplatTagJsonDatabase(jsonFile);
      splatTagController = new SplatTagController(splatTagDatabase);
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
          var players = splatTagController.MatchPlayer(input);
          foreach (var p in players)
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
          var teams = splatTagController.MatchTeam(input);
          foreach (var t in teams)
          {
            Console.WriteLine(t);
          }
          break;
        }
      }
    }

    private static void BeginFetch()
    {
      Console.WriteLine("Not currently implemented.");

      // TODO -- ask for local file or website.
      // A local file should be a readable format, in priority order: json, html, database (misp), xls, xml
      // A site should simply download the file or an html contents rep, and load the contents as if it were a local file.
      // We should be mindful about loading files that come from the internet though: always validate first.
    }
  }
}