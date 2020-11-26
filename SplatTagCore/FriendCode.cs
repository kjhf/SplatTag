using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace SplatTagCore
{
  [Serializable]
  public class FriendCode
  {
    /// <summary>
    /// We have exactly 12 digits, or we have 3 lots of 4 digits separated by - or . or space. The code may be wrapped in brackets ().
    /// </summary>
    private static readonly Regex FRIEND_CODE_REGEX = new Regex(@"\(?(SW|FC|sw|fc)?\s*(:|-)?\s?(\d{4})\s*(-| |\.|_|/)\s*(\d{4})\s*(-| |\.|_|/)\s*(\d{4})\s*\)?", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

    private static readonly Regex TWELVE_DIGITS_REGEX = new Regex(@"(\D|^)(\d{12})(\D|$)", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

    [JsonProperty("FC", Required = Required.Default)]
    public short[]? FC { get; private set; }

    /// <summary>
    /// Take a string and parse a friend code from it, returning it and true if parsed, or null and false if not.
    /// </summary>
    /// <param name="value">The string to search</param>
    /// <param name="outFriendCode">The resulting friend code.</param>
    public static bool TryParse(string value, out FriendCode? outFriendCode)
    {
      (outFriendCode, _) = ParseAndStripFriendCode(value);
      return outFriendCode != null;
    }

    /// <summary>
    /// Take a string and parse a friend code from it, returning it or null, and the input string with the friend code stripped.
    /// Returns null if a FC is not parsed.
    /// </summary>
    /// <param name="value">The string to search</param>
    /// <returns>A tuple containing the friend code and the stripped value result.</returns>
    public static (FriendCode?, string) ParseAndStripFriendCode(string value)
    {
      if (string.IsNullOrWhiteSpace(value)) return (null, value);

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
        return (null, value);
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
        }
        return (outFriendCode, value);
      }
    }

    public FriendCode()
    {
    }

    [JsonConstructor]
    public FriendCode(short[] fc)
    {
      if (fc != null && fc.Length != 3) throw new ArgumentException("FC is not length 3", nameof(fc));
      this.FC = fc;
    }

    /// <summary>
    /// String to FriendCode.
    /// </summary>
    /// <param name="toParse"></param>
    /// <exception cref="ArgumentException">String is not in the correct format. Use <see cref="TryParse(string, out FriendCode)"/>.</exception>
    public FriendCode(string fc)
    {
      if (TryParse(fc, out FriendCode? friendCode) && friendCode != null)
      {
        this.FC = friendCode.FC;
      }
      else
      {
        throw new ArgumentException("String was not in a valid format. Failed to parse.", nameof(fc));
      }
    }

    public string ToString(string separator)
    {
      return FC == null ? ("(not set)") : $"{FC[0].ToString().PadLeft(4, '0')}{separator}{FC[1].ToString().PadLeft(4, '0')}{separator}{FC[2].ToString().PadLeft(4, '0')}";
    }

    public override string ToString()
    {
      return ToString("-");
    }
  }
}