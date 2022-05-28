using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class DiscordIdsHandler :
    NamesHandler<Name>,
    ISerializable
  {
    public const string SerializationName = "DIds";
    public override string SerializedHandlerName => SerializationName;
    protected override FilterOptions NameOption => FilterOptions.DiscordId;

    public DiscordIdsHandler()
    {
    }

    #region Serialization

    public DiscordIdsHandler(SerializationInfo info, StreamingContext context)
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