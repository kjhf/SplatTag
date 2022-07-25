using SplatTagCore.Social;
using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class TwitterHandler :
    NamesHandler<Twitter>,
    ISerializable
  {
    public const string SerializationName = "Twtr";
    public override string SerializedHandlerName => SerializationName;
    protected override FilterOptions NameOption => FilterOptions.Twitter;

    public TwitterHandler()
    {
    }

    #region Serialization

    public TwitterHandler(SerializationInfo info, StreamingContext context)
    {
      base.DeserializeNameItems(info, context);
    }

    /// <summary>Serialize</summary>
    /// <remarks>Handled in <see cref="BaseSourcedItemHandler{T}.GetObjectData(SerializationInfo, StreamingContext)"/>.</remarks>

    #endregion Serialization
  }
}