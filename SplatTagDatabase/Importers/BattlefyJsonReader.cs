﻿using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    public override bool Equals(object? obj)
    {
      return obj is BattlefyJsonReader reader &&
             source.Equals(reader.source);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(nameof(BattlefyJsonReader), source);
    }

    public Source Load()
    {
      Debug.WriteLine("Loading " + jsonFile);
      string json = File.ReadAllText(jsonFile);
      BattlefyJsonTeam[] rows = JsonSerializer.Deserialize<BattlefyJsonTeam[]>(json) ?? Array.Empty<BattlefyJsonTeam>();

      List<Team> teams = new();
      List<Player> players = new();
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

        Team newTeam = new(row.TeamName, source);
        if (teamTag != null)
        {
          newTeam.AddClanTag(teamTag);
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

          if (p.PersistentPlayerId == null)
          {
            Console.WriteLine($"Warning: Player ({p.Name}) does not have a PersistentPlayerId. Did they sub? Ignoring this player entry. File: " + jsonFile);
            continue;
          }

          // Filter the friend code from the name, if found
          var (parsedFriendCode, strippedName) = FriendCode.ParseAndStripFriendCode(p.Name);
          if (!parsedFriendCode.NoCode)
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
            if (parsedFriendCode.NoCode && !row.CaptainFriendCode.NoCode)
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
          newPlayer.AddFCs(parsedFriendCode, source);
          players.Add(newPlayer);
        }
      }

      source.Players = players.ToArray();
      source.Teams = teams.ToArray();
      return source;
    }

    internal class BattlefyJsonPlayer
    {
      [JsonPropertyName("_id")]
      public string? BattlefyId { get; set; }

      // [JsonProperty("onTeam", Required = Required.Default)]
      // public bool OnTeam { get; set; }

      // [JsonProperty("isFreeAgent", Required = Required.Default)]
      // public bool IsFreeAgent { get; set; }

      // [JsonProperty("beCaptain", Required = Required.Default)]
      // public bool BeCaptain { get; set; }

      [JsonPropertyName("username")]
      public string? BattlefyName { get; set; }

      [JsonPropertyName("userSlug")]
      public string? BattlefyUserSlug { get; set; }

      [JsonPropertyName("inGameName")]
      public string? Name { get; set; }

      [JsonPropertyName("persistentPlayerID")]
      public string? PersistentPlayerId { get; set; }
    }

    internal class BattlefyJsonTeam
    {
      [JsonPropertyName("_id")]
      public string? BattlefyId { get; set; }

      [JsonPropertyName("persistentTeamID")]
      public string? BattlefyPersistentTeamId { get; set; }

      [JsonPropertyName("captain")]
      public BattlefyJsonPlayer? Captain { get; set; }

      [JsonPropertyName("checkedInAt")]
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

      [JsonPropertyName("customFields")]
      public Dictionary<string, string>[]? CustomFields { get; set; }

      // [JsonProperty("checkedInAt", Required = Required.Default)]
      // public string CheckedInAt { get; set; }
      [JsonPropertyName("players")]
      public BattlefyJsonPlayer[]? Players { get; set; }

      [JsonPropertyName("name")]
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