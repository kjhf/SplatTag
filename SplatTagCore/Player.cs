using NLog;
using SplatTagCore.Social;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class Player : BaseSplatTagCoreObject<Player>
  {
    private const string BattlefySerialization = "Battlefy";
    private const string CountrySerialization = "Country";
    private const string DiscordSerialization = "Discord";
    private const string FCsSerialization = "FCs";
    private const string NamesSerialization = "N";
    private const string PlusSerialization = "Plus";
    private const string PronounSerialization = "Pro";
    private const string SendouSerialization = "Sendou";
    private const string TeamSerialization = "Teams";
    private const string Top500Serialization = "Top500";
    private const string TwitchSerialization = "Twitch";
    private const string TwitterSerialization = "Twitter";
    private const string WeaponsSerialization = "Weapons";
    private const string SkillSerialization = "Skill";

    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Default construct a player
    /// </summary>
    public Player()
      : base()
    {
    }

    /// <summary>
    /// Construct a player with their name and source
    /// </summary>
    /// <param name="ign"></param>
    /// <param name="source"></param>
    public Player(string ign, Source source)
      : base()
    {
      this.NamesInformation.Add(new Name(ign, source));
    }

    /// <summary>
    /// Construct a player with their name, teams, and source
    /// </summary>
    /// <param name="ign"></param>
    /// <param name="source"></param>
    public Player(string ign, IList<Guid> teams, Source source)
      : base()
    {
      this.NamesInformation.Add(new Name(ign, source));
      this.TeamInformation.Add(teams, source);
    }

    protected override void InitialiseHandlers()
    {
      handlers.Clear();
      handlers.Add(BattlefySerialization, new BattlefyHandler());
      handlers.Add(CountrySerialization, new CountryHandler());
      handlers.Add(DiscordSerialization, new DiscordHandler());
      handlers.Add(FCsSerialization, new FriendCodesHandler());
      handlers.Add(NamesSerialization, new NamesHandler<Name>(FilterOptions.PlayerName, NamesSerialization));
      handlers.Add(PlusSerialization, new NamesHandler<PlusMembership>(null, PlusSerialization));
      handlers.Add(PronounSerialization, new PronounsHandler());
      handlers.Add(SendouSerialization, new NamesHandler<Sendou>(FilterOptions.PlayerSendou, SendouSerialization));
      handlers.Add(SkillSerialization, new SkillHandler());
      handlers.Add(TeamSerialization, new TeamsHandler());
      handlers.Add(Top500Serialization, new Top500Handler());
      handlers.Add(TwitchSerialization, new NamesHandler<Twitch>(FilterOptions.Twitch, TwitchSerialization));
      handlers.Add(TwitterSerialization, new NamesHandler<Twitter>(FilterOptions.Twitter, TwitterSerialization));
      handlers.Add(WeaponsSerialization, new WeaponsHandler());
      handlers.Add(IdHandler.IdSerialization, IdHandler);
    }

    /// <summary>
    /// Any names (social or IGN) this player is known by; does NOT include Battlefy.
    /// </summary>
    public IReadOnlyList<Name> AllKnownNames
      => new List<Name>(Names.Concat(SendouProfiles).Concat(DiscordInformation.Ids).Concat(DiscordInformation.Usernames).Concat(TwitchProfiles).Concat(TwitterProfiles).Distinct());

    /// <summary>
    /// Get the player's Battlefy profile details.
    /// </summary>
    public BattlefyHandler BattlefyInformation => (BattlefyHandler)this[BattlefySerialization];

    /// <summary>
    /// Get or Set the Country.
    /// Null by default.
    /// To set this field, the value must be a two-letter abbreviation.
    /// </summary>
    public string? Country { get => CountryInformation.CountryCode; set => CountryInformation.Merge(value); }

    /// <summary>
    /// Get the player's Country details.
    /// </summary>
    public CountryHandler CountryInformation => (CountryHandler)this[CountrySerialization];

    /// <summary>
    /// The current team id this player plays for, or <see cref="Team.NoTeam.Id"/> if not set.
    /// </summary>
    public Guid CurrentTeam => TeamInformation.CurrentTeam ?? Team.NoTeam.Id;

    /// <summary>
    /// Get the player's Discord profile details.
    /// </summary>
    public DiscordHandler DiscordInformation => (DiscordHandler)this[DiscordSerialization];

    /// <summary>
    /// The known Discord Ids of the player.
    /// </summary>
    public IReadOnlyCollection<Name> DiscordIds => DiscordInformation.Ids;

    /// <summary>
    /// The known Discord usernames of the player.
    /// </summary>
    public IReadOnlyCollection<Name> DiscordNames => DiscordInformation.Usernames;

    /// <summary>
    /// Get the information regarding the friend codes for this player.
    /// </summary>
    public FriendCodesHandler FCInformation => (FriendCodesHandler)this[FCsSerialization];

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
    public NamesHandler<Name> NamesInformation => (NamesHandler<Name>)this[NamesSerialization];

    /// <summary>
    /// The plus membership(s) this player belongs to.
    /// </summary>
    public IReadOnlyCollection<PlusMembership> PlusMembership => PlusMembershipInformation.GetItemsUnordered();

    /// <summary>
    /// The plus membership(s) this player belongs to.
    /// </summary>
    public NamesHandler<PlusMembership> PlusMembershipInformation => (NamesHandler<PlusMembership>)this[PlusSerialization];

    /// <summary>
    /// Player's pronoun(s)
    /// </summary>
    public PronounsHandler PronounInformation => (PronounsHandler)this[PronounSerialization];

    /// <summary>
    /// The Sendou social information this player belongs to.
    /// </summary>
    public NamesHandler<Sendou> SendouInformation => (NamesHandler<Sendou>)this[SendouSerialization];

    /// <summary>
    /// Get the player's Sendou profile details.
    /// </summary>
    public IReadOnlyCollection<Sendou> SendouProfiles => SendouInformation.GetItemsUnordered();

    /// <summary>
    /// Get the player's Skill/clout.
    /// </summary>
    public Skill? Skill { get => SkillInformation.Skill; set => SkillInformation.Merge(value); }

    /// <summary>
    /// Get the player's Skill/clout.
    /// </summary>
    public SkillHandler SkillInformation => (SkillHandler)this[SkillSerialization];

    /// <summary>
    /// The Splatnet database Id of the player (a hex string).
    /// Null by default.
    /// </summary>
    public string? SplatnetId { get; set; }

    /// <summary>
    /// Get the information regarding teams for this player.
    /// </summary>
    public TeamsHandler TeamInformation => (TeamsHandler)this[TeamSerialization];

    /// <summary>
    /// Get the player's top 500 information.
    /// </summary>
    public Top500Handler Top500Information => (Top500Handler)this[Top500Serialization];

    /// <summary>
    /// Get if this player is a Top 500.
    /// False by default.
    /// </summary>
    public bool Top500 { get => Top500Information.Top500; set => Top500Information.Merge(value); }

    /// <summary>
    /// The Twitch social information this player belongs to.
    /// </summary>
    public NamesHandler<Twitch> TwitchInformation => (NamesHandler<Twitch>)this[TwitchSerialization];

    /// <summary>
    /// The Twitch social information this player belongs to.
    /// </summary>
    public IReadOnlyCollection<Twitch> TwitchProfiles => TwitchInformation.GetItemsUnordered();

    /// <summary>
    /// The Twitter social information this player belongs to.
    /// </summary>
    public NamesHandler<Twitter> TwitterInformation => (NamesHandler<Twitter>)this[TwitterSerialization];

    /// <summary>
    /// The Twitter social information this player belongs to.
    /// </summary>
    public IReadOnlyCollection<Twitter> TwitterProfiles => TwitterInformation.GetItemsUnordered();

    /// <summary>
    /// The weapons this player uses.
    /// </summary>
    public WeaponsHandler WeaponsInformation => (WeaponsHandler)this[WeaponsSerialization];

    /// <summary>
    /// The weapons this player uses.
    /// </summary>
    public IReadOnlyCollection<string> Weapons => WeaponsInformation.MostRecent ?? (IReadOnlyCollection<string>)Array.Empty<string>();

    public void AddBattlefyInformation(string slug, string username, string persistentId, Source source)
    {
      AddBattlefySlug(slug, source);
      AddBattlefyUsername(username, source);
      AddBattlefyPersistentId(persistentId, source);
    }

    public void AddBattlefyPersistentId(string persistentId, Source source)
      => BattlefyInformation.AddPersistentId(persistentId, source);

    public void AddBattlefySlug(string slug, Source source)
      => BattlefyInformation.AddSlug(slug, source);

    public void AddBattlefyUsername(string username, Source source)
      => BattlefyInformation.AddUsername(username, source);

    public void AddDiscordId(string id, Source source)
      => DiscordInformation.AddId(id, source);

    public void AddDiscordUsername(string discordNameIncludingDiscrim, Source source)
    {
      Debug.WriteLineIf(!discordNameIncludingDiscrim.Contains("#"), $"Added Discord name to player {this.Name} but it does not have a #!");
      DiscordInformation.AddUsername(discordNameIncludingDiscrim, source);
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

    public void AddWeapons(IEnumerable<string> incoming, Source source)
      => WeaponsInformation.Add(incoming.Distinct().ToList(), source);

    /// <summary>
    /// Overridden ToString, returns the player's name.
    /// </summary>
    public override string ToString() => Name.Value;

    #region Serialization

    // Deserialize
    protected Player(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      DeserializeHandlers(info, context);
    }

    // Serialize
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      SerializeHandlers(info, context);
    }

    #endregion Serialization
  }
}