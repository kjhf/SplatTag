using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class PronounsHandler : BaseSourcedItemHandler<Pronoun>, ISerializable
  {
    private const string SerializedPronounName = "P";
    private const string SerializedSourceName = "S";
    public override string SerializedHandlerName => SerializedPronounName;

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

    public override void Add(Pronoun incoming, IEnumerable<Source> sources)
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

    /// <summary>
    /// If the Sourced Item Handler generic matches in the <see cref="MatchWithReason(object)"/> function, get the reason why.
    /// </summary>
    public override FilterOptions GetMatchReason() => FilterOptions.None;  // Pronouns have no bearing.

    #region Serialization

    // Deserialize
    protected PronounsHandler(SerializationInfo info, StreamingContext context)
    {
      DeserializeBaseSourcedItems(info, context);

      Source.SourceStringConverter? converter = context.Context as Source.SourceStringConverter ?? new Source.SourceStringConverter();
      var val = info.GetValueOrDefault(SerializedPronounName, PronounFlags.NONE);
      var sourceString = info.GetValueOrDefault(SerializedSourceName, string.Empty);
      var resolver = string.IsNullOrWhiteSpace(sourceString) ? Source.SourceStringConverter.UseManual : Source.SourceStringConverter.ConstructSource;
      var source = converter.Convert(sourceString, resolver);
      Pronoun pronoun = new(val, source);
      Add(pronoun, source);
    }

    // Serialize
    public override void GetObjectData(SerializationInfo info, StreamingContext _)
    {
      SerializeBaseSourcedItems(info, _);

      if (HasDataToSerialize)
      {
        // Only save the most recent.
        var item = OrderedItems.FirstOrDefault();
        info.AddValue(SerializedPronounName, item.Key.value);
        info.AddValue(SerializedSourceName, item.Value.Max(s => s).Id);
      }
    }

    #endregion Serialization
  }
}