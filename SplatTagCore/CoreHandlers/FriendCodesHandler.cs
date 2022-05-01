using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class FriendCodesHandler : BaseSourcedItemHandler<FriendCode>, ISerializable
  {
    private const string SerializedFCName = "C";
    public override string SerializedHandlerName => SerializedFCName;

    public FriendCodesHandler()
    {
    }

    public override void Add(FriendCode incoming, IEnumerable<Source> sources)
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
    /// If the Sourced Item Handler generic matches in the <see cref="BaseSourcedItemHandler{T}.MatchWithReason(object)"/> function, get the reason why.
    /// </summary>
    public override FilterOptions GetMatchReason() => FilterOptions.FriendCode;

    /// <summary>
    /// Get all the codes in an unordered collection.
    /// </summary>
    public IReadOnlyCollection<FriendCode> GetCodesUnordered() => GetItemsUnordered();

    #region Serialization

    // Deserialize
    protected FriendCodesHandler(SerializationInfo info, StreamingContext context)
    {
      DeserializeBaseSourcedItems(info, context);

      //Source.SourceStringConverter? converter = context.Context as Source.SourceStringConverter;
      //var val = info.GetValueOrDefault(SerializedFCName, new Dictionary<string, List<string>>());
      //Merge(val.ToDictionary(pair => new FriendCode(ulong.Parse(pair.Key)), pair => (converter?.Convert(pair.Value) ?? pair.Value.Select(s => new Source(s))).ToList()));
    }

    // Serialize
    public override void GetObjectData(SerializationInfo info, StreamingContext _)
    {
      SerializeBaseSourcedItems(info, _);

      //if (HasDataToSerialize)
      //{
      //  info.AddValue(SerializedFCName, OrderedItems.ToDictionary(pair => pair.Key.ToULong(), pair => pair.Value.Select(s => s.Id)));
      //}
    }

    #endregion Serialization
  }
}