using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SplatTagCore
{
  public class Discord : IMergable<Discord>, IReadonlySourceable
  {
    /// <summary>Parameterless constructor</summary>
    /// <remarks>Required for serialization - do not delete.</remarks>
    public Discord()
    {
    }

    [JsonIgnore]
    public static readonly Regex DISCORD_NAME_REGEX = new(@"\(?.*#[0-9]{4}\)?", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

    /// <summary>
    /// The Discord ids
    /// </summary>
    [JsonPropertyName("Ids")]
    public IReadOnlyCollection<Name> Ids
    {
      get => IdsHandler.GetItemsUnordered();
      protected set => AddIds(value);
    }

    /// <summary>
    /// The Discord ids
    /// </summary>
    [JsonIgnore]
    public NamesHandler<Name> IdsHandler { get; } = new();

    /// <summary>
    /// The Discord usernames
    /// </summary>
    [JsonPropertyName("Usernames")]
    public IReadOnlyCollection<Name> Usernames
    {
      get => UsernamesHandler.GetItemsUnordered();
      protected set => AddUsernames(value);
    }

    /// <summary>
    /// The Discord usernames
    /// </summary>
    [JsonIgnore]
    public NamesHandler<Name> UsernamesHandler { get; } = new();

    [JsonIgnore]
    public IReadOnlyList<Source> Sources
    {
      get
      {
        var sources = new HashSet<Source>(IdsHandler.Sources);
        sources.UnionWith(UsernamesHandler.Sources);
        return sources.ToList();
      }
    }

    /// <summary>
    /// Add a new Discord id to the front of this profile
    /// </summary>
    public void AddId(string slug, Source source)
      => IdsHandler.Add(new Name(slug, source));

    /// <summary>
    /// Add Discord ids to this Discord profile
    /// </summary>
    /// <param name="ids"></param>
    public void AddIds(IEnumerable<Name> ids)
      => IdsHandler.Add(ids);

    /// <summary>
    /// Add a new Discord name to the front of this profile
    /// </summary>
    /// <param name="ids"></param>
    public void AddUsername(string username, Source source)
      => UsernamesHandler.Add(new Name(username, source));

    /// <summary>
    /// Add Discord usernames to this Discord profile
    /// </summary>
    public void AddUsernames(IEnumerable<Name> incoming)
      => UsernamesHandler.Add(incoming);

    /// <summary>
    /// Return if this Discord matches another by persistent data (ids).
    /// </summary>
    public bool MatchPersistent(Discord other)
      => IdsHandler.Match(other.IdsHandler);

    /// <summary>
    /// Return if this Discord matches by username
    /// </summary>
    public bool MatchUsernames(Discord other)
      => UsernamesHandler.Match(other.UsernamesHandler);

    /// <summary>
    /// Merge this <see cref="Discord"/> instance with another.
    /// Handles Sources and timings.
    /// </summary>
    public void Merge(Discord other)
    {
      this.IdsHandler.Merge(other.IdsHandler);
      this.UsernamesHandler.Merge(other.UsernamesHandler);
    }

    public override string ToString()
    {
      return $"Ids: [{string.Join(", ", Ids)}], Usernames: [{string.Join(", ", Usernames)}]";
    }
  }
}