using SplatTagCore.Social;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SplatTagCore
{
  public class Battlefy : IMergable<Battlefy>, IReadonlySourceable
  {
    /// <summary>Parameterless constructor</summary>
    /// <remarks>Required for serialization - do not delete.</remarks>
    public Battlefy()
    {
    }

    /// <summary>
    /// The persistent Battlefy slugs
    /// </summary>
    [JsonPropertyName("Slugs")]
    public IReadOnlyCollection<BattlefyUserSocial> Slugs
    {
      get => SlugsHandler.GetItemsUnordered();
      protected set => AddSlugs(value);
    }

    /// <summary>
    /// The persistent Battlefy slugs
    /// </summary>
    [JsonIgnore]
    public NamesHandler<BattlefyUserSocial> SlugsHandler { get; } = new();

    /// <summary>
    /// The Battlefy usernames
    /// </summary>
    [JsonPropertyName("Usernames")]
    public IReadOnlyCollection<Name> Usernames
    {
      get => UsernamesHandler.GetItemsUnordered();
      protected set => AddUsernames(value);
    }

    /// <summary>
    /// The Battlefy usernames
    /// </summary>
    [JsonIgnore]
    public NamesHandler<Name> UsernamesHandler { get; } = new();

    /// <summary>
    /// The persistent Battlefy ids
    /// </summary>
    [JsonPropertyName("PersistentIds")]
    public IReadOnlyCollection<Name> PersistentIds
    {
      get => PersistentIdsHandler.GetItemsUnordered();
      protected set => AddPersistentIds(value);
    }

    /// <summary>
    /// The Battlefy persistent ids
    /// </summary>
    [JsonIgnore]
    public NamesHandler<Name> PersistentIdsHandler { get; } = new();

    /// <summary>
    /// Combination of Battlefy slugs and ids
    /// </summary>
    [JsonIgnore]
    public IReadOnlyCollection<Name> AllNames => new List<Name>(Usernames.Concat(Slugs).Concat(PersistentIds).Distinct());

    [JsonIgnore]
    public IReadOnlyList<Source> Sources
    {
      get
      {
        var sources = new HashSet<Source>(UsernamesHandler.Sources);
        sources.UnionWith(SlugsHandler.Sources);
        sources.UnionWith(PersistentIdsHandler.Sources);
        return sources.ToList();
      }
    }

    /// <summary>
    /// Add a new Battlefy slug to the Battlefy profile
    /// </summary>
    public void AddSlug(string slug, Source source)
      => SlugsHandler.Add(new BattlefyUserSocial(slug, source));

    /// <summary>
    /// Add new Battlefy slugs to the Battlefy profile
    /// </summary>
    public void AddSlugs(IEnumerable<BattlefyUserSocial> incoming)
      => SlugsHandler.Add(incoming);

    /// <summary>
    /// Add a new Battlefy username to the Battlefy profile
    /// </summary>
    public void AddUsername(string username, Source source)
      => UsernamesHandler.Add(new Name(username, source));

    /// <summary>
    /// Add new Battlefy usernames to the Battlefy profile
    /// </summary>
    public void AddUsernames(IEnumerable<Name> incoming)
      => UsernamesHandler.Add(incoming);

    /// <summary>
    /// Add a new Battlefy persistent id to the Battlefy profile
    /// </summary>
    public void AddPersistentId(string persistentId, Source source)
      => PersistentIdsHandler.Add(new Name(persistentId, source));

    /// <summary>
    /// Add new Battlefy persistent ids to the Battlefy profile
    /// </summary>
    public void AddPersistentIds(IEnumerable<Name> incoming)
      => PersistentIdsHandler.Add(incoming);

    /// <summary>
    /// Return if this Battlefy matches another by slugs.
    /// </summary>
    public bool MatchSlugs(Battlefy other)
      => SlugsHandler.Match(other.SlugsHandler);

    /// <summary>
    /// Return if this Battlefy matches another by usernames.
    /// </summary>
    public bool MatchUsernames(Battlefy other)
      => UsernamesHandler.Match(other.UsernamesHandler);

    /// <summary>
    /// Return if this Battlefy matches another by persistent data (ids).
    /// </summary>
    public bool MatchPersistent(Battlefy other)
      => PersistentIdsHandler.Match(other.PersistentIdsHandler);

    public override string ToString()
    {
      return $"Slugs: [{string.Join(", ", SlugsHandler)}], Usernames: [{string.Join(", ", UsernamesHandler)}], Ids: [{string.Join(", ", PersistentIdsHandler)}]";
    }

    /// <summary>
    /// Merge this <see cref="Battlefy"/> instance with another.
    /// Handles Sources and timings.
    /// </summary>
    public void Merge(Battlefy other)
    {
      this.PersistentIdsHandler.Merge(other.PersistentIdsHandler);
      this.SlugsHandler.Merge(other.SlugsHandler);
      this.UsernamesHandler.Merge(other.UsernamesHandler);
    }
  }
}