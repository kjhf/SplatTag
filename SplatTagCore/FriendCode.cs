using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SplatTagCore
{
  public record FriendCode : IEquatable<FriendCode>
  {
    [JsonIgnore]
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    [JsonIgnore]
    public static readonly FriendCode NO_FRIEND_CODE = new();

    /// <summary>
    /// We have exactly 12 digits, or we have 3 lots of 4 digits separated by - or . or space or =. The code may be wrapped in brackets ().
    /// </summary>
    [JsonIgnore]
    private static readonly Regex FRIEND_CODE_REGEX = new(@"\(?(SW|FC|sw|fc)?\s*(:|-|=)?\s?(\d{4})\s*(-| |\.|_|/|=)\s*(\d{4})\s*(-| |\.|_|/|=)\s*(\d{4})\s*\)?", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

    [JsonIgnore]
    private static readonly Regex TWELVE_DIGITS_REGEX = new(@"(\D|^)(\d{12})(\D|$)", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

    [JsonIgnore]
    private readonly short FCShort1;

    [JsonIgnore]
    private readonly short FCShort2;

    [JsonIgnore]
    private readonly short FCShort3;

    private FriendCode()
    {
    }

    internal FriendCode(ulong fc)
    {
      var str = fc.ToString();
      if (str.Length < 9 || str.Length > 12)
      {
        string error = $"The stored FC value should be 9-12 characters [it's a friend code of 12 chars as an int without zero pad], actually {str.Length}.";
        logger.Error(error);
        throw new ArgumentException(nameof(fc), error);
      }

      // Count back 4 chars
      FCShort3 = short.Parse(str.Substring(str.Length - 4, 4));
      FCShort2 = short.Parse(str.Substring(str.Length - 8, 4));
      // The first group may have had zeros at the start, in which case, the start is index 0, until the second group.
      FCShort1 = short.Parse(str.Substring(0, str.Length - 8));
    }

    internal FriendCode(ICollection<short> fc)
    {
      if (fc == null || fc.Count == 0)
      {
        FCShort1 = FCShort2 = FCShort3 = 0;
        return;
      }

      if (fc.Count != 3)
      {
        throw new ArgumentException($"FC is not length 3 (actually {fc} == {fc.Count})", nameof(fc));
      }

      // else

      short[] x = fc switch
      {
        short[] => (short[])fc,
        _ => fc.ToArray(),
      };

      FCShort1 = x[0] <= 9999 ? x[0] : throw new ArgumentOutOfRangeException(nameof(fc), $"The first code short is out of range ({x[0]} > 9999).");
      FCShort2 = x[1] <= 9999 ? x[1] : throw new ArgumentOutOfRangeException(nameof(fc), $"The second code short is out of range ({x[1]} > 9999).");
      FCShort3 = x[2] <= 9999 ? x[2] : throw new ArgumentOutOfRangeException(nameof(fc), $"The third code short is out of range ({x[2]} > 9999).");
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

    [JsonIgnore]
    public int Count => 3;

    [JsonIgnore]
    public bool IsReadOnly => true;

    [JsonIgnore]
    public bool NoCode => FCShort1 == 0 && FCShort2 == 0 && FCShort3 == 0;

    [JsonPropertyName("FC")]
    /// <summary>
    /// Get the underlying friend code as an array of 3 shorts.
    /// </summary>
    public short[] Code => new short[3] { FCShort1, FCShort2, FCShort3 };

    /// <summary>
    /// Take a string and parse a friend code from it, returning it or NO_FRIEND_CODE, and the input string with the friend code stripped.
    /// Returns NO_FRIEND_CODE if a FC is not parsed.
    /// </summary>
    /// <param name="value">The string to search</param>
    /// <returns>A tuple containing the friend code and the stripped value result.</returns>
    public static (FriendCode, string) ParseAndStripFriendCode(string value)
    {
      if (string.IsNullOrWhiteSpace(value)) return (NO_FRIEND_CODE, value);
      if (ulong.TryParse(value, out var numericCode)) return (new FriendCode(numericCode), value);

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
            string error = $"Not accepting FC containing a group with all zeros, value={value}, trimmed={trimmed}";
            logger.Warn(error);
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

    public bool Contains(short item)
    {
      return
        FCShort1 == item
        || FCShort2 == item
        || FCShort3 == item;
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

    public string GetDisplayValue() => ToString();

    public static FriendCode MakeFriendCodeFromArray(short[] code)
    {
      return new FriendCode(code);
    }
  }
}