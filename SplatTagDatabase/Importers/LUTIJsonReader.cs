using SplatTagCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SplatTagDatabase.Importers
{
  internal class LUTIJsonReader : IImporter
  {
    internal class LUTIJsonRow
    {
      [JsonPropertyName("Team Name")]
      public string TeamName { get; set; } = Builtins.UNKNOWN_TEAM;

      [JsonNumberHandling(JsonNumberHandling.WriteAsString)]
      [JsonPropertyName("Div")]
      public string Div { get => Division; set => Division = value; }

      [JsonNumberHandling(JsonNumberHandling.WriteAsString)]
      [JsonPropertyName("Division")]
      public string Division { get; set; } = "Unknown";

      private string? tag = Team.NoTeam.Tag?.Value;

      [JsonPropertyName("Tag")]
      public string? Tag
      {
        get => tag;
        set
        {
          if (value != null && !string.IsNullOrWhiteSpace(value))
          {
            tag = value.Trim();
          }
        }
      }

      [JsonPropertyName("Team Captain")]
      public string TeamCaptain { get; set; } = "";

      [JsonPropertyName("Player 1")]
      public string Player1 { get => TeamCaptain; set => TeamCaptain = value; }

      [JsonPropertyName("Player 2")]
      public string Player2 { get; set; } = "";

      [JsonPropertyName("Player 3")]
      public string Player3 { get; set; } = "";

      [JsonPropertyName("Player 4")]
      public string Player4 { get; set; } = "";

      [JsonPropertyName("Player 5")]
      public string Player5 { get; set; } = "";

      [JsonPropertyName("Player 6")]
      public string Player6 { get; set; } = "";

      [JsonPropertyName("Player 7")]
      public string Player7 { get; set; } = "";

      [JsonPropertyName("Player 8")]
      public string Player8 { get; set; } = "";

      [JsonPropertyName("Player 9")]
      public string Player9 { get; set; } = "";

      [JsonPropertyName("Player 10")]
      public string Player10 { get; set; } = "";

      /// <summary>
      /// Get the players of this team (checks/omits null and empty)
      /// </summary>
      public IEnumerable<string> Players => new[] { Player1, Player2, Player3, Player4, Player5, Player6, Player7, Player8, Player9, Player10 }.Where(p => !string.IsNullOrWhiteSpace(p));
    }

    private readonly string jsonFile;
    private readonly Source source;
    private readonly string season;

    public LUTIJsonReader(string jsonFile)
    {
      this.jsonFile = jsonFile ?? throw new ArgumentNullException(nameof(jsonFile));
      string fileName = Path.GetFileNameWithoutExtension(jsonFile);
      this.source = new Source(fileName);
      if (fileName.Contains("LUTI-"))
      {
        season = fileName.Substring(fileName.IndexOf("LUTI-") + "LUTI-".Length);
      }
      else
      {
        season = fileName;
      }
    }

    public override bool Equals(object? obj)
    {
      return obj is LUTIJsonReader reader &&
             source.Equals(reader.source);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(nameof(LUTIJsonReader), source);
    }

    public Source Load()
    {
      Debug.WriteLine("Loading " + jsonFile);
      string json = File.ReadAllText(jsonFile); // N.B. by default this reads UTF-8.
      LUTIJsonRow[] rows = JsonSerializer.Deserialize<LUTIJsonRow[]>(json) ?? Array.Empty<LUTIJsonRow>();

      var teams = new ConcurrentBag<Team>();
      var players = new ConcurrentBag<Player>();
      Parallel.ForEach(rows, row =>
      {
        if (row.TeamCaptain == null)
        {
          throw new ArgumentException("JSON does not contain a Team Captain. Check format of spreadsheet.");
        }

        string[] playerNames = row.Players.ToArray();

        Team newTeam = new Team(row.TeamName, source);
        newTeam.AddDivision(new Division(row.Division, DivType.LUTI, season), source);

        if (row.Tag != null && row.Tag.Length != 0 && row.TeamCaptain.Contains(row.Tag))
        {
          // Handle tag placements from the captain's name
          newTeam.AddClanTag(row.Tag, source, ClanTag.CalculateTagOption(row.Tag, row.TeamCaptain));
        }
        else
        {
          // Calculate a tag
          ClanTag? newTag = ClanTag.CalculateTagFromNames(playerNames, source);

          if (newTag != null)
          {
            newTeam.AddClanTag(newTag);
          }
        }

        teams.Add(newTeam);
        foreach (string player in playerNames)
        {
          var playerName = player.Trim();
          playerName = newTeam.Tag?.StripFromPlayer(playerName) ?? playerName;

          var p = new Player(playerName, new[] { newTeam.Id }, source);
          players.Add(p);
        }
      });

      source.Players = players.ToArray();
      source.Teams = teams.ToArray();
      return source;
    }

    public static bool AcceptsInput(string input)
    {
      // Must contain -LUTI- (in all caps)
      return Path.GetFileName(input).Contains("-LUTI-") && Path.GetExtension(input).Equals(".json", StringComparison.OrdinalIgnoreCase);
    }
  }
}