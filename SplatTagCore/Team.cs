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
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public static readonly Team NoTeam = new("(Free Agent)", Builtins.BuiltinSource);
    public static readonly Team UnlinkedTeam = new("(UNLINKED TEAM)", Builtins.BuiltinSource);

    private const string BattlefyPersistentTeamIdsSerialization = "BattlefyPersistentTeamIds";
    private const string ClanTagsSerialization = "ClanTags";
    private const string DivisionsSerialization = "Divisions";
    private const string NamesSerialization = "N";
    private const string TwitterSerialization = "Twitter";

    /// <summary>
    /// Default construct a Team
    /// </summary>
    public Team()
      : base()
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

    protected override void InitialiseHandlers()
    {
      handlers.Clear();
      handlers.Add(BattlefyPersistentTeamIdsSerialization, new NamesHandler<BattlefyTeamSocial>(FilterOptions.BattlefyPersistentIds, BattlefyPersistentTeamIdsSerialization));
      handlers.Add(ClanTagsSerialization, new NamesHandler<ClanTag>(FilterOptions.ClanTag, ClanTagsSerialization));
      handlers.Add(DivisionsSerialization, new DivisionsHandler());
      handlers.Add(NamesSerialization, new NamesHandler<Name>(FilterOptions.TeamName, NamesSerialization));
      handlers.Add(TwitterSerialization, new NamesHandler<Twitter>(FilterOptions.Twitter, TwitterSerialization));
      handlers.Add(IdHandler.IdSerialization, IdHandler);
    }

    /// <summary>
    /// The last known Battlefy Persistent Ids of the team.
    /// </summary>
    public BattlefyTeamSocial? BattlefyPersistentTeamId => BattlefyPersistentTeamIdInformation.MostRecent;

    /// <summary>
    /// The known Battlefy Persistent Ids of the team.
    /// </summary>
    public NamesHandler<BattlefyTeamSocial> BattlefyPersistentTeamIdInformation => (NamesHandler<BattlefyTeamSocial>)this[BattlefyPersistentTeamIdsSerialization];

    /// <summary>
    /// The known Battlefy Persistent Ids of the team.
    /// </summary>
    public IReadOnlyCollection<BattlefyTeamSocial> BattlefyPersistentTeamIds => BattlefyPersistentTeamIdInformation.GetItemsUnordered();

    /// <summary>
    /// The team tag(s) of the team
    /// </summary>
    public NamesHandler<ClanTag> ClanTagInformation => (NamesHandler<ClanTag>)this[ClanTagsSerialization];

    /// <summary>
    /// The team tag(s) of the team
    /// </summary>
    public IReadOnlyCollection<ClanTag> ClanTags => ClanTagInformation.GetItemsUnordered();

    /// <summary>
    /// The current division of the team
    /// </summary>
    public Division CurrentDiv => DivisionInformation.CurrentDivision ?? Division.Unknown;

    /// <summary>
    /// The division information of the team.
    /// </summary>
    public DivisionsHandler DivisionInformation => (DivisionsHandler)this[DivisionsSerialization];

    /// <summary>
    /// The last known used name for the team
    /// </summary>
    public Name Name => NamesInformation.MostRecent ?? Builtins.UnknownPlayerName;

    /// <summary>
    /// The registered names this team is known by.
    /// </summary>
    public IReadOnlyCollection<Name> Names => NamesInformation.GetItemsUnordered();

    /// <summary>
    /// The in-game or registered names this team is known by.
    /// </summary>
    public NamesHandler<Name> NamesInformation => (NamesHandler<Name>)this[NamesSerialization];

    /// <summary>
    /// The most recent tag of the team
    /// </summary>
    public ClanTag? Tag => ClanTagInformation.MostRecent;

    /// <summary>
    /// The Twitter social information this team belongs to.
    /// </summary>
    public NamesHandler<Twitter> TwitterInformation => (NamesHandler<Twitter>)this[TwitterSerialization];

    /// <summary>
    /// The Twitter social information this team belongs to.
    /// </summary>
    public IReadOnlyCollection<Twitter> TwitterProfiles => TwitterInformation.GetItemsUnordered();

    public void AddBattlefyId(string id, Source source)
      => BattlefyPersistentTeamIdInformation.Add(new BattlefyTeamSocial(id, source));

    public void AddClanTag(string tag, Source source, TagOption option = TagOption.Unknown)
      => ClanTagInformation.Add(new ClanTag(tag, source, option));

    public void AddClanTag(ClanTag tag)
      => ClanTagInformation.Add(tag);

    public void AddDivision(Division division, Source source)
      => DivisionInformation.Add(division, source);

    public void AddDivisions(DivisionsHandler value)
      => DivisionInformation.Merge(value);

    public void AddName(string name, Source source)
      => NamesInformation.Add(new Name(name, source));

    public void AddTwitter(string handle, Source source)
      => TwitterInformation.Add(new Twitter(handle, source));

    /// <summary>
    /// Get the team's best division, or null if not known.
    /// </summary>
    /// <param name="lastNDivisions">Limit the search to this many divisions in most-recent chronological order (-1 = no limit)</param>
    public Division? GetBestDiv(int lastNDivisions = -1)
      => DivisionInformation.GetBestDiv(lastNDivisions);

    /// <summary>
    /// Filter all players to return only those in this team.
    /// </summary>
    public IEnumerable<Player> GetPlayers(IEnumerable<Player> allPlayers)
    {
      return allPlayers.Where(p => p.TeamInformation.Contains(this.Id));
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