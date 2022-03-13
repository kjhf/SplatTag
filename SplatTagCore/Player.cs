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
    /// Player's pronoun(s)
    /// </summary>
    public PronounsHandler PronounInformation { get; } = new();

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
        .Concat(PronounInformation.Sources)
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

    public void AddDiscordId(string id, Source source)
    {
      Discord.AddId(id, source);
    }

    public void AddDiscordUsername(string discordNameIncludingDiscrim, Source source)
    {
      Debug.WriteLineIf(!discordNameIncludingDiscrim.Contains("#"), $"Added Discord name to player {this.Name} but it does not have a #!");
      Discord.AddUsername(discordNameIncludingDiscrim, source);
    }

    public void AddFCs(FriendCode value, Source source)
      => FCInformation.Add(value, source);

    public void AddFCs(IList<FriendCode> value, Source source)
      => FCInformation.Add(value, source);

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

    public void AddWeapons(IEnumerable<string> incoming)
    {
      weapons.AddUnique(incoming.Distinct());
    }

    /// <summary>
    /// Merge this player with another player instance.
    /// Chronology safe.
    /// </summary>
    public void Merge(Player other)
    {
      if (other == null) throw new ArgumentNullException(nameof(other));
      if (ReferenceEquals(this, other)) return;

      // Merge the teams.
      TeamInformation.Merge(other.TeamInformation);

      // Merge the player's name(s).
      NamesInformation.Merge(other.NamesInformation);

      // Merge the weapons.
      AddWeapons(other.weapons);

      // Merge the Battlefy Slugs and usernames.
      Battlefy.Merge(other.Battlefy);

      // Merge the Discord Slugs and usernames.
      Discord.Merge(other.Discord);

      // Merge the Social Data.
      PlusMembershipInformation.Merge(other.PlusMembershipInformation);
      SendouInformation.Merge(other.SendouInformation);
      TwitchInformation.Merge(other.TwitchInformation);
      TwitterInformation.Merge(other.TwitterInformation);

      // Merge the misc data
      FCInformation.Merge(other.FCInformation);
      PronounInformation.Merge(other.PronounInformation);

      if (!string.IsNullOrWhiteSpace(other.Country))
      {
        this.Country = other.Country;
      }

      if (other.Top500)
      {
        this.Top500 = true;
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
      this.Battlefy = info.GetValueOrDefault("Battlefy", new Battlefy());
      this.Country = info.GetValueOrDefault("Country", default(string));
      this.Discord = info.GetValueOrDefault("Discord", new Discord());
      this.FCInformation = info.GetValueOrDefault("FCs", new FriendCodesHandler());
      this.NamesInformation.Add(info.GetValueOrDefault("N", Array.Empty<Name>()));
      this.PlusMembershipInformation.Add(info.GetValueOrDefault("Plus", Array.Empty<PlusMembership>()));
      this.PronounInformation = info.GetValueOrDefault("Pro", new PronounsHandler());
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

      if (this.PronounInformation.Count > 0)
        info.AddValue("Pro", this.PronounInformation);

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