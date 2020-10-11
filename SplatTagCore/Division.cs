using Newtonsoft.Json;
using System;
using System.Text;

namespace SplatTagCore
{
  [Serializable]
  /// <summary>
  /// A Division.
  /// For LUTI, X is div 0. X+ is div -1.
  /// Higher divs are LOWER in number.
  /// </summary>
  public class Division
  {
    public const int UNKNOWN = int.MaxValue;
    public const int X = 0;
    public const int X_PLUS = -1;
    public static readonly Division Unknown = new Division();

    [JsonProperty("DivType", Required = Required.Always)]
    public readonly DivType DivType = DivType.Unknown;

    [JsonProperty("Value", Required = Required.Always)]
    public readonly int Value = UNKNOWN;

    [JsonConstructor]
    public Division(int value = UNKNOWN, DivType divType = DivType.Unknown)
    {
      this.Value = value;
      this.DivType = divType;
    }

    public Division(string valueStr, DivType divType = DivType.LUTI)
    {
      this.DivType = divType;
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
        else if (valueStr.Equals("X+", System.StringComparison.OrdinalIgnoreCase))
        {
          this.Value = X_PLUS;
        }
        else if (valueStr.Equals("X", System.StringComparison.OrdinalIgnoreCase))
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

    [JsonIgnore]
    public string Name => ToString();

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
  }
}