﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class Name : ISerializable, ISourceable, IEquatable<Name?>
  {
    /// <summary>
    /// Cached transformed name
    /// </summary>
    private string? transformedName;

    /// <summary>
    /// The name.
    /// </summary>
    public string Value { get; protected set; }

    /// <summary>
    /// The name as a transformed string
    /// </summary>
    public string Transformed => transformedName ??= Value.TransformString();

    /// <summary>
    /// List of sources that this name has been used under
    /// </summary>
    public IList<Source> Sources => sources;

    /// <summary>
    /// List of sources that this name has been used under
    /// </summary>
    private readonly List<Source> sources = new List<Source>();

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
    /// Constructor for Name
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
    /// Overridden string, get the name's value.
    /// </summary>
    public override string ToString()
    {
      return Value;
    }

    #region Serialization

    // Deserialize
    protected Name(SerializationInfo info, StreamingContext context)
    {
      this.Value = (string)info.GetValue("Value", typeof(string));
      this.sources = (List<Source>)info.GetValue("Sources", typeof(List<Source>));
    }

    // Serialize
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Value", this.Value);
      info.AddValue("Sources", this.sources);
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
      return -1937169414 + EqualityComparer<string>.Default.GetHashCode(Value);
    }

    #endregion Serialization
  }
}