using System.Text.Json.Serialization;

namespace SplatTagCore
{
  public class Skill
  {
    /// <summary>
    /// Back-store for the μ rating.
    /// </summary>
    [JsonPropertyName("μ")]
    public readonly double mu;

    /// <summary>
    /// Back-store for the σ (SD).
    /// </summary>
    [JsonPropertyName("σ")]
    public readonly double sigma;

    public Skill()
    {
    }

    /// <summary>
    /// Get if the mu and sigma values are unset.
    /// </summary>
    public bool IsDefault => this.mu == default && this.sigma == default;

    public override string ToString()
    {
      return $"Skill: μ={mu}, σ={sigma}.";
    }
  }
}