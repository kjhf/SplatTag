using System;
using System.Collections.Generic;
using System.Linq;

namespace SplatTagCore
{
  public class Name
  {
    private readonly Lazy<string> transformedName;

    /// <summary>
    /// The name
    /// </summary>
    public string Value { get; protected set; }

    /// <summary>
    /// The name as a transformed string
    /// </summary>
    public string TransformedName => transformedName.Value;

    /// <summary>
    /// List of sources that this name has been used under
    /// </summary>
    public IReadOnlyCollection<Source> Sources => sources;

    /// <summary>
    /// List of sources that this name has been used under
    /// </summary>
    private readonly List<Source> sources = new List<Source>();

    /// <summary>
    /// Constructor for Name
    /// </summary>
    public Name(string name, Source source)
    {
      this.Value = name;
      this.transformedName = new Lazy<string>(() => Value.TransformString());
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
      this.transformedName = new Lazy<string>(() => Value.TransformString());
      this.sources.AddRange(sources.Distinct());
    }

    public void AddSources(IEnumerable<Source> sources)
    {
      SplatTagCommon.AddSources(sources, this.sources);
    }

    /// <summary>
    /// Generate Names list from a collection of strings
    /// </summary>
    /// <param name="strings"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public static List<Name> FromStrings(IEnumerable<string> strings, Source source)
    {
      List<Name> names = new List<Name>();
      foreach (var s in strings.Distinct())
      {
        names.Add(new Name(s, source));
      }
      return names;
    }

    /// <summary>
    /// Get the name's value.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return Value;
    }
  }
}