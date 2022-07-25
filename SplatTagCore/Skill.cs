using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Transactions;

namespace SplatTagCore
{
  [Serializable]
  public record Skill : ICoreObject, ISerializable
  {
    private const double MU_DEFAULT = 25;
    private const double SIGMA_DEFAULT = MU_DEFAULT / 3;
    private const double BETA_DEFAULT = SIGMA_DEFAULT / 2;
    private const double TAU_DEFAULT = SIGMA_DEFAULT / 100;

    /// <summary>
    /// Back-store for the μ rating.
    /// </summary>
    private readonly double mu;

    /// <summary>
    /// Back-store for the σ (SD).
    /// </summary>
    private readonly double sigma;

    public Skill()
      : this(MU_DEFAULT, SIGMA_DEFAULT)
    {
    }

    [JsonConstructor]
    public Skill(double mu, double sigma)
    {
      this.mu = mu;
      this.sigma = sigma;
    }

    /// <summary>
    /// Seed a skill by a division
    /// </summary>
    /// <param name="division"></param>
    public Skill(Division division)
    {
      if (division.IsUnknown || division.NormalisedValue >= 9)
      {
        this.mu = MU_DEFAULT;
        this.sigma = SIGMA_DEFAULT;
      }
      else
      {
        this.mu = (9 - division.NormalisedValue) * MU_DEFAULT * 0.5;
        this.sigma = SIGMA_DEFAULT;
      }
    }

    /// <summary>
    /// Get if the mu and sigma values are unset.
    /// </summary>
    public bool IsDefault => this.mu == default && this.sigma == default;

    public override string ToString()
    {
      return $"Skill: μ={mu}, σ={sigma}.";
    }
    public string GetDisplayValue() => ToString(); // TODO - transfer Python messages over

    public bool Equals(ICoreObject other) => Equals(other as Skill);

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