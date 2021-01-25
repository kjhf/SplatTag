using System;

namespace SplatTagCore
{
  [Flags]
  public enum FilterOptions
  {
    None = 0,

    /// <summary>
    /// The query can specify a name
    /// </summary>
    Name = 0x1,

    /// <summary>
    /// The query can specify a friend code
    /// </summary>
    FriendCode = 0x2,

    /// <summary>
    /// The query can specify a Discord name
    /// </summary>
    DiscordName = 0x4,

    /// <summary>
    /// The query can specify a Clan (Team) Tag
    /// </summary>
    ClanTag = 0x8,

    /// <summary>
    /// The query can specify a tournament or other source
    /// </summary>
    Sources = 0x10,

    /// <summary>
    /// The query can specify a Twitter handle
    /// </summary>
    Twitter = 0x20,

    /// <summary>
    /// The query can specify a Twitch handle
    /// </summary>
    Twitch = 0x40,

    /// <summary>
    /// The query can specify a Battlefy Slug
    /// </summary>
    BattlefySlugs = 0x80,

    /// <summary>
    /// The query can specify a Battlefy username
    /// </summary>
    BattlefyUsername = 0x100,

    /// <summary>
    /// The query can specify a Battlefy persistent id
    /// </summary>
    BattlefyPersistentIds = 0x200,

    /// <summary>
    /// The query can specify a Discord id
    /// </summary>
    DiscordId = 0x400,

    /// <summary> Default search </summary>
    /// <remarks>Omits Sources</remarks>
    Default = (Name | FriendCode | DiscordName | ClanTag | Twitter | Twitch | BattlefySlugs | BattlefyUsername | BattlefyPersistentIds | DiscordId),

    /// <summary> Persistent search </summary>
    /// <remarks>Omits Sources, Clan Tags, and non-persistent names</remarks>
    Persistent = (FriendCode | Twitter | Twitch | BattlefySlugs | BattlefyPersistentIds | DiscordId)
  }
}