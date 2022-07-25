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

    /// <summary>Serialize</summary>
    /// <remarks>Handled in <see cref="BaseSourcedItemHandler{T}.GetObjectData(SerializationInfo, StreamingContext)"/>.</remarks>

    #endregion Serialization
  }
}