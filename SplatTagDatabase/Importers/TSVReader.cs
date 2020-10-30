using Newtonsoft.Json;
using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SplatTagDatabase.Importers
{
  internal class TSVReader : IImporter
  {
    private enum PropertyEnum
    {
      UNKNOWN = -1,
      Team_Offset = 0,
      TeamName = 1,
      Tag = 2,
      LUTIDivision = 3,

      UnspecifiedPlayer_Offset = 10,
      Name,
      FC,
      DiscordName,
      DiscordId,
      Twitch,
      Twitter,
      Country,
      Role,

      /// <summary>
      /// Player N offset, where N = 1, 2, 3... to give 100, 200, 300 ...
      /// </summary>
      PlayerN_Offset = 100,
    }

    private readonly string tsvFile;

    private static readonly ReadOnlyDictionary<string, PropertyEnum> propertyValueStringMap = new ReadOnlyDictionary<string, PropertyEnum>(new Dictionary<string, PropertyEnum>()
    {
      { "team", PropertyEnum.TeamName },
      { "teamname", PropertyEnum.TeamName },
      { "tag", PropertyEnum.Tag },
      { "teamtag", PropertyEnum.Tag },
      { "div", PropertyEnum.LUTIDivision },
      { "teamdiv", PropertyEnum.LUTIDivision },
      { "division", PropertyEnum.LUTIDivision },
      { "teamdivision", PropertyEnum.LUTIDivision },
      { "lutidivision", PropertyEnum.LUTIDivision },
      { "teamlutidivision", PropertyEnum.LUTIDivision },
      { "name", PropertyEnum.Name },
      { "playername", PropertyEnum.Name },
      { "player", PropertyEnum.Name },
      { "ign", PropertyEnum.Name },
      { "playerign", PropertyEnum.Name },
      { "fc", PropertyEnum.FC },
      { "playerfc", PropertyEnum.FC },
      { "friend", PropertyEnum.FC },
      { "playerfriend", PropertyEnum.FC },
      { "friendcode", PropertyEnum.FC },
      { "playerfriendcode", PropertyEnum.FC },
      { "sw", PropertyEnum.FC },
      { "playersw", PropertyEnum.FC },
      { "switchcode", PropertyEnum.FC },
      { "playerswitchcode", PropertyEnum.FC },
      { "switchfriendcode", PropertyEnum.FC },
      { "playerswitchfriendcode", PropertyEnum.FC },
      { "discord", PropertyEnum.DiscordName },
      { "playerdiscord", PropertyEnum.DiscordName },
      { "discordname", PropertyEnum.DiscordName },
      { "playerdiscordname", PropertyEnum.DiscordName },
      { "discordid", PropertyEnum.DiscordId },
      { "playerdiscordid", PropertyEnum.DiscordId },
      { "twitch", PropertyEnum.Twitch },
      { "playertwitch", PropertyEnum.Twitch },
      { "twitchname", PropertyEnum.Twitch },
      { "playertwitchname", PropertyEnum.Twitch },
      { "twitter", PropertyEnum.Twitter },
      { "playertwitter", PropertyEnum.Twitter },
      { "twittername", PropertyEnum.Twitter },
      { "playertwittername", PropertyEnum.Twitter },
      { "country", PropertyEnum.Country },
      { "playercountry", PropertyEnum.Country },
      { "role", PropertyEnum.Role },
      { "playerrole", PropertyEnum.Role },
      { "weapons", PropertyEnum.Role },
      { "playerweapons", PropertyEnum.Role },

      // Special captain handling
      { "captain", (int)PropertyEnum.PlayerN_Offset + PropertyEnum.Name }
    });

    public TSVReader(string tsvFile)
    {
      this.tsvFile = tsvFile ?? throw new ArgumentNullException(nameof(tsvFile));
    }

    public (Player[], Team[]) Load()
    {
      if (tsvFile == null)
      {
        throw new InvalidOperationException(nameof(tsvFile) + " is not set.");
      }

      Debug.WriteLine("Loading " + tsvFile);
      string[] text = File.ReadAllLines(tsvFile);
      if (text.Length < 1)
      {
        throw new ArgumentException("TSV does not have any data. Check format of spreadsheet.");
      }

      string headerRow = text[0];
      string[] columns = headerRow.Split('\t').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
      int numberOfHeaders = columns.Length;

      // Build a map of the incoming data
      PropertyEnum[] resolved = new PropertyEnum[numberOfHeaders];
      for (int i = 0; i < numberOfHeaders; ++i)
      {
        string header = columns[i].ToLowerInvariant()
          .Replace(" ", "")
          .Replace("_", "")
          .Replace("'s", "")
          .Replace("teamcaptain", "captain")
          .Replace("teammember", "player")
          ;
        if (string.IsNullOrWhiteSpace(header))
        {
          Trace.WriteLine($"Warning: Unable to resolve header, it is blank after processing: \"{header}\", (was \"{columns[i]}\")");
          resolved[i] = PropertyEnum.UNKNOWN;
        }
        else if (propertyValueStringMap.ContainsKey(header))
        {
          resolved[i] = propertyValueStringMap[header];
        }
        else
        {
          int playerNum = 0;
          if (header.Contains("captain"))
          {
            playerNum = 1;
            header = header.Replace("captain", "");
          }
          else
          {
            for (playerNum = 10; playerNum > 0; --playerNum)
            {
              if (header.Contains(playerNum.ToString()))
              {
                header = header.Replace(playerNum.ToString(), "");
                break;
              }
            }
          }

          if (playerNum == 0)
          {
            resolved[i] = PropertyEnum.UNKNOWN;
            Trace.WriteLine("Warning: Unable to resolve header: " + header);
          }
          else
          {
            if (propertyValueStringMap.ContainsKey(header))
            {
              resolved[i] = (playerNum * (int)PropertyEnum.PlayerN_Offset) + propertyValueStringMap[header];
            }
            else
            {
              resolved[i] = PropertyEnum.UNKNOWN;
              Trace.WriteLine("Warning: Unable to resolve header for player " + playerNum + ": " + header);
            }
          }
        }
      }

      // Now read.
      List<Team> teams = new List<Team>();
      List<Player> players = new List<Player>();

      for (int lineIndex = 1; lineIndex < text.Length; ++lineIndex)
      {
        string line = text[lineIndex];
        if (!line.Contains('\t')) continue;

        string[] cells = line.Split('\t').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        if (numberOfHeaders < cells.Length)
        {
          Trace.WriteLine($"Warning: Skipping line {lineIndex}, the number of cells in this row {cells.Length} is greater than the number of headers {numberOfHeaders}.");
          Debug.WriteLine(line);
          continue;
        }

        SortedDictionary<int, Player> rowPlayers = new SortedDictionary<int, Player>();
        Team t = new Team
        {
          Id = -teams.Count - 1,  // This will be updated when the merge happens.
          ClanTagOption = TagOption.Unknown,
          Div = new Division(),
          Name = "(unknown)",
          Sources = new string[] { Path.GetFileNameWithoutExtension(tsvFile) }
        };

        for (int i = 0; i < cells.Length && i < numberOfHeaders; ++i)
        {
          int playerNum = 0;
          PropertyEnum resolvedProperty = resolved[i];
          if (resolvedProperty > PropertyEnum.PlayerN_Offset)
          {
            playerNum = (int)resolved[i] / (int)PropertyEnum.PlayerN_Offset;
            resolvedProperty = (PropertyEnum)((int)resolved[i] % (int)PropertyEnum.PlayerN_Offset);
          }

          string value = cells[i];
          if (string.IsNullOrWhiteSpace(value))
          {
            continue;
          }

          switch (resolvedProperty)
          {
            case PropertyEnum.UNKNOWN:
            {
              continue;
            }

            case PropertyEnum.Country:
            {
              var p = GetCurrentPlayer(ref rowPlayers, playerNum, tsvFile);
              p.Country = value;
              break;
            }

            case PropertyEnum.DiscordId:
            {
              var p = GetCurrentPlayer(ref rowPlayers, playerNum, tsvFile);
              if (ulong.TryParse(value, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out ulong parsedId))
              {
                p.DiscordId = parsedId;
              }
              else
              {
                Trace.WriteLine($"Warning: DiscordId was specified ({lineIndex},{i}), but the value could not be parsed from a hex string. {value}.");
                Debug.WriteLine(line);
              }
              break;
            }

            case PropertyEnum.DiscordName:
            {
              var p = GetCurrentPlayer(ref rowPlayers, playerNum, tsvFile);
              if (Player.DISCORD_NAME_REGEX.IsMatch(value))
              {
                p.DiscordName = value;
              }
              else
              {
                Trace.WriteLine($"Warning: DiscordName was specified ({lineIndex},{i}), but the value was not in a Discord format of name#0000. {value}.");
                Debug.WriteLine(line);
              }
              break;
            }

            case PropertyEnum.FC:
            {
              var p = GetCurrentPlayer(ref rowPlayers, playerNum, tsvFile);
              if (FriendCode.TryParse(value, out FriendCode friendCode))
              {
                p.FriendCode = friendCode.ToString();
              }
              else
              {
                Trace.WriteLine($"Warning: FC was specified ({lineIndex},{i}), but the value was not in an FC format of 0000-0000-0000 or 0000 0000 0000. {value}.");
                Debug.WriteLine(line);
              }
              break;
            }

            case PropertyEnum.LUTIDivision:
            {
              t.Div = new Division(value);
              break;
            }

            case PropertyEnum.Name:
            {
              var p = GetCurrentPlayer(ref rowPlayers, playerNum, tsvFile);
              p.Name = value;
              break;
            }

            case PropertyEnum.Role:
            {
              var p = GetCurrentPlayer(ref rowPlayers, playerNum, tsvFile);
              p.Weapons = value.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s));
              break;
            }

            case PropertyEnum.Tag:
            {
              t.ClanTags = new string[] { value };
              break;
            }

            case PropertyEnum.TeamName:
            {
              t.Name = value;
              break;
            }

            case PropertyEnum.Twitter:
            {
              var p = GetCurrentPlayer(ref rowPlayers, playerNum, tsvFile);
              p.Names = p.Names.Concat(new[] { value });
              p.Twitter = value;
              break;
            }

            case PropertyEnum.Twitch:
            {
              var p = GetCurrentPlayer(ref rowPlayers, playerNum, tsvFile);
              p.Names = p.Names.Concat(new[] { value });
              p.Twitch = value;
              break;
            }

            case PropertyEnum.Team_Offset:
            case PropertyEnum.UnspecifiedPlayer_Offset:
            case PropertyEnum.PlayerN_Offset:
            default:
            {
              Trace.WriteLine($"Warning: Unhandled header {resolvedProperty}. {value}. Line {lineIndex}:");
              Debug.WriteLine(line);
              break;
            }
          }
        }

        // End of the row, add the data.
        foreach (var pair in rowPlayers)
        {
          Player p = pair.Value;
          p.CurrentTeam = t.Id;
          players.Add(p);
        }
        teams.Add(t);
      }

      return (players.ToArray(), teams.ToArray());
    }

    private Player GetCurrentPlayer(ref SortedDictionary<int, Player> rowPlayers, int playerNum, string tsvFile)
    {
      if (rowPlayers.ContainsKey(playerNum))
      {
        return rowPlayers[playerNum];
      }
      else
      {
        Player p = new Player
        {
          Sources = new string[] { Path.GetFileNameWithoutExtension(tsvFile) }
        };
        rowPlayers.Add(playerNum, p);
        return p;
      }
    }

    public static bool AcceptsInput(string input)
    {
      return Path.GetExtension(input).Equals(".tsv", StringComparison.OrdinalIgnoreCase);
    }
  }
}