using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SplatTagCore
{
  public static class StringExtensions
  {
    /// <summary>
    /// Get if a string contains another by <see cref="StringComparison"/>.
    /// </summary>
    public static bool Contains(this string s, string other, StringComparison comp)
    {
      return s.IndexOf(other, comp) != -1;
    }

    /// <summary>
    /// Get the number of times a string contains another string.
    /// </summary>
    public static int Count(this string s, string other)
      => Regex.Matches(s, Regex.Escape(other)).Count;

    /// <summary>
    /// Get the number of times a string contains a character.
    /// </summary>
    public static int Count(this string s, char other)
      => s.Count(c => c == other);
  }
}