using SplatTagCore.Social;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SplatTagCore
{
  public class Team : IReadonlySourceable
  {
    [JsonIgnore]
    public static readonly Team NoTeam = new("(Free Agent)", Builtins.BuiltinSource);

    [JsonIgnore]
    public static readonly Team UnlinkedTeam = new("(UNLINKED TEAM)", Builtins.BuiltinSource);

    /// <summary>
    /// The GUID of the team.
    /// </summary>
    [JsonPropertyName("Id")]
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
    [JsonIgnore]
    public BattlefyTeamSocial? BattlefyPersistentTeamId => BattlefyPersistentTeamIdInformation.MostRecent;

    /// <summary>
    /// The known Battlefy Persistent Ids of the team.
    /// </summary>
    [JsonIgnore]
    public NamesHandler<BattlefyTeamSocial> BattlefyPersistentTeamIdInformation { get; } = new();

    /// <summary>
    /// The known Battlefy Persistent Ids of the team.
    /// </summary>
    [JsonPropertyName("BattlefyPersistentTeamIds")]
    public IReadOnlyCollection<BattlefyTeamSocial> BattlefyPersistentTeamIds
    {
      get => BattlefyPersistentTeamIdInformation.GetItemsUnordered();
      protected set => BattlefyPersistentTeamIdInformation.Add(value);
    }

    /// <summary>
    /// The team tag(s) of the team
    /// </summary>
    [JsonIgnore]
    public NamesHandler<ClanTag> ClanTagInformation { get; } = new();

    /// <summary>
    /// The team tag(s) of the team
    /// </summary>
    [JsonPropertyName("ClanTags")]
    public IReadOnlyCollection<ClanTag> ClanTags
    {
      get => ClanTagInformation.GetItemsUnordered();
      protected set => ClanTagInformation.Add(value);
    }

    /// <summary>
    /// The current division of the team
    /// </summary>
    [JsonIgnore]
    public Division CurrentDiv => DivisionInformation.CurrentDivision ?? Division.Unknown;

    /// <summary>
    /// The division information of the team.
    /// </summary>
    [JsonPropertyName("Divisions")]
    public DivisionsHandler DivisionInformation { get; } = new DivisionsHandler();

    /// <summary>
    /// The last known used name for the team
    /// </summary>
    [JsonIgnore]
    public Name Name => NamesInformation.MostRecent ?? Builtins.UnknownPlayerName;

    /// <summary>
    /// The registered names this team is known by.
    /// </summary>
    [JsonPropertyName("N")]
    public IReadOnlyCollection<Name> Names
    {
      get => NamesInformation.GetItemsUnordered();
      protected set => NamesInformation.Add(value);
    }

    /// <summary>
    /// The in-game or registered names this team is known by.
    /// </summary>
    [JsonIgnore]
    public NamesHandler<Name> NamesInformation { get; } = new();

    /// <summary>
    /// Get the current sources that make up this Team instance.
    /// </summary>
    [JsonIgnore]
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
    [JsonIgnore]
    public ClanTag? Tag => ClanTagInformation.MostRecent;

    /// <summary>
    /// The Twitter social information this team belongs to.
    /// </summary>
    [JsonIgnore]
    public NamesHandler<Twitter> TwitterInformation { get; } = new();

    /// <summary>
    /// The Twitter social information this team belongs to.
    /// </summary>
    [JsonPropertyName("Twitter")]
    public IReadOnlyCollection<Twitter> TwitterProfiles
    {
      get => TwitterInformation.GetItemsUnordered();
      protected set => TwitterInformation.Add(value);
    }

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
  }
}