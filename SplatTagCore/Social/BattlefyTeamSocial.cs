using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SplatTagCore.Social
{
  [Serializable]
  public class BattlefyTeamSocial : Social
  {
    private const string baseAddress = "battlefy.com/teams";

    public BattlefyTeamSocial(string teamPersistentId, Source source)
      : base(teamPersistentId, source, baseAddress)
    {
    }

    public BattlefyTeamSocial(string teamPersistentId, IEnumerable<Source> sources)
      : base(teamPersistentId, sources, baseAddress)
    {
    }

    #region Serialization

    // Deserialize
    protected BattlefyTeamSocial(SerializationInfo info, StreamingContext context)
      : base(info, context, baseAddress)
    {
    }

    #endregion Serialization
  }
}