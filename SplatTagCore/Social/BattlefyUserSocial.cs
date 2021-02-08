using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SplatTagCore.Social
{
  [Serializable]
  public class BattlefyUserSocial : Social
  {
    private const string baseAddress = "battlefy.com/users";

    public BattlefyUserSocial(string battlefySlug, Source source)
      : base(battlefySlug, source, baseAddress)
    {
    }

    public BattlefyUserSocial(string battlefySlug, IEnumerable<Source> sources)
      : base(battlefySlug, sources, baseAddress)
    {
    }

    #region Serialization

    // Deserialize
    protected BattlefyUserSocial(SerializationInfo info, StreamingContext context)
      : base(info, context, baseAddress)
    {
    }

    #endregion Serialization
  }
}