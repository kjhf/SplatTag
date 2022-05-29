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
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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
      : base()
    {
      this.NamesInformationWithCreate.Add(new Name(ign, source));
    }

    /// <summary>
    /// Construct a player with their name, teams, and source
    /// </summary>
    /// <param name="ign"></param>
    /// <param name="source"></param>
    public Player(string ign, IList<Guid> teams, Source source)
      : base()
    {
      this.NamesInformationWithCreate.Add(new Name(ign, source));
      this.TeamInformationWithCreate.Add(teams, source);
    }

    /// <summary>
    /// Any names (social or IGN) this player is known by; does NOT include Battlefy.
    /// </summary>
    public IReadOnlyList<Name> AllKnownNames
      => new List<Name>(Names.Concat(SendouProfiles).Concat(DiscordIds).Concat(DiscordNames).Concat(TwitchProfiles).Concat(TwitterProfiles).Distinct());

    /// <summary>
    /// The known Battlefy Ids of the player.
    /// </summary>
    public IReadOnlyCollection<Name> BattlefyIds => BattlefyInformationNoCreate?.PersistentIds ?? Array.Empty<Name>();

    /// <summary>
    /// The known Battlefy Ids of the player in recent source order.
    /// </summary>
    public IReadOnlyList<Name> BattlefyIdsOrdered => BattlefyInformationNoCreate?.PersistentIdsOrdered ?? Array.Empty<Name>();

    /// <summary>
    /// The known Battlefy usernames of the player.
    /// </summary>
    public IReadOnlyCollection<Name> BattlefyNames => BattlefyInformationNoCreate?.Usernames ?? Array.Empty<Name>();

    /// <summary>
    /// The known Battlefy usernames of the player in recent source order.
    /// </summary>
    public IReadOnlyList<Name> BattlefyNamesOrdered => BattlefyInformationNoCreate?.UsernamesOrdered ?? Array.Empty<Name>();

    /// <summary>
    /// The known Battlefy slugs of the player.
    /// </summary>
    public IReadOnlyCollection<BattlefyUserSocial> BattlefySlugs => BattlefyInformationNoCreate?.Slugs ?? Array.Empty<BattlefyUserSocial>();

    /// <summary>
    /// The known Battlefy slugs of the player in recent source order.
    /// </summary>
    public IReadOnlyList<BattlefyUserSocial> BattlefySlugsOrdered => BattlefyInformationNoCreate?.SlugsOrdered ?? Array.Empty<BattlefyUserSocial>();

    /// <summary>
    /// Get or Set the Country.
    /// Null by default.
    /// To set this field, the value must be a two-letter abbreviation.
    /// </summary>
    public string? Country { get => CountryInformationNoCreate?.CountryCode; set => CountryInformation.Merge(value); }

    /// <summary>
    /// Get or Set the Country Flag.
    /// Null by default.
    /// </summary>
    public string? CountryFlag { get => CountryInformationNoCreate?.CountryFlag; }

    /// <summary>
    /// The current team id this player plays for, or <see cref="Team.NoTeam.Id"/> if not set.
    /// </summary>
    public Guid CurrentTeam => TeamInformationNoCreate?.CurrentTeam ?? Team.NoTeam.Id;

    /// <summary>
    /// The known Discord Ids of the player.
    /// </summary>
    public IReadOnlyCollection<Name> DiscordIds => DiscordInformationNoCreate?.Ids ?? Array.Empty<Name>();

    /// <summary>
    /// The known Discord usernames of the player.
    /// </summary>
    public IReadOnlyCollection<Name> DiscordNames => DiscordInformationNoCreate?.Usernames ?? Array.Empty<Name>();

    /// <summary>
    /// The known friend codes of the player.
    /// </summary>
    public IReadOnlyCollection<FriendCode> FCs => FriendCodesInformationNoCreate?.GetCodesUnordered() ?? Array.Empty<FriendCode>();

    /// <summary>
    /// The last known used name for the player
    /// </summary>
    public Name Name => NamesInformationNoCreate?.MostRecent ?? Builtins.UnknownPlayerName;

    /// <summary>
    /// The in-game or registered names this player is known by.
    /// </summary>
    public IReadOnlyCollection<Name> Names => NamesInformationNoCreate?.GetItemsUnordered() ?? Array.Empty<Name>();

    /// <summary>
    /// The plus membership(s) this player belongs to.
    /// </summary>
    public IReadOnlyCollection<PlusMembership> PlusMembership => PlusMembershipInformationNoCreate?.GetItemsUnordered() ?? Array.Empty<PlusMembership>();

    /// <summary>
    /// Get the player's Sendou profile details.
    /// </summary>
    public IReadOnlyCollection<Sendou> SendouProfiles => SendouInformationNoCreate?.GetItemsUnordered() ?? Array.Empty<Sendou>();

    /// <summary>
    /// Get the player's Skill/clout.
    /// </summary>
    public Skill? Skill { get => SkillInformationNoCreate?.Skill; set => SkillInformation.Merge(value); }

    /// <summary>
    /// The Splatnet database Id of the player (a hex string).
    /// Null by default.
    /// </summary>
    public string? SplatnetId { get => SplatnetIdInformationNoCreate?.Value; set => SplatnetIdInformation.Merge(value); }

    /// <summary>
    /// All team ids this player has player for, unordered.
    /// </summary>
    public IReadOnlyCollection<Guid> Teams => TeamInformationNoCreate?.GetAllTeamsUnordered() ?? Array.Empty<Guid>();

    /// <summary>
    /// Get if this player is a Top 500.
    /// False by default.
    /// </summary>
    public bool Top500 { get => Top500InformationNoCreate?.Top500 ?? false; set => Top500Information.Merge(value); }

    /// <summary>
    /// The Twitch social information this player belongs to.
    /// </summary>
    public IReadOnlyCollection<Twitch> TwitchProfiles => TwitchInformationNoCreate?.GetItemsUnordered() ?? Array.Empty<Twitch>();

    /// <summary>
    /// The Twitter social information this player belongs to.
    /// </summary>
    public IReadOnlyCollection<Twitter> TwitterProfiles => TwitterInformationNoCreate?.GetItemsUnordered() ?? Array.Empty<Twitter>();

    /// <summary>
    /// The weapons this player uses.
    /// </summary>
    public IReadOnlyCollection<string> Weapons => WeaponsInformationNoCreate?.MostRecent ?? (IReadOnlyCollection<string>)Array.Empty<string>();

    /// <inheritdoc/>
    protected override IReadOnlyDictionary<string, (Type, Func<BaseHandler>)> SupportedHandlers => new Dictionary<string, (Type, Func<BaseHandler>)>
    {
      { BattlefyHandler.SerializationName, (typeof(BattlefyHandler), () => new BattlefyHandler()) },
      { CountryHandler.SerializationName, (typeof(CountryHandler), () => new CountryHandler()) },
      { DiscordHandler.SerializationName, (typeof(DiscordHandler), () => new DiscordHandler()) },
      { FriendCodesHandler.SerializationName, (typeof(FriendCodesHandler), () => new FriendCodesHandler()) },
      { PlayerNamesHandler.SerializationName, (typeof(PlayerNamesHandler), () => new PlayerNamesHandler()) },
      { PlusHandler.SerializationName, (typeof(PlusHandler), () => new PlusHandler()) },
      { PronounsHandler.SerializationName, (typeof(PronounsHandler), () => new PronounsHandler()) },
      { SendouNamesHandler.SerializationName, (typeof(SendouNamesHandler), () => new SendouNamesHandler())},
      { SkillHandler.SerializationName, (typeof(SkillHandler), () => new SkillHandler()) },
      { SplatnetIdHandler.SerializationName, (typeof(SplatnetIdHandler), () => new SplatnetIdHandler()) },
      { TeamsHandler.SerializationName, (typeof(TeamsHandler), () => new TeamsHandler()) },
      { Top500Handler.SerializationName, (typeof(Top500Handler), () => new Top500Handler()) },
      { TwitchHandler.SerializationName, (typeof(TwitchHandler), () => new TwitchHandler()) },
      { TwitterHandler.SerializationName, (typeof(TwitterHandler), () => new TwitterHandler()) },
      { WeaponsHandler.SerializationName, (typeof(WeaponsHandler), () => new WeaponsHandler()) },
      { IdHandler.SerializationName, (typeof(IdHandler), () => new IdHandler()) },
    };

    /// <summary>
    /// Get the player's Battlefy profile details.
    /// </summary>
    private BattlefyHandler BattlefyInformation => GetHandler<BattlefyHandler>(BattlefyHandler.SerializationName);

    /// <summary>
    /// Get the player's Battlefy profile details.
    /// </summary>
    private BattlefyHandler? BattlefyInformationNoCreate => GetHandlerNoCreate<BattlefyHandler>(BattlefyHandler.SerializationName);

    /// <summary>
    /// Get the player's Country details.
    /// </summary>
    private CountryHandler CountryInformation => GetHandler<CountryHandler>(CountryHandler.SerializationName);

    /// <summary>
    /// Get the player's Country details.
    /// </summary>
    private CountryHandler? CountryInformationNoCreate => GetHandlerNoCreate<CountryHandler>(CountryHandler.SerializationName);

    /// <summary>
    /// Get the player's Discord profile details.
    /// </summary>
    private DiscordHandler DiscordInformation => GetHandler<DiscordHandler>(DiscordHandler.SerializationName);

    /// <summary>
    /// Get the player's Discord profile details.
    /// </summary>
    internal DiscordHandler? DiscordInformationNoCreate => GetHandlerNoCreate<DiscordHandler>(DiscordHandler.SerializationName);

    /// <summary>
    /// Get the information regarding the friend codes for this player.
    /// </summary>
    private FriendCodesHandler FriendCodesInformation => GetHandler<FriendCodesHandler>(FriendCodesHandler.SerializationName);

    /// <summary>
    /// Get the information regarding the friend codes for this player.
    /// </summary>
    private FriendCodesHandler? FriendCodesInformationNoCreate => GetHandlerNoCreate<FriendCodesHandler>(FriendCodesHandler.SerializationName);

    /// <summary>
    /// The in-game or registered names this player is known by.
    /// </summary>
    public PlayerNamesHandler? NamesInformation => NamesInformationNoCreate;

    /// <summary>
    /// Get the handler for the in-game or registered names this player is known by or create it if it doesn't exist.
    /// </summary>
    private PlayerNamesHandler NamesInformationWithCreate => GetHandler<PlayerNamesHandler>(PlayerNamesHandler.SerializationName);

    /// <summary>
    /// Get the handler for the in-game or registered names this player is known by or null if it doesn't exist.
    /// </summary>
    private PlayerNamesHandler? NamesInformationNoCreate => GetHandlerNoCreate<PlayerNamesHandler>(PlayerNamesHandler.SerializationName);

    /// <summary>
    /// The plus membership(s) this player belongs to.
    /// </summary>
    private PlusHandler PlusMembershipInformation => GetHandler<PlusHandler>(PlusHandler.SerializationName);

    /// <summary>
    /// The plus membership(s) this player belongs to.
    /// </summary>
    private PlusHandler? PlusMembershipInformationNoCreate => GetHandlerNoCreate<PlusHandler>(PlusHandler.SerializationName);

    /// <summary>
    /// Player's pronoun(s)
    /// </summary>
    private PronounsHandler PronounInformation => GetHandler<PronounsHandler>(PronounsHandler.SerializationName);

    /// <summary>
    /// Player's pronoun(s)
    /// </summary>
    private PronounsHandler? PronounInformationNoCreate => GetHandlerNoCreate<PronounsHandler>(PronounsHandler.SerializationName);

    /// <summary>
    /// The Sendou social information this player belongs to.
    /// </summary>
    private SendouNamesHandler SendouInformation => GetHandler<SendouNamesHandler>(SendouNamesHandler.SerializationName);

    /// <summary>
    /// The Sendou social information this player belongs to.
    /// </summary>
    private SendouNamesHandler? SendouInformationNoCreate => GetHandlerNoCreate<SendouNamesHandler>(SendouNamesHandler.SerializationName);

    /// <summary>
    /// Get the player's Skill/clout.
    /// </summary>
    private SkillHandler SkillInformation => GetHandler<SkillHandler>(SkillHandler.SerializationName);

    /// <summary>
    /// Get the player's Skill/clout.
    /// </summary>
    private SkillHandler? SkillInformationNoCreate => GetHandlerNoCreate<SkillHandler>(SkillHandler.SerializationName);

    /// <summary>
    /// Player's Splatnet Id.
    /// </summary>
    private SplatnetIdHandler SplatnetIdInformation => GetHandler<SplatnetIdHandler>(SplatnetIdHandler.SerializationName);

    /// <summary>
    /// Player's Splatnet Id.
    /// </summary>
    private SplatnetIdHandler? SplatnetIdInformationNoCreate => GetHandlerNoCreate<SplatnetIdHandler>(SplatnetIdHandler.SerializationName);

    /// <summary>
    /// Get the information regarding teams for this player.
    /// </summary>
    public TeamsHandler? TeamInformation => TeamInformationNoCreate;

    /// <summary>
    /// Get the information regarding teams for this player.
    /// </summary>
    public TeamsHandler TeamInformationWithCreate => GetHandler<TeamsHandler>(TeamsHandler.SerializationName);

    /// <summary>
    /// Get the information regarding teams for this player.
    /// </summary>
    private TeamsHandler? TeamInformationNoCreate => GetHandlerNoCreate<TeamsHandler>(TeamsHandler.SerializationName);

    /// <summary>
    /// Get the player's top 500 information.
    /// </summary>
    private Top500Handler Top500Information => GetHandler<Top500Handler>(Top500Handler.SerializationName);

    /// <summary>
    /// Get the player's top 500 information.
    /// </summary>
    private Top500Handler? Top500InformationNoCreate => GetHandlerNoCreate<Top500Handler>(Top500Handler.SerializationName);

    /// <summary>
    /// The Twitch social information this player belongs to.
    /// </summary>
    private TwitchHandler TwitchInformation => GetHandler<TwitchHandler>(TwitchHandler.SerializationName);

    /// <summary>
    /// The Twitch social information this player belongs to.
    /// </summary>
    private TwitchHandler? TwitchInformationNoCreate => GetHandlerNoCreate<TwitchHandler>(TwitchHandler.SerializationName);

    /// <summary>
    /// The Twitter social information this player belongs to.
    /// </summary>
    private TwitterHandler TwitterInformation => GetHandler<TwitterHandler>(TwitterHandler.SerializationName);

    /// <summary>
    /// The Twitter social information this player belongs to.
    /// </summary>
    private TwitterHandler? TwitterInformationNoCreate => GetHandlerNoCreate<TwitterHandler>(TwitterHandler.SerializationName);

    /// <summary>
    /// The weapons this player uses.
    /// </summary>
    private WeaponsHandler WeaponsInformation => GetHandler<WeaponsHandler>(WeaponsHandler.SerializationName);

    /// <summary>
    /// The weapons this player uses.
    /// </summary>
    private WeaponsHandler? WeaponsInformationNoCreate => GetHandlerNoCreate<WeaponsHandler>(WeaponsHandler.SerializationName);

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
      => FriendCodesInformation.Add(value, source);

    public void AddFCs(IList<FriendCode> value, Source source)
      => FriendCodesInformation.Add(value, source);

    public void AddName(string name, Source source)
      => NamesInformationWithCreate.Add(new Name(name, source));

    public void AddPlusServerMembership(int? plusLevel, Source source)
      => PlusMembershipInformation.Add(new PlusMembership(plusLevel, source));

    public void AddPronoun(string description, Source source)
      => PronounInformation.SetPronoun(description, source);

    public void AddSendou(string handle, Source source)
      => SendouInformation.Add(new Sendou(handle, source));

    public void AddTeams(Guid value, Source source)
      => TeamInformationWithCreate.Add(value, source);

    public void AddTwitch(string handle, Source source)
      => TwitchInformation.Add(new Twitch(handle, source));

    public void AddTwitter(string handle, Source source)
      => TwitterInformation.Add(new Twitter(handle, source));

    public void AddWeapons(IEnumerable<string> incoming, Source source)
      => WeaponsInformation.Add(incoming.Distinct().ToList(), source);

    /// <summary>
    /// Correct the item ids for this player given a merge result (containing old id --> the replacement id)
    /// Returns if any work was done.
    /// </summary>
    public bool CorrectTeamIds(IReadOnlyDictionary<Guid, Guid> teamsMergeResult) => TeamInformationNoCreate?.CorrectTeamIds(teamsMergeResult) ?? false;

    /// <summary>
    /// Overridden ToString, returns the player's name.
    /// </summary>
    public override string ToString() => Name.Value;

    #region Serialization

    // Deserialize
    protected Player(SerializationInfo info, StreamingContext context)
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