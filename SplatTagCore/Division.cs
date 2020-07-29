namespace SplatTagCore
{
  /// <summary>
  /// A division. X is div 0. X+ is div -1. Unknown is int.MaxValue.
  /// Higher divs are LOWER in number.
  /// </summary>
  public class Division
  {
    public const int X_PLUS = -1;
    public const int X = 0;
    public const int UNKNOWN = int.MaxValue;

    public static Division Unknown = new Division();
    public int div = UNKNOWN;

    public Division(int div = UNKNOWN)
    {
      this.div = div;
    }

    public Division(string div)
    {
      if (string.IsNullOrWhiteSpace(div))
      {
        // Unknown
        this.div = int.MaxValue;
      }
      else if (div.Equals("X+", System.StringComparison.OrdinalIgnoreCase))
      {
        this.div = X_PLUS;
      }
      else if (div.Equals("X", System.StringComparison.OrdinalIgnoreCase))
      {
        this.div = X;
      }
      else if (int.TryParse(div, out int divParse))
      {
        this.div = divParse;
      }
      else if (int.TryParse(div[0].ToString(), out int divParse2))
      {
        // 8U and 8M, hopefully will protect against mixed divisions in future.
        // May cause a problem for strings such as 10M (if we get to 10+ divisions).
        this.div = divParse2;
      }
      else
      {
        this.div = UNKNOWN;
      }
    }

    public static implicit operator int(Division d)
    {
      return d.div;
    }

    public override string ToString()
    {
      switch (div)
      {
        case UNKNOWN: return "Unknown";
        case X_PLUS: return "X+";
        case X: return "X";
        default: return div.ToString();
      }
    }
  }
}