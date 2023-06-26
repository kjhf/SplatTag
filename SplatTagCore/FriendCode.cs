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
    /// <summary>
    /// Representation of a no code part.
    /// </summary>
    /// <remarks>
    /// This code cannot happen (-1 = 65535 > 9999)
    /// </remarks>
    private const short NO_CODE_DIGIT = -1;

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

    public FriendCode()
    {
      FCShort1 = FCShort2 = FCShort3 = NO_CODE_DIGIT;
    }

    /// <summary>
    /// Constructor for individual parts.
    /// </summary>
    internal FriendCode(short fcShort1, short fcShort2, short fcShort3)
    {
      this.FCShort1 = (fcShort1 is > 0 and <= 9999) ? fcShort1 : throw new ArgumentOutOfRangeException(nameof(fcShort1), $"The first code short is out of range (0 > {fcShort1} > 9999).");
      this.FCShort2 = (fcShort2 is > 0 and <= 9999) ? fcShort2 : throw new ArgumentOutOfRangeException(nameof(fcShort2), $"The second code short is out of range (0 > {fcShort2} > 9999).");
      this.FCShort3 = (fcShort3 is > 0 and <= 9999) ? fcShort3 : throw new ArgumentOutOfRangeException(nameof(fcShort3), $"The third code short is out of range (0 > {fcShort3} > 9999).");
    }

    /// <summary>
    /// Friend code object that needs to be read
    /// </summary>
    public FriendCode(params object[] args)
    {
      FriendCode temp;
      switch (args.Length)
      {
        case 0:
          temp = NO_FRIEND_CODE;
          break;

        case 1:
        {
          var fc = args[0];
          temp = fc switch
          {
            string str => FromString(str),
            ulong num => FromNumber(num),
            long num => FromNumber((ulong)num),
            int num => FromNumber((ulong)num),
            IEnumerable<short> collection => FromArray(collection as ICollection<short> ?? collection.ToArray()),
            IEnumerable<int> collection => FromArray((collection as ICollection<int> ?? collection.ToArray()).Cast<short>().ToArray()),
            _ => throw new ArgumentException(
              "fc is in an invalid format. Expected a string, ulong, or array of 3 shorts. " +
              "Actually: " + fc.GetType().Name, nameof(fc)),
          };
          break;
        }

        case 3:
          temp = new FriendCode((short)args[0], (short)args[1], (short)args[2]);
          break;

        default:
          throw new ArgumentException(
                "args is in an invalid format. Expected a single string, ulong, or array of 3 shorts, or 3 shorts. " +
                "Actually: " + args.GetType().Name, nameof(args));
      }
      FCShort1 = temp.FCShort1;
      FCShort2 = temp.FCShort2;
      FCShort3 = temp.FCShort3;
    }

    /// <summary>
    /// String to FriendCode. Will be parsed: does not have to be in standard format.
    /// </summary>
    public static FriendCode FromString(string str)
    {
      if (TryParse(str, out FriendCode friendCode) && !friendCode.NoCode)
      {
        return new FriendCode(friendCode.FCShort1, friendCode.FCShort2, friendCode.FCShort3);
      }
      // else
      throw new ArgumentException("String was not in a valid format. Failed to parse.", nameof(str));
    }

    /// <summary>
    /// Unpadded number to FriendCode. Must be 9-12 characters.
    /// </summary>
    /// <param name="fc">The friend code of 12 chars as an int (zero padding not needed).</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static FriendCode FromNumber(ulong fc)
    {
      var str = fc.ToString();
      if (str.Length is < 9 or > 12)
      {
        string error = $"The FC value should be 9-12 characters [it's a friend code of 12 chars as an int without zero pad], actually {str.Length}.";
        logger.Error(error);
        throw new ArgumentOutOfRangeException(nameof(fc), error);
      }

      return new FriendCode(
        // Count backwards as
        // the first group may have had zeros at the start, in which case, the start is index 0, until the second group.
        short.Parse(str[..^8]),
        short.Parse(str.Substring(str.Length - 8, 4)),
        short.Parse(str.Substring(str.Length - 4, 4))
      );
    }

    /// <summary>
    /// Array of 3 shorts to FriendCode.
    /// </summary>
    /// <param name="fc">The friend code of 3 shorts, each [1-9999].</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static FriendCode FromArray(ICollection<short> fc)
    {
      if (fc is null || fc.Count == 0)
      {
        return NO_FRIEND_CODE;
      }

      if (fc.Count != 3)
      {
        throw new ArgumentOutOfRangeException($"FC is not length 3 (actually {fc.Count})", nameof(fc));
      }

      // else
      IList<short> x = fc as IList<short> ?? fc.ToArray();
      return new FriendCode(x[0], x[1], x[2]);
    }

    [JsonIgnore]
    public int Count => 3;

    [JsonIgnore]
    public bool IsReadOnly => true;

    [JsonIgnore]
    public bool NoCode => FCShort1 == NO_CODE_DIGIT && FCShort2 == NO_CODE_DIGIT && FCShort3 == NO_CODE_DIGIT;

    [JsonPropertyName("FC")]
    /// <summary>
    /// Get the underlying friend code as an array of 3 shorts.
    /// </summary>
    public short[] Code => new short[3] { FCShort1, FCShort2, FCShort3 };

    /// <summary>
    /// Parse a friend code from given input.
    /// Returns the friend code and the string value stripped (i.e. the string that is not part of the friend code).
    /// If no friend code was found, returns NO_FRIEND_CODE and the input value.
    /// </summary>
    /// <param name="value">The string to search</param>
    public static (FriendCode, string) ParseAndStripFriendCode(string value)
    {
      (FriendCode, string) reject = (NO_FRIEND_CODE, value);

      // If no data, reject
      if (string.IsNullOrWhiteSpace(value))
      {
        return reject;
      }

      // Attempt to parse as number
      if (ulong.TryParse(value, out var numericCode))
      {
        try
        {
          return (new FriendCode(numericCode), value);
        }
        catch (ArgumentOutOfRangeException)
        {
          // The friend code was read successfully, but is invalid
          return reject;
        }
      }

      // Extract the FC from the regex and return the stripped
      Match fcMatch = FRIEND_CODE_REGEX.Match(value);
      if (fcMatch.Success)
      {
        // Create the FC from the matched groups
        short[] matched = new short[3]
        {
          short.Parse(fcMatch.Groups[3].Value),
          short.Parse(fcMatch.Groups[5].Value),
          short.Parse(fcMatch.Groups[7].Value)
        };

        try
        {
          value = FRIEND_CODE_REGEX.Replace(value, "").Trim();
          return (new FriendCode(matched), value);
        }
        catch (ArgumentOutOfRangeException)
        {
          // The friend code was read successfully, but is invalid
          return reject;
        }
      }

      // If the regex didn't match, we'll try to strip away more input and match based on digits
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
        return reject;
      }

      // else
      short[] outFriendCode = new short[3];
      for (int outer = 0; outer < 3; ++outer)
      {
        outFriendCode[outer] = short.Parse(fcMatch.Groups[2].Value.Substring(outer * 4, 4));

        if (outFriendCode[outer] == 0)
        {
          // Not expecting the friend code to contain 0000-
          string error = $"Not accepting FC containing a group with all zeros, value={value}, trimmed={trimmed}";
          logger.Warn(error);
          return reject;
        }
      }
      return (new FriendCode(outFriendCode), value);
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
  }
}