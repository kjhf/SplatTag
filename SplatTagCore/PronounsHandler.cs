using System.Collections.Generic;
using System.Linq;

namespace SplatTagCore
{
  public class PronounsHandler : SourcedItemHandler<Pronoun>
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
  }
}