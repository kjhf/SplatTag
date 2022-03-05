using SplatTagCore.Social;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class Player : ISerializable, IReadonlySourceable
  {
    /// <summary>
    /// The database Id of the player.
    /// </summary>
    public readonly Guid Id = Guid.NewGuid();

    /// <summary>
    /// Back-store for the two-letter country abbreviation.
    /// </summary>
    private string? country;

    /// <summary>
    /// Back-store for the names of this player. The this element is the current name.
    /// </summary>
    private readonly List<Name> names = new List<Name>();

    /// <summary>
    /// Back-store for the plus membership this player belongs to.
    /// </summary>
    private readonly List<PlusMembership> plusMembership = new List<PlusMembership>();

    /// <summary>
    /// Player's pronoun(s)
    /// </summary>
    private Pronoun? pronoun;

    /// <summary>
    /// Back-store for the Sendou Profiles of this player.
    /// </summary>
    private readonly List<Sendou> sendouProfiles = new List<Sendou>();

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
    }

    /// <summary>
    /// Construct a player with their name, teams, and source
    /// </summary>
    /// <param name="ign"></param>
    /// <param name="source"></param>
    public Player(string ign, IList<Guid> teams, Source source)
    {
      this.names.Add(new Name(ign, source));
      this.TeamInformation.Add(teams, source);
    }

    /// <summary>
    /// Get the player's Battlefy profile details.
    /// </summary>
    public Battlefy Battlefy { get; } = new Battlefy();

    /// <summary>
    /// Get or Set the Country.
    /// Null by default.
    /// To set this field, the value must be a two-letter abbreviation.
    /// </summary>
    public string? Country
    {
      get => country;
      set
      {
        if (value == null)
        {
          country = null;
        }
        else
        {
          value = value.Trim();
          if (value.Length == 2)
          {
            country = value.ToUpper();
          }
        }
      }
    }

    /// <summary>
    /// Get the emoji flag of the <see cref="Country"/> specified.
    /// </summary>
    /// <remarks>
    /// Magic number is the offset '🇦' - 'A'
    /// </remarks>
    public string? CountryFlag
    {
      get
      {
        if (Country == null) return null;
        return string.Concat(Country.Select(x => char.ConvertFromUtf32(x + 0x1F1A5)));
      }
    }

    /// <summary>
    /// The current team id this player plays for, or <see cref="Team.NoTeam.Id"/> if not set.
    /// </summary>
    public Guid CurrentTeam => TeamInformation.CurrentTeam ?? Team.NoTeam.Id;

    /// <summary>
    /// Get the information regarding teams for this player.
    /// </summary>
    public TeamsHandler TeamInformation { get; } = new TeamsHandler();

    /// <summary>
    /// Get the player's Discord profile details.
    /// </summary>
    public Discord Discord { get; } = new Discord();

    /// <summary>
    /// The known Discord Ids of the player.
    /// </summary>
    public IReadOnlyList<Name> DiscordIds => Discord.Ids;

    /// <summary>
    /// The known Discord usernames of the player.
    /// </summary>
    public IReadOnlyList<Name> DiscordNames => Discord.Usernames;

    /// <summary>
    /// Get the information regarding the friend codes for this player.
    /// </summary>
    public FriendCodesHandler FCInformation { get; } = new FriendCodesHandler();

    /// <summary>
    /// The last known used name for the player
    /// </summary>
    public Name Name => names.Count > 0 ? names[0] : Builtins.UnknownPlayerName;

    /// <summary>
    /// The in-game or registered names this player is known by.
    /// </summary>
    public IReadOnlyList<Name> Names => names;

    /// <summary>
    /// Any names (social or IGN) this player is known by; does NOT include Battlefy.
    /// </summary>
    public IReadOnlyList<Name> AllKnownNames => new List<Name>(names.Concat(sendouProfiles).Concat(Discord.AllNames).Concat(twitchProfiles).Concat(twitterProfiles).Distinct());

    /// <summary>
    /// The plus membership(s) this player belongs to.
    /// </summary>
    public IReadOnlyList<PlusMembership> PlusMembership => plusMembership;

    /// <summary>
    /// Get the player's Sendou profile details.
    /// </summary>
    public IReadOnlyList<Sendou> SendouProfiles => sendouProfiles;

    /// <summary>
    /// Get the player's Skill/clout.
    /// </summary>
    public Skill Skill { get; } = new Skill();

    public IReadOnlyList<Source> Sources =>
      names.SelectMany(n => n.Sources)
      .Concat(Battlefy.PersistentIds.SelectMany(s => s.Sources))
      .Concat(Discord.Usernames.SelectMany(s => s.Sources))
      .Concat(FCInformation.Sources)
      .Concat(plusMembership.SelectMany(s => s.Sources))
      .Concat(pronoun?.Sources ?? Array.Empty<Source>())
      .Concat(sendouProfiles.SelectMany(s => s.Sources))
      .Concat(twitchProfiles.SelectMany(s => s.Sources))
      .Concat(twitterProfiles.SelectMany(s => s.Sources))
      .Distinct()
      .OrderByDescending(s => s)
      .ToList()
      ;

    /// <summary>
    /// The Splatnet database Id of the player (a hex string).
    /// Null by default.
    /// </summary>
    public string? SplatnetId { get; set; }

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
      Battlefy.AddPersistentIds(value.PersistentIds);
    }

    public void AddBattlefyInformation(string slug, string username, string persistentId, Source source)
    {
      AddBattlefySlug(slug, source);
      AddBattlefyUsername(username, source);
      AddBattlefyPersistentId(persistentId, source);
    }

    public void AddBattlefySlug(string slug, Source source)
    {
      Battlefy.AddSlug(slug, source);
    }

    public void AddBattlefyUsername(string username, Source source)
    {
      Battlefy.AddUsername(username, source);
    }

    public void AddBattlefyPersistentId(string persistentId, Source source)
    {
      Battlefy.AddPersistentId(persistentId, source);
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

    public void AddDiscordUsername(string discordNameIncludingDiscrim, Source source)
    {
      Debug.WriteLineIf(!discordNameIncludingDiscrim.Contains("#"), $"Added Discord name to player {this.Name} but it does not have a #!");
      Discord.AddUsername(discordNameIncludingDiscrim, source);
      // AddName(discordNameIncludingDiscrim.Split('#')[0], source);
    }

    public void AddFCs(FriendCode value, Source source) => FCInformation.Add(value, source);

    public void AddFCs(IList<FriendCode> value, Source source) => FCInformation.Add(value, source);

    public void AddFCs(FriendCodesHandler value) => FCInformation.Merge(value);

    public void AddName(string name, Source source)
    {
      SplatTagCommon.AddName(new Name(name, source), names);
    }

    public void AddNames(IEnumerable<Name> value)
    {
      SplatTagCommon.AddNames(value, names);
    }

    /// <summary>
    /// Conditionally set the pronouns of this player.
    /// If NONE is returned from the searcher, it is not set.
    /// Returns the Pronoun object set (or null).
    /// </summary>
    public Pronoun? SetPronoun(string description, Source source)
    {
      var incoming = new Pronoun(description, source);
      if (incoming.value != PronounFlags.NONE)
      {
        this.pronoun = incoming;
      }
      return this.pronoun;
    }

    public void AddPlusServerMembership(int? plusLevel, Source source)
    {
      SplatTagCommon.AddName(new PlusMembership(plusLevel, source), plusMembership);
    }

    public void AddPlusServerMembership(IEnumerable<PlusMembership> value)
    {
      SplatTagCommon.AddNames(value, plusMembership);
    }

    public void AddSendou(string handle, Source source)
    {
      SplatTagCommon.AddName(new Sendou(handle, source), sendouProfiles);
    }

    public void AddSendou(IEnumerable<Sendou> value)
    {
      SplatTagCommon.AddNames(value, sendouProfiles);
    }

    public void AddTeams(Guid value, Source source) => TeamInformation.Add(value, source);

    public void AddTeams(IList<Guid> value, Source source) => TeamInformation.Add(value, source);

    public void AddTeams(TeamsHandler value) => TeamInformation.Merge(value);

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
    /// Merge this player with another (newer) player instance
    /// </summary>
    /// <param name="newerPlayer">The new import record</param>
    /// <exception cref="ArgumentNullException"><paramref name="newerPlayer"/> is <c>null</c>.</exception>
    public void Merge(Player newerPlayer)
    {
      if (newerPlayer == null) throw new ArgumentNullException(nameof(newerPlayer));
      if (ReferenceEquals(this, newerPlayer)) return;

      // Merge the teams.
      AddTeams(newerPlayer.TeamInformation);

      // Merge the player's name(s).
      AddNames(newerPlayer.names);

      // Merge the weapons.
      AddWeapons(newerPlayer.weapons);

      // Merge the Battlefy Slugs and usernames.
      AddBattlefy(newerPlayer.Battlefy);

      // Merge the Discord Slugs and usernames.
      AddDiscord(newerPlayer.Discord);

      // Merge the Social Data.
      AddPlusServerMembership(newerPlayer.plusMembership);
      AddSendou(newerPlayer.SendouProfiles);
      AddTwitch(newerPlayer.twitchProfiles);
      AddTwitter(newerPlayer.twitterProfiles);

      // Merge the misc data
      AddFCs(newerPlayer.FCInformation);

      if (!string.IsNullOrWhiteSpace(newerPlayer.Country))
      {
        this.Country = newerPlayer.Country;
      }

      if (newerPlayer.Top500)
      {
        this.Top500 = true;
      }

      if (newerPlayer.pronoun != null)
      {
        this.pronoun = newerPlayer.pronoun;
      }
    }

    /// <summary>
    /// Overridden ToString, returns the player's name.
    /// </summary>
    public override string ToString()
    {
      return Name.Value;
    }

    #region Serialization

    // Deserialize
    protected Player(SerializationInfo info, StreamingContext context)
    {
      AddBattlefy(info.GetValueOrDefault("Battlefy", new Battlefy()));
      this.Country = info.GetValueOrDefault("Country", default(string));
      AddDiscord(info.GetValueOrDefault("Discord", new Discord()));
      AddFCs(info.GetValueOrDefault("FCs", new FriendCodesHandler()));
      AddNames(info.GetValueOrDefault("N", Array.Empty<Name>()));
      AddPlusServerMembership(info.GetValueOrDefault("Plus", Array.Empty<PlusMembership>()));
      this.pronoun = info.GetValueOrDefault("Pro", (Pronoun?)null);
      AddSendou(info.GetValueOrDefault("Sendou", Array.Empty<Sendou>()));

      Skill[] skills = info.GetValueOrDefault("Skill", Array.Empty<Skill>());
      this.Skill = skills.Length == 1 ? skills[0] : new Skill();
      AddTeams(info.GetValueOrDefault("Teams", new TeamsHandler()));
      this.Top500 = info.GetValueOrDefault("Top500", false);
      AddTwitch(info.GetValueOrDefault("Twitch", Array.Empty<Twitch>()));
      AddTwitter(info.GetValueOrDefault("Twitter", Array.Empty<Twitter>()));
      AddWeapons(info.GetValueOrDefault("Weapons", Array.Empty<string>()));

      this.Id = info.GetValueOrDefault("Id", Guid.Empty);
      if (this.Id == Guid.Empty)
      {
        throw new SerializationException($"Guid cannot be empty for player: {this.Name} from source(s) [{string.Join(", ", this.Sources)}].");
      }
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (this.Battlefy.Slugs.Count > 0 || this.Battlefy.Usernames.Count > 0 || this.Battlefy.PersistentIds.Count > 0)
        info.AddValue("Battlefy", this.Battlefy);

      if (this.Country != null)
        info.AddValue("Country", this.Country);

      if (this.DiscordIds.Count > 0 || this.DiscordNames.Count > 0)
        info.AddValue("Discord", this.Discord);

      if (this.FCInformation.Count > 0)
        info.AddValue("FCs", this.FCInformation);

      info.AddValue("Id", this.Id);

      if (this.names.Count > 0)
        info.AddValue("N", this.names);

      if (this.plusMembership.Count > 0)
        info.AddValue("Plus", this.plusMembership);

      if (this.pronoun != null)
        info.AddValue("Pro", this.pronoun);

      if (this.sendouProfiles.Count > 0)
        info.AddValue("Sendou", this.sendouProfiles);

      if (!this.Skill.IsDefault)
        info.AddValue("Skill", this.Skill);

      if (this.TeamInformation.Count > 0)
        info.AddValue("Teams", this.TeamInformation);

      if (this.Top500)
        info.AddValue("Top500", this.Top500);

      if (this.twitchProfiles.Count > 0)
        info.AddValue("Twitch", this.twitchProfiles);

      if (this.twitterProfiles.Count > 0)
        info.AddValue("Twitter", this.twitterProfiles);

      if (this.weapons.Count > 0)
        info.AddValue("Weapons", this.weapons);
    }

    /*
    [OnDeserialized]
    private void OnDeserialization(StreamingContext context)
    {
      // Nothing to do yet - versioning information and compatibility may
      // go here in the future.
    }
    */

    #endregion Serialization
  }
}