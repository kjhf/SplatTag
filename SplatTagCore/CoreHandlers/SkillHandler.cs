using NLog;
using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class SkillHandler :
    SingleValueHandler<Skill?>,
    ISerializable
  {
    public const string SerializationName = "Skill";
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public override string SerializedHandlerName => SerializationName;

    public SkillHandler()
      : base(FilterOptions.None)
    {
    }

    public Skill? Skill => Value;

    public override bool HasDataToSerialize => Skill?.IsDefault == false;

    public override void Merge(Skill? other)
    {
      // If the incoming skill is not default, accept it.
      if (other?.IsDefault == false)
      {
        Value = other;
      }
    }

    #region Serialization

    // Deserialize
    protected SkillHandler(SerializationInfo info, StreamingContext context)
    {
      DeserializeSingleValue(info, context);
    }

    /// <summary>Serialize</summary>
    /// <remarks>Handled in <see cref="SingleValueHandler{T}.GetObjectData(SerializationInfo, StreamingContext)"/>.</remarks>

    #endregion Serialization
  }
}