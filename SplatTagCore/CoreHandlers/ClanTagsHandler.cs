using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class ClanTagsHandler :
    NamesHandler<ClanTag>,
    ISerializable
  {
    public const string SerializationName = "Tags";
    public override string SerializedHandlerName => SerializationName;
    protected override FilterOptions NameOption => FilterOptions.ClanTag;

    public ClanTagsHandler()
    {
    }

    #region Serialization

    public ClanTagsHandler(SerializationInfo info, StreamingContext context)
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