using System;
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
    /// Get an IComparer orderer for two <see cref="IReadonlySourceable"/> instances based off of its most recent source.
    /// </summary>
    public static IComparer<IReadonlySourceable> GetMostRecentComparer()
    {
      return Comparer<IReadonlySourceable>.Create(CompareToBySourceDescending);
    }

    /// <summary>
    /// CompareTo function for ordering between two <see cref="IReadonlySourceable"/> instances.
    /// Orders by the Sources' start values in ascending order (oldest first).
    /// A Min function is performed on the sources start values to get the oldest.
    /// Like with CompareTo, 0 indicates equal, -1 indicates left is older than right, and 1 indicates right is older than left.
    /// </summary>
    public static int CompareToBySourceAscending(this IReadonlySourceable left, IReadonlySourceable right)
    {
      var first = left;
      var second = right;

      // Shortcut if only one source
      if (first.Sources.Count == 1 && second.Sources.Count == 1)
      {
        return CompareToBySourceInternal(first.Sources[0], second.Sources[0]);
      }

      // else
      return CompareToBySourceInternal(first.Sources.Min(s => s.Start), second.Sources.Min(s => s.Start));
    }

    /// <summary>
    /// CompareTo function for ordering between two <see cref="IReadonlySourceable"/> instances.
    /// Orders by the Sources' start values in descending order (most recent first).
    /// A Max function is performed on the sources start values to get the most recent.
    /// Like with CompareTo, 0 indicates equal, -1 indicates left is more recent than right, and 1 indicates right is more recent than left.
    /// </summary>
    public static int CompareToBySourceDescending(this IReadonlySourceable left, IReadonlySourceable right)
    {
      var first = right;
      var second = left;

      // Shortcut if only one source
      if (first.Sources.Count == 1 && second.Sources.Count == 1)
      {
        return CompareToBySourceInternal(first.Sources[0], second.Sources[0]);
      }

      // else
      return CompareToBySourceInternal(first.Sources.Max(s => s.Start), second.Sources.Max(s => s.Start));
    }

    private static int CompareToBySourceInternal(this Source x, Source y) => x.Start.CompareTo(y.Start);

    private static int CompareToBySourceInternal(this DateTime x, DateTime y) => x.CompareTo(y);
  }
}