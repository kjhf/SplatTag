using SplatTagCore.Social;
using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class TwitchHandler :
    NamesHandler<Twitch>,
    ISerializable
  {
    public const string SerializationName = "Twch";
    public override string SerializedHandlerName => SerializationName;
    protected override FilterOptions NameOption => FilterOptions.Twitch;

    public TwitchHandler()
    {
    }

    #region Serialization

    public TwitchHandler(SerializationInfo info, StreamingContext context)
    {
      base.DeserializeNameItems(info, context);
    }

    /// <summary>Serialize</summary>
    /// <remarks>Handled in <see cref="BaseSourcedItemHandler{T}.GetObjectData(SerializationInfo, StreamingContext)"/>.</remarks>

    #endregion Serialization
  }
}