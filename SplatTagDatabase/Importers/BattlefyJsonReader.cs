using Newtonsoft.Json;
using SplatTagCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SplatTagDatabase.Importers
{
  internal class BattlefyJsonReader : IImporter
  {
    [Serializable]
    internal class BattlefyJsonPlayer
    {
      [JsonProperty("_id", Required = Required.Default)]
      public string BattlefyId { get; set; }

      // [JsonProperty("onTeam", Required = Required.Default)]
      // public bool OnTeam { get; set; }

      // [JsonProperty("isFreeAgent", Required = Required.Default)]
      // public bool IsFreeAgent { get; set; }

      // [JsonProperty("beCaptain", Required = Required.Default)]
      // public bool BeCaptain { get; set; }

      [JsonProperty("inGameName")]
      public string Name { get; set; }

      // [JsonProperty("userSlug", Required = Required.Default)]
      // public string BattlefyUserSlug { get; set; }

      [JsonProperty("username")]
      public string BattlefyName { get; set; }
    }

    [Serializable]
    internal class BattlefyJsonTeam
    {
      [JsonProperty("_id", Required = Required.Default)]
      public string BattlefyId { get; set; }

      [JsonProperty("name")]
      public string TeamName { get; set; }

      // [JsonProperty("pendingTeamID", Required = Required.Default)]
      // public string BattlefyPendingTeamId { get; set; }

      // [JsonProperty("persistentTeamID", Required = Required.Default)]
      // public string BattlefyPersistentTeamId { get; set; }

      // [JsonProperty("tournamentID", Required = Required.Default)]
      // public string BattlefyTournamentId { get; set; }

      // [JsonProperty("userID", Required = Required.Default)]
      // public string BattlefyUserId { get; set; }

      [JsonProperty("customFields")]
      public Dictionary<string, string>[] CustomFields { get; set; }

      public string CaptainDiscordName
      {
        get
        {
          if (CustomFields.Length > 0)
          {
            if (Player.DISCORD_NAME_REGEX.IsMatch(CustomFields[0]["value"]))
            {
              return CustomFields[0]["value"];
            }
            else if (CustomFields.Length > 1)
            {
              if (Player.DISCORD_NAME_REGEX.IsMatch(CustomFields[1]["value"]))
              {
                return CustomFields[1]["value"];
              }
            }
          }
          return null;
        }
      }

      public string CaptainFriendCode
      {
        get
        {
          if (CustomFields.Length > 0)
          {
            if (Player.FRIEND_CODE_REGEX.IsMatch(CustomFields[0]["value"]))
            {
              return CustomFields[0]["value"];
            }
            else if (CustomFields.Length > 1)
            {
              if (Player.FRIEND_CODE_REGEX.IsMatch(CustomFields[1]["value"]))
              {
                return CustomFields[1]["value"];
              }
            }
          }
          return null;
        }
      }

      // [JsonProperty("ownerID", Required = Required.Default)]
      // public string BattlefyOwnerId { get; set; }

      // [JsonProperty("createdAt", Required = Required.Default)]
      // public string CreatedAt { get; set; }

      // [JsonProperty("playerIDs", Required = Required.Default)]
      // public string[] PlayerIDs { get; set; }

      // [JsonProperty("captainID", Required = Required.Default)]
      // public string CaptainId { get; set; }

      // [JsonProperty("checkedInAt", Required = Required.Default)]
      // public string CheckedInAt { get; set; }

      // [JsonProperty("checkedInAt", Required = Required.Default)]
      // public string CheckedInAt { get; set; }

      [JsonProperty("captain")]
      public BattlefyJsonPlayer Captain { get; set; }

      [JsonProperty("players")]
      public BattlefyJsonPlayer[] Players { get; set; }
    }

    private readonly string jsonFile;

    public BattlefyJsonReader(string jsonFile)
    {
      this.jsonFile = jsonFile ?? throw new ArgumentNullException(nameof(jsonFile));
    }

    public (Player[], Team[]) Load()
    {
      if (jsonFile == null)
      {
        throw new InvalidOperationException("jsonFile is not set.");
      }

      string json = File.ReadAllText(jsonFile);
      BattlefyJsonTeam[] rows = JsonConvert.DeserializeObject<BattlefyJsonTeam[]>(json);

      List<Team> teams = new List<Team>();
      List<Player> players = new List<Player>();
      foreach (BattlefyJsonTeam row in rows)
      {
        if (row.Captain == null)
        {
          Console.WriteLine("JSON does not contain a Team Captain for this team. Check format of the incoming JSON: " + row.TeamName + " in file " + jsonFile);
          continue;
        }

        if (row.Players.Length < 3)
        {
          Console.WriteLine("JSON does not contain 3+ players for this team. Check format of the incoming JSON: " + row.TeamName + " in file " + jsonFile);
          continue;
        }

        if (row.CustomFields.Length < 2)
        {
          Console.WriteLine("JSON does not contain Custom Field containing the Discord/Switch FC for this team. Continuing anyway. " + row.TeamName + " in file " + jsonFile);
        }

        // Attempt to resolve the team tag
        int i;
        string tag = "";
        int minLength = row.Players.Min(p => p.Name.Length);
        for (i = 0; row.Players[0].Name[i] == row.Players[1].Name[i] && row.Players[2].Name[i] == row.Players[0].Name[i] && i < minLength; i++) { }
        if (i >= 2)
        {
          tag = row.Players[0].Name.Substring(0, i);
        }

        Team newTeam = new Team
        {
          Id = -teams.Count - 1,  // This will be updated when the merge happens.
          ClanTags = new string[1] { tag },
          ClanTagOption = TagOption.Front,
          Div = new Division(),
          Name = row.TeamName,
          Sources = new List<string> { Path.GetFileNameWithoutExtension(jsonFile) }
        };

        teams.Add(newTeam);

        foreach (BattlefyJsonPlayer p in row.Players)
        {
          // Add the player
          if (p.Name.StartsWith(tag) && p.Name != tag)
          {
            p.Name = p.Name.Substring(tag.Length).Trim();
          }

          // Filter the friend code from the name, if found
          Match fcMatch = Player.FRIEND_CODE_REGEX.Match(p.Name);
          if (fcMatch.Success)
          {
            p.Name = Player.FRIEND_CODE_REGEX.Replace(p.Name, "").Trim();
          }

          players.Add(new Player
          {
            CurrentTeam = newTeam.Id,
            Names = new string[] { p.Name, p.BattlefyName },
            Sources = new List<string> { Path.GetFileNameWithoutExtension(jsonFile) },
            FriendCode = fcMatch.Success ? fcMatch.Value.Trim(new char[] { '(', ')'}) : ((p.BattlefyName == row.Captain.BattlefyName) ? row.CaptainFriendCode : null),
            DiscordName = (p.BattlefyName == row.Captain.BattlefyName) ? row.CaptainDiscordName : null,
          });
        }
      }

      return (players.ToArray(), teams.ToArray());
    }

    public bool AcceptsInput(string input)
    {
      // NOT a LUTI file but is a JSON file
      return !Path.GetFileName(input).StartsWith("LUTI-", StringComparison.InvariantCultureIgnoreCase) && Path.GetExtension(input).Equals(".json", StringComparison.OrdinalIgnoreCase);
    }
  }
}