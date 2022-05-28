using NLog;
using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SplatTagDatabase.Importers
{
  internal class TSVReader : IImporter
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private static readonly ReadOnlyDictionary<string, PropertyEnum> propertyValueStringMap = new ReadOnlyDictionary<string, PropertyEnum>(new Dictionary<string, PropertyEnum>()
    {
      { "team", PropertyEnum.TeamName },
      { "teamname", PropertyEnum.TeamName },
      { "tag", PropertyEnum.Tag },
      { "teamtag", PropertyEnum.Tag },
      { "div", PropertyEnum.Div },
      { "teamdiv", PropertyEnum.Div },
      { "division", PropertyEnum.Div },
      { "teamdivision", PropertyEnum.Div },
      { "lutidiv", PropertyEnum.LUTIDiv },
      { "lutidivision", PropertyEnum.LUTIDiv },
      { "teamlutidivision", PropertyEnum.LUTIDiv },
      { "ebtvdiv", PropertyEnum.EBTVDiv },
      { "ebtvdivision", PropertyEnum.EBTVDiv },
      { "teamebtvdivision", PropertyEnum.EBTVDiv },
      { "dsbdiv", PropertyEnum.EBTVDiv },
      { "dsbdivision", PropertyEnum.DSBDiv },
      { "teamdsbdivision", PropertyEnum.DSBDiv },
      { "name", PropertyEnum.Name },
      { "playername", PropertyEnum.Name },
      { "player", PropertyEnum.Name },
      { "ign", PropertyEnum.Name },
      { "playerign", PropertyEnum.Name },
      { "ingamename", PropertyEnum.Name },
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
      { "discordservernickname", PropertyEnum.Name },  // Importantly should come before "discord" which we'll assume is the discord name in user#0000 format. Nickname is not.
      { "discordnickname", PropertyEnum.Name },
      { "discord", PropertyEnum.DiscordName },
      { "playerdiscord", PropertyEnum.DiscordName },
      { "discordname", PropertyEnum.DiscordName },
      { "discordtag", PropertyEnum.DiscordName },
      { "playerdiscordname", PropertyEnum.DiscordName },
      { "playerdiscordtag", PropertyEnum.DiscordName },
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
      { "pronoun", PropertyEnum.Pronouns },
      { "playerpronoun", PropertyEnum.Pronouns },
      { "pronouns", PropertyEnum.Pronouns },
      { "playerpronouns", PropertyEnum.Pronouns },
      { "timestamp", PropertyEnum.Timestamp },

      // Special captain handling
      { "cap", (int)PropertyEnum.PlayerN_Offset + PropertyEnum.Name },
      { "captain", (int)PropertyEnum.PlayerN_Offset + PropertyEnum.Name }
    });

    private readonly DivType divType;

    private readonly string season;

    private readonly Source source;

    private readonly string tsvFile;

    public TSVReader(string tsvFile)
    {
      this.tsvFile = tsvFile ?? throw new ArgumentNullException(nameof(tsvFile));
      string fileName = Path.GetFileNameWithoutExtension(tsvFile);
      this.source = new Source(fileName);
      this.season = fileName;
      this.divType = DivType.Unknown;

      // Try and calculate Div Type and season
      foreach (DivType type in Enum.GetValues(typeof(DivType)))
      {
        if (fileName.Contains(type.ToString(), StringComparison.OrdinalIgnoreCase))
        {
          divType = type;
          season = fileName.Substring(fileName.IndexOf(type.ToString()) + type.ToString().Length).Trim('-');
          break;
        }
      }
    }

    private enum PropertyEnum
    {
      UNKNOWN = -1,
      Team_Offset = 0,
      TeamName = 1,
      Tag = 2,
      Div = 3,
      LUTIDiv = 4,
      EBTVDiv = 5,
      DSBDiv = 6,
      Timestamp = 7,

      UnspecifiedPlayer_Offset = 10,
      Name,
      FC,
      DiscordName,
      DiscordId,
      Twitch,
      Twitter,
      Country,
      Role,
      Pronouns,

      /// <summary>
      /// Player N offset, where N = 1, 2, 3... to give 100, 200, 300 ...
      /// </summary>
      PlayerN_Offset = 100,
    }

    public static bool AcceptsInput(string input)
    {
      return Path.GetExtension(input).Equals(".tsv", StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
      return obj is TSVReader reader &&
             source.Equals(reader.source);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(nameof(TSVReader), source);
    }

    public Source Load()
    {
      logger.ConditionalDebug("Loading " + tsvFile);
      string[] text = File.ReadAllLines(tsvFile);
      if (text.Length < 1)
      {
        throw new ArgumentException("TSV does not have any data. Check format of spreadsheet.");
      }

      // Build a map of the incoming data
      string headerRow = text[0];
      string[] columns = headerRow.Split('\t').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
      int numberOfHeaders = columns.Length;

      PropertyEnum[] resolved = new PropertyEnum[numberOfHeaders];
      for (int i = 0; i < numberOfHeaders; ++i)
      {
        string header = columns[i].ToLowerInvariant()
          .Replace("your ", "")
          .Replace(" ", "")
          .Replace("_", "")
          .Replace(":", "")
          .Replace("-", "")
          .Replace("'s", "")
          .Replace("teamcaptain", "captain")
          .Replace("teammember", "player")
          ;
        if (string.IsNullOrWhiteSpace(header))
        {
          logger.Warn($"Unable to resolve header, it is blank after processing: \"{header}\", (was \"{columns[i]}\")");
          resolved[i] = PropertyEnum.UNKNOWN;
        }
        // Quick case
        else if (propertyValueStringMap.ContainsKey(header))
        {
          resolved[i] = propertyValueStringMap[header];
        }
        else
        {
          // Need to do some searching
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

          var matchedKey = propertyValueStringMap.Keys.FirstOrDefault(key => header.StartsWith(key));
          if (matchedKey != null)
          {
            // For player num = 0 (not found), this will just return the appropriate header.
            resolved[i] = (playerNum * (int)PropertyEnum.PlayerN_Offset) + propertyValueStringMap[matchedKey];
          }
          else if (playerNum == 0)
          {
            resolved[i] = PropertyEnum.UNKNOWN;
            logger.Warn("Unable to resolve header: " + header);
          }
          else
          {
            resolved[i] = PropertyEnum.UNKNOWN;
            logger.Warn("Unable to resolve header for player " + playerNum + ": " + header);
          }
        }
      }

      // Now read the data...
      // Most tsv files will be team sheets, but for draft cups and verif they could be player records instead.
      // Each row therefore might be a single team with players, or a single player entry.
      List<Team> teams = new List<Team>();
      List<Player> players = new List<Player>();

      // From 1 as [0] is header row
      for (int lineIndex = 1; lineIndex < text.Length; ++lineIndex)
      {
        string line = text[lineIndex];
        if (!line.Contains('\t')) continue;

        string[] cells = line.Split('\t').ToArray();

        // Warn if the values exceeds the number of defined headers (but don't bother if we're only one over and it's empty -- trailing tab)
        if (numberOfHeaders < cells.Length && (numberOfHeaders != cells.Length - 1 || !string.IsNullOrWhiteSpace(cells[cells.Length - 1])))
        {
          logger.Warn($"Line {lineIndex} contains more cells in this row {cells.Length} than headers {numberOfHeaders}.");
          logger.ConditionalDebug(line);
        }

        SortedDictionary<int, Player> rowPlayers = new SortedDictionary<int, Player>();
        Team t = new Team();

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
              if (value.Length >= 14 && value.Length < 21)
              {
                // First, test is decimal (of length 17+)
                bool isDecimal = value.Length >= 17 && value.All("0123456789".Contains);
                if (isDecimal)
                {
                  p.AddDiscordId(value, source);
                }
                // Otherwise test if we can get from a hex string (of length 14+ to give the correct 17 digit decimal)
                else if (ulong.TryParse(value, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out ulong parsedId))
                {
                  p.AddDiscordId(value, source);
                }
                else
                {
                  logger.Warn($"DiscordId was specified ({lineIndex},{i}), but the value could not be parsed from a decimal or hex string. {value}.");
                  logger.ConditionalDebug(line);
                }
              }
              else
              {
                logger.Warn($"DiscordId was specified ({lineIndex},{i}), but the value length does not fit into a discord id. {value}.");
                logger.ConditionalDebug(line);
              }
              break;
            }

            case PropertyEnum.DiscordName:
            {
              var p = GetCurrentPlayer(ref rowPlayers, playerNum, tsvFile);
              if (DiscordHandler.DISCORD_NAME_REGEX.IsMatch(value))
              {
                p.AddDiscordUsername(value, source);
              }
              else if (FriendCode.TryParse(value, out FriendCode friendCode))
              {
                p.AddFCs(friendCode, source);
                logger.Warn($"This value was declared as a Discord name but looks like a friend code. Bad data formatting? {value} on ({lineIndex},{i}).");
              }
              else
              {
                logger.Warn($"DiscordName was specified ({lineIndex},{i}), but the value was not in a Discord format of name#0000. {value}.");
                logger.ConditionalDebug(line);
              }
              break;
            }

            case PropertyEnum.FC:
            {
              var p = GetCurrentPlayer(ref rowPlayers, playerNum, tsvFile);
              if (FriendCode.TryParse(value, out FriendCode friendCode))
              {
                p.AddFCs(friendCode, source);
              }
              else
              {
                logger.Warn($"FC was specified ({lineIndex},{i}), but the value was not in an FC format of 0000-0000-0000 or 0000 0000 0000. {value}.");
                logger.ConditionalDebug(line);
              }
              break;
            }

            case PropertyEnum.Div:
            {
              t.AddDivision(new Division(value, divType, season), source);

              if (divType == DivType.Unknown)
              {
                logger.Warn($"Div was specified ({lineIndex},{i}), but I don't know what type of division this file represents.");
              }
              break;
            }
            case PropertyEnum.LUTIDiv:
            {
              t.AddDivision(new Division(value, DivType.LUTI, season), source);
              break;
            }
            case PropertyEnum.EBTVDiv:
            {
              t.AddDivision(new Division(value, DivType.EBTV, season), source);
              break;
            }
            case PropertyEnum.DSBDiv:
            {
              t.AddDivision(new Division(value, DivType.DSB, season), source);
              break;
            }
            case PropertyEnum.Timestamp:
            {
              // TODO - not supported right now. In future we could customise the source timestamp for this entry.
              break;
            }

            case PropertyEnum.Name:
            {
              var p = GetCurrentPlayer(ref rowPlayers, playerNum, tsvFile);

              var playerName = value.Trim();
              playerName = t.Tag?.StripFromPlayer(playerName) ?? playerName;
              p.AddName(playerName, source);

              if (FriendCode.TryParse(value, out FriendCode friendCode))
              {
                p.AddFCs(friendCode, source);
                logger.Warn($"This value was declared as a name but looks like a friend code. Bad data formatting? {value} on ({lineIndex},{i}).");
                logger.ConditionalDebug(line);
              }
              break;
            }

            case PropertyEnum.Role:
            {
              var p = GetCurrentPlayer(ref rowPlayers, playerNum, tsvFile);
              p.AddWeapons(value.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)), source);
              break;
            }

            case PropertyEnum.Pronouns:
            {
              var p = GetCurrentPlayer(ref rowPlayers, playerNum, tsvFile);
              p.AddPronoun(value, source);
              break;
            }

            case PropertyEnum.Tag:
            {
              t.AddClanTag(value, source);
              break;
            }

            case PropertyEnum.TeamName:
            {
              t.AddName(value, source);
              break;
            }

            case PropertyEnum.Twitter:
            {
              var p = GetCurrentPlayer(ref rowPlayers, playerNum, tsvFile);
              p.AddTwitter(value, source);
              break;
            }

            case PropertyEnum.Twitch:
            {
              var p = GetCurrentPlayer(ref rowPlayers, playerNum, tsvFile);
              p.AddTwitch(value, source);
              break;
            }

            case PropertyEnum.Team_Offset:
            case PropertyEnum.UnspecifiedPlayer_Offset:
            case PropertyEnum.PlayerN_Offset:
            default:
            {
              logger.Warn($"Unhandled header {resolvedProperty}. {value}. Line {lineIndex}:");
              logger.ConditionalDebug(line);
              break;
            }
          }
        }

        // End of the row, add the data.
        foreach (var pair in rowPlayers)
        {
          // Don't add empty players
          Player p = pair.Value;
          if (p.Name.Equals(Builtins.UnknownPlayerName))
          {
            continue;
          }
          // Don't add the team to the player if it's not complete (e.g. single player record)
          if (!t.Name.Equals(Builtins.UnknownTeamName))
          {
            p.AddTeams(t.Id, source);
          }
          players.Add(p);
        }

        // Don't bother adding the team if it has no players
        if (players.Count > 0)
        {
          // Don't register a team if that information doesn't exist.
          if (!t.Name.Equals(Builtins.UnknownTeamName))
          {
            // Recalculate the ClanTag layout
            if (t.Tag != null)
            {
              t.Tag.CalculateTagOption(players[0].Name.Value);
            }
            else
            {
              ClanTag? newTag = ClanTag.CalculateTagFromNames(players.Select(p => p.Name.Value).ToArray(), source);

              if (newTag != null)
              {
                t.AddClanTag(newTag);
              }
            }
            teams.Add(t);
          }
        }
      }

      source.Players = players.ToArray();
      source.Teams = teams.ToArray();
      return source;
    }

    private Player GetCurrentPlayer(ref SortedDictionary<int, Player> rowPlayers, int playerNum, string tsvFile)
    {
      if (rowPlayers.ContainsKey(playerNum))
      {
        return rowPlayers[playerNum];
      }
      else
      {
        Player p = new();
        rowPlayers.Add(playerNum, p);
        return p;
      }
    }
  }
}