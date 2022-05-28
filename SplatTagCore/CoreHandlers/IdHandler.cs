using NLog;
using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class IdHandler :
    SingleValueHandler<Guid>,
    ISerializable
  {
    internal const string SerializationName = "Id";
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Construct the IdHandler.
    /// </summary>
    internal IdHandler()
      : base(FilterOptions.SlappId, Guid.NewGuid())
    {
    }

    /// <summary>
    /// Construct the IdHandler.
    /// </summary>
    internal IdHandler(Guid id)
      : base(FilterOptions.SlappId, id)
    {
    }

    /// <inheritdoc/>
    public override bool HasDataToSerialize => Id != Guid.Empty;

    public Guid Id => base.Value;
    public override string SerializedName => SerializationName;

    public override void Merge(Guid other)
    {
      // In future we may add the other Slapp ids to the handler but for now it's kinda pointless.
    }

    #region Serialization

    // Deserialize
    internal IdHandler(SerializationInfo info, StreamingContext context)
    {
      DeserializeSingleValue(info, context);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      SerializeSingleValue(info, context);
    }

    protected override void DeserializeSingleValue(SerializationInfo info, StreamingContext context)
    {
      base.DeserializeSingleValue(info, context);
      if (Value == Guid.Empty)
      {
        const string error = "GUID cannot be empty.";
        logger.Fatal(error);
        throw new SerializationException(error);
      }
    }

    #endregion Serialization
  }
}