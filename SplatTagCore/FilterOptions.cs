﻿using System;

namespace SplatTagCore
{
  [Flags]
  public enum FilterOptions
  {
    None = 0,

    /// <summary>
    /// The query can specify a Player IGN or alias
    /// </summary>
    PlayerName = 0x1,

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

    /// <summary>
    /// The query can specify an internal Slapp id
    /// </summary>
    SlappId = 0x800,

    /// <summary>
    /// The query can specify a Team IGN or alias
    /// </summary>
    TeamName = 0x1000,

    /// <summary>
    /// The query can specify a team (team name or clan tag)
    /// </summary>
    Team = (TeamName | ClanTag | SlappId),

    /// <summary>
    /// The query can specify a player (relevant details)
    /// </summary>
    Player = (PlayerName | FriendCode | DiscordName | Twitter | Twitch | BattlefySlugs | BattlefyUsername | BattlefyPersistentIds | DiscordId | SlappId),

    /// <summary> Default search </summary>
    /// <remarks>Omits Sources</remarks>
    Default = (PlayerName | TeamName | FriendCode | DiscordName | ClanTag | Twitter | Twitch | BattlefySlugs | BattlefyUsername | BattlefyPersistentIds | DiscordId | SlappId),

    /// <summary> Persistent search </summary>
    /// <remarks>This is the information we can safely merge records together with.
    /// FC, Discord Name, and BattlefySlugs are not here because people sometimes enter their captain's info into their own account ...</remarks>
    Persistent = (Twitter | Twitch | BattlefyPersistentIds | DiscordId)
  }
}