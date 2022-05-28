using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class BattlefyUsernamesHandler :
    NamesHandler<Name>,
    ISerializable
  {
    public const string SerializationName = "Usernames";
    public override string SerializedHandlerName => SerializationName;
    protected override FilterOptions NameOption => FilterOptions.BattlefyUsername;

    public BattlefyUsernamesHandler()
    {
    }

    #region Serialization

    public BattlefyUsernamesHandler(SerializationInfo info, StreamingContext context)
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