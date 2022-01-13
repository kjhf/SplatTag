using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class Source : ISerializable, ITeamResolver, IComparable<Source>
  {
    /// <summary>
    /// The brackets that make up the source.
    /// </summary>
    public Bracket[] Brackets { get; set; } = Array.Empty<Bracket>();

    /// <summary>
    /// The source identifier, which is its name.
    /// </summary>
    public string Id => Name;

    /// <summary>
    /// The friendly name for the source
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The players that this source represents
    /// e.g. all players that have signed up to this tournament
    /// </summary>
    public Player[] Players { get; set; } = Array.Empty<Player>();

    /// <summary>
    /// Get the start time of the source, e.g. the tourney time or when the source was created.
    /// Used in figuring out 'current' order.
    /// Defaults to <see cref="Builtins.UnknownDateTime"/>
    /// </summary>
    public DateTime Start { get; set; }

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
    /// Construct a source with a name and optional date.
    /// Date will be inferred if not specified, or set to Builtins.UnknownDateTime.
    /// </summary>
    public Source(string name = Builtins.UNKNOWN_SOURCE, DateTime? start = null)
    {
      Name = name;
      if (start == null && name.Length > "yyyy-mm-dd".Length)
      {
        string dateStr = name.Substring(0, "yyyy-mm-dd".Length);
        if (DateTime.TryParse(dateStr, out var temp))
        {
          start = temp;
        }
      }

      Start = start ?? Builtins.UnknownDateTime;
    }

    /// <summary>
    /// Compare start time to another Source's start time.
    /// 0 is same, &lt; 0 is earlier, &gt; 0 is later.
    /// </summary>
    public int CompareTo(Source other) => Start.CompareTo(other.Start);

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
      this.Name = info.GetString("Name");
      this.Players = info.GetValueOrDefault("Players", Array.Empty<Player>());
      this.Start = new DateTime(info.GetValueOrDefault("Start", Builtins.UNKNOWN_DATE_TIME_TICKS));
      this.Teams = info.GetValueOrDefault("Teams", Array.Empty<Team>());
      this.Uris = info.GetValueOrDefault("Uris", Array.Empty<Uri>());
    }

    // Serialize

    public class GuidToSourceConverter
    {
      private readonly Dictionary<string, Source> lookup;

      public GuidToSourceConverter(Dictionary<string, Source> lookup)
      {
        this.lookup = lookup;
      }

      public IEnumerable<Source> Convert(IEnumerable<string> names)
      {
        foreach (var id in names)
          yield return Convert(id);
      }

      public Source Convert(string name)
      {
        return lookup.ContainsKey(name) ? lookup[name] : Builtins.BuiltinSource;
      }
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (this.Brackets.Length > 0)
        info.AddValue("Brackets", this.Brackets);

      if (this.Name != null)
        info.AddValue("Name", this.Name);

      if (this.Players.Length > 0)
        info.AddValue("Players", this.Players);

      if (this.Start != Builtins.UnknownDateTime)
        info.AddValue("Start", this.Start.Ticks);

      if (this.Teams.Length > 0)
        info.AddValue("Teams", this.Teams);

      if (this.Uris.Length > 0)
        info.AddValue("Uris", this.Uris);
    }

    #endregion Serialization
  }
}