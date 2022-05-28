using SplatTagCore.Social;
using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class PlusHandler :
    NamesHandler<PlusMembership>,
    ISerializable
  {
    public const string SerializationName = "Plus";
    public override string SerializedHandlerName => SerializationName;
    protected override FilterOptions NameOption => FilterOptions.None; // Nothing to match here.

    public PlusHandler()
    {
    }

    #region Serialization

    public PlusHandler(SerializationInfo info, StreamingContext context)
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