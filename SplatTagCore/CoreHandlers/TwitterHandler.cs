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

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.SerializeNameItems(info, context);
    }

    #endregion Serialization
  }
}