using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SplatTagCore
{
  [Serializable]
  public struct FriendCode : IEquatable<FriendCode>, IReadOnlyCollection<short>, ICollection<short>
  {
    public static readonly FriendCode NO_FRIEND_CODE = new();

    /// <summary>
    /// We have exactly 12 digits, or we have 3 lots of 4 digits separated by - or . or space or =. The code may be wrapped in brackets ().
    /// </summary>
    private static readonly Regex FRIEND_CODE_REGEX = new Regex(@"\(?(SW|FC|sw|fc)?\s*(:|-|=)?\s?(\d{4})\s*(-| |\.|_|/|=)\s*(\d{4})\s*(-| |\.|_|/|=)\s*(\d{4})\s*\)?", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

    private static readonly Regex TWELVE_DIGITS_REGEX = new Regex(@"(\D|^)(\d{12})(\D|$)", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
    private readonly short FCShort1;
    private readonly short FCShort2;
    private readonly short FCShort3;

    internal FriendCode(ulong fc)
    {
      var str = fc.ToString();
      if (str.Length < 9 || str.Length > 12)
      {
        string error = $"The stored FC value should be 9-12 characters [it's a friend code of 12 chars as an int without zero pad], actually {str.Length}.";
        Console.WriteLine(error);
        throw new ArgumentException(nameof(fc), error);
      }

      // Count back 4 chars
      FCShort3 = short.Parse(str.Substring(str.Length - 4, 4));
      FCShort2 = short.Parse(str.Substring(str.Length - 8, 4));
      // The first group may have had zeros at the start, in which case, the start is index 0, until the second group.
      FCShort1 = short.Parse(str.Substring(0, str.Length - 8));
    }

    [JsonConstructor]
    internal FriendCode(ICollection<short> fc)
    {
      if (fc == null)
        throw new ArgumentNullException(nameof(fc));

      short[] x = fc switch
      {
        short[] => (short[])fc,
        _ => fc.ToArray(),
      };

      if (x.Length != 3)
        throw new ArgumentException($"FC is not length 3 (actually {fc} == {x.Length})", nameof(fc));
      FCShort1 = x[0];
      FCShort2 = x[1];
      FCShort3 = x[2];
    }

    /// <summary>
    /// String to FriendCode. Will be parsed: does not have to be in standard format.
    /// </summary>
    internal FriendCode(string fc)
    {
      if (TryParse(fc, out FriendCode friendCode) && !friendCode.NoCode)
      {
        this.FCShort1 = friendCode.FCShort1;
        this.FCShort2 = friendCode.FCShort2;
        this.FCShort3 = friendCode.FCShort3;
      }
      else
      {
        throw new ArgumentException("String was not in a valid format. Failed to parse.", nameof(fc));
      }
    }

    public int Count => 3;
    public bool IsReadOnly => true;
    public readonly bool NoCode => FCShort1 == 0 && FCShort2 == 0 && FCShort3 == 0;

    /// <summary>
    /// Get the underlying friend code as an array of 3 shorts.
    /// </summary>
    private short[] FCShorts => new short[3] { FCShort1, FCShort2, FCShort3 };

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
      return !outFriendCode.NoCode;
    }

    /// <summary>
    /// Take a string and parse a friend code from it, returning it or null.
    /// </summary>
    /// <param name="value">The string to search</param>
    public static FriendCode? Parse(string value)
    {
      var (result, _) = ParseAndStripFriendCode(value);
      return result.NoCode ? null : result;
    }

    public void Add(short item)
    {
      throw new InvalidOperationException();
    }

    public void Clear()
    {
      throw new InvalidOperationException();
    }

    public bool Contains(short item)
    {
      return
        FCShort1 == item
        || FCShort2 == item
        || FCShort3 == item;
    }

    public void CopyTo(short[] array, int arrayIndex)
    {
      ((ICollection<short>)FCShorts).CopyTo(array, arrayIndex);
    }

    public override readonly bool Equals(object? obj)
    {
      return obj is FriendCode friendCode && Equals(friendCode);
    }

    public readonly bool Equals(FriendCode other)
    {
      return
        FCShort1 == other.FCShort1
        && FCShort2 == other.FCShort2
        && FCShort3 == other.FCShort3;
    }

    public IEnumerator<short> GetEnumerator()
    {
      return ((IEnumerable<short>)FCShorts).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return FCShorts.GetEnumerator();
    }

    public override readonly int GetHashCode()
    {
      return NoCode ? NO_FRIEND_CODE.GetHashCode()
          : FCShort1.GetHashCode()
          + FCShort2.GetHashCode()
          + FCShort3.GetHashCode();
    }

    public bool Remove(short item)
    {
      throw new InvalidOperationException();
    }

    /// <summary>
    /// Overridden ToString, returns the <see cref="FriendCode"/> as a string separated by the specified <paramref name="separator"/>.
    /// </summary>
    public string ToString(string separator)
    {
      return NoCode ? ("(not set)") :
        $"{FCShort1:0000}{separator}{FCShort2:0000}{separator}{FCShort3:0000}";
    }

    /// <summary>
    /// Overridden ToString, returns the <see cref="FriendCode"/> as a string separated by -
    /// </summary>
    public override string ToString()
    {
      return ToString("-");
    }

    /// <summary>
    /// Returns the <see cref="FriendCode"/> as an unseparated int.
    /// </summary>
    public ulong ToULong() => ulong.Parse(ToString(string.Empty));
  }
}