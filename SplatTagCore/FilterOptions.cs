using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SplatTagCore
{
  [Flags]
  public enum FilterOptions
  {
    None = 0,

    /// <summary>
    /// The query can specify a Player IGN or alias
    /// </summary>
    PlayerName = 1,

    /// <summary>
    /// The query can specify a friend code
    /// </summary>
    FriendCode = 1 << 1,

    /// <summary>
    /// The query can specify a Discord name
    /// </summary>
    DiscordName = 1 << 2,

    /// <summary>
    /// The query can specify a Clan (Team) Tag
    /// </summary>
    ClanTag = 1 << 3,

    /// <summary>
    /// The query can specify a tournament or other source
    /// </summary>
    Sources = 1 << 4,

    /// <summary>
    /// The query can specify a Twitter handle
    /// </summary>
    Twitter = 1 << 5,

    /// <summary>
    /// The query can specify a Twitch handle
    /// </summary>
    Twitch = 1 << 6,

    /// <summary>
    /// The query can specify a Battlefy Slug
    /// </summary>
    BattlefySlugs = 1 << 7,

    /// <summary>
    /// The query can specify a Battlefy username
    /// </summary>
    BattlefyUsername = 1 << 8,

    /// <summary>
    /// The query can specify a Battlefy persistent id
    /// </summary>
    BattlefyPersistentIds = 1 << 9,

    /// <summary>
    /// The query can specify a Discord id
    /// </summary>
    DiscordId = 1 << 10,

    /// <summary>
    /// The query can specify an internal Slapp id
    /// </summary>
    SlappId = 1 << 11,

    /// <summary>
    /// The query can specify a Team IGN or alias
    /// </summary>
    TeamName = 1 << 12,

    /// <summary>
    /// The query can specify a Player Sendou handle
    /// </summary>
    PlayerSendou = 1 << 13,

    /// <summary>
    /// The query can specify a weapon
    /// </summary>
    Weapon = 1 << 14,

    /// <summary>
    /// The query can specify a team (team name or clan tag)
    /// </summary>
    Team = (TeamName | ClanTag | SlappId),

    /// <summary>
    /// The query can specify a player (relevant details)
    /// </summary>
    Player = (PlayerName | FriendCode | DiscordName | Twitter | Twitch | PlayerSendou | BattlefySlugs | BattlefyUsername | BattlefyPersistentIds | DiscordId | SlappId),

    /// <summary> Default search </summary>
    /// <remarks>Omits Sources</remarks>
    Default = (Player | Team),

    /// <summary> Persistent search </summary>
    /// <remarks>This is the information we can safely merge records together with.
    /// FC, Discord Name, and BattlefySlugs are not here because people sometimes enter their captain's info into their own account ...</remarks>
    Persistent = (Twitter | Twitch | PlayerSendou | BattlefyPersistentIds | DiscordId)
  }

  public static class FilterOptionWeights
  {
    public static readonly ReadOnlyDictionary<FilterOptions, int> Weight = InitialiseWeights();
    public const int MergableThresholdWeight = 51;

    private static ReadOnlyDictionary<FilterOptions, int> InitialiseWeights()
    {
      Dictionary<FilterOptions, int> weights = new();
      foreach (FilterOptions option in Enum.GetValues(typeof(FilterOptions)))
      {
        switch (option)
        {
          case FilterOptions.None:
            weights.Add(option, 0);
            break;

          case FilterOptions.PlayerName:
            weights.Add(option, 30);
            break;

          case FilterOptions.FriendCode:
            weights.Add(option, 10000);
            break;

          case FilterOptions.DiscordName:
            weights.Add(option, 10);
            break;

          case FilterOptions.ClanTag:
            weights.Add(option, 2);
            break;

          case FilterOptions.Sources:
            weights.Add(option, 1);
            break;

          case FilterOptions.Twitter:
            weights.Add(option, 100);
            break;

          case FilterOptions.Twitch:
            weights.Add(option, 200);
            break;

          case FilterOptions.BattlefySlugs:
            weights.Add(option, 50000);
            break;

          case FilterOptions.BattlefyUsername:
            weights.Add(option, 10000);
            break;

          case FilterOptions.BattlefyPersistentIds:
            weights.Add(option, 100000);
            break;

          case FilterOptions.DiscordId:
            weights.Add(option, 100000);
            break;

          case FilterOptions.SlappId:
            weights.Add(option, ushort.MaxValue);
            break;

          case FilterOptions.TeamName:
            weights.Add(option, 50);
            break;

          case FilterOptions.PlayerSendou:
            weights.Add(option, 10);
            break;

          case FilterOptions.Weapon:
            weights.Add(option, 1);
            break;

          case FilterOptions.Team:
          case FilterOptions.Player:
            weights.Add(option, short.MaxValue - 1);
            break;

          case FilterOptions.Default:
          case FilterOptions.Persistent:
            weights.Add(option, 0);
            break;

          default:
            throw new NotImplementedException("InitialiseWeights - Please implement " + option);
        }
      }
      return new(weights);
    }

    public static int ToWeight(this FilterOptions options)
    {
      return Enum.GetValues(typeof(FilterOptions))!
        .Cast<FilterOptions>()
        .Where(o => options.HasFlag(o))
        .Sum(o => Weight[o]);
    }
  }
}