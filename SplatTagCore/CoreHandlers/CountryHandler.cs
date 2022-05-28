using NLog;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class CountryHandler :
    SingleValueHandler<string>,
    ISerializable
  {
    public const string SerializationName = "Ctry";
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public override string SerializedName => SerializationName;

    public CountryHandler()
      : base(FilterOptions.None)
    {
    }

    /// <summary>
    /// Get or Set the Country.
    /// Null by default.
    /// To set this field, the value must be a two-letter abbreviation.
    /// </summary>
    public string? CountryCode
    {
      get => Value;
      set
      {
        if (value != null)
        {
          value = value.Trim();
          if (value.Length == 2)
          {
            Value = value.ToUpper();
          }
        }
      }
    }

    /// <summary>
    /// Get the emoji flag of the <see cref="CountryCode"/> specified.
    /// </summary>
    /// <remarks>
    /// Magic number is the offset '🇦' - 'A'
    /// </remarks>
    public string? CountryFlag
    {
      get
      {
        if (CountryCode == null) return null;
        return string.Concat(CountryCode.Select(x => char.ConvertFromUtf32(x + 0x1F1A5)));
      }
    }

    /// <summary>
    /// Replace a country with another. NOT chronology compliant (because who cares).
    /// If the replacement is invalid, the current is not replaced.
    /// </summary>
    /// <param name="other"></param>
    public override void Merge(string? other)
    {
      this.CountryCode = other;
    }

    #region Serialization

    // Deserialize
    protected CountryHandler(SerializationInfo info, StreamingContext context)
    {
      DeserializeSingleValue(info, context);
    }

    // Serialize
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      SerializeSingleValue(info, context);
    }

    #endregion Serialization
  }
}