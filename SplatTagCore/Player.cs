using SplatTagCore.Social;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class Player : IMergable<Player>, IReadonlySourceable, ISerializable
  {
    /// <summary>
    /// The database Id of the player.
    /// </summary>
    public readonly Guid Id = Guid.NewGuid();

    /// <summary>
    /// Back-store for the weapons that the player uses (if any).
    /// </summary>
    private readonly List<string> weapons = new();

    /// <summary>
    /// Back-store for the two-letter country abbreviation.
    /// </summary>
    private string? country;

    /// <summary>
    /// Player's pronoun(s)
    /// </summary>
    private Pronoun? pronoun;

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
      this.NamesInformation.Add(new Name(ign, source));
    }

    /// <summary>
    /// Construct a player with their name, teams, and source
    /// </summary>
    /// <param name="ign"></param>
    /// <param name="source"></param>
    public Player(string ign, IList<Guid> teams, Source source)
    {
      this.NamesInformation.Add(new Name(ign, source));
      this.TeamInformation.Add(teams, source);
    }

    /// <summary>
    /// Any names (social or IGN) this player is known by; does NOT include Battlefy.
    /// </summary>
    public IReadOnlyList<Name> AllKnownNames
      => new List<Name>(Names.Concat(SendouProfiles).Concat(Discord.Ids).Concat(Discord.Usernames).Concat(TwitchProfiles).Concat(TwitterProfiles).Distinct());

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
    /// Get the player's Discord profile details.
    /// </summary>
    public Discord Discord { get; } = new Discord();

    /// <summary>
    /// The known Discord Ids of the player.
    /// </summary>
    public IReadOnlyCollection<Name> DiscordIds => Discord.Ids;

    /// <summary>
    /// The known Discord usernames of the player.
    /// </summary>
    public IReadOnlyCollection<Name> DiscordNames => Discord.Usernames;

    /// <summary>
    /// Get the information regarding the friend codes for this player.
    /// </summary>
    public FriendCodesHandler FCInformation { get; } = new FriendCodesHandler();

    /// <summary>
    /// The last known used name for the player
    /// </summary>
    public Name Name => NamesInformation.MostRecent ?? Builtins.UnknownPlayerName;

    /// <summary>
    /// The in-game or registered names this player is known by.
    /// </summary>
    public IReadOnlyCollection<Name> Names => NamesInformation.GetItemsUnordered();

    /// <summary>
    /// The in-game or registered names this player is known by.
    /// </summary>
    public NamesHandler<Name> NamesInformation { get; } = new();

    /// <summary>
    /// The plus membership(s) this player belongs to.
    /// </summary>
    public IReadOnlyCollection<PlusMembership> PlusMembership => PlusMembershipInformation.GetItemsUnordered();

    /// <summary>
    /// The plus membership(s) this player belongs to.
    /// </summary>
    public NamesHandler<PlusMembership> PlusMembershipInformation { get; } = new();

    /// <summary>
    /// The Sendou social information this player belongs to.
    /// </summary>
    public NamesHandler<Sendou> SendouInformation { get; } = new();

    /// <summary>
    /// Get the player's Sendou profile details.
    /// </summary>
    public IReadOnlyCollection<Sendou> SendouProfiles => SendouInformation.GetItemsUnordered();

    /// <summary>
    /// The Twitch social information this player belongs to.
    /// </summary>
    public NamesHandler<Twitch> TwitchInformation { get; } = new();

    /// <summary>
    /// The Twitch social information this player belongs to.
    /// </summary>
    public IReadOnlyCollection<Twitch> TwitchProfiles => TwitchInformation.GetItemsUnordered();

    /// <summary>
    /// The Twitter social information this player belongs to.
    /// </summary>
    public NamesHandler<Twitter> TwitterInformation { get; } = new();

    /// <summary>
    /// The Twitter social information this player belongs to.
    /// </summary>
    public IReadOnlyCollection<Twitter> TwitterProfiles => TwitterInformation.GetItemsUnordered();

    /// <summary>
    /// Get the player's Skill/clout.
    /// </summary>
    public Skill Skill { get; } = new Skill();

    public IReadOnlyList<Source> Sources =>
      NamesInformation.Sources
        .Concat(Battlefy.Sources)
        .Concat(Discord.Sources)
        .Concat(FCInformation.Sources)
        .Concat(PlusMembershipInformation.Sources)
        .Concat(pronoun?.Sources ?? Array.Empty<Source>())
        .Concat(SendouInformation.Sources)
        .Concat(TeamInformation.Sources)
        .Concat(TwitchInformation.Sources)
        .Concat(TwitterInformation.Sources)
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
    /// Get the information regarding teams for this player.
    /// </summary>
    public TeamsHandler TeamInformation { get; } = new TeamsHandler();

    /// <summary>
    /// Get or Set Top 500 flag.
    /// False by default.
    /// </summary>
    public bool Top500 { get; set; }

    /// <summary>
    /// The weapons this player uses.
    /// </summary>
    public IReadOnlyList<string> Weapons => weapons;

    public void AddBattlefyInformation(string slug, string username, string persistentId, Source source)
    {
      AddBattlefySlug(slug, source);
      AddBattlefyUsername(username, source);
      AddBattlefyPersistentId(persistentId, source);
    }

    public void AddBattlefyPersistentId(string persistentId, Source source)
    {
      Battlefy.AddPersistentId(persistentId, source);
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

    public void AddDiscordUsername(string discordNameIncludingDiscrim, Source source)
    {
      Debug.WriteLineIf(!discordNameIncludingDiscrim.Contains("#"), $"Added Discord name to player {this.Name} but it does not have a #!");
      Discord.AddUsername(discordNameIncludingDiscrim, source);
    }

    public void AddFCs(FriendCode value, Source source) => FCInformation.Add(value, source);

    public void AddFCs(IList<FriendCode> value, Source source) => FCInformation.Add(value, source);

    public void AddName(string name, Source source)
      => NamesInformation.Add(new Name(name, source));

    public void AddPlusServerMembership(int? plusLevel, Source source)
      => PlusMembershipInformation.Add(new PlusMembership(plusLevel, source));

    public void AddSendou(string handle, Source source)
      => SendouInformation.Add(new Sendou(handle, source));

    public void AddTeams(Guid value, Source source)
      => TeamInformation.Add(value, source);

    public void AddTwitch(string handle, Source source)
      => TwitchInformation.Add(new Twitch(handle, source));

    public void AddTwitter(string handle, Source source)
      => TwitterInformation.Add(new Twitter(handle, source));

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
      TeamInformation.Merge(newerPlayer.TeamInformation);

      // Merge the player's name(s).
      NamesInformation.Merge(newerPlayer.NamesInformation);

      // Merge the weapons.
      AddWeapons(newerPlayer.weapons);

      // Merge the Battlefy Slugs and usernames.
      Battlefy.Merge(newerPlayer.Battlefy);

      // Merge the Discord Slugs and usernames.
      Discord.Merge(newerPlayer.Discord);

      // Merge the Social Data.
      PlusMembershipInformation.Merge(newerPlayer.PlusMembershipInformation);
      SendouInformation.Merge(newerPlayer.SendouInformation);
      TwitchInformation.Merge(newerPlayer.TwitchInformation);
      TwitterInformation.Merge(newerPlayer.TwitterInformation);

      // Merge the misc data
      FCInformation.Merge(newerPlayer.FCInformation);

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
      this.Battlefy = info.GetValueOrDefault("Battlefy", new Battlefy());
      this.Country = info.GetValueOrDefault("Country", default(string));
      this.Discord = info.GetValueOrDefault("Discord", new Discord());
      this.FCInformation = info.GetValueOrDefault("FCs", new FriendCodesHandler());
      this.NamesInformation.Add(info.GetValueOrDefault("N", Array.Empty<Name>()));
      this.PlusMembershipInformation.Add(info.GetValueOrDefault("Plus", Array.Empty<PlusMembership>()));
      this.pronoun = info.GetValueOrDefault("Pro", (Pronoun?)null);
      this.SendouInformation.Add(info.GetValueOrDefault("Sendou", Array.Empty<Sendou>()));
      this.TeamInformation = info.GetValueOrDefault("Teams", new TeamsHandler());
      this.Top500 = info.GetValueOrDefault("Top500", false);
      this.TwitchInformation.Add(info.GetValueOrDefault("Twitch", Array.Empty<Twitch>()));
      this.TwitterInformation.Add(info.GetValueOrDefault("Twitter", Array.Empty<Twitter>()));
      AddWeapons(info.GetValueOrDefault("Weapons", Array.Empty<string>()));

      Skill[] skills = info.GetValueOrDefault("Skill", Array.Empty<Skill>());
      this.Skill = skills.Length == 1 ? skills[0] : new Skill();
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

      if (this.NamesInformation.Count > 0)
        info.AddValue("N", this.Names);

      if (this.PlusMembershipInformation.Count > 0)
        info.AddValue("Plus", this.PlusMembership);

      if (this.pronoun != null)
        info.AddValue("Pro", this.pronoun);

      if (this.SendouInformation.Count > 0)
        info.AddValue("Sendou", this.SendouProfiles);

      if (!this.Skill.IsDefault)
        info.AddValue("Skill", this.Skill);

      if (this.TeamInformation.Count > 0)
        info.AddValue("Teams", this.TeamInformation);

      if (this.Top500)
        info.AddValue("Top500", this.Top500);

      if (this.TwitchInformation.Count > 0)
        info.AddValue("Twitch", this.TwitchProfiles);

      if (this.TwitterInformation.Count > 0)
        info.AddValue("Twitter", this.TwitterProfiles);

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