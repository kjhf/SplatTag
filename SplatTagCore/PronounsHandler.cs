using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class PronounsHandler : SourcedItemHandler<Pronoun>, ISerializable
  {
    public PronounsHandler()
    {
    }

    /// <summary>
    /// Conditionally set the pronouns.
    /// If NONE is returned from the searcher, it is not set.
    /// Returns the most recent Pronoun object (or null).
    /// </summary>
    public Pronoun? SetPronoun(string description, Source source)
    {
      var incoming = new Pronoun(description, source);
      if (incoming.value != PronounFlags.NONE)
      {
        Add(incoming, source);
      }
      return MostRecent;
    }

    public override void Add(Pronoun incoming, IList<Source> sources)
    {
      if (incoming.value == PronounFlags.NONE) return;
      base.Add(incoming, sources);
    }

    /// <summary>
    /// Add codes to this handler.
    /// </summary>
    /// <param name="incoming">Codes to add</param>
    /// <param name="source">The source these codes come from</param>
    public override void Add(IList<Pronoun> incoming, Source source)
    {
      if (incoming.Count == 0) return;
      base.Add(incoming.Where(x => x.value != PronounFlags.NONE).ToArray(), source);
    }

    #region Serialization

    // Deserialize
    protected PronounsHandler(SerializationInfo info, StreamingContext context)
    {
      Source.GuidToSourceConverter? converter = context.Context as Source.GuidToSourceConverter;
      var val = info.GetValueOrDefault("P", PronounFlags.NONE);
      var sourceString = info.GetValueOrDefault("S", "");
      var source = converter?.Convert(sourceString) ?? (string.IsNullOrWhiteSpace(sourceString) ? Builtins.ManualSource : new Source(sourceString));
      Pronoun pronoun = new(val, source);
      Add(pronoun, source);
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext _)
    {
      if (Count > 0)
      {
        // Only save the most recent.
        var item = OrderedItems.FirstOrDefault();
        info.AddValue("P", item.Key.value);
        info.AddValue("S", item.Value.Max(s => s).Id);
      }
    }

    #endregion Serialization
  }
}