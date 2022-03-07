using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  public class NamesHandler<T> : SourcedHandlerBase<T> where T : Name
  {
    public NamesHandler()
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

    /// <summary>
    /// Return if this handler matches another.
    /// </summary>
    public override bool Match(SourcedHandlerBase<T> other)
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
  }
}