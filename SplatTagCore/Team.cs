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
    public static readonly Team NoTeam = new Team("(Free Agent)", Builtins.BuiltinSource);
    public static readonly Team UnlinkedTeam = new Team("(UNLINKED TEAM)", Builtins.BuiltinSource);

    /// <summary>
    /// The GUID of the team.
    /// </summary>
    public readonly Guid Id = Guid.NewGuid();

    /// <summary>
    /// Back-store for the persistent ids of this team.
    /// </summary>
    /// <remarks>
    private readonly List<BattlefyTeamSocial> battlefyPersistentTeamIds = new List<BattlefyTeamSocial>();

    /// <summary>
    /// The tag(s) of the team, first is the current tag.
    /// </summary>
    private readonly List<ClanTag> clanTags = new List<ClanTag>();

    /// <summary>
    /// Back-store for the names of this team. The first element is the current name.
    /// </summary>
    /// <remarks>
    /// Though a HashSet may seem more performant, for collections with
    /// a small number of elements (under 20), List is actually better
    /// https://stackoverflow.com/questions/150750/hashset-vs-list-performance
    /// </remarks>
    private readonly List<Name> names = new List<Name>();

    /// <summary>
    /// Back-store for the Twitter Profiles of this player.
    /// </summary>
    private readonly List<Twitter> twitterProfiles = new List<Twitter>();

    /// <summary>
    /// The division information of the team.
    /// </summary>
    public DivisionsHandler DivisionInformation { get; } = new DivisionsHandler();

    /// <summary>
    /// Default construct a Team
    /// </summary>
    public Team()
    {
    }

    /// <summary>
    /// Construct a Team with their name and source
    /// </summary>
    /// <param name="ign"></param>
    /// <param name="source"></param>
    public Team(string ign, Source source)
    {
      this.names.Add(new Name(ign, source));
    }

    /// <summary>
    /// The last known Battlefy Persistent Ids of the team.
    /// </summary>
    public BattlefyTeamSocial? BattlefyPersistentTeamId => battlefyPersistentTeamIds.Count > 0 ? battlefyPersistentTeamIds[0] : null;

    /// <summary>
    /// The known Battlefy Persistent Ids of the team.
    /// </summary>
    public IReadOnlyList<Name> BattlefyPersistentTeamIds => battlefyPersistentTeamIds;

    /// <summary>
    /// The placement of the current tag (null if the team does not have a tag)
    /// </summary>
    public TagOption? ClanTagOption => Tag?.LayoutOption;

    /// <summary>
    /// The tag(s) of the team
    /// </summary>
    public IReadOnlyList<ClanTag> ClanTags => clanTags;

    /// <summary>
    /// The current division of the team
    /// </summary>
    public Division CurrentDiv => DivisionInformation.CurrentDivision ?? Division.Unknown;

    /// <summary>
    /// The last known used name for the Team
    /// </summary>
    public Name Name => names.Count > 0 ? names[0] : Builtins.UnknownTeamName;

    /// <summary>
    /// The names this player is known by.
    /// </summary>
    public IReadOnlyList<Name> Names => names;

    /// <summary>
    /// Get the current sources that make up this Team instance.
    /// </summary>
    public IReadOnlyList<Source> Sources =>
      names.SelectMany(n => n.Sources)
      .Concat(battlefyPersistentTeamIds.SelectMany(s => s.Sources))
      .Concat(clanTags.SelectMany(s => s.Sources))
      .Concat(twitterProfiles.SelectMany(s => s.Sources))
      .Distinct()
      .OrderByDescending(s => s)
      .ToList()
      ;

    /// <summary>
    /// The most recent tag of the team
    /// </summary>
    public ClanTag? Tag => ClanTags.Count > 0 ? ClanTags[0] : null;

    /// <summary>
    /// The names this player is known by transformed into searchable query.
    /// </summary>
    public IEnumerable<string> TransformedNames => Names.Select(n => n.Transformed);

    /// <summary>
    /// Get the team's Twitter profile details.
    /// </summary>
    public IReadOnlyList<Twitter> Twitter => twitterProfiles;

    public void AddBattlefyIds(IEnumerable<BattlefyTeamSocial> value)
    {
      SplatTagCommon.AddNames(value, battlefyPersistentTeamIds);
    }

    public BattlefyTeamSocial AddBattlefyId(string id, Source source)
    {
      return SplatTagCommon.AddName(new BattlefyTeamSocial(id, source), battlefyPersistentTeamIds);
    }

    public ClanTag AddClanTag(string tag, Source source, TagOption option = TagOption.Unknown)
    {
      return SplatTagCommon.AddName(new ClanTag(tag, option, source.AsEnumerable()), clanTags);
    }

    public void AddClanTags(IEnumerable<ClanTag> value)
    {
      SplatTagCommon.AddNames(value, clanTags);
    }

    public void AddDivision(Division division, Source source)
    {
      DivisionInformation.Add(division, source);
    }

    public void AddDivisions(Division value, Source source) => DivisionInformation.Add(value, source);

    public void AddDivisions(IList<Division> value, Source source) => DivisionInformation.Add(value, source);

    public void AddDivisions(DivisionsHandler value) => DivisionInformation.Merge(value);

    public void AddName(string name, Source source) => SplatTagCommon.AddName(new Name(name, source), names);

    public void AddNames(IEnumerable<Name> value) => SplatTagCommon.AddNames(value, names);

    public Twitter AddTwitter(string handle, Source source) => SplatTagCommon.AddName(new Twitter(handle, source), twitterProfiles);

    public void AddTwitterProfiles(IEnumerable<Twitter> value) => SplatTagCommon.AddNames(value, twitterProfiles);

    /// <summary>
    /// Filter all players to return only those in this team.
    /// </summary>
    public IEnumerable<Player> GetPlayers(IEnumerable<Player> allPlayers)
    {
      return allPlayers.Where(p => p.TeamInformation.Contains(this.Id));
    }

    /// <summary>
    /// Get the team's best division, or null if not known.
    /// </summary>
    /// <param name="lastNDivisions">Limit the search to this many divisions in most-recent chronological order (-1 = no limit)</param>
    public Division? GetBestDiv(int lastNDivisions = -1)
    {
      if (DivisionInformation.CurrentDivision == null)
      {
        return null;
      }
      // else
      IEnumerable<Division> divs =
        (lastNDivisions == -1) ?
          this.DivisionInformation.GetDivisionsOrdered() :
          // Take is a limit operation (does not throw if limit > count)
          this.DivisionInformation.GetDivisionsOrdered().Take(lastNDivisions);

      var bestDiv = divs.Min();
      return bestDiv.IsUnknown ? null : bestDiv;
    }

    public static Team? GetBestTeamByDiv(IEnumerable<Team> teams)
    {
      if (teams?.Any() != true)
      {
        return null;
      }

      Team? bestTeam = null;
      var currentHighestDiv = Division.Unknown;
      foreach (var team in teams)
      {
        var currentDiv = team.GetBestDiv();
        if (currentDiv != null && currentDiv < currentHighestDiv)
        {
          currentHighestDiv = currentDiv;
          bestTeam = team;
        }
      }
      return bestTeam;
    }

    /// <summary>
    /// Merge this team with another (newer) team instance
    /// </summary>
    public void Merge(Team newerTeam)
    {
      // Merge the tags.
      AddClanTags(newerTeam.clanTags);

      // Merge Twitter
      AddTwitterProfiles(newerTeam.twitterProfiles);

      // Merge divisions
      AddDivisions(newerTeam.DivisionInformation);

      // Merge the team's name(s).
      AddNames(newerTeam.names);

      // Merge the team's persistent battlefy id(s).
      AddBattlefyIds(newerTeam.battlefyPersistentTeamIds);
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
      AddBattlefyIds(info.GetValueOrDefault("BattlefyPersistentTeamIds", Array.Empty<BattlefyTeamSocial>()));
      AddClanTags(info.GetValueOrDefault("ClanTags", Array.Empty<ClanTag>()));
      AddDivisions(info.GetValueOrDefault("Divisions", new DivisionsHandler()));
      AddNames(info.GetValueOrDefault("N", Array.Empty<Name>()));
      AddTwitterProfiles(info.GetValueOrDefault("Twitter", Array.Empty<Twitter>()));

      this.Id = info.GetValueOrDefault("Id", Guid.Empty);
      if (this.Id == Guid.Empty)
      {
        throw new SerializationException($"Guid cannot be empty for team: {this.Name} from source(s) [{string.Join(", ", this.Sources)}].");
      }
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (battlefyPersistentTeamIds.Count > 0)
        info.AddValue("BattlefyPersistentTeamIds", this.battlefyPersistentTeamIds);

      if (clanTags.Count > 0)
        info.AddValue("ClanTags", this.clanTags);

      if (DivisionInformation.Count > 0)
        info.AddValue("Divisions", this.DivisionInformation);

      info.AddValue("Id", this.Id);

      if (names.Count > 0)
        info.AddValue("N", this.names);

      if (twitterProfiles.Count > 0)
        info.AddValue("Twitter", this.twitterProfiles);
    }

    #endregion Serialization
  }
}