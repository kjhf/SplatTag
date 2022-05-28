using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class BattlefyIdsHandler :
    NamesHandler<Name>,
    ISerializable
  {
    public const string SerializationName = "PIds";
    public override string SerializedHandlerName => SerializationName;
    protected override FilterOptions NameOption => FilterOptions.BattlefyPersistentIds;

    public BattlefyIdsHandler()
    {
    }

    #region Serialization

    public BattlefyIdsHandler(SerializationInfo info, StreamingContext context)
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