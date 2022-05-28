using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class TeamNamesHandler :
    NamesHandler<Name>,
    ISerializable
  {
    public const string SerializationName = "TN";
    public override string SerializedHandlerName => SerializationName;
    protected override FilterOptions NameOption => FilterOptions.TeamName;

    public TeamNamesHandler()
    {
    }

    #region Serialization

    public TeamNamesHandler(SerializationInfo info, StreamingContext context)
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