using SplatTagCore.Social;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class Team : ISerializable, ISourceable
  {
    public static readonly Team NoTeam = new Team("(Free Agent)", Builtins.BuiltinSource);
    public static readonly Team UnlinkedTeam = new Team("(UNLINKED TEAM)", Builtins.BuiltinSource);

    /// <summary>
    /// The GUID of the team.
    /// </summary>
    public readonly Guid Id = Guid.NewGuid();

    /// <summary>
    /// The tag(s) of the team, first is the current tag.
    /// </summary>
    private readonly List<ClanTag> clanTags = new List<ClanTag>();

    /// <summary>
    /// The division(s) of the team, first is the current.
    /// </summary>
    private readonly List<Division> divisions = new List<Division>();

    /// <summary>
    /// Back-store for the sources of this team.
    /// </summary>
    private readonly List<Source> sources = new List<Source>();

    /// <summary>
    /// Back-store for the Twitter Profiles of this player.
    /// </summary>
    private readonly List<Twitter> twitterProfiles = new List<Twitter>();

    /// <summary>
    /// Back-store for the persistent ids of this team.
    /// </summary>
    /// <remarks>
    private readonly List<Name> battlefyPersistentTeamIds = new List<Name>();

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
      this.sources.Add(source);
    }

    /// <summary>
    /// The last known Battlefy Persistent Ids of the team.
    /// </summary>
    public Name? BattlefyPersistentTeamId => battlefyPersistentTeamIds.Count > 0 ? battlefyPersistentTeamIds[0] : null;

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
    public Division CurrentDiv => divisions.Any() ? divisions[0] : Division.Unknown;

    /// <summary>
    /// The divisions of the team
    /// </summary>
    public IList<Division> Divisions => divisions;

    /// <summary>
    /// The last known used name for the Team
    /// </summary>
    public Name Name => names.Count > 0 ? names[0] : Builtins.UnknownTeamName;

    /// <summary>
    /// The names this player is known by.
    /// </summary>
    public IReadOnlyList<Name> Names => names;

    /// <summary>
    /// List of sources that this name has been used under
    /// </summary>
    public IList<Source> Sources => sources;

    /// <summary>
    /// The most recent tag of the team
    /// </summary>
    public ClanTag? Tag => ClanTags.Count > 0 ? ClanTags[0] : null;

    /// <summary>
    /// The names this player is known by transformed into searchable query.
    /// </summary>
    public IReadOnlyList<string> TransformedNames => Names.Select(n => n.Transformed).ToArray();

    /// <summary>
    /// Get the team's Twitter profile details.
    /// </summary>
    public IReadOnlyList<Twitter> Twitter => twitterProfiles;

    public void AddBattlefyIds(IEnumerable<Name> value)
    {
      SplatTagCommon.AddNames(value, battlefyPersistentTeamIds);
    }

    public Name AddBattlefyId(string id, Source source)
    {
      return SplatTagCommon.AddName(new Name(id, source), battlefyPersistentTeamIds);
    }

    public ClanTag AddClanTag(string tag, Source source, TagOption option = TagOption.Unknown)
    {
      return SplatTagCommon.AddName(new ClanTag(tag, option, source.AsEnumerable()), clanTags);
    }

    public void AddClanTags(IEnumerable<ClanTag> value)
    {
      SplatTagCommon.AddNames(value, clanTags);
    }

    public void AddDivision(Division division)
    {
      if (division != Division.Unknown && (division != CurrentDiv || !division.Season.Equals(CurrentDiv.Season)))
      {
        SplatTagCommon.InsertFrontUnique(division, this.divisions);
      }
    }

    public void AddDivisions(IEnumerable<Division> value)
    {
      foreach (var div in value.Reverse())
      {
        AddDivision(div);
      }
    }

    public void AddName(string name, Source source)
    {
      SplatTagCommon.AddName(new Name(name, source), names);
    }

    public void AddNames(IEnumerable<Name> value)
    {
      SplatTagCommon.AddNames(value, names);
    }

    public void AddSources(IEnumerable<Source> value)
    {
      SplatTagCommon.AddSources(value, sources);
    }

    public Twitter AddTwitter(string handle, Source source)
    {
      return SplatTagCommon.AddName(new Twitter(handle, source), twitterProfiles);
    }

    public void AddTwitterProfiles(IEnumerable<Twitter> value)
    {
      SplatTagCommon.AddNames(value, twitterProfiles);
    }

    /// <summary>
    /// Filter all players to return only those in this team.
    /// </summary>
    public IEnumerable<Player> GetPlayers(IEnumerable<Player> allPlayers)
    {
      return allPlayers.Where(p => p.Teams.Contains(this.Id));
    }

    /// <summary>
    /// Merge this team with another (newer) team instance
    /// </summary>
    /// <param name="newerTeam"></param>
    public void Merge(Team newerTeam)
    {
      // Merge the tags.
      AddClanTags(newerTeam.clanTags);

      // Merge Twitter
      AddTwitterProfiles(newerTeam.twitterProfiles);

      // Merge divisions
      AddDivisions(newerTeam.Divisions);

      // Merge the team's name(s).
      AddNames(newerTeam.names);

      // Merge the team's persistent battlefy id(s).
      AddBattlefyIds(newerTeam.battlefyPersistentTeamIds);

      // Merge the sources.
      AddSources(newerTeam.sources);
    }

    /// <summary>
    /// Overridden ToString.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return $"{Tag} {Name} ({CurrentDiv})";
    }

    #region Serialization

    // Deserialize
    protected Team(SerializationInfo info, StreamingContext context)
    {
      AddBattlefyIds(info.GetValueOrDefault("BattlefyPersistentTeamIds", Array.Empty<Name>()));
      AddClanTags(info.GetValueOrDefault("ClanTags", Array.Empty<ClanTag>()));
      AddDivisions(info.GetValueOrDefault("Divisions", Array.Empty<Division>()));
      this.Id = (Guid)info.GetValue("Id", typeof(Guid));
      AddNames(info.GetValueOrDefault("Names", Array.Empty<Name>()));
      AddSources(info.GetValueOrDefault("Sources", Array.Empty<Source>()));
      AddTwitterProfiles(info.GetValueOrDefault("Twitter", Array.Empty<Twitter>()));
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (battlefyPersistentTeamIds.Any())
        info.AddValue("BattlefyPersistentTeamIds", this.battlefyPersistentTeamIds);

      if (clanTags.Any())
        info.AddValue("ClanTags", this.clanTags);

      if (divisions.Any())
        info.AddValue("Divisions", this.divisions);

      info.AddValue("Id", this.Id);

      if (names.Any())
        info.AddValue("Names", this.names);

      if (sources.Any())
        info.AddValue("Sources", this.sources);

      if (twitterProfiles.Any())
        info.AddValue("Twitter", this.twitterProfiles);
    }

    #endregion Serialization
  }
}