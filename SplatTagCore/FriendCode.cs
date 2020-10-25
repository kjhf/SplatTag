using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace SplatTagCore
{
  [Serializable]
  public class FriendCode
  {
    /// <summary>
    /// We have exactly 12 digits, or we have 3 lots of 4 digits separated by - or . or space. The code may be wrapped in brackets ().
    /// </summary>
    public static readonly Regex FRIEND_CODE_REGEX = new Regex(@"\(?\d{4}(-| |\.|_|/)\d{4}(-| |\.|_|/)\d{4}\)?", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

    public static readonly Regex TWELVE_DIGITS_REGEX = new Regex(@"(\D|^)(\d{12})(\D|$)", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

    [JsonProperty("FC", Required = Required.Default)]
    public short[] FC { get; private set; }

    ~
      // Add a new function that strips out a Friend code from the string, and returns the old string

      // This is the old behaviour to parse a FC from a name.

    public static bool TryParse(string value, out FriendCode outFriendCode)
    {
      outFriendCode = null;
      if (string.IsNullOrWhiteSpace(value)) return false;

      value = value.TrimStart('S', 'W', 's', 'w', 'F', 'C', 'f', 'c', '-', ':').Trim();
      if (value.Length < 12)
      {
        value = value.PadLeft(12, '0');
      }
      if (value.Length > 12)
      {
        // Remove any separators.
        value = value.Replace("-", "").Replace(".", "").Replace(" ", "").Replace("_", "").Replace("/", "").Replace("(", "").Replace(")", "");
      }

      // Filter the friend code from the value
      var fcMatch = TWELVE_DIGITS_REGEX.Match(value);
      if (fcMatch.Captures.Count != 1)
      {
        return false;
      }
      else
      {
        outFriendCode = new FriendCode
        {
          FC = new short[3]
        };
        for (int outer = 0; outer < 3; ++outer)
        {
          outFriendCode.FC[outer] = short.Parse(fcMatch.Groups[2].Value.Substring(outer * 4, 4));
        }
        return true;
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
      if (TryParse(fc, out FriendCode friendCode))
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
      return FC == null ? ("(not set)") : $"{FC[0].ToString().PadLeft(4, '0')}{separator}{FC[1]}{separator}{FC[2]}";
    }

    public override string ToString()
    {
      return ToString("-");
    }
  }
}