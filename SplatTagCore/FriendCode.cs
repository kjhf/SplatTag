using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SplatTagCore
{
  [Serializable]
  public struct FriendCode : IEquatable<FriendCode>
  {
    private static readonly short[] NO_FRIEND_CODE_SHORTS = new short[3] { 0, 0, 0 };
    public static readonly FriendCode NO_FRIEND_CODE = new FriendCode(NO_FRIEND_CODE_SHORTS);

    /// <summary>
    /// We have exactly 12 digits, or we have 3 lots of 4 digits separated by - or . or space. The code may be wrapped in brackets ().
    /// </summary>
    private static readonly Regex FRIEND_CODE_REGEX = new Regex(@"\(?(SW|FC|sw|fc)?\s*(:|-)?\s?(\d{4})\s*(-| |\.|_|/)\s*(\d{4})\s*(-| |\.|_|/)\s*(\d{4})\s*\)?", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

    private static readonly Regex TWELVE_DIGITS_REGEX = new Regex(@"(\D|^)(\d{12})(\D|$)", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

    [JsonProperty("FC", Required = Required.Default)]
    public short[] FC { get; private set; }

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

        var outFriendCode = new FriendCode
        {
          FC = new short[3]
        };
        outFriendCode.FC[0] = short.Parse(fcMatch.Groups[3].Value);
        outFriendCode.FC[1] = short.Parse(fcMatch.Groups[5].Value);
        outFriendCode.FC[2] = short.Parse(fcMatch.Groups[7].Value);
        return (outFriendCode, value);
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
        var outFriendCode = new FriendCode
        {
          FC = new short[3]
        };
        for (int outer = 0; outer < 3; ++outer)
        {
          outFriendCode.FC[outer] = short.Parse(fcMatch.Groups[2].Value.Substring(outer * 4, 4));

          if (outFriendCode.FC[outer] == 0)
          {
            // Not expecting the friend code to contain 0000-
            Console.WriteLine($"Warning: Not accepting FC containing a group with all zeros, value={value}, trimmed={trimmed}");
            return (NO_FRIEND_CODE, value);
          }
        }
        return (outFriendCode, value);
      }
    }

    [JsonConstructor]
    public FriendCode(short[] fc)
    {
      if (fc == null || fc.Length != 3)
        throw new ArgumentException("FC is not length 3", nameof(fc));

      this.FC = fc;
    }

    /// <summary>
    /// String to FriendCode.
    /// </summary>
    /// <param name="toParse"></param>
    /// <exception cref="ArgumentException">String is not in the correct format. Use <see cref="TryParse(string, out FriendCode)"/>.</exception>
    public FriendCode(string fc)
    {
      if (TryParse(fc, out FriendCode friendCode) && friendCode != NO_FRIEND_CODE)
      {
        this.FC = friendCode.FC;
      }
      else
      {
        throw new ArgumentException("String was not in a valid format. Failed to parse.", nameof(fc));
      }
    }

    /// <summary>
    /// Overridden ToString, returns the <see cref="FriendCode"/> as a string separated by the specified <paramref name="separator"/>.
    /// </summary>
    public string ToString(string separator)
    {
      return FC.SequenceEqual(NO_FRIEND_CODE_SHORTS) ? ("(not set)") :
        $"{FC[0].ToString().PadLeft(4, '0')}{separator}{FC[1].ToString().PadLeft(4, '0')}{separator}{FC[2].ToString().PadLeft(4, '0')}";
    }

    /// <summary>
    /// Overridden ToString, returns the <see cref="FriendCode"/> as a string separated by -
    /// </summary>
    public override string ToString()
    {
      return ToString("-");
    }

    public override bool Equals(object? obj)
    {
      return obj is FriendCode friendCode && Equals(friendCode);
    }

    public bool Equals(FriendCode other)
    {
      return FC.SequenceEqual(other.FC);
    }

    public override int GetHashCode()
    {
      return FC[0].GetHashCode() + FC[1].GetHashCode() + FC[2].GetHashCode();
    }

    public static bool operator ==(FriendCode left, FriendCode right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(FriendCode left, FriendCode right)
    {
      return !(left == right);
    }
  }
}