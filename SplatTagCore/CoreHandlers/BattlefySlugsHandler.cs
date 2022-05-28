using SplatTagCore.Social;
using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class BattlefySlugsHandler :
    NamesHandler<BattlefyUserSocial>,
    ISerializable
  {
    public const string SerializationName = "Slugs";
    public override string SerializedHandlerName => SerializationName;
    protected override FilterOptions NameOption => FilterOptions.BattlefySlugs;

    public BattlefySlugsHandler()
    {
    }

    #region Serialization

    public BattlefySlugsHandler(SerializationInfo info, StreamingContext context)
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