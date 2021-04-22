using Newtonsoft.Json;
using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SplatTagDatabase.Importers
{
  internal class BattlefyJsonReader : IImporter
  {
    private readonly string jsonFile;

    private readonly Source source;

    public BattlefyJsonReader(string jsonFile)
    {
      this.jsonFile = jsonFile ?? throw new ArgumentNullException(nameof(jsonFile));
      this.source = new Source(Path.GetFileNameWithoutExtension(jsonFile));
    }

    public static bool AcceptsInput(string input)
    {
      return Path.GetExtension(input).Equals(".json", StringComparison.OrdinalIgnoreCase);
    }

    public Source Load()
    {
      Debug.WriteLine("Loading " + jsonFile);
      string json = File.ReadAllText(jsonFile);
      BattlefyJsonTeam[] rows = JsonConvert.DeserializeObject<BattlefyJsonTeam[]>(json);

      List<Team> teams = new List<Team>();
      List<Player> players = new List<Player>();
      foreach (BattlefyJsonTeam row in rows)
      {
        if (row.TeamName == null || row.Players == null)
        {
          Console.Error.WriteLine("ERROR: JSON did not import a team correctly. Ignoring this team entry. File: " + jsonFile);
          continue;
        }

        if (row.Players.Length < 1)
        {
          // Report if the team name doesn't begin with "bye"
          if (!row.TeamName.StartsWith("bye", StringComparison.OrdinalIgnoreCase))
          {
            Console.Error.WriteLine($"ERROR: JSON does not contain a player for team \"{row.TeamName}\". Ignoring this team entry. File: " + jsonFile);
          }
          continue;
        }

        // Set the source start time if not already
        if (source.Start == Builtins.UnknownDateTime && row.CheckInTime != null)
        {
          source.Start = new DateTime(row.CheckInTime.Value.Year, row.CheckInTime.Value.Month, row.CheckInTime.Value.DayOfYear);
        }

        if (row.Captain == null)
        {
          Console.WriteLine($"Warning: JSON does not contain a Team Captain for team \"{row.TeamName}\". Assuming player 1 is captain. File: " + jsonFile);
          row.Captain = row.Players[0];
        }

        if (string.IsNullOrEmpty(row.Captain.Name))
        {
          Console.Error.WriteLine($"ERROR: The captain for team \"{row.TeamName}\" does not have a name. Ignoring this team entry. File: " + jsonFile);
          continue;
        }

        // Attempt to resolve the team tags
        ClanTag? teamTag = ClanTag.CalculateTagFromNames(row.Players.Select(p => p.Name).Where(name => name != null && !string.IsNullOrWhiteSpace(name)).ToArray()!, source);

        Team newTeam = new Team(row.TeamName, source);
        if (teamTag != null)
        {
          newTeam.AddClanTags(new[] { teamTag });
        }

        if (row.BattlefyPersistentTeamId != null)
        {
          newTeam.AddBattlefyId(row.BattlefyPersistentTeamId, source);
        }

        // If we already have a team with this id then merge it.
        if (newTeam.BattlefyPersistentTeamId != null)
        {
          var knownTeam = teams.Find(t => newTeam.BattlefyPersistentTeamId.Equals(t.BattlefyPersistentTeamId));
          if (knownTeam != null)
          {
            knownTeam.Merge(newTeam);
          }
          else
          {
            teams.Add(newTeam);
          }
        }
        else
        {
          teams.Add(newTeam);
        }

        foreach (BattlefyJsonPlayer p in row.Players)
        {
          if (p.Name == null)
          {
            Console.Error.WriteLine($"ERROR: Player's Name ({p.Name}) not populated. Ignoring this player entry. File: " + jsonFile);
            continue;
          }

          // Filter the friend code from the name, if found
          var (parsedFriendCode, strippedName) = FriendCode.ParseAndStripFriendCode(p.Name);
          if (parsedFriendCode != FriendCode.NO_FRIEND_CODE)
          {
            p.Name = strippedName;
          }

          // Remove tag from player
          if (teamTag != null)
          {
            p.Name = teamTag.StripFromPlayer(p.Name.Trim());
          }

          // Add Discord information, if we have it
          var newPlayer = new Player(p.Name, new[] { newTeam.Id }, source);

          if (p.BattlefyName != null && p.BattlefyName == row.Captain.BattlefyName)
          {
            if (parsedFriendCode == FriendCode.NO_FRIEND_CODE && row.CaptainFriendCode != FriendCode.NO_FRIEND_CODE)
            {
              parsedFriendCode = row.CaptainFriendCode;
            }

            if (row.CaptainDiscordName != null)
            {
              newPlayer.AddDiscordUsername(row.CaptainDiscordName, source);
            }
          }

          // Add Battlefy
          if (p.BattlefyName != null && p.BattlefyUserSlug != null && p.PersistentPlayerId != null)
          {
            newPlayer.AddBattlefyInformation(p.BattlefyUserSlug, p.BattlefyName, p.PersistentPlayerId, source);
          }
          newPlayer.AddFCs(parsedFriendCode.AsEnumerable());
          players.Add(newPlayer);
        }
      }

      source.Players = players.ToArray();
      source.Teams = teams.ToArray();
      return source;
    }

    [Serializable]
    internal class BattlefyJsonPlayer
    {
      [JsonProperty("_id", Required = Required.Default)]
      public string? BattlefyId { get; set; }

      // [JsonProperty("onTeam", Required = Required.Default)]
      // public bool OnTeam { get; set; }

      // [JsonProperty("isFreeAgent", Required = Required.Default)]
      // public bool IsFreeAgent { get; set; }

      // [JsonProperty("beCaptain", Required = Required.Default)]
      // public bool BeCaptain { get; set; }

      [JsonProperty("username")]
      public string? BattlefyName { get; set; }

      [JsonProperty("userSlug")]
      public string? BattlefyUserSlug { get; set; }

      [JsonProperty("inGameName")]
      public string? Name { get; set; }

      [JsonProperty("persistentPlayerID")]
      public string? PersistentPlayerId { get; set; }
    }

    [Serializable]
    internal class BattlefyJsonTeam
    {
      [JsonProperty("_id", Required = Required.Default)]
      public string? BattlefyId { get; set; }

      [JsonProperty("persistentTeamID", Required = Required.Default)]
      public string? BattlefyPersistentTeamId { get; set; }

      [JsonProperty("captain")]
      public BattlefyJsonPlayer? Captain { get; set; }

      [JsonProperty("checkedInAt", Required = Required.Default)]
      public DateTime? CheckInTime { get; set; }

      public string? CaptainDiscordName
      {
        get
        {
          if (CustomFields?.Length > 0)
          {
            if (Discord.DISCORD_NAME_REGEX.IsMatch(CustomFields[0]["value"]))
            {
              return CustomFields[0]["value"];
            }
            else if (CustomFields.Length > 1)
            {
              if (Discord.DISCORD_NAME_REGEX.IsMatch(CustomFields[1]["value"]))
              {
                return CustomFields[1]["value"];
              }
            }
          }
          return null;
        }
      }

      public FriendCode CaptainFriendCode
      {
        get
        {
          if (CustomFields?.Length > 0)
          {
            if (FriendCode.TryParse(CustomFields[0]["value"], out FriendCode fc))
            {
              return fc;
            }
            else if (CustomFields.Length > 1)
            {
              if (FriendCode.TryParse(CustomFields[1]["value"], out FriendCode fc1))
              {
                return fc1;
              }
            }
          }
          return FriendCode.NO_FRIEND_CODE;
        }
      }

      [JsonProperty("customFields")]
      public Dictionary<string, string>[]? CustomFields { get; set; }

      // [JsonProperty("checkedInAt", Required = Required.Default)]
      // public string CheckedInAt { get; set; }
      [JsonProperty("players")]
      public BattlefyJsonPlayer[]? Players { get; set; }

      [JsonProperty("name")]
      public string? TeamName { get; set; }

      // [JsonProperty("pendingTeamID", Required = Required.Default)]
      // public string BattlefyPendingTeamId { get; set; }
      // [JsonProperty("tournamentID", Required = Required.Default)]
      // public string BattlefyTournamentId { get; set; }

      // [JsonProperty("userID", Required = Required.Default)]
      // public string BattlefyUserId { get; set; }
      // [JsonProperty("ownerID", Required = Required.Default)]
      // public string BattlefyOwnerId { get; set; }

      // [JsonProperty("createdAt", Required = Required.Default)]
      // public string CreatedAt { get; set; }

      // [JsonProperty("playerIDs", Required = Required.Default)]
      // public string[] PlayerIDs { get; set; }

      // [JsonProperty("captainID", Required = Required.Default)]
      // public string CaptainId { get; set; }
    }
  }
}