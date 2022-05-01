using NLog;
using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class IdHandler : SingleValueHandler<Guid>
  {
    internal const string IdSerialization = "Id";
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Construct the IdHandler.
    /// </summary>
    internal IdHandler()
      : base(FilterOptions.SlappId, Guid.NewGuid())
    {
    }

    /// <inheritdoc/>
    public override bool HasDataToSerialize => Id != Guid.Empty;

    public Guid Id => base.Value;
    public override string SerializedName => IdSerialization;

    public override void Merge(Guid other)
    {
      // In future we may add the other Slapp ids to the handler but for now it's kinda pointless.
    }

    #region Serialization

    // Deserialize
    internal IdHandler(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    protected override void DeserialieSingleValue(SerializationInfo info, StreamingContext context)
    {
      base.DeserialieSingleValue(info, context);
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