using System.Collections.Generic;
using System.Linq;

namespace SplatTagCore
{
  public interface ISourceable
  {
    public IList<Source> Sources { get; }
  }

  public static class ISourceableExtensions
  {
    /// <summary>
    /// Get the ordering between two <see cref="ISourceable"/> instances based off of its most recent source.
    /// </summary>
    public static int CompareToBySourceChronology(this ISourceable sourceable, ISourceable other)
    {
      if (sourceable.Equals(other)) return 0;
      return sourceable.Sources.Max(s => s.Start).CompareTo(other.Sources.Max(s => s.Start));
    }
  }
}