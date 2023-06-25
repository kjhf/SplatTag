using SplatTagCore.Social;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using static SplatTagCore.JSONConverters;

namespace SplatTagCore
{
  [JsonDerivedType(typeof(BattlefyTeamSocial), nameof(BattlefyTeamSocial))]
  [JsonDerivedType(typeof(BattlefyUserSocial), nameof(BattlefyUserSocial))]
  [JsonDerivedType(typeof(PlusMembership), nameof(PlusMembership))]
  [JsonDerivedType(typeof(Sendou), nameof(Sendou))]
  [JsonDerivedType(typeof(Twitch), nameof(Twitch))]
  [JsonDerivedType(typeof(Twitter), nameof(Twitter))]
  public class Name : ISourceable, IEquatable<Name?>
  {
    /// <summary>
    /// List of sources that this name has been used under
    /// </summary>
    [JsonIgnore]
    private readonly List<Source> sources = new List<Source>();

    /// <summary>
    /// Cached transformed name
    /// </summary>
    [JsonIgnore]
    private string? transformedName;

    [JsonConstructor]
    public Name()
    {
      this.Value = "(Unnamed)";
    }

    /// <summary>
    /// Constructor for Name that contains a piece of data that is some sort of name or tag, and the source this information comes from.
    /// </summary>
    public Name(string name, Source source)
    {
      this.Value = name;
      this.sources.Add(source);
    }

    /// <summary>
    /// Constructor for Name
    /// </summary>
    public Name(string ign, IEnumerable<Source> sources)
    {
      this.Value = ign;
      this.sources.AddRange(sources.Distinct());
    }

    /// <summary>
    /// Constructor for Name (source only).
    /// Derived class is responsible for setting <see cref="Value"/>.
    /// </summary>
    protected Name(Source source)
      : this(string.Empty, source)
    {
    }

    /// <summary>
    /// Constructor for Name (source only).
    /// Derived class is responsible for setting <see cref="Value"/>.
    /// </summary>
    protected Name(IEnumerable<Source> sources)
      : this(string.Empty, sources)
    {
    }

    /// <summary>
    /// List of sources that this name has been used under
    /// </summary>
    [JsonPropertyName("S")]
    [JsonConverter(typeof(SourceIdsConverter))]
    [JsonRequired]
    public IList<Source> Sources
    { get => sources; set { sources.Clear(); sources.AddRange(value); } }

    /// <summary>
    /// List of sources that this name has been used under
    /// </summary>
    [JsonIgnore]
    IReadOnlyList<Source> IReadonlySourceable.Sources => sources;

    /// <summary>
    /// The name as a transformed string
    /// </summary>
    [JsonIgnore]
    public string Transformed => transformedName ??= Value.TransformString();

    /// <summary>
    /// The name.
    /// </summary>
    [JsonPropertyName("N")]
    [JsonRequired]
    public string Value { get; set; }

    /// <summary>
    /// Generate Names list from a collection of strings
    /// </summary>
    /// <param name="strings"></param>
    /// <param name="source"></param>
    public static List<Name> FromStrings(IEnumerable<string> strings, Source source)
    {
      return (from s in strings.Distinct().Reverse()
              select new Name(s, source)).ToList();
    }

    public override bool Equals(object? obj) => Equals(obj as Name);

    public bool Equals(Name? other) => other != null && Value == other.Value;

    public override int GetHashCode()
    {
      return -1937169414 + Value.GetHashCode();
    }

    /// <summary>
    /// Overridden string, get the name's value.
    /// </summary>
    public override string ToString()
    {
      return Value ?? base.ToString();
    }
  }
}