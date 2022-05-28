using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  public abstract class NamesHandler<T> :
    BaseSourcedItemHandler<T>
    where T : Name
  {
    protected virtual FilterOptions NameOption { get; } = FilterOptions.None;

    public IEnumerable<string> TransformedNames => GetItemsUnordered().Select(n => n.Transformed);

    /// <summary>
    /// Default constructor for deserialization
    /// </summary>
    protected NamesHandler()
    {
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
    public override bool ItemsMatch(BaseSourcedItemHandler<T>? other)
    {
      if (other == null || MostRecent == null || other.MostRecent == null) return false;

      if (other is NamesHandler<T> otherNameHandler)
      {
        return items.Keys.NamesMatch(otherNameHandler.items.Keys);
      }
      else
      {
        return base.ItemsMatch(other);
      }
    }

    /// <summary>
    /// If the Sourced Item Handler generic matches in the <see cref="BaseSourcedItemHandler{T}.MatchWithReason(object)"/> function, get the reason why.
    /// </summary>
    public override FilterOptions GetMatchReason() => NameOption;

    public bool TransformedNamesMatch(BaseSourcedItemHandler<T> other)
    {
      return GetItemsUnordered().TransformedNamesMatch(other.GetItemsUnordered());
    }

    #region Serialization

    protected virtual void DeserializeNameItems(SerializationInfo info, StreamingContext context)
    {
      base.DeserializeBaseSourcedItems(info, context);
    }

    protected virtual void SerializeNameItems(SerializationInfo info, StreamingContext context)
    {
      base.SerializeBaseSourcedItems(info, context);
    }

    #endregion Serialization

    /// <summary>
    /// ToString on <see cref="NamesHandler{T}"/> returns the serialized name and its items
    /// </summary>
    public override string ToString() => $"{SerializedHandlerName}: [{string.Join(", ", GetItemsUnordered())}]";
  }
}