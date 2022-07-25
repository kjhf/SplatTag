using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public record SourcedItemContainer<T> :
    ISerializable
    where T : ICoreObject
  {
    private const string SerializedSourceName = "S";
    private const string SerializedItemName = "V";

    public string[] SourceIds => sourceIdFn().Distinct().ToArray();
    public readonly T? item;
    private readonly Func<IEnumerable<string>> sourceIdFn;

    public SourcedItemContainer(T item, Func<IEnumerable<string>> sourceIds)
    {
      this.item = item;
      this.sourceIdFn = sourceIds;
    }

    public SourcedItemContainer(ISourcedCoreObject item)
    {
      this.item = (T)item;
      this.sourceIdFn = () => item.Sources.Select(s => s.Id);
    }

    public SourcedItemContainer(T item, IEnumerable<string> sourceIds)
    {
      this.item = item;
      this.sourceIdFn = () => sourceIds;
    }

    public override int GetHashCode() => item?.GetHashCode() ?? 0;

    #region Serialization

    // Deserialize
    protected SourcedItemContainer(SerializationInfo info, StreamingContext context)
      : base()
    {
      var sourceIds = info.GetValueOrDefault(SerializedSourceName, Array.Empty<string>());
      this.sourceIdFn = () => sourceIds;
      this.item = info.GetValueOrDefault(SerializedItemName, default(T));
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (item is null) return;

      info.AddValue(SerializedSourceName, SourceIds);
      info.AddValue(SerializedItemName, this.item);
    }

    #endregion Serialization
  }
}