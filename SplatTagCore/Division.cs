using System;
using System.Runtime.Serialization;
using System.Text;

namespace SplatTagCore
{
  [Serializable]
  /// <summary>
  /// A Division.
  /// For LUTI, X is div 0. X+ is div -1.
  /// Higher divs are LOWER in number.
  /// </summary>
  public class Division : ISerializable, IComparable<Division>
  {
    public const int UNKNOWN = int.MaxValue;
    public const int X = 0;
    public const int X_PLUS = -1;
    public static readonly Division Unknown = new Division();

    public readonly DivType DivType = DivType.Unknown;
    public readonly string Season = "";
    public readonly int Value = UNKNOWN;

    public Division(int value = UNKNOWN, DivType divType = DivType.Unknown, string season = "")
    {
      this.Value = value;
      this.DivType = divType;
      this.Season = season;
    }

    public Division(string valueStr, DivType divType, string season)
    {
      this.DivType = divType;
      this.Season = season;

      if (valueStr == null)
      {
        this.Value = UNKNOWN;
      }
      else
      {
        valueStr = valueStr.Trim(new char[] { '(', 'D', 'd', 'i', 'v', ')', ' ', });

        if (string.IsNullOrWhiteSpace(valueStr))
        {
          this.Value = UNKNOWN;
        }
        else if (valueStr.Equals("X+", StringComparison.OrdinalIgnoreCase))
        {
          this.Value = X_PLUS;
        }
        else if (valueStr.Equals("X", StringComparison.OrdinalIgnoreCase))
        {
          this.Value = X;
        }
        else if (int.TryParse(valueStr, out int divParse))
        {
          this.Value = divParse;
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
            this.Value = negative ? -divParse2 : divParse2;
          }
          else
          {
            this.Value = UNKNOWN;
          }
        }
        else
        {
          this.Value = UNKNOWN;
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
            switch (Value)
            {
              case 1: return 2;
              case 2: return 5;
              default: return 8;
            }
          }

          case DivType.EBTV:
            return Value + 2;

          case DivType.LUTI:
            return Value;

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

    public override string ToString()
    {
      if (this.Value == UNKNOWN)
      {
        return "Div Unknown";
      }
      else
      {
        StringBuilder sb = new StringBuilder();
        sb.Append(DivType);
        sb.Append(" ");
        sb.Append(Season);
        sb.Append(" Div ");
        switch (Value)
        {
          case UNKNOWN: sb.Append("Unknown"); break;
          case X_PLUS: sb.Append("X+"); break;
          case X: sb.Append("X"); break;
          default: sb.Append(Value); break;
        }
        return sb.ToString();
      }
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