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
    Name = 1,

    /// <summary>
    /// The query can specify a friend code
    /// </summary>
    FriendCode = 2,

    /// <summary>
    /// The query can specify a Discord name
    /// </summary>
    DiscordName = 4,

    /// <summary>
    /// The query can specify a Clan (Team) Tag
    /// </summary>
    ClanTag = 8,

    /// <summary>
    /// The query can specify a tournament or other source
    /// </summary>
    Sources = 16,

    /// <summary>
    /// The query can specify a Twitter handle
    /// </summary>
    Twitter = 32,

    /// <summary>
    /// The query can specify a Twitch handle
    /// </summary>
    Twitch = 64,

    /// <summary>
    /// The query can specify a Battlefy Slug
    /// </summary>
    BattlefySlugs = 128,

    /// <summary>
    /// Default search
    /// </summary>
    Default = (Name | FriendCode | DiscordName | ClanTag | Twitter | Twitch | BattlefySlugs)
  }
}