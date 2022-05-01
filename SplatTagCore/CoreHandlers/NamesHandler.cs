using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class NamesHandler<T> : BaseSourcedItemHandler<T>, ISerializable where T : Name
  {
    internal readonly string serializedName;
    private readonly FilterOptions nameOption;

    public IEnumerable<string> TransformedNames => GetItemsUnordered().Select(n => n.Transformed);

    public override string SerializedHandlerName => serializedName;

    /// <summary>
    /// Construct the NamesHandler with the context of the Name.
    /// </summary>
    /// <param name="nameOption"></param>
    public NamesHandler(FilterOptions? nameOption, string serializedName)
    {
      this.nameOption = nameOption ?? FilterOptions.None;
      this.serializedName = serializedName;
    }

    public void Add(T item)
      => Add(item, item.Sources);

    public void Add(IEnumerable<T> items)
    {
      foreach (var item in items)
      {
        Add(item);
      }
    }

    public bool Contains(string nameValue, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
    {
      return GetItemsUnordered().Any(n => n.Value.Equals(nameValue, stringComparison));
    }

    /// <summary>
    /// Return if this names handler matches another by any of its names.
    /// </summary>
    public override bool Match(BaseSourcedItemHandler<T> other)
    {
      if (MostRecent == null || other.MostRecent == null) return false;

      if (other is NamesHandler<T> otherNameHandler)
      {
        return items.Keys.NamesMatch(otherNameHandler.items.Keys);
      }
      else
      {
        return base.Match(other);
      }
    }

    /// <summary>
    /// If the Sourced Item Handler generic matches in the <see cref="BaseSourcedItemHandler{T}.MatchWithReason(object)"/> function, get the reason why.
    /// </summary>
    public override FilterOptions GetMatchReason() => nameOption;

    public bool TransformedNamesMatch(BaseSourcedItemHandler<T> other)
    {
      return GetItemsUnordered().TransformedNamesMatch(other.GetItemsUnordered());
    }

    #region Serialization

    // Deserialize
    protected NamesHandler(SerializationInfo info, StreamingContext context)
    {
      serializedName = ReadSerializedHandlerName(info);
      base.DeserializeBaseSourcedItems(info, context);
    }

    // Serialize
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.SerializeBaseSourcedItems(info, context);
    }

    #endregion Serialization

    /// <summary>
    /// ToString on <see cref="NamesHandler{T}"/> returns the serialized name and its items
    /// </summary>
    public override string ToString() => $"{serializedName}: [{string.Join(", ", GetItemsUnordered())}]";
  }
}