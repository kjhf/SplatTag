using NLog;
using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class Top500Handler :
    SingleValueHandler<bool?>,
    ISerializable
  {
    public const string SerializationName = "T500";
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public Top500Handler()
      : base(FilterOptions.None)
    {
    }

    public override string SerializedName => SerializationName;
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