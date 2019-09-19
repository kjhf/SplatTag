using System;
using System.Collections.Generic;
using System.Linq;

namespace SplatTagCore
{
  public static class Extensions
  {
    /// <summary>
    /// Get if a string contains another by <see cref="StringComparison"/>.
    /// </summary>
    public static bool Contains(this string s, string other, StringComparison comp)
    {
      return s.IndexOf(other, comp) != -1;
    }

    /// <summary>
    /// Get if a string[] contains a string by <see cref="StringComparison"/>.
    /// </summary>
    public static bool Contains(this IEnumerable<string> s, string other, StringComparison comp)
    {
      return s.Any(str => str.IndexOf(other, comp) != -1);
    }
  }
}