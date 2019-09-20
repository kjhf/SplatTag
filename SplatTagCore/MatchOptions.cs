namespace SplatTagCore
{
  public class MatchOptions
  {
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
  }
}
