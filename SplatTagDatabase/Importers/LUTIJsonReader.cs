using Newtonsoft.Json;
using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SplatTagDatabase.Importers
{
  internal class LUTIJsonReader : IImporter
  {
    [Serializable]
    internal class LUTIJsonRow
    {
      [JsonProperty("Team Name")]
      public string TeamName { get; set; } = Builtins.UNKNOWN_TEAM;

      [JsonProperty("Div", Required = Required.Default)]
      public string Div { get => Division; set => Division = value; }

      [JsonProperty("Division", Required = Required.Default)]
      public string Division { get; set; } = "Unknown";

      private string? tag = Team.NoTeam.Tag?.Value;

      [JsonProperty("Tag")]
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

      [JsonProperty("Team Captain", Required = Required.Default)]
      public string TeamCaptain { get; set; } = "";

      [JsonProperty("Player 1", Required = Required.Default)]
      public string Player1 { get => TeamCaptain; set => TeamCaptain = value; }

      [JsonProperty("Player 2")]
      public string Player2 { get; set; } = "";

      [JsonProperty("Player 3")]
      public string Player3 { get; set; } = "";

      [JsonProperty("Player 4")]
      public string Player4 { get; set; } = "";

      [JsonProperty("Player 5")]
      public string Player5 { get; set; } = "";

      [JsonProperty("Player 6")]
      public string Player6 { get; set; } = "";

      [JsonProperty("Player 7")]
      public string Player7 { get; set; } = "";

      [JsonProperty("Player 8")]
      public string Player8 { get; set; } = "";

      [JsonProperty("Player 9")]
      public string Player9 { get; set; } = "";

      [JsonProperty("Player 10")]
      public string Player10 { get; set; } = "";
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

    public (Player[], Team[]) Load()
    {
      Debug.WriteLine("Loading " + jsonFile);
      string json = File.ReadAllText(jsonFile); // N.B. by default this reads UTF-8.
      LUTIJsonRow[] rows = JsonConvert.DeserializeObject<LUTIJsonRow[]>(json);

      List<Team> teams = new List<Team>();
      List<Player> players = new List<Player>();
      foreach (LUTIJsonRow row in rows)
      {
        if (row.TeamCaptain == null)
        {
          throw new ArgumentException("JSON does not contain a Team Captain. Check format of spreadsheet.");
        }

        Team newTeam = new Team(row.TeamName, source);
        newTeam.AddDivision(new Division(row.Division, DivType.LUTI, season));

        if (row.Tag != null)
        {
          // Handle tag placements from the captain's name
          newTeam.AddClanTag(row.Tag, source, ClanTag.CalculateTagOption(row.Tag, row.TeamCaptain));
        }

        teams.Add(newTeam);
        Merger.AddPlayerFromTag(row.TeamCaptain, newTeam, players, source);
        Merger.AddPlayerFromTag(row.Player2, newTeam, players, source);
        Merger.AddPlayerFromTag(row.Player3, newTeam, players, source);
        Merger.AddPlayerFromTag(row.Player4, newTeam, players, source);
        Merger.AddPlayerFromTag(row.Player5, newTeam, players, source);
        Merger.AddPlayerFromTag(row.Player6, newTeam, players, source);
        Merger.AddPlayerFromTag(row.Player7, newTeam, players, source);
        Merger.AddPlayerFromTag(row.Player8, newTeam, players, source);
        Merger.AddPlayerFromTag(row.Player9, newTeam, players, source);
        Merger.AddPlayerFromTag(row.Player10, newTeam, players, source);
      }

      return (players.ToArray(), teams.ToArray());
    }

    public static bool AcceptsInput(string input)
    {
      // Must contain -LUTI- (in all caps)
      return Path.GetFileName(input).Contains("-LUTI-") && Path.GetExtension(input).Equals(".json", StringComparison.OrdinalIgnoreCase);
    }
  }
}