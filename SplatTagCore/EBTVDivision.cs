namespace SplatTagCore
{
  /// <summary>
  /// An EBTV (French Leagues) Division.
  /// Higher divs are LOWER in number.
  /// </summary>
  public class EBTVDivision : IDivision
  {
    public const int UNKNOWN = int.MaxValue;

    public static readonly EBTVDivision Unknown = new EBTVDivision();
    public readonly int div = UNKNOWN;

    public string Name => ToString();

    public int Value => div;

    public EBTVDivision(int div = UNKNOWN)
    {
      this.div = div;
    }

    public EBTVDivision(string div)
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
          this.div = UNKNOWN;
        }
        else if (int.TryParse(div, out int divParse))
        {
          this.div = divParse;
        }
        else
        {
          this.div = UNKNOWN;
        }
      }
    }

    public static implicit operator int(EBTVDivision d)
    {
      return d.div;
    }

    public override string ToString()
    {
      return $"EBTV Div {div}";
    }
  }
}