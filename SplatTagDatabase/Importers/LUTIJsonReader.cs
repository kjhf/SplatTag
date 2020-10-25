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
      public string TeamName { get; set; }

      [JsonProperty("Div", Required = Required.Default)]
      public string Div { get => Division; set => Division = value; }

      [JsonProperty("Division", Required = Required.Default)]
      public string Division { get; set; } = "Unknown";

      private string tag;

      [JsonProperty("Tag")]
      public string Tag { get => tag; set => tag = value.Trim(); }

      [JsonProperty("Team Captain", Required = Required.Default)]
      public string TeamCaptain { get; set; } = "";

      [JsonProperty("Player 1", Required = Required.Default)]
      public string Player1 { get => TeamCaptain; set => TeamCaptain = value; }

      [JsonProperty("Player 2")]
      public string Player2 { get; set; }

      [JsonProperty("Player 3")]
      public string Player3 { get; set; }

      [JsonProperty("Player 4")]
      public string Player4 { get; set; }

      [JsonProperty("Player 5")]
      public string Player5 { get; set; }

      [JsonProperty("Player 6")]
      public string Player6 { get; set; }

      [JsonProperty("Player 7")]
      public string Player7 { get; set; }

      [JsonProperty("Player 8")]
      public string Player8 { get; set; }

      [JsonProperty("Player 9")]
      public string Player9 { get; set; }

      [JsonProperty("Player 10")]
      public string Player10 { get; set; }
    }

    private readonly string jsonFile;

    public LUTIJsonReader(string jsonFile)
    {
      this.jsonFile = jsonFile ?? throw new ArgumentNullException(nameof(jsonFile));
    }

    public (Player[], Team[]) Load()
    {
      void CheckAndAddPlayer(string tryPlayerName, string _tag, string _transformedTag, Team _newTeam, List<Player> _players)
      {
        if (!string.IsNullOrWhiteSpace(tryPlayerName))
        {
          switch (_newTeam.ClanTagOption)
          {
            case TagOption.Front:
              if (tryPlayerName.StartsWith(_tag))
              {
                tryPlayerName = tryPlayerName.Substring(_tag.Length).Trim();
              }
              else if (tryPlayerName.StartsWith(_transformedTag))
              {
                tryPlayerName = tryPlayerName.Substring(_transformedTag.Length).Trim();
              }
              break;

            case TagOption.Back:
              if (tryPlayerName.EndsWith(_tag))
              {
                tryPlayerName = tryPlayerName.Substring(0, tryPlayerName.Length - _tag.Length - 1).Trim();
              }
              else if (tryPlayerName.EndsWith(_transformedTag))
              {
                tryPlayerName = tryPlayerName.Substring(0, tryPlayerName.Length - _transformedTag.Length - 1).Trim();
              }
              break;

            default:
              // Just leave it.
              break;
          }

          _players.Add(new Player
          {
            CurrentTeam = _newTeam.Id,
            Name = tryPlayerName,
            Sources = new string[] { Path.GetFileNameWithoutExtension(jsonFile) }
          });
        }
      }

      if (jsonFile == null)
      {
        throw new InvalidOperationException(nameof(jsonFile) + " is not set.");
      }

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

        Team newTeam = new Team
        {
          Id = (uint)teams.Count,  // This will be updated when the merge happens.
          ClanTags = new string[] { row.Tag },
          ClanTagOption = TagOption.Unknown,
          Div = new Division(row.Division),
          Name = row.TeamName,
          Sources = new string[] { Path.GetFileNameWithoutExtension(jsonFile) }
        };

        // Handle tag placements from the captain's name
        newTeam.SetTagOption(row.Tag, row.TeamCaptain);
        string transformedTag = row.Tag?.TransformString();

        teams.Add(newTeam);
        CheckAndAddPlayer(row.TeamCaptain, row.Tag, transformedTag, newTeam, players);
        CheckAndAddPlayer(row.Player2, row.Tag, transformedTag, newTeam, players);
        CheckAndAddPlayer(row.Player3, row.Tag, transformedTag, newTeam, players);
        CheckAndAddPlayer(row.Player4, row.Tag, transformedTag, newTeam, players);
        CheckAndAddPlayer(row.Player5, row.Tag, transformedTag, newTeam, players);
        CheckAndAddPlayer(row.Player6, row.Tag, transformedTag, newTeam, players);
        CheckAndAddPlayer(row.Player7, row.Tag, transformedTag, newTeam, players);
        CheckAndAddPlayer(row.Player8, row.Tag, transformedTag, newTeam, players);
        CheckAndAddPlayer(row.Player9, row.Tag, transformedTag, newTeam, players);
        CheckAndAddPlayer(row.Player10, row.Tag, transformedTag, newTeam, players);
      }

      return (players.ToArray(), teams.ToArray());
    }

    public static bool AcceptsInput(string input)
    {
      return Path.GetFileName(input).Contains("-LUTI-", StringComparison.InvariantCultureIgnoreCase) && Path.GetExtension(input).Equals(".json", StringComparison.OrdinalIgnoreCase);
    }
  }
}