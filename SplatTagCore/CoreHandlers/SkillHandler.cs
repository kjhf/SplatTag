using NLog;
using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class SkillHandler : SingleValueHandler<Skill?>
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private const string SkillSerialization = "Skill";
    public override string SerializedName => SkillSerialization;

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
      : base(info, context)
    {
    }

    #endregion Serialization
  }
}