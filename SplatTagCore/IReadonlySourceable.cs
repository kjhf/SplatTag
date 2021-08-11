using System.Collections.Generic;
using System.Linq;

namespace SplatTagCore
{
  public interface IReadonlySourceable
  {
    public IReadOnlyList<Source> Sources { get; }
  }

  public static class IReadonlySourceableExtensions
  {
    /// <summary>
    /// Get the ordering between two <see cref="ISourceable"/> instances based off of its most recent source.
    /// </summary>
    public static int CompareToBySourceChronology(this IReadonlySourceable sourceable, IReadonlySourceable other)
    {
      if (sourceable.Equals(other)) return 0;
      return sourceable.Sources.Max(s => s.Start).CompareTo(other.Sources.Max(s => s.Start));
    }
  }
}