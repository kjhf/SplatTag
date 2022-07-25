using NLog;
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
    public IEnumerable<string> TransformedNames => GetItemsUnordered().Select(n => n.Transformed);

    public void Add(T item)
      => Add(item, item.Sources);

    public void Add(IEnumerable<T> items)
      => items.ForEach(item => Add(item));

    public bool Contains(string nameValue, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
      => GetItemsUnordered().Any(n => n.Value.Equals(nameValue, stringComparison));

    /// <summary>
    /// If the Sourced Item Handler generic matches in the <see cref="BaseSourcedItemHandler{T}.MatchWithReason(object)"/> function, get the reason why.
    /// </summary>
    public override FilterOptions GetMatchReason() => NameOption;

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
    /// ToString on <see cref="NamesHandler{T}"/> returns the serialized name and its items
    /// </summary>
    public override string ToString() => $"{nameof(NamesHandler<T>)} - {SerializedHandlerName}: [{string.Join(", ", GetItemsUnordered())}]";

    public bool TransformedNamesMatch(BaseSourcedItemHandler<T> other)
      => GetItemsUnordered().TransformedNamesMatch(other.GetItemsUnordered());

    /// <summary>
    /// Default constructor for deserialization
    /// </summary>
    protected NamesHandler()
    {
      logger.Trace($"{nameof(NamesHandler<T>)} constructor in {this.GetType()} called.");
    }

    protected virtual FilterOptions NameOption { get; } = FilterOptions.None;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    #region Serialization

    protected virtual void DeserializeNameItems(SerializationInfo info, StreamingContext context)
    {
      logger.Trace($"{nameof(DeserializeNameItems)} in {this.GetType()} called.");
      base.DeserializeBaseSourcedItems(info, context);
    }

    protected virtual object SerializeNameItems()
    {
      logger.Trace($"{nameof(SerializeNameItems)} in {this.GetType()} called.");
      return base.SerializeBaseSourcedItems();
    }

    public override object ToSerializedObject()
    {
      return SerializeNameItems();
    }

    #endregion Serialization
  }
}