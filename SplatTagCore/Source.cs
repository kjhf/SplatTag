using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class Source : ISerializable, ITeamResolver
  {
    /// <summary>
    /// The brackets that make up the source.
    /// </summary>
    public Bracket[] Brackets { get; set; } = Array.Empty<Bracket>();

    /// <summary>
    /// The source identifier
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// The friendly name for the source
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Final placements for teams and players
    /// </summary>
    public Placement Placements { get; } = new Placement();

    /// <summary>
    /// The players that this source represents
    /// e.g. all players that have signed up to this tournament
    /// </summary>
    public Player[] Players { get; set; } = Array.Empty<Player>();

    /// <summary>
    /// The teams that this source represents
    /// e.g. all teams that have signed up to this tournament
    /// </summary>
    public Team[] Teams { get; set; } = Array.Empty<Team>();

    /// <summary>
    /// Relevant URI(s) for the source
    /// </summary>
    public Uri[] Uris { get; set; } = Array.Empty<Uri>();

    /// <summary>
    /// Construct a source with a name.
    /// </summary>
    /// <param name="name"></param>
    public Source(string name = Builtins.UNKNOWN_SOURCE)
    {
      Name = name;
    }

    /// <summary>
    /// Construct a source with an id.
    /// </summary>
    /// <param name="name"></param>
    public Source(Guid id)
    {
      Id = id;
      Name = Builtins.UNKNOWN_SOURCE;
    }

    /// <summary>
    /// Match a Team by its id.
    /// Returns <see cref="Team.UnlinkedTeam"/> if not found.
    /// </summary>
    public Team GetTeamById(Guid id)
    {
      if (id == Team.NoTeam.Id)
      {
        return Team.NoTeam;
      }
      return Array.Find(Teams, t => t.Id == id) ?? Team.UnlinkedTeam;
    }

    public override string ToString()
    {
      return Name ?? base.ToString();
    }

    #region Serialization

    // Deserialize
    protected Source(SerializationInfo info, StreamingContext context)
    {
      this.Brackets = info.GetValueOrDefault("Brackets", Array.Empty<Bracket>());
      this.Id = (Guid)info.GetValue("Id", typeof(Guid));
      this.Name = info.GetString("Name");
      this.Placements = info.GetValueOrDefault("Placements", new Placement());
      this.Players = info.GetValueOrDefault("Players", Array.Empty<Player>());
      this.Teams = info.GetValueOrDefault("Teams", Array.Empty<Team>());
      this.Uris = info.GetValueOrDefault("Uris", Array.Empty<Uri>());
    }

    // Serialize

    public class GuidToSourceConverter
    {
      private readonly Dictionary<Guid, Source> lookup;

      public GuidToSourceConverter(Dictionary<Guid, Source> lookup)
      {
        this.lookup = lookup;
      }

      public IEnumerable<Source> Convert(IEnumerable<Guid> ids)
      {
        foreach (var id in ids)
          yield return Convert(id);
      }

      public Source Convert(Guid id)
      {
        return lookup.ContainsKey(id) ? lookup[id] : Builtins.BuiltinSource;
      }
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (this.Brackets.Length > 0)
        info.AddValue("Brackets", this.Brackets);

      info.AddValue("Id", this.Id);

      if (this.Name != null)
        info.AddValue("Name", this.Name);

      if (this.Placements.PlayersByPlacement.Length > 0 || this.Placements.TeamsByPlacement.Length > 0)
        info.AddValue("Placements", this.Placements);

      if (this.Players.Length > 0)
        info.AddValue("Players", this.Players);

      if (this.Teams.Length > 0)
        info.AddValue("Teams", this.Teams);

      if (this.Uris.Length > 0)
        info.AddValue("Uris", this.Uris);
    }

    #endregion Serialization
  }
}