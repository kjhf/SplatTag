using NLog;
using SplatTagCore.Social;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class Team : BaseSplatTagCoreObject<Team>
  {
    public static readonly Team NoTeam = new("(Free Agent)", Builtins.BuiltinSource);
    public static readonly Team UnlinkedTeam = new("(UNLINKED TEAM)", Builtins.BuiltinSource);
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Default construct a Team
    /// </summary>
    public Team()
    {
    }

    /// <summary>
    /// Construct a Team with their name and source
    /// </summary>
    public Team(string ign, Source source)
      : base()
    {
      AddName(ign, source);
    }

    /// <summary>
    /// The last known Battlefy Persistent Id of the team.
    /// </summary>
    public BattlefyTeamSocial? BattlefyPersistentTeamId => BattlefyTeamInformationNoCreate?.MostRecent;

    /// <summary>
    /// The known Battlefy Persistent Ids of the team.
    /// </summary>
    public IReadOnlyCollection<BattlefyTeamSocial> BattlefyPersistentTeamIds => BattlefyTeamInformationNoCreate?.GetItemsUnordered() ?? Array.Empty<BattlefyTeamSocial>();

    /// <summary>
    /// The known Battlefy Persistent Ids of the team.
    /// </summary>
    public IReadOnlyList<BattlefyTeamSocial> BattlefyPersistentTeamIdsOrdered => BattlefyTeamInformationNoCreate?.GetItemsOrdered() ?? Array.Empty<BattlefyTeamSocial>();

    /// <summary>
    /// The known Battlefy Persistent Ids of the team.
    /// </summary>
    public BattlefyTeamIdsHandler? BattlefyTeamInformation => BattlefyTeamInformationNoCreate;

    /// <summary>
    /// The team tag(s) of the team
    /// </summary>
    public ClanTagsHandler? ClanTagInformation => ClanTagInformationNoCreate;

    /// <summary>
    /// The team tag(s) of the team
    /// </summary>
    public IReadOnlyCollection<ClanTag> ClanTags => ClanTagInformationNoCreate?.GetItemsUnordered() ?? Array.Empty<ClanTag>();

    /// <summary>
    /// The team tag(s) of the team in most recent order
    /// </summary>
    public IReadOnlyList<ClanTag> ClanTagsOrdered => ClanTagInformationNoCreate?.GetItemsOrdered() ?? Array.Empty<ClanTag>();

    /// <summary>
    /// The current division of the team, or Division.Unknown.
    /// </summary>
    public Division CurrentDiv => DivisionInformationNoCreate?.CurrentDivision ?? Division.Unknown;

    /// <summary>
    /// The division information of the team.
    /// </summary>
    public DivisionsHandler? DivisionInformation => DivisionInformationNoCreate;

    /// <summary>
    /// The divisions of the team
    /// </summary>
    public IReadOnlyList<Division> DivsOrdered => DivisionInformationNoCreate?.GetItemsOrdered() ?? Array.Empty<Division>();

    /// <summary>
    /// The last known used name for the team or Builtins.UnknownTeamName.
    /// </summary>
    public Name Name => TeamNamesInformationNoCreate?.MostRecent ?? Builtins.UnknownTeamName;

    /// <summary>
    /// The registered names this team is known by.
    /// </summary>
    public IReadOnlyCollection<Name> Names => TeamNamesInformationNoCreate?.GetItemsUnordered() ?? Array.Empty<Name>();

    /// <summary>
    /// The most recent tag of the team
    /// </summary>
    public ClanTag? Tag => ClanTagInformationNoCreate?.MostRecent;

    /// <summary>
    /// The in-game or registered names this team is known by.
    /// </summary>
    public TeamNamesHandler? TeamNamesInformation => TeamNamesInformationNoCreate;

    /// <summary>
    /// The Twitter social information this team belongs to.
    /// </summary>
    public TwitterHandler? TwitterInformation => TwitterInformationNoCreate;

    /// <summary>
    /// The Twitter social information this team belongs to.
    /// </summary>
    public IReadOnlyCollection<Twitter> TwitterProfiles => TwitterInformationNoCreate?.GetItemsUnordered() ?? Array.Empty<Twitter>();

    protected override IReadOnlyDictionary<string, (Type, Func<BaseHandler>)> SupportedHandlers => new Dictionary<string, (Type, Func<BaseHandler>)>
    {
      { BattlefyTeamIdsHandler.SerializationName, (typeof(BattlefyTeamIdsHandler), () => new BattlefyTeamIdsHandler()) },
      { ClanTagsHandler.SerializationName, (typeof(ClanTagsHandler), () => new ClanTagsHandler()) },
      { DivisionsHandler.SerializationName, (typeof(DivisionsHandler), () => new DivisionsHandler()) },
      { TeamNamesHandler.SerializationName, (typeof(TeamNamesHandler), () => new TeamNamesHandler()) },
      { TwitterHandler.SerializationName, (typeof(TwitterHandler), () => new TwitterHandler()) },
      { IdHandler.SerializationName, (typeof(IdHandler), () => new IdHandler()) },
    };

    /// <summary>
    /// Get the Battlefy Team Information for this team or null if it doesn't exist.
    /// </summary>
    private BattlefyTeamIdsHandler? BattlefyTeamInformationNoCreate => GetHandlerNoCreate<BattlefyTeamIdsHandler>(BattlefyTeamIdsHandler.SerializationName);

    /// <summary>
    /// Get the Battlefy Team Information for this team or create it if it doesn't exist.
    /// </summary>
    private BattlefyTeamIdsHandler BattlefyTeamInformationWithCreate => GetHandler<BattlefyTeamIdsHandler>(BattlefyTeamIdsHandler.SerializationName);

    /// <summary>
    /// Get the handler for the clan tag information for this team or null if it doesn't exist.
    /// </summary>
    private ClanTagsHandler? ClanTagInformationNoCreate => GetHandlerNoCreate<ClanTagsHandler>(ClanTagsHandler.SerializationName);

    /// <summary>
    /// Get the handler for the clan tag information for this team or create it if it doesn't exist.
    /// </summary>
    private ClanTagsHandler ClanTagInformationWithCreate => GetHandler<ClanTagsHandler>(ClanTagsHandler.SerializationName);

    /// <summary>
    /// Get the division handler for this team or null if it doesn't exist.
    /// </summary>
    private DivisionsHandler? DivisionInformationNoCreate => GetHandlerNoCreate<DivisionsHandler>(DivisionsHandler.SerializationName);

    /// <summary>
    /// Get the division handler for this team or create it if it doesn't exist.
    /// </summary>
    private DivisionsHandler DivisionInformationWithCreate => GetHandler<DivisionsHandler>(DivisionsHandler.SerializationName);

    /// <summary>
    /// Get the team names handler or null if it doesn't exist.
    /// </summary>
    private TeamNamesHandler? TeamNamesInformationNoCreate => GetHandlerNoCreate<TeamNamesHandler>(TeamNamesHandler.SerializationName);

    /// <summary>
    /// Get the team names handler or create it if it doesn't exist.
    /// </summary>
    private TeamNamesHandler TeamNamesInformationWithCreate => GetHandler<TeamNamesHandler>(TeamNamesHandler.SerializationName);

    /// <summary>
    /// Get the Twitter social information handler or null if it doesn't exist.
    /// </summary>
    private TwitterHandler? TwitterInformationNoCreate => GetHandlerNoCreate<TwitterHandler>(TwitterHandler.SerializationName);

    /// <summary>
    /// Get the Twitter social information handler or create it if it doesn't exist.
    /// </summary>
    private TwitterHandler TwitterInformationWithCreate => GetHandler<TwitterHandler>(TwitterHandler.SerializationName);

    public void AddBattlefyId(string id, Source source)
      => BattlefyTeamInformationWithCreate.Add(new BattlefyTeamSocial(id, source));

    public void AddClanTag(string tag, Source source, TagOption option = TagOption.Unknown)
      => ClanTagInformationWithCreate.Add(new ClanTag(tag, source, option));

    public void AddClanTag(ClanTag tag)
      => ClanTagInformationWithCreate.Add(tag);

    public void AddDivision(Division division, Source source)
      => DivisionInformationWithCreate.Add(division, source);

    public void AddDivisions(DivisionsHandler value)
      => DivisionInformationWithCreate.Merge(value);

    public void AddName(string name, Source source)
      => TeamNamesInformationWithCreate.Add(new Name(name, source));

    public void AddTwitter(string handle, Source source)
      => TwitterInformationWithCreate.Add(new Twitter(handle, source));

    /// <summary>
    /// Get the team's best division, or null if not known.
    /// </summary>
    /// <param name="lastNDivisions">Limit the search to this many divisions in most-recent chronological order (-1 = no limit)</param>
    public Division? GetBestDiv(int lastNDivisions = -1)
      => DivisionInformationNoCreate?.GetBestDiv(lastNDivisions);

    /// <summary>
    /// Filter all players to return only those in this team.
    /// </summary>
    public IEnumerable<Player> GetPlayers(IEnumerable<Player> allPlayers)
    {
      return allPlayers.Where(p => p.Teams.Contains(this.Id));
    }

    /// <summary>
    /// Overridden ToString, gets the team's tag, name, and div.
    /// </summary>
    public override string ToString()
    {
      return $"{(Tag != null ? Tag.Value + " " : "")}{Name}{(CurrentDiv == Division.Unknown ? "" : $" ({CurrentDiv})")}";
    }

    #region Serialization

    // Deserialize
    protected Team(SerializationInfo info, StreamingContext context)
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