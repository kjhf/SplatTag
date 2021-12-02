using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SplatTagCore
{
  [Serializable]
  public readonly struct FriendCode : IEquatable<FriendCode>, IReadOnlyCollection<short>
  {
    /// <summary>
    /// We have exactly 12 digits, or we have 3 lots of 4 digits separated by - or . or space or =. The code may be wrapped in brackets ().
    /// </summary>
    private static readonly Regex FRIEND_CODE_REGEX = new Regex(@"\(?(SW|FC|sw|fc)?\s*(:|-|=)?\s?(\d{4})\s*(-| |\.|_|/|=)\s*(\d{4})\s*(-| |\.|_|/|=)\s*(\d{4})\s*\)?", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

    private static readonly short[] NO_FRIEND_CODE_SHORTS = new short[3] { 0, 0, 0 };

    private static readonly Regex TWELVE_DIGITS_REGEX = new Regex(@"(\D|^)(\d{12})(\D|$)", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

    public static readonly FriendCode NO_FRIEND_CODE = new FriendCode(NO_FRIEND_CODE_SHORTS);

    public FriendCode(ulong fc)
    {
      this.FCShorts = new short[3];
      var str = fc.ToString();
      if (str.Length < 10 || str.Length > 12)
      {
        throw new ArgumentException(nameof(fc), $"The stored FC value should be 9-12 characters [it's a friend code of 12 chars as an int without zero pad], actually {str.Length}.");
      }

      // Count back 4 chars
      FCShorts[2] = short.Parse(str.Substring(str.Length - 4, 4));
      FCShorts[1] = short.Parse(str.Substring(str.Length - 8, 4));
      // The first group may have had zeros at the start, in which case, the start is index 0, until the second group.
      FCShorts[0] = short.Parse(str.Substring(0, 12 - str.Length));
    }

    [JsonConstructor]
    internal FriendCode(ICollection<short> fc)
    {
      if (fc == null || fc.Count != 3)
        throw new ArgumentException($"FC is not length 3 (actually {fc} == {fc?.Count})", nameof(fc));

      this.FCShorts = fc is short[] x ? x : fc.ToArray();
    }

    /// <summary>
    /// String to FriendCode.
    /// </summary>
    /// <exception cref="ArgumentException">String is not in the correct format. Use <see cref="TryParse(string, out FriendCode)"/>.</exception>
    internal FriendCode(string fc)
    {
      if (TryParse(fc, out FriendCode friendCode) && friendCode != NO_FRIEND_CODE)
      {
        this.FCShorts = friendCode.FCShorts;
      }
      else
      {
        throw new ArgumentException("String was not in a valid format. Failed to parse.", nameof(fc));
      }
    }

    /// <summary>
    /// Get the underlying friend code as an array of 3 shorts.
    /// </summary>
    private readonly short[] FCShorts { get; }

    public readonly int Count => 3;

    public readonly bool IsReadOnly => true;

    public static bool operator !=(FriendCode left, FriendCode right)
    {
      return !(left == right);
    }

    public static bool operator ==(FriendCode left, FriendCode right)
    {
      return left.Equals(right);
    }

    /// <summary>
    /// Take a string and parse a friend code from it, returning it or NO_FRIEND_CODE, and the input string with the friend code stripped.
    /// Returns NO_FRIEND_CODE if a FC is not parsed.
    /// </summary>
    /// <param name="value">The string to search</param>
    /// <returns>A tuple containing the friend code and the stripped value result.</returns>
    public static (FriendCode, string) ParseAndStripFriendCode(string value)
    {
      if (string.IsNullOrWhiteSpace(value)) return (NO_FRIEND_CODE, value);

      // Extract the FC from the regex and return the stripped
      Match fcMatch = FRIEND_CODE_REGEX.Match(value);
      if (fcMatch.Success)
      {
        value = FRIEND_CODE_REGEX.Replace(value, "").Trim();

        short[] outFriendCode = new short[3]
        {
          short.Parse(fcMatch.Groups[3].Value),
          short.Parse(fcMatch.Groups[5].Value),
          short.Parse(fcMatch.Groups[7].Value)
        };
        return (new FriendCode(outFriendCode), value);
      }

      // If the regex didn't match, we'll try to match through digits instead.
      string trimmed = value.TrimStart('S', 'W', 's', 'w', 'F', 'C', 'f', 'c', '-', ':', '-', '(', ' ').TrimEnd(' ', ')', '\n');
      if (trimmed.Length < 12)
      {
        trimmed = trimmed.PadLeft(12, '0');
      }
      if (trimmed.Length > 12)
      {
        // Remove any separators.
        trimmed = trimmed.Replace("-", "").Replace(".", "").Replace(" ", "").Replace("_", "").Replace("/", "").Replace("(", "").Replace(")", "");
      }

      // Filter the friend code from the value
      fcMatch = TWELVE_DIGITS_REGEX.Match(trimmed);
      if (fcMatch.Captures.Count != 1)
      {
        return (NO_FRIEND_CODE, value);
      }
      else
      {
        short[] outFriendCode = new short[3];
        for (int outer = 0; outer < 3; ++outer)
        {
          outFriendCode[outer] = short.Parse(fcMatch.Groups[2].Value.Substring(outer * 4, 4));

          if (outFriendCode[outer] == 0)
          {
            // Not expecting the friend code to contain 0000-
            Console.WriteLine($"Warning: Not accepting FC containing a group with all zeros, value={value}, trimmed={trimmed}");
            return (NO_FRIEND_CODE, value);
          }
        }
        return (new FriendCode(outFriendCode), value);
      }
    }

    /// <summary>
    /// Take a string and parse a friend code from it, returning it and true if parsed, or NO_FRIEND_CODE and false if not.
    /// </summary>
    /// <param name="value">The string to search</param>
    /// <param name="outFriendCode">The resulting friend code.</param>
    public static bool TryParse(string value, out FriendCode outFriendCode)
    {
      (outFriendCode, _) = ParseAndStripFriendCode(value);
      return outFriendCode != NO_FRIEND_CODE;
    }

    public override readonly bool Equals(object? obj)
    {
      return obj is FriendCode friendCode && Equals(friendCode);
    }

    public readonly bool Equals(FriendCode other)
    {
      return FCShorts.SequenceEqual(other.FCShorts);
    }

    public override readonly int GetHashCode()
    {
      return FCShorts[0].GetHashCode() + FCShorts[1].GetHashCode() + FCShorts[2].GetHashCode();
    }

    /// <summary>
    /// Overridden ToString, returns the <see cref="FriendCode"/> as a string separated by the specified <paramref name="separator"/>.
    /// </summary>
    public readonly string ToString(string separator)
    {
      return FCShorts.SequenceEqual(NO_FRIEND_CODE_SHORTS) ? ("(not set)") :
        $"{FCShorts[0]:0000}{separator}{FCShorts[1]:0000}{separator}{FCShorts[2]:0000}";
    }

    /// <summary>
    /// Overridden ToString, returns the <see cref="FriendCode"/> as a string separated by -
    /// </summary>
    public override readonly string ToString()
    {
      return ToString("-");
    }

    public readonly void Add(short item)
    {
      throw new InvalidOperationException();
    }

    public readonly void Clear()
    {
      throw new InvalidOperationException();
    }

    public readonly bool Contains(short item)
    {
      return ((ICollection<short>)FCShorts).Contains(item);
    }

    public readonly void CopyTo(short[] array, int arrayIndex)
    {
      ((ICollection<short>)FCShorts).CopyTo(array, arrayIndex);
    }

    public readonly bool Remove(short item)
    {
      throw new InvalidOperationException();
    }

    public readonly IEnumerator<short> GetEnumerator()
    {
      return ((IEnumerable<short>)FCShorts).GetEnumerator();
    }

    readonly IEnumerator IEnumerable.GetEnumerator()
    {
      return FCShorts.GetEnumerator();
    }
  }
}