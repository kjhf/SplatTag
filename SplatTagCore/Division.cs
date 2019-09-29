namespace SplatTagCore
{
  /// <summary>
  /// A division. X is div 0. Unknown is -1.
  /// </summary>
  public class Division
  {
    public static Division Unknown = new Division();
    public int div = -1;

    public Division(int div = -1)
    {
      this.div = div;
    }

    public Division(string div)
    {
      if (string.IsNullOrWhiteSpace(div))
      {
        // Unknown
        this.div = -1;
      }
      else if (div.Equals("X", System.StringComparison.OrdinalIgnoreCase))
      {
        this.div = 0;
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
        this.div = -1;
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
        case -1: return "Unknown";
        case 0: return "X";
        default: return div.ToString();
      }
    }
  }
}