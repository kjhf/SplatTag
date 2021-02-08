using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class Skill : ISerializable
  {
    /// <summary>
    /// Back-store for the μ rating.
    /// </summary>
    private readonly double mu;

    /// <summary>
    /// Back-store for the σ (SD).
    /// </summary>
    private readonly double sigma;

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

    #region Serialization

    // Deserialize
    protected Skill(SerializationInfo info, StreamingContext context)
    {
      var mu_obj = info.GetValueOrDefault("μ", default(object));
      var sigma_obj = info.GetValueOrDefault("σ", default(object));
      if (mu_obj != null)
      {
        this.mu = double.Parse(mu_obj.ToString());
      }
      if (sigma_obj != null)
      {
        this.sigma = double.Parse(sigma_obj.ToString());
      }
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (!IsDefault)
      {
        info.AddValue("μ", this.mu);
        info.AddValue("σ", this.sigma);
      }
    }

    #endregion Serialization
  }
}