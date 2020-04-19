using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("SplatTagUnitTests")]

namespace SplatTagCore
{
  public class SplatTagController
  {
    private readonly ISplatTagDatabase database;
    private SortedDictionary<uint, Player> players;
    private SortedDictionary<uint, Team> teams;

    public SplatTagController(ISplatTagDatabase database)
    {
      this.database = database;
      this.players = new SortedDictionary<uint, Player>();
      this.teams = new SortedDictionary<uint, Team>();
    }

    public void Initialise(string[] commandArgs)
    {
      // TODO parse command line arguments.

      LoadDatabase();
    }

    public void LoadDatabase()
    {
      var result = database.Load();
      players = new SortedDictionary<uint, Player>(result.Item1.ToDictionary(x => x.Id, x => x));
      teams = new SortedDictionary<uint, Team>(result.Item2.ToDictionary(x => x.Id, x => x));
    }

    public void SaveDatabase()
    {
      database.Save(players.Values, teams.Values);
    }

    /// <summary>
    /// Get all players associated with a team.
    /// </summary>
    public Player[] GetAllPlayersForTeam(Team t)
    {
      return players.Values.Where(p => p.Teams.Contains(t)).ToArray();
    }

    /// <summary>
    /// Get current players on a given team.
    /// </summary>
    public Player[] GetCurrentPlayersForTeam(Team t)
    {
      return players.Values.Where(p => p.CurrentTeam.Equals(t)).ToArray();
    }

    /// <summary>
    /// Match a query to players with default options.
    /// </summary>
    public Player[] MatchPlayer(string query)
    {
      return MatchPlayer(query, new MatchOptions());
    }

    /// <summary>
    /// Match a query to players with given options.
    /// </summary>
    public Player[] MatchPlayer(string query, MatchOptions matchOptions)
    {
      List<Player> retVal = new List<Player>();
      Func<Player, bool> func;
      if (matchOptions.QueryIsRegex)
      {
        try
        {
          Regex regex;
          if (matchOptions.IgnoreCase)
          {
            regex = new Regex(query, RegexOptions.IgnoreCase);
          }
          else
          {
            regex = new Regex(query);
          }

          func = (p) =>
          {
            string[] names = matchOptions.NearCharacterRecognition ? ASCIIFold.TransformEnumerable(p.Names) : p.Names;
            return names.Any(n => regex.IsMatch(n));
          };
        }
        catch (ArgumentException)
        {
          // Regex parsing error
          return new Player[0];
        }
      }
      else
      {
        // Standard query
        StringComparison comparion = matchOptions.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        func = (p) =>
        {
          string[] names = matchOptions.NearCharacterRecognition ? ASCIIFold.TransformEnumerable(p.Names) : p.Names;
          return names.Contains(query, comparion);
        };
      }

      retVal.AddRange(players.Values.Where(p => func(p)));
      return retVal.ToArray();
    }

    /// <summary>
    /// Match a query to teams with default options.
    /// </summary>
    public Team[] MatchTeam(string query)
    {
      return MatchTeam(query, new MatchOptions());
    }

    /// <summary>
    /// Match a query to teams with given options.
    /// </summary>
    public Team[] MatchTeam(string query, MatchOptions matchOptions)
    {
      List<Team> retVal = new List<Team>();

      // First, derive the function for clan tags.
      Func<Team, bool> func;
      if (matchOptions.QueryIsRegex)
      {
        try
        {
          Regex regex = matchOptions.IgnoreCase ? new Regex(query, RegexOptions.IgnoreCase) : new Regex(query);
          func = (t) => (matchOptions.NearCharacterRecognition ? ASCIIFold.TransformEnumerable(t.ClanTags) : t.ClanTags).Any(n => regex.IsMatch(n));
        }
        catch (ArgumentException)
        {
          // Regex parsing error
          return new Team[0];
        }
      }
      else
      {
        // Standard query
        StringComparison comparion = matchOptions.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        func = (t) => (matchOptions.NearCharacterRecognition ? ASCIIFold.TransformEnumerable(t.ClanTags) : t.ClanTags).Contains(query, comparion);
      }

      // Add matching clan tags to the result.
      retVal.AddRange(teams.Values.Where(t => func(t)));

      // Now the names.
      if (matchOptions.QueryIsRegex)
      {
        try
        {
          Regex regex = matchOptions.IgnoreCase ? new Regex(query, RegexOptions.IgnoreCase) : new Regex(query);
          func = (t) => regex.IsMatch(matchOptions.NearCharacterRecognition ? t.Name.TransformString() : t.Name);
        }
        catch (ArgumentException)
        {
          // Regex parsing error
          return new Team[0];
        }
      }
      else
      {
        // Standard query
        StringComparison comparion = matchOptions.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        func = (t) => (matchOptions.NearCharacterRecognition ? t.Name.TransformString() : t.Name).Contains(query, comparion);
      }

      // Add matching names to the result.
      retVal.AddRange(teams.Values.Where(t => func(t)));

      // Filter unique
      retVal = retVal.Distinct().ToList();
      return retVal.ToArray();
    }

    public string TryImport(string input)
    {
      // Remove preceding and seceding quotes from path.
      input = input.TrimStart('"').TrimEnd('"');

      // TODO --
      // A local file should be a readable format, in priority order: json, html, database (misp), xls, xml
      // A site should simply download the file or an html contents rep, and load the contents as if it were a local file.
      // We should be mindful about loading files that come from the internet though: always validate first.
      if (!File.Exists(input))
      {
        return "Input does not exist on disk. Remote is not currently supported.";
      }

      if (Path.GetExtension(input).Equals(".json", StringComparison.OrdinalIgnoreCase))
      {
        // Try the LUTI importer.
        Importers.LUTIJsonReader jsonReader = new Importers.LUTIJsonReader(input);

        try
        {
          string errorMessage = TryImport(jsonReader);
          if (string.IsNullOrWhiteSpace(errorMessage))
          {
            return "";
          }
          else
          {
            // TODO -- We can try other importers here.
            return errorMessage;
          }
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

    public string TryImport(ISplatTagDatabase importer)
    {
      try
      {
        var retVal = importer.Load();
        foreach (Team importTeam in retVal.Item2)
        {
          if (!teams.Values.Any(t => t.Name.Equals(importTeam.Name)))
          {
            uint key = teams.Keys.LastOrDefault() + 1;
            importTeam.Id = key;
            teams.Add(key, importTeam);
          }
          else
          {
            // Update the values
            Team foundTeam = teams.Values.FirstOrDefault(t => t.Name.Equals(importTeam.Name));
            importTeam.Id = foundTeam.Id;
            foundTeam = importTeam;
          }
        }

        foreach (Player importPlayer in retVal.Item1)
        {
          if (!players.Values.Any(p => p.Name.Equals(importPlayer.Name)))
          {
            uint key = players.Keys.LastOrDefault() + 1;
            importPlayer.Id = key;
            players.Add(key, importPlayer);
          }
          else
          {
            Player foundPlayer = players.Values.FirstOrDefault(p => p.Name.Equals(importPlayer.Name));
            importPlayer.Id = foundPlayer.Id;
            foundPlayer = importPlayer;
          }
        }
        return "";
      }
      catch (Exception ex)
      {
        return ex.Message;
      }
    }

    public Player CreatePlayer()
    {
      Player p = new Player
      {
        Id = players.Keys.LastOrDefault() + 1
      };
      players.Add(p.Id, p);
      return p;
    }

    public Team CreateTeam()
    {
      Team t = new Team
      {
        Id = teams.Keys.LastOrDefault() + 1
      };
      teams.Add(t.Id, t);
      return t;
    }
  }
}