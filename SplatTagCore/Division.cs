using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Text;

namespace SplatTagCore
{
  [Serializable]
  public record Division : ICoreObject, ISerializable, IComparable<Division>, IEquatable<Division?>
  {
    public const string UNKNOWN_STR = "Div Unknown";
    public const int UNKNOWN = int.MaxValue;
    public const int X = 0;
    public const int X_PLUS = -1;
    private const string VALUE_SEPARATOR_STR = " Div ";

    public static readonly Division Unknown = new();

    public readonly DivType DivType = DivType.Unknown;
    public readonly string Season = "";
    public readonly int Value = UNKNOWN;

    /// <summary>
    /// Get if a division is unknown.
    /// </summary>
    public bool IsUnknown => this.Value == UNKNOWN || this.DivType == DivType.Unknown;

    [JsonConstructor]
    internal Division(string serialized)
    {
      if (serialized.Equals(UNKNOWN_STR))
      {
        this.Value = Unknown.Value;
        this.DivType = Unknown.DivType;
        this.Season = Unknown.Season;
      }
      else
      {
        int index = serialized.IndexOf(" ");
        if (index < 0)
        {
          this.Value = Unknown.Value;
          this.DivType = Unknown.DivType;
          this.Season = Unknown.Season;
        }
        else
        {
          var divTypeStr = serialized.Substring(0, index);
          bool parsed = Enum.TryParse(divTypeStr, out this.DivType);
          if (!parsed)
          {
            this.Value = Unknown.Value;
            this.DivType = Unknown.DivType;
            this.Season = Unknown.Season;
          }
          else
          {
            serialized = serialized.Substring(index + 1);
            index = serialized.IndexOf(VALUE_SEPARATOR_STR);
            if (index < 0)
            {
              this.Value = Unknown.Value;
              this.DivType = Unknown.DivType;
              this.Season = Unknown.Season;
            }
            else
            {
              this.Season = serialized.Substring(0, index).Trim();
              serialized = serialized.Substring(index + VALUE_SEPARATOR_STR.Length).Trim();
              this.Value = ParseValueString(serialized);
            }
          }
        }
      }
    }

    public Division(int value = UNKNOWN, DivType divType = DivType.Unknown, string season = "")
    {
      this.Value = value;
      this.DivType = divType;
      this.Season = season.Trim();
    }

    public Division(string valueStr, DivType divType, string season)
    {
      this.DivType = divType;
      this.Season = season.Trim();
      this.Value = ParseValueString(valueStr);
    }

    private static int ParseValueString(string valueStr)
    {
      if (valueStr == null)
      {
        return UNKNOWN;
      }
      else
      {
        valueStr = valueStr.Trim(new char[] { '(', 'D', 'd', 'i', 'v', ')', ' ', });

        if (string.IsNullOrWhiteSpace(valueStr))
        {
          return UNKNOWN;
        }
        else if (valueStr.Equals("X+", StringComparison.OrdinalIgnoreCase))
        {
          return X_PLUS;
        }
        else if (valueStr.Equals("X", StringComparison.OrdinalIgnoreCase))
        {
          return X;
        }
        else if (int.TryParse(valueStr, out int divParse))
        {
          return divParse;
        }
        else if (char.IsDigit(valueStr[0]) || valueStr[0] == '-')
        {
          bool negative = valueStr[0] == '-';
          StringBuilder sb = new StringBuilder();
          foreach (char c in valueStr)
          {
            if (negative) continue;
            if (char.IsDigit(c)) sb.Append(c);
          }
          valueStr = sb.ToString();
          if (int.TryParse(valueStr, out int divParse2))
          {
            return negative ? -divParse2 : divParse2;
          }
          else
          {
            return UNKNOWN;
          }
        }
        else
        {
          return UNKNOWN;
        }
      }
    }

    public string Name => ToString();

    /// <summary>
    /// Get a LUTI-equivalent value representing <see cref="Value"/>.
    /// </summary>
    public int NormalisedValue
    {
      get
      {
        switch (DivType)
        {
          case DivType.DSB:
          {
            /*
             *  DSB | LUTI
                ----------
                D1  | D1-D3
                D2  | D4-D7
                D3-8| D8
             */
            return Value switch
            {
              1 => 2,
              2 => 5,
              _ => 8,
            };
          }

          case DivType.EBTV:
            return Value + 2;

          case DivType.LUTI:
            return Value == X_PLUS ? X : Value;  // Season 11 removed X+ again. Just group X altogether.

          default:
            return UNKNOWN;
        }
      }
    }

    /// <summary>
    /// Compare left is lower (better) than right.
    /// </summary>
    public static bool operator <(Division left, Division right)
    {
      return left.CompareTo(right) == -1;
    }

    public static bool operator <=(Division left, Division right)
    {
      return left.CompareTo(right) <= 0;
    }

    /// <summary>
    /// Compare left is higher (worse) than right.
    /// </summary>
    public static bool operator >(Division left, Division right)
    {
      return left.CompareTo(right) == 1;
    }

    public static bool operator >=(Division left, Division right)
    {
      return left.CompareTo(right) >= 0;
    }

    /// <summary>
    /// Compare one Div to another. Remember, lower is better!
    /// </summary>
    public int CompareTo(Division other)
    {
      return NormalisedValue.CompareTo(other.NormalisedValue);
    }

    public string GetDisplayValue() => ToString();

    public bool Equals(ICoreObject other) => Equals(other as Division);

    public override string ToString()
    {
      if (this.Value == UNKNOWN)
      {
        return UNKNOWN_STR;
      }

      StringBuilder sb = new StringBuilder();
      sb.Append(DivType)
        .Append(" ")
        .Append(Season)
        .Append(VALUE_SEPARATOR_STR);

      switch (Value)
      {
        case UNKNOWN: sb.Append("Unknown"); break;
        case X_PLUS: sb.Append("X+"); break;
        case X: sb.Append("X"); break;
        default: sb.Append(Value); break;
      }

      return sb.ToString();
    }

    #region Serialization

    // Deserialize
    protected Division(SerializationInfo info, StreamingContext context)
    {
      this.Value = info.GetValueOrDefault("Value", UNKNOWN);
      this.DivType = info.GetEnumOrDefault("DivType", DivType.Unknown);
      this.Season = info.GetValueOrDefault("Season", "");
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Value", this.Value);
      info.AddValue("DivType", this.DivType.ToString());
      info.AddValue("Season", this.Season);
    }

    #endregion Serialization
  }
}