using System;
using System.Collections.Generic;
using System.Linq;

namespace SplatTagCore
{
  public class NamesHandler<T> : SourcedItemHandler<T> where T : Name
  {
    public IEnumerable<string> TransformedNames => GetItemsUnordered().Select(n => n.Transformed);

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
    public override bool Match(SourcedItemHandler<T> other)
    {
      if (other is NamesHandler<T> otherNameHandler)
      {
        return items.Keys.NamesMatch(otherNameHandler.items.Keys);
      }
      else
      {
        return base.Match(other);
      }
    }

    public bool TransformedNamesMatch(NamesHandler<T> other)
    {
      return GetItemsUnordered().TransformedNamesMatch(other.GetItemsUnordered());
    }
  }
}