using NLog;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class WeaponsHandler : BaseSourcedItemHandler<List<string>>, ISerializable
  {
    public const string SerializationName = "Weps";
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public override string SerializedHandlerName => SerializationName;

    public WeaponsHandler()
      : base()
    {
    }

    #region Serialization

    // Deserialize
    protected WeaponsHandler(SerializationInfo info, StreamingContext context)
    {
      DeserializeBaseSourcedItems(info, context);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      SerializeBaseSourcedItems(info, context);
    }

    public override FilterOptions GetMatchReason() => FilterOptions.Weapon;

    #endregion Serialization
  }
}