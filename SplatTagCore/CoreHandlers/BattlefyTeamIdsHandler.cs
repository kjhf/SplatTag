using SplatTagCore.Social;
using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class BattlefyTeamIdsHandler :
    NamesHandler<BattlefyTeamSocial>,
    ISerializable
  {
    public const string SerializationName = "BfyTIds";
    public override string SerializedHandlerName => SerializationName;
    protected override FilterOptions NameOption => FilterOptions.BattlefyPersistentIds;

    public BattlefyTeamIdsHandler()
    {
    }

    #region Serialization

    public BattlefyTeamIdsHandler(SerializationInfo info, StreamingContext context)
    {
      base.DeserializeNameItems(info, context);
    }

    /// <summary>Serialize</summary>
    /// <remarks>Handled in <see cref="BaseSourcedItemHandler{T}.GetObjectData(SerializationInfo, StreamingContext)"/>.</remarks>

    #endregion Serialization
  }
}