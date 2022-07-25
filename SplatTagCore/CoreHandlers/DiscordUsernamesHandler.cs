using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class DiscordUsernamesHandler :
    NamesHandler<Name>,
    ISerializable
  {
    public const string SerializationName = "Dnames";
    public override string SerializedHandlerName => SerializationName;
    protected override FilterOptions NameOption => FilterOptions.DiscordName;

    public DiscordUsernamesHandler()
    {
    }

    #region Serialization

    public DiscordUsernamesHandler(SerializationInfo info, StreamingContext context)
    {
      base.DeserializeNameItems(info, context);
    }

    /// <summary>Serialize</summary>
    /// <remarks>Handled in <see cref="BaseSourcedItemHandler{T}.GetObjectData(SerializationInfo, StreamingContext)"/>.</remarks>

    #endregion Serialization
  }
}