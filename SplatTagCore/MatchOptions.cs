using System.Text.Json.Serialization;

namespace SplatTagCore
{
  public class MatchOptions
  {
    public MatchOptions()
    {
    }

    /// <summary>
    /// Get or set if the query should ignore the input case. Defaults to true.
    /// </summary>
    [JsonPropertyName("IgnoreCase")]
    public bool IgnoreCase { get; set; } = true;

    /// <summary>
    /// Get or set if the query is in Regex format. Defaults to false. Don't forget that Regex has a lot of characters that need escaping.
    /// </summary>
    [JsonPropertyName("QueryIsRegex")]
    public bool QueryIsRegex { get; set; } = false;

    /// <summary>
    /// Get or set if the query should recognise 'near' characters, e.g. κ should be equivalent to k. Defaults to true.
    /// </summary>
    [JsonPropertyName("NearCharacterRecognition")]
    public bool NearCharacterRecognition { get; set; } = true;

    /// <summary>
    /// Get or set the limit of the query (i.e. max number of results to return). -1 is unset (return all).
    /// </summary>
    [JsonPropertyName("Limit")]
    public int Limit { get; set; } = -1;

    /// <summary>
    /// Get or set how the query should match players and teams.
    /// </summary>
    [JsonPropertyName("FilterOptions")]
    public FilterOptions FilterOptions { get; set; } = FilterOptions.Default;
  }
}