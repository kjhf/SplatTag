using Newtonsoft.Json;
using SplatTagCore.Social;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SplatTagCore
{
  [Serializable]
  public class Player
  {
    public static readonly Regex DISCORD_NAME_REGEX = new Regex(@"\(?.*#[0-9]{4}\)?", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

    [JsonProperty("Id", Required = Required.Always)]
    /// <summary>
    /// The database Id of the player.
    /// </summary>
    public readonly Guid Id = Guid.NewGuid();

    /// <summary>
    /// Back-store for player's Battlefy information.
    /// </summary>
    private readonly List<Battlefy> battlefy = new List<Battlefy>();

    /// <summary>
    /// Back-store for player's Discord information.
    /// </summary>
    private readonly Discord discord = new Discord();

    /// <summary>
    /// Back-store for player's FCs.
    /// </summary>
    private readonly List<FriendCode> friendCodes = new List<FriendCode>();

    /// <summary>
    /// Back-store for the names of this player. The first element is the current name.
    /// </summary>
    private readonly List<Name> names = new List<Name>();

    /// <summary>
    /// Back-store for the Sendou Profiles of this player.
    /// </summary>
    private readonly List<Sendou> sendouProfiles = new List<Sendou>();

    /// <summary>
    /// Back-store for the sources of this player.
    /// </summary>
    private readonly List<Source> sources = new List<Source>();

    /// <summary>
    /// Back-store for the team GUIDs for this player. The first element is the current team.
    /// No team represented by <see cref="Team.NoTeam.Id"/>.
    /// </summary>
    private readonly List<Guid> teams = new List<Guid>();

    /// <summary>
    /// Back-store for the Twitch Profiles of this player.
    /// </summary>
    private readonly List<Twitch> twitchProfiles = new List<Twitch>();

    /// <summary>
    /// Back-store for the Twitter Profiles of this player.
    /// </summary>
    private readonly List<Twitter> twitterProfiles = new List<Twitter>();

    /// <summary>
    /// Back-store for the weapons that the player uses (if any).
    /// </summary>
    private readonly List<string> weapons = new List<string>();

    /// <summary>
    /// Default construct a player
    /// </summary>
    public Player()
    {
    }

    /// <summary>
    /// Construct a player with their name and source
    /// </summary>
    /// <param name="ign"></param>
    /// <param name="source"></param>
    public Player(string ign, Source source)
    {
      this.names.Add(new Name(ign, source));
      this.sources.Add(source);
    }

    /// <summary>
    /// Construct a player with their name, teams, and source
    /// </summary>
    /// <param name="ign"></param>
    /// <param name="source"></param>
    public Player(string ign, IEnumerable<Guid> teams, Source source)
    {
      this.names.Add(new Name(ign, source));
      this.teams.AddRange(teams);
      this.sources.Add(source);
    }

    [JsonProperty("Battlefy", Required = Required.Default)]
    /// <summary>
    /// Get the player's Battlefy profile details. This iterates over Battlefy slugs if used as a <see cref="SplatTagCore.Name"/> class.
    /// </summary>
    public IReadOnlyList<Battlefy> Battlefy => battlefy;

    [JsonIgnore]
    /// <summary>
    /// Get the player's Battlefy Usernames.
    /// </summary>
    public IReadOnlyList<string> BattlefyUsernames => battlefy.SelectMany(bf => bf.Usernames).Distinct().ToArray();

    [JsonProperty("Country", Required = Required.Default)]
    /// <summary>
    /// Get or Set the Country.
    /// Null by default.
    /// </summary>
    public string? Country { get; set; }

    [JsonIgnore]
    /// <summary>
    /// Get the emoji flag of the <see cref="Country"/> specified.
    /// </summary>
    public string? CountryFlag
    {
      get
      {
        if (Country == null) return null;
        return string.Concat(Country.ToUpper().Select(x => char.ConvertFromUtf32(x + 0x1F1A5)));
      }
    }

    [JsonIgnore]
    /// <summary>
    /// The current team id this player plays for, or <see cref="Team.NoTeam.Id"/> if not set.
    /// </summary>
    public Guid CurrentTeam => teams.Count > 0 ? teams[0] : Team.NoTeam.Id;

    [JsonProperty("Discord", Required = Required.Default)]
    /// <summary>
    /// Get the player's Discord profile details.
    /// </summary>
    public Discord Discord => discord;

    [JsonProperty("DiscordId", Required = Required.Default)]
    /// <summary>
    /// The last known Discord Id of the player. Returns null if none.
    /// </summary>
    public string? DiscordId => Discord.Ids.FirstOrDefault()?.Value;

    [JsonProperty("DiscordName", Required = Required.Default)]
    /// <summary>
    /// The last known Discord name of the player. Returns null if none.
    /// </summary>
    public string? DiscordName => Discord.Usernames.FirstOrDefault()?.Value;

    [JsonProperty("FriendCode", Required = Required.Default)]
    /// <summary>
    /// Get the Friend Code. Returns <see cref="FriendCode.NO_FRIEND_CODE"/> if none.
    /// </summary>
    public FriendCode FC => friendCodes.Count > 0 ? friendCodes[0] : FriendCode.NO_FRIEND_CODE;

    [JsonIgnore]
    /// <summary>
    /// The last known used name for the player
    /// </summary>
    public Name Name => names.Count > 0 ? names[0] : Builtins.UnknownPlayerName;

    [JsonProperty("Names", Required = Required.Always)]
    /// <summary>
    /// The names this player is known by.
    /// </summary>
    public IReadOnlyList<Name> Names => names;

    [JsonIgnore]
    /// <summary>
    /// The old teams this player has played for.
    /// </summary>
    public IReadOnlyList<Guid> OldTeams => teams.Skip(1).ToArray();

    [JsonProperty("Sendou", Required = Required.Default)]
    /// <summary>
    /// Get the player's Sendou profile details.
    /// </summary>
    public IReadOnlyList<Sendou> SendouProfiles => sendouProfiles;

    [JsonProperty("Sources", Required = Required.Default)]
    /// <summary>
    /// Get or Set the current sources that make up this Player instance.
    /// </summary>
    public IReadOnlyList<Source> Sources => sources;

    [JsonProperty("SplatnetId", Required = Required.Default)]
    /// <summary>
    /// The Splatnet database Id of the player (a hex string).
    /// Null by default.
    /// </summary>
    public string? SplatnetId { get; set; }

    [JsonProperty("Teams", Required = Required.Always)]
    /// <summary>
    /// The teams this player is played for.
    /// No Team represented by <see cref="Team.NoTeam.Id"/>
    /// </summary>
    public IReadOnlyList<Guid> Teams => teams.ToArray();

    [JsonIgnore]
    /// <summary>
    /// The Names of this Player transformed.
    /// </summary>
    public IEnumerable<string> TransformedNames => Names.Select(n => n.TransformedName);

    [JsonProperty("Top500", Required = Required.Default)]
    /// <summary>
    /// Get or Set Top 500 flag.
    /// False by default.
    /// </summary>
    public bool Top500 { get; set; }

    [JsonProperty("Twitch", Required = Required.Default)]
    /// <summary>
    /// Get the player's Twitch profile details.
    /// </summary>
    public IReadOnlyList<Twitch> Twitch => twitchProfiles;

    [JsonProperty("Twitter", Required = Required.Default)]
    /// <summary>
    /// Get the player's Twitter profile details.
    /// </summary>
    public IReadOnlyList<Twitter> Twitter => twitterProfiles;

    [JsonProperty("Weapons", Required = Required.Default)]
    /// <summary>
    /// The weapons this player uses.
    /// </summary>
    public IReadOnlyList<string> Weapons => weapons;

    public void AddBattlefyInformation(string slug, string username, Source source)
    {
      SplatTagCommon.AddBattlefy(slug, username.AsEnumerable(), source.AsEnumerable(), battlefy);
    }

    public void AddBattlefyInformation(IEnumerable<Battlefy> value)
    {
      foreach (var bf in value)
      {
        SplatTagCommon.AddBattlefy(bf.Value, bf.Usernames, bf.Sources, battlefy);
      }
    }

    public void AddDiscord(Discord value)
    {
      Discord.AddIds(value.Ids);
      Discord.AddUsernames(value.Usernames);
    }

    public void AddDiscordId(string id, Source source)
    {
      Discord.AddIds(new Name(id, source).AsEnumerable());
    }

    public void AddDiscordName(string username, Source source)
    {
      Discord.AddUsernames(new Name(username, source).AsEnumerable());
    }

    public void AddFCs(IEnumerable<FriendCode> value)
    {
      if (value != null)
      {
        foreach (FriendCode fc in value)
        {
          if (fc != FriendCode.NO_FRIEND_CODE)
          {
            if (friendCodes.Count == 0)
            {
              friendCodes.Add(fc);
            }
            else if (friendCodes[0].Equals(value))
            {
              // Nothing to do.
            }
            else
            {
              friendCodes.Remove(fc);
              friendCodes.Insert(0, fc);
            }
          }
        }
      }
    }

    public void AddName(string name, Source source)
    {
      SplatTagCommon.AddName(name, source, names);
    }

    public void AddNames(IEnumerable<Name> value)
    {
      SplatTagCommon.AddNames(value, names);
    }

    public void AddSendou(string handle, Source source)
    {
      SplatTagCommon.AddName(handle, source, sendouProfiles);
    }

    public void AddSendou(IEnumerable<Sendou> value)
    {
      SplatTagCommon.AddNames(value, sendouProfiles);
    }

    public void AddSources(IEnumerable<Source> value)
    {
      SplatTagCommon.AddSources(value, sources);
    }

    public void AddTeams(IEnumerable<Guid> value)
    {
      SplatTagCommon.AddIds(value, teams);
    }

    public void AddTwitch(string handle, Source source)
    {
      SplatTagCommon.AddName(handle, source, twitchProfiles);
    }

    public void AddTwitch(IEnumerable<Twitch> value)
    {
      SplatTagCommon.AddNames(value, twitchProfiles);
    }

    public void AddTwitter(string handle, Source source)
    {
      SplatTagCommon.AddName(handle, source, twitterProfiles);
    }

    public void AddTwitter(IEnumerable<Twitter> value)
    {
      SplatTagCommon.AddNames(value, twitterProfiles);
    }

    public void AddWeapons(IEnumerable<string> value)
    {
      SplatTagCommon.AddStrings(value, weapons);
    }

    /// <summary>
    /// Correct the team ids for this player given a merge result (containing old team id --> new id)
    /// </summary>
    public void CorrectTeamIds(IDictionary<Guid, Guid> teamsMergeResult)
    {
      // Reverse order to reduce Remove impact
      for (int i = teams.Count - 1; i >= 0; i--)
      {
        if (teamsMergeResult.ContainsKey(teams[i]))
        {
          var correctedId = teamsMergeResult[teams[i]];

          // If the id already exists in this list, remove this entry.
          if (teams.Contains(correctedId))
          {
            teams.RemoveAt(i);
          }
          else
          {
            // Otherwise set the new id.
            teams[i] = correctedId;
          }
        }
      }
    }

    /// <summary>
    /// Merge this player with another (newer) player instance
    /// </summary>
    /// <param name="newerPlayer">The new import record</param>
    /// <exception cref="ArgumentNullException"><paramref name="newerPlayer"/> is <c>null</c>.</exception>
    public void Merge(Player newerPlayer)
    {
      if (newerPlayer == null) throw new ArgumentNullException(nameof(newerPlayer));
      if (ReferenceEquals(this, newerPlayer)) return;

      // Merge the teams.
      AddTeams(newerPlayer.teams);

      // Merge the player's name(s).
      AddNames(newerPlayer.names);

      // Merge the sources.
      AddSources(newerPlayer.sources);

      // Merge the weapons.
      AddWeapons(newerPlayer.weapons);

      // Merge the Battlefy Slugs and usernames.
      AddBattlefyInformation(newerPlayer.battlefy);

      // Merge the Discord Slugs and usernames.
      AddDiscord(newerPlayer.discord);

      // Merge the Social Data.
      AddSendou(newerPlayer.SendouProfiles);
      AddTwitch(newerPlayer.twitchProfiles);
      AddTwitter(newerPlayer.twitterProfiles);

      // Merge the misc data
      AddFCs(newerPlayer.friendCodes);

      if (!string.IsNullOrWhiteSpace(newerPlayer.Country))
      {
        this.Country = newerPlayer.Country;
      }

      if (newerPlayer.Top500)
      {
        this.Top500 = true;
      }
    }

    /// <summary>
    /// Overridden ToString.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return Name.Value;
    }
  }
}