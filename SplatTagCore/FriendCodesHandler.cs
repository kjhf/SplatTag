using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class FriendCodesHandler : SourcedItemHandlerBase<FriendCode>, ISerializable
  {
    public FriendCodesHandler()
    {
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

    #region Serialization

    // Deserialize
    protected FriendCodesHandler(SerializationInfo info, StreamingContext context)
    {
      Source.GuidToSourceConverter? converter = context.Context as Source.GuidToSourceConverter;
      var val = info.GetValueOrDefault("C", new Dictionary<string, List<string>>());
      Merge(val.ToDictionary(pair => new FriendCode(ulong.Parse(pair.Key)), pair => (converter?.Convert(pair.Value) ?? pair.Value.Select(s => new Source(s))).ToList()));
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext _)
    {
      if (Count > 0)
      {
        info.AddValue("C", OrderedItems.ToDictionary(pair => pair.Key.ToULong(), pair => pair.Value.Select(s => s.Id)));
      }
    }

    #endregion Serialization
  }
}