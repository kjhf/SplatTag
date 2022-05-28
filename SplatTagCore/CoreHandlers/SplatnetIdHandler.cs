using NLog;
using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class SplatnetIdHandler :
    SingleValueHandler<string?>,
    ISerializable
  {
    public const string SerializationName = "SpNetId";
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public override string SerializedName => SerializationName;

    public SplatnetIdHandler()
      : base(FilterOptions.None)
    {
    }

    public string? SplatnetId => Value;

    public override bool HasDataToSerialize => !string.IsNullOrWhiteSpace(SplatnetId);

    #region Serialization

    // Deserialize
    protected SplatnetIdHandler(SerializationInfo info, StreamingContext context)
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