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
    public override string SerializedHandlerName => SerializationName;

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

    /// <summary>Serialize</summary>
    /// <remarks>Handled in <see cref="SingleValueHandler{T}.GetObjectData(SerializationInfo, StreamingContext)"/>.</remarks>

    #endregion Serialization
  }
}