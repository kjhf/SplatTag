using NLog;
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
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public const string SerializationName = "Slugs";
    public override string SerializedHandlerName => SerializationName;
    protected override FilterOptions NameOption => FilterOptions.BattlefySlugs;

    public BattlefySlugsHandler()
    {
      logger.Trace($"{nameof(BattlefySlugsHandler)} constructor called.");
    }

    #region Serialization

    public BattlefySlugsHandler(SerializationInfo info, StreamingContext context)
    {
      base.DeserializeNameItems(info, context);
    }

    /// <summary>Serialize</summary>
    /// <remarks>Handled in <see cref="BaseSourcedItemHandler{T}.GetObjectData(SerializationInfo, StreamingContext)"/>.</remarks>

    #endregion Serialization
  }
}