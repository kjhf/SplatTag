using Newtonsoft.Json;
using SplatTagCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SplatTagDatabase
{
  public class SplatTagJsonDatabase : ISplatTagDatabase
  {
    private readonly string saveDirectory;
    private string PlayersFile => Path.Combine(saveDirectory, "players.json");
    private string TeamsFile => Path.Combine(saveDirectory, "teams.json");

    public SplatTagJsonDatabase(string saveDirectory)
    {
      this.saveDirectory = saveDirectory ?? throw new ArgumentNullException(nameof(saveDirectory));
      Directory.CreateDirectory(saveDirectory);
    }

    public (Player[], Team[]) Load()
    {
      DbPlayer[] dbplayers = new DbPlayer[0];
      DbTeam[] dbteams = new DbTeam[0];

      if (File.Exists(PlayersFile))
      {
        string json = File.ReadAllText(PlayersFile);
        dbplayers = JsonConvert.DeserializeObject<DbPlayers>(json).players;
      }

      if (File.Exists(TeamsFile))
      {
        string json = File.ReadAllText(TeamsFile);
        dbteams = JsonConvert.DeserializeObject<DbTeams>(json).teams;
      }

      // Transform the database teams into team objects
      List<Team> teams = new List<Team>();
      foreach (var t in dbteams)
      {
        teams.Add(new Team
        {
          Id = t.id,
          ClanTags = t.clanTags,
          ClanTagOption = (TagOption)t.clanTagOption,
          Div = new Division(t.div),
          Name = t.name,
        });
      }

      // Transform the database players into player objects
      List<Player> players = new List<Player>();
      foreach (var p in dbplayers)
      {
        players.Add(new Player
        {
          Id = p.id,
          Names = p.names,
          Teams = p.teams.ToArray()
        });
      }

      return (players.ToArray(), teams.ToArray());
    }

    public void Save(IEnumerable<Player> savePlayers, IEnumerable<Team> saveTeams)
    {
      // First, move the files as a backup
      if (File.Exists(PlayersFile))
      {
        File.Copy(PlayersFile, PlayersFile + "_old-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss.json"));
      }

      if (File.Exists(TeamsFile))
      {
        File.Copy(TeamsFile, TeamsFile + "_old-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss.json"));
      }

      // Transform Players
      DbPlayers dbPlayers = new DbPlayers
      {
        players = savePlayers.Select(p =>
        {
          return new DbPlayer
          {
            id = p.Id,
            names = p.Names.ToArray(),
            teams = p.Teams.ToArray()
          };
        }).ToArray()
      };

      // Write Players
      string json = JsonConvert.SerializeObject(dbPlayers);
      File.WriteAllText(PlayersFile, json);

      // Transform Teams
      DbTeams dbTeams = new DbTeams
      {
        teams = saveTeams.Select(t =>
        {
          if (t.Id < 0)
          {
            throw new ArgumentException("Team id cannot be negative - check teams have been merged.");
          }

          return new DbTeam
          {
            id = (uint)t.Id,
            clanTagOption = (int)t.ClanTagOption,
            clanTags = t.ClanTags,
            name = t.Name,
            div = t.Div
          };
        }).ToArray()
      };

      // Write Teams
      json = JsonConvert.SerializeObject(dbTeams);
      File.WriteAllText(TeamsFile, json);
    }
  }
}