using SplatTagCore.Social;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class Player : ISerializable, ISourceable
  {
    /// <summary>
    /// The database Id of the player.
    /// </summary>
    public readonly Guid Id = Guid.NewGuid();

    /// <summary>
    /// Back-store for player's Battlefy information.
    /// </summary>
    private readonly Battlefy battlefy = new Battlefy();

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

    /// <summary>
    /// Get the player's Battlefy profile details. This iterates over Battlefy slugs if used as a <see cref="SplatTagCore.Name"/> class.
    /// </summary>
    public Battlefy Battlefy => battlefy;

    /// <summary>
    /// Get the player's Battlefy Usernames.
    /// </summary>
    public IReadOnlyList<Social.Social> BattlefySlugs => battlefy.Slugs;

    /// <summary>
    /// Get the player's Battlefy Usernames.
    /// </summary>
    public IReadOnlyList<Name> BattlefyUsernames => battlefy.Usernames;

    /// <summary>
    /// Get or Set the Country.
    /// Null by default.
    /// </summary>
    public string? Country { get; set; }

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

    /// <summary>
    /// The current team id this player plays for, or <see cref="Team.NoTeam.Id"/> if not set.
    /// </summary>
    public Guid CurrentTeam => teams.Count > 0 ? teams[0] : Team.NoTeam.Id;

    /// <summary>
    /// Get the player's Discord profile details.
    /// </summary>
    public Discord Discord => discord;

    /// <summary>
    /// The known Discord Ids of the player.
    /// </summary>
    public IReadOnlyList<Name> DiscordIds => Discord.Ids;

    /// <summary>
    /// The known Discord usernames of the player.
    /// </summary>
    public IReadOnlyList<Name> DiscordNames => Discord.Usernames;

    /// <summary>
    /// The known Friend Codes of the player.
    /// </summary>
    public IReadOnlyList<FriendCode> FriendCodes => friendCodes;

    /// <summary>
    /// The last known used name for the player
    /// </summary>
    public Name Name => names.Count > 0 ? names[0] : Builtins.UnknownPlayerName;

    /// <summary>
    /// The names this player is known by.
    /// </summary>
    public IReadOnlyList<Name> Names => names;

    /// <summary>
    /// The old teams this player has played for.
    /// </summary>
    public IReadOnlyList<Guid> OldTeams => teams.Skip(1).ToArray();

    /// <summary>
    /// Get the player's Sendou profile details.
    /// </summary>
    public IReadOnlyList<Sendou> SendouProfiles => sendouProfiles;

    /// <summary>
    /// Get or Set the current sources that make up this Player instance.
    /// </summary>
    public IList<Source> Sources => sources;

    /// <summary>
    /// The Splatnet database Id of the player (a hex string).
    /// Null by default.
    /// </summary>
    public string? SplatnetId { get; set; }

    /// <summary>
    /// The teams this player is played for.
    /// No Team represented by <see cref="Team.NoTeam.Id"/>
    /// </summary>
    public IReadOnlyList<Guid> Teams => teams.ToArray();

    /// <summary>
    /// Get or Set Top 500 flag.
    /// False by default.
    /// </summary>
    public bool Top500 { get; set; }

    /// <summary>
    /// The Names of this Player transformed.
    /// </summary>
    public IEnumerable<string> TransformedNames => Names.Select(n => n.Transformed);

    /// <summary>
    /// Get the player's Twitch profile details.
    /// </summary>
    public IReadOnlyList<Twitch> Twitch => twitchProfiles;

    /// <summary>
    /// Get the player's Twitter profile details.
    /// </summary>
    public IReadOnlyList<Twitter> Twitter => twitterProfiles;

    /// <summary>
    /// The weapons this player uses.
    /// </summary>
    public IReadOnlyList<string> Weapons => weapons;

    public void AddBattlefy(Battlefy value)
    {
      Battlefy.AddSlugs(value.Slugs);
      Battlefy.AddUsernames(value.Usernames);
    }

    public void AddBattlefyInformation(string slug, string username, Source source)
    {
      AddBattlefySlug(slug, source);
      AddBattlefyUsername(username, source);
    }

    public void AddBattlefySlug(string slug, Source source)
    {
      Battlefy.AddSlug(slug, source);
    }

    public void AddBattlefyUsername(string username, Source source)
    {
      Battlefy.AddUsername(username, source);
    }

    public void AddDiscord(Discord value)
    {
      Discord.AddIds(value.Ids);
      Discord.AddUsernames(value.Usernames);
    }

    public void AddDiscordId(string id, Source source)
    {
      Discord.AddId(id, source);
    }

    public void AddDiscordUsername(string username, Source source)
    {
      Discord.AddUsername(username, source);
    }

    public void AddFCs(IEnumerable<FriendCode> value)
    {
      if (value != null)
      {
        value = value.Where(fc => fc != FriendCode.NO_FRIEND_CODE);
        SplatTagCommon.InsertFrontUnique(value, friendCodes);
      }
    }

    public void AddName(string name, Source source)
    {
      SplatTagCommon.AddName(new Name(name, source), names);
    }

    public void AddNames(IEnumerable<Name> value)
    {
      SplatTagCommon.AddNames(value, names);
    }

    public void AddSendou(string handle, Source source)
    {
      SplatTagCommon.AddName(new Sendou(handle, source), sendouProfiles);
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
      SplatTagCommon.InsertFrontUnique(value, teams);
    }

    public void AddTwitch(string handle, Source source)
    {
      SplatTagCommon.AddName(new Twitch(handle, source), twitchProfiles);
    }

    public void AddTwitch(IEnumerable<Twitch> value)
    {
      SplatTagCommon.AddNames(value, twitchProfiles);
    }

    public void AddTwitter(string handle, Source source)
    {
      SplatTagCommon.AddName(new Twitter(handle, source), twitterProfiles);
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
      AddBattlefy(newerPlayer.battlefy);

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

    #region Serialization

    // Deserialize
    protected Player(SerializationInfo info, StreamingContext context)
    {
      AddBattlefy(info.GetValueOrDefault("Battlefy", new Battlefy()));
      AddDiscord(info.GetValueOrDefault("Discord", new Discord()));
      AddFCs(info.GetValueOrDefault("FriendCode", Array.Empty<FriendCode>()));
      this.Id = (Guid)info.GetValue("Id", typeof(Guid));
      AddNames(info.GetValueOrDefault("Names", Array.Empty<Name>()));
      AddSendou(info.GetValueOrDefault("Sendou", Array.Empty<Sendou>()));
      AddSources(info.GetValueOrDefault("Sources", Array.Empty<Source>()));
      AddTeams(info.GetValueOrDefault("Teams", Array.Empty<Guid>()));
      AddTwitch(info.GetValueOrDefault("Twitch", Array.Empty<Twitch>()));
      AddTwitter(info.GetValueOrDefault("Twitter", Array.Empty<Twitter>()));
      AddWeapons(info.GetValueOrDefault("Weapons", Array.Empty<string>()));
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (this.BattlefySlugs.Any() || this.BattlefyUsernames.Any())
        info.AddValue("Battlefy", this.battlefy);

      if (this.DiscordIds.Any() || this.DiscordNames.Any())
        info.AddValue("Discord", this.discord);

      if (this.friendCodes.Any())
        info.AddValue("FriendCode", this.friendCodes);

      info.AddValue("Id", this.Id);

      if (this.names.Any())
        info.AddValue("Names", this.names);

      if (this.sendouProfiles.Any())
        info.AddValue("Sendou", this.sendouProfiles);

      if (this.sources.Any())
        info.AddValue("Sources", this.sources);

      if (this.teams.Any())
        info.AddValue("Teams", this.teams);

      if (this.twitchProfiles.Any())
        info.AddValue("Twitch", this.twitchProfiles);

      if (this.twitterProfiles.Any())
        info.AddValue("Twitter", this.twitterProfiles);

      if (this.weapons.Any())
        info.AddValue("Weapons", this.weapons);
    }

    [OnDeserialized]
    private void OnDeserialization(StreamingContext context)
    {
      // Nothing to do yet - versioning information and compatibility may
      // go here in the future.
    }

    #endregion Serialization
  }
}