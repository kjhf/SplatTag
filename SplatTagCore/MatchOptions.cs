using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class MatchOptions : ISerializable
  {
    public MatchOptions()
    {
    }

    /// <summary>
    /// Get or set if the query should ignore the input case. Defaults to true.
    /// </summary>
    public bool IgnoreCase { get; set; } = true;

    /// <summary>
    /// Get or set if the query is in Regex format. Defaults to false. Don't forget that Regex has a lot of characters that need escaping.
    /// </summary>
    public bool QueryIsRegex { get; set; } = false;

    /// <summary>
    /// Get or set if the query should recognise 'near' characters, e.g. κ should be equivalent to k. Defaults to true.
    /// </summary>
    public bool NearCharacterRecognition { get; set; } = true;

    /// <summary>
    /// Get or set how the query should match players and teams.
    /// </summary>
    public FilterOptions FilterOptions { get; set; } = FilterOptions.Default;

    #region Serialization

    // Deserialize
    protected MatchOptions(SerializationInfo info, StreamingContext context)
    {
      this.IgnoreCase = info.GetBoolean("IgnoreCase");
      this.QueryIsRegex = info.GetBoolean("QueryIsRegex");
      this.NearCharacterRecognition = info.GetBoolean("NearCharacterRecognition");
      this.FilterOptions = info.GetEnumOrDefault("FilterOptions", FilterOptions.Default);
    }

    // Serialize
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("IgnoreCase", this.IgnoreCase);
      info.AddValue("QueryIsRegex", this.QueryIsRegex);
      info.AddValue("NearCharacterRecognition", this.NearCharacterRecognition);

      if (this.FilterOptions != FilterOptions.Default)
      {
        info.AddValue("FilterOptions", this.FilterOptions);
      }
    }

    #endregion Serialization
  }
}