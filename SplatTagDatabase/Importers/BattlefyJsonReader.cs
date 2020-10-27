using Newtonsoft.Json;
using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            if (FriendCode.TryParse(CustomFields[0]["value"], out FriendCode fc))
            {
              return fc.ToString();
            }
            else if (CustomFields.Length > 1)
            {
              if (FriendCode.TryParse(CustomFields[1]["value"], out FriendCode fc1))
              {
                return fc1.ToString();
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
        throw new InvalidOperationException(nameof(jsonFile) + " is not set.");
      }

      Debug.WriteLine("Loading " + jsonFile);
      string json = File.ReadAllText(jsonFile);
      BattlefyJsonTeam[] rows = JsonConvert.DeserializeObject<BattlefyJsonTeam[]>(json);

      List<Team> teams = new List<Team>();
      List<Player> players = new List<Player>();
      foreach (BattlefyJsonTeam row in rows)
      {
        if (row.Players.Length < 1)
        {
          Console.Error.WriteLine($"ERROR: JSON does not contain a player for team \"{row.TeamName}\". Ignoring this team entry. File: " + jsonFile);
          continue;
        }

        if (row.Players.Length < 4)
        {
          Console.WriteLine($"Warning: JSON does not contain 4+ players for team \"{row.TeamName}\". Continuing anyway. File: " + jsonFile);
        }

        if (row.Captain == null)
        {
          Console.WriteLine($"Warning: JSON does not contain a Team Captain for team \"{row.TeamName}\". Assuming player 1 is captain. File: " + jsonFile);
          row.Captain = row.Players[0];
        }

        if (row.CustomFields.Length < 2)
        {
          Console.WriteLine($"Warning: JSON contains {row.CustomFields.Length}/2 Custom Fields (Discord/FC) for team \"{row.TeamName}\". Continuing anyway. File: " + jsonFile);
        }

        // Attempt to resolve the team tag
        int i;
        string tag = "";
        char firstPlayerLetter = row.Players[0].Name[0];
        int smallestNameLength = row.Players.Min(p => p.Name.Length);
        var halfTheTeam = (players.Count / 2) + 1;
        for (i = 0; (players.Count(p => p.Name[i] == firstPlayerLetter) >= halfTheTeam) && i < smallestNameLength; i++, firstPlayerLetter = row.Players[0].Name[i]) { }

        if (i >= 2)
        {
          tag = row.Players[0].Name.Substring(0, i);
        }

        Team newTeam = new Team
        {
          Id = -teams.Count - 1,  // This will be updated when the merge happens.
          ClanTags = tag.Length == 0 ? new string[0] : new string[1] { tag },
          ClanTagOption = tag.Length == 0 ? TagOption.Unknown : TagOption.Front,
          Div = new Division(),
          Name = row.TeamName,
          Sources = new string[] { Path.GetFileNameWithoutExtension(jsonFile) }
        };

        // If we already have a team of this name then merge it.
        var knownTeam = teams.Find(t => t.SearchableName.Equals(newTeam.SearchableName));
        if (knownTeam != null)
        {
          knownTeam.Merge(newTeam);
        }
        else
        {
          teams.Add(newTeam);
        }

        foreach (BattlefyJsonPlayer p in row.Players)
        {
          // Add the player
          if (p.Name.StartsWith(tag) && p.Name != tag)
          {
            p.Name = p.Name.Substring(tag.Length).Trim();
          }

          // Filter the friend code from the name, if found
          var (parsedFriendCode, strippedName) = FriendCode.ParseAndStripFriendCode(p.Name);
          if (parsedFriendCode != null)
          {
            p.Name = strippedName;
          }

          players.Add(new Player
          {
            CurrentTeam = newTeam.Id,
            Names = new string[] { p.Name, p.BattlefyName },
            Sources = new string[] { Path.GetFileNameWithoutExtension(jsonFile) },
            FriendCode = parsedFriendCode != null ? parsedFriendCode.ToString() : ((p.BattlefyName == row.Captain.BattlefyName) ? row.CaptainFriendCode : null),
            DiscordName = (p.BattlefyName == row.Captain.BattlefyName) ? row.CaptainDiscordName : null,
          });
        }
      }

      return (players.ToArray(), teams.ToArray());
    }

    public static bool AcceptsInput(string input)
    {
      return Path.GetExtension(input).Equals(".json", StringComparison.OrdinalIgnoreCase);
    }
  }
}