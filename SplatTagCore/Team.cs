using SplatTagCore.Social;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class Team : ISerializable, IReadonlySourceable
  {
    public static readonly Team NoTeam = new("(Free Agent)", Builtins.BuiltinSource);
    public static readonly Team UnlinkedTeam = new("(UNLINKED TEAM)", Builtins.BuiltinSource);

    /// <summary>
    /// The GUID of the team.
    /// </summary>
    public readonly Guid Id = Guid.NewGuid();

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
    {
      AddName(ign, source);
    }

    /// <summary>
    /// The last known Battlefy Persistent Ids of the team.
    /// </summary>
    public BattlefyTeamSocial? BattlefyPersistentTeamId => BattlefyPersistentTeamIdInformation.MostRecent;

    /// <summary>
    /// The known Battlefy Persistent Ids of the team.
    /// </summary>
    public NamesHandler<BattlefyTeamSocial> BattlefyPersistentTeamIdInformation { get; } = new();

    /// <summary>
    /// The known Battlefy Persistent Ids of the team.
    /// </summary>
    public IReadOnlyCollection<BattlefyTeamSocial> BattlefyPersistentTeamIds => BattlefyPersistentTeamIdInformation.GetItemsUnordered();

    /// <summary>
    /// The team tag(s) of the team
    /// </summary>
    public NamesHandler<ClanTag> ClanTagInformation { get; } = new();

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
    public DivisionsHandler DivisionInformation { get; } = new DivisionsHandler();

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
    public NamesHandler<Name> NamesInformation { get; } = new();

    /// <summary>
    /// Get the current sources that make up this Team instance.
    /// </summary>
    public IReadOnlyList<Source> Sources =>
      NamesInformation.Sources
      .Concat(BattlefyPersistentTeamIdInformation.Sources)
      .Concat(ClanTagInformation.Sources)
      .Concat(TwitterInformation.Sources)
      .Distinct()
      .OrderByDescending(s => s)
      .ToList()
      ;

    /// <summary>
    /// The most recent tag of the team
    /// </summary>
    public ClanTag? Tag => ClanTagInformation.MostRecent;

    /// <summary>
    /// The Twitter social information this team belongs to.
    /// </summary>
    public NamesHandler<Twitter> TwitterInformation { get; } = new();

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
    /// Merge this team with another team instance.
    /// Chronology safe.
    /// </summary>
    public void Merge(Team other)
    {
      // Merge the tags.
      ClanTagInformation.Merge(other.ClanTagInformation);

      // Merge Twitter
      TwitterInformation.Merge(other.TwitterInformation);

      // Merge divisions
      DivisionInformation.Merge(other.DivisionInformation);

      // Merge the team's name(s).
      NamesInformation.Merge(other.NamesInformation);

      // Merge the team's persistent battlefy id(s).
      BattlefyPersistentTeamIdInformation.Merge(other.BattlefyPersistentTeamIdInformation);
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
      this.BattlefyPersistentTeamIdInformation.Add(info.GetValueOrDefault("BattlefyPersistentTeamIds", Array.Empty<BattlefyTeamSocial>()));
      this.ClanTagInformation.Add(info.GetValueOrDefault("ClanTags", Array.Empty<ClanTag>()));
      this.DivisionInformation = info.GetValueOrDefault("Divisions", new DivisionsHandler());
      this.NamesInformation.Add(info.GetValueOrDefault("N", Array.Empty<Name>()));
      this.TwitterInformation.Add(info.GetValueOrDefault("Twitter", Array.Empty<Twitter>()));

      this.Id = info.GetValueOrDefault("Id", Guid.Empty);
      if (this.Id == Guid.Empty)
      {
        throw new SerializationException($"Guid cannot be empty for team: {this.Name} from source(s) [{string.Join(", ", this.Sources)}].");
      }
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (BattlefyPersistentTeamIdInformation.Count > 0)
        info.AddValue("BattlefyPersistentTeamIds", this.BattlefyPersistentTeamIds);

      if (ClanTagInformation.Count > 0)
        info.AddValue("ClanTags", this.ClanTags);

      if (DivisionInformation.Count > 0)
        info.AddValue("Divisions", this.DivisionInformation);

      info.AddValue("Id", this.Id);

      if (NamesInformation.Count > 0)
        info.AddValue("N", this.Names);

      if (TwitterInformation.Count > 0)
        info.AddValue("Twitter", this.TwitterProfiles);
    }

    #endregion Serialization
  }
}