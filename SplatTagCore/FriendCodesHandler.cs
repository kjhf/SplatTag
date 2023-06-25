using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using static SplatTagCore.JSONConverters;

namespace SplatTagCore
{
  [Serializable]
  public class FriendCodesHandler : SourcedItemHandler<FriendCode>
  {
    public FriendCodesHandler()
    {
    }

    [JsonPropertyName("C")]
    protected Dictionary<string, string[]> Model
    {
      get => OrderedItems.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value.Select(s => s.Id).ToArray());
      set
      {
        if (GuidToSourceConverter.Instance != null)
        {
          Merge(value.ToDictionary(pair => new FriendCode(pair.Key), pair => GuidToSourceConverter.Instance.Convert(pair.Value).ToList()));
        }
        else
        {
          Merge(value.ToDictionary(pair => new FriendCode(pair.Key), pair => pair.Value.Select(s => new Source(s)).ToList()));
        }
      }
    }

    public override void Add(FriendCode incoming, IList<Source> sources)
    {
      if (incoming.NoCode) return;
      base.Add(incoming, sources);
    }

    /// <summary>
    /// Add codes to this handler.
    /// </summary>
    /// <param name="incoming">Codes to add</param>
    /// <param name="source">The source these codes come from</param>
    public override void Add(IList<FriendCode> incoming, Source source)
    {
      if (incoming.Count == 0) return;
      base.Add(incoming.Where(x => x != FriendCode.NO_FRIEND_CODE).ToArray(), source);
    }

    /// <summary>
    /// Get all the codes in an unordered collection.
    /// </summary>
    public IReadOnlyCollection<FriendCode> GetCodesUnordered() => GetItemsUnordered();
  }
}