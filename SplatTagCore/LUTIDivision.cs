namespace SplatTagCore
{
  /// <summary>
  /// A LUTIDivision. X is div 0. X+ is div -1. Unknown is int.MaxValue.
  /// Higher divs are LOWER in number.
  /// </summary>
  public class LUTIDivision : IDivision
  {
    public const int X_PLUS = -1;
    public const int X = 0;
    public const int UNKNOWN = int.MaxValue;

    public static readonly LUTIDivision Unknown = new LUTIDivision();
    public readonly int div = UNKNOWN;

    public string Name => ToString();

    public int Value => div;

    public LUTIDivision(int div = UNKNOWN)
    {
      this.div = div;
    }

    public LUTIDivision(string div)
    {
      if (div == null)
      {
        this.div = UNKNOWN;
      }
      else
      {
        div = div.Trim(new char[] { '(', 'D', 'd', 'i', 'v', ')', ' ', });

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
          // 8U and 8M, hopefully will protect against mixed LUTIDivisions in future.
          // May cause a problem for strings such as 10M (if we get to 10+ LUTIDivisions).
          this.div = divParse2;
        }
        else
        {
          this.div = UNKNOWN;
        }
      }
    }

    public static implicit operator int(LUTIDivision d)
    {
      return d.div;
    }

    public override string ToString()
    {
      string val;
      switch (div)
      {
        case UNKNOWN: val = "Unknown"; break;
        case X_PLUS: val = "X+"; break;
        case X: val = "X"; break;
        default: val = div.ToString(); break;
      }
      return $"LUTI Div {val}";
    }
  }
}