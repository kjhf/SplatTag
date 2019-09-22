using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace SplatTagCore.Importers
{
  internal class LUTIJsonReader : ISplatTagDatabase
  {
    [Serializable]
    internal class LUTIJsonRow
    {
      [JsonProperty("Team Name")]
      public string TeamName { get; set; }

      [JsonProperty("Division")]
      public string Division { get; set; }

      [JsonProperty("Tag")]
      public string Tag { get; set; }

      [JsonProperty("Team Captain")]
      public string TeamCaptain { get; set; }

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
      void CheckAndAddPlayer(string tryPlayerName, string _tag, Team _newTeam, List<Player> _players)
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
              break;

            case TagOption.Back:
              if (tryPlayerName.EndsWith(_tag))
              {
                tryPlayerName = tryPlayerName.Substring(0, tryPlayerName.Length - _tag.Length - 1).Trim();
              }
              break;

            default:
              // Just leave it.
              break;
          }

          _players.Add(new Player
          {
            CurrentTeam = _newTeam,
            Name = tryPlayerName,
          });
        }
      }

      string json = File.ReadAllText(jsonFile);
      LUTIJsonRow[] rows = JsonConvert.DeserializeObject<LUTIJsonRow[]>(json);

      List<Team> teams = new List<Team>();
      List<Player> players = new List<Player>();
      foreach (LUTIJsonRow row in rows)
      {
        Team newTeam = new Team
        {
          ClanTags = new string[] { row.Tag },
          ClanTagOption = TagOption.Front,
          Name = row.TeamName,
        };

        // Handle tag placements from the captain's name
        if (string.IsNullOrWhiteSpace(row.Tag))
        {
          // Nothing to do, no tag
        }
        else if (row.TeamCaptain.StartsWith(row.Tag, StringComparison.OrdinalIgnoreCase))
        {
          // Nothing to do, the tag is at the default Front
        }
        else if (row.TeamCaptain.EndsWith(row.Tag, StringComparison.OrdinalIgnoreCase))
        {
          // Tag is at the back.
          newTeam.ClanTagOption = TagOption.Back;
        }
        else if (row.Tag.Length >= 2)
        {
          // If the tag has 2 or more characters, check 'surrounding' criteria which is take the
          // first character of the tag and check if the captain's name begins with this character,
          // then take the last character of the tag and check if the captain's name ends with this character.
          // e.g. Tag: //, Captain's name: /captain/
          if (row.TeamCaptain.StartsWith(row.Tag[0].ToString(), StringComparison.OrdinalIgnoreCase)
            && row.TeamCaptain.EndsWith(row.Tag[row.Tag.Length - 1].ToString(), StringComparison.OrdinalIgnoreCase))
          {
            newTeam.ClanTagOption = TagOption.Surrounding;
          }
        }
        // else tag is not present in the captain's name and therefore assume default front.

        teams.Add(newTeam);
        CheckAndAddPlayer(row.TeamCaptain, row.Tag, newTeam, players);
        CheckAndAddPlayer(row.Player2, row.Tag, newTeam, players);
        CheckAndAddPlayer(row.Player3, row.Tag, newTeam, players);
        CheckAndAddPlayer(row.Player4, row.Tag, newTeam, players);
        CheckAndAddPlayer(row.Player5, row.Tag, newTeam, players);
        CheckAndAddPlayer(row.Player6, row.Tag, newTeam, players);
        CheckAndAddPlayer(row.Player7, row.Tag, newTeam, players);
        CheckAndAddPlayer(row.Player8, row.Tag, newTeam, players);
        CheckAndAddPlayer(row.Player9, row.Tag, newTeam, players);
        CheckAndAddPlayer(row.Player10, row.Tag, newTeam, players);
      }

      return (players.ToArray(), teams.ToArray());
    }

    public void Save(IEnumerable<Player> players, IEnumerable<Team> teams)
    {
      throw new InvalidOperationException("This is a JSON Reader only");
    }
  }
}