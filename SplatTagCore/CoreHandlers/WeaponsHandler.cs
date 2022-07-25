using NLog;
using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class WeaponsHandler :
    BaseSourcedItemHandler<WeaponsContainer>,
    ISerializable
  {
    public const string SerializationName = "Weps";
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public WeaponsHandler()
    {
    }

    public override string SerializedHandlerName => SerializationName;

    public override FilterOptions GetMatchReason() => FilterOptions.Weapon;

    #region Serialization

    // Deserialize
    protected WeaponsHandler(SerializationInfo info, StreamingContext context)
    {
      DeserializeBaseSourcedItems(info, context);
    }

    /// <summary>Serialize</summary>
    /// <remarks>Handled in <see cref="BaseSourcedItemHandler{T}.GetObjectData(SerializationInfo, StreamingContext)"/>.</remarks>

    #endregion Serialization
  }
}