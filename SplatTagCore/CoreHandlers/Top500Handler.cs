using NLog;
using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class Top500Handler : SingleValueHandler<bool?>
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private const string Top500Serialization = "Top500";
    public override string SerializedName => Top500Serialization;

    public Top500Handler()
      : base(FilterOptions.None)
    {
    }

    public bool Top500 => Value == true;

    /// <summary>
    /// Set a top500 flag. NOT chronology compliant (because who cares).
    /// The top500 flag is sticky: once set it stays.
    /// </summary>
    public override void Merge(bool? other)
    {
      if (other == true)
      {
        Value = true;
      }
    }

    #region Serialization

    // Deserialize
    protected Top500Handler(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion Serialization
  }
}