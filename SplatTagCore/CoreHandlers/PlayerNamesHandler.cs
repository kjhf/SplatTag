using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class PlayerNamesHandler :
    NamesHandler<Name>,
    ISerializable
  {
    public const string SerializationName = "PN";
    public override string SerializedHandlerName => SerializationName;
    protected override FilterOptions NameOption => FilterOptions.PlayerName;

    public PlayerNamesHandler()
    {
    }

    #region Serialization

    public PlayerNamesHandler(SerializationInfo info, StreamingContext context)
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