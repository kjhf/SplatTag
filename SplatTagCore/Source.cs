using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class Source : ISerializable
  {
    /// <summary>
    /// The brackets that make up the source.
    /// </summary>
    public IList<Bracket>? Brackets { get; set; }

    /// <summary>
    /// The source identifier
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// The friendly name for the source
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Final placements for teams and players
    /// </summary>
    public IList<Placements>? Placements { get; set; }

    /// <summary>
    /// The players that this source represents
    /// e.g. all players that have signed up to this tournament
    /// </summary>
    public IList<Player>? Players { get; set; }

    /// <summary>
    /// The teams that this source represents
    /// e.g. all teams that have signed up to this tournament
    /// </summary>
    public IList<Team>? Teams { get; set; }

    /// <summary>
    /// Relevant URI for the source
    /// </summary>
    public Uri? Uri { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public Source()
    {
    }

    /// <summary>
    /// Construct a source with a name.
    /// </summary>
    /// <param name="name"></param>
    public Source(string name)
    {
      Name = name;
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
      this.Name = info.GetValueOrDefault("Name", default(string?));
      this.Placements = info.GetValueOrDefault("Placements", Array.Empty<Placements>());
      this.Players = info.GetValueOrDefault("Players", Array.Empty<Player>());
      this.Teams = info.GetValueOrDefault("Teams", Array.Empty<Team>());
      this.Uri = info.GetValueOrDefault("Uri", default(Uri?));
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (this.Brackets != null && this.Brackets.Any())
        info.AddValue("Brackets", this.Brackets);

      info.AddValue("Id", this.Id);

      if (this.Name != null)
        info.AddValue("Name", this.Name);

      if (this.Placements != null && this.Placements.Any())
        info.AddValue("Placements", this.Placements);

      if (this.Players != null && this.Players.Any())
        info.AddValue("Players", this.Players);

      if (this.Teams != null && this.Teams.Any())
        info.AddValue("Teams", this.Teams);

      if (this.Uri != null)
        info.AddValue("Uri", this.Uri);
    }

    #endregion Serialization
  }
}