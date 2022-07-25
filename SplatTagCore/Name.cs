using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class Name : ISourcedCoreObject, ISourceable, IEquatable<Name?>
  {
    public const string NameKeySerialization = "N";
    public const string NameSourceSerialization = "S";

    /// <summary>
    /// List of sources that this name has been used under
    /// </summary>
    private readonly List<Source> sources = new();

    /// <summary>
    /// Cached transformed name
    /// </summary>
    private string? transformedName;

    /// <summary>
    /// Constructor for Name that contains a piece of data that is some sort of name or tag, and the source this information comes from.
    /// </summary>
    public Name(string name, Source source)
    {
      this.Value = name;
      sources.Add(source);
    }

    /// <summary>
    /// Constructor for Name
    /// </summary>
    /// <remarks>
    /// This constructor is used by <see cref="Activator"/>.
    /// </remarks>
    public Name(string name, IEnumerable<Source> sources)
    {
      this.Value = name;
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
    public IList<Source> Sources => sources;

    /// <summary>
    /// List of sources that this name has been used under
    /// </summary>
    IReadOnlyList<Source> IReadonlySourceable.Sources => sources;

    /// <summary>
    /// The name as a transformed string
    /// </summary>
    public string Transformed => transformedName ??= Value.TransformString();

    /// <summary>
    /// The name.
    /// </summary>
    public string Value { get; protected set; }

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

    public override bool Equals(object? obj)
    {
      return Equals(obj as Name);
    }

    public bool Equals(Name? other)
    {
      return other != null &&
             Value == other.Value;
    }

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

    public string GetDisplayValue() => ToString();

    public bool Equals(ICoreObject other) => Equals(other as Name);

    #region Serialization

    // Deserialize as dict
    protected Name(SerializationInfo info, StreamingContext context)
    {
      this.Value = info.GetString(NameKeySerialization);
      var sourceIds = info.GetValueOrDefault(NameSourceSerialization, Array.Empty<string>());
      if (context.Context is Source.SourceStringConverter converter)
      {
        this.sources = converter.Convert(sourceIds).Distinct().ToList();
      }
      else
      {
        this.sources = sourceIds.Select(s => new Source(s)).Distinct().ToList();
      }
    }

    // Serialize as dict
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue(NameKeySerialization, this.Value);
      if (this.sources.Count > 0)
      {
        info.AddValue(NameSourceSerialization, this.sources.Select(s => s.Id).ToArray());
      }
    }

    #endregion Serialization
  }
}