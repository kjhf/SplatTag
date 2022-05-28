using SplatTagCore.Social;
using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class TwitchHandler :
    NamesHandler<Twitch>,
    ISerializable
  {
    public const string SerializationName = "Twch";
    public override string SerializedHandlerName => SerializationName;
    protected override FilterOptions NameOption => FilterOptions.Twitch;

    public TwitchHandler()
    {
    }

    #region Serialization

    public TwitchHandler(SerializationInfo info, StreamingContext context)
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