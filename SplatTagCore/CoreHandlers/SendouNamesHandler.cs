using SplatTagCore.Social;
using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class SendouNamesHandler :
    NamesHandler<Sendou>,
    ISerializable
  {
    public const string SerializationName = "Send";
    public override string SerializedHandlerName => SerializationName;
    protected override FilterOptions NameOption => FilterOptions.PlayerSendou;

    public SendouNamesHandler()
    {
    }

    #region Serialization

    public SendouNamesHandler(SerializationInfo info, StreamingContext context)
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