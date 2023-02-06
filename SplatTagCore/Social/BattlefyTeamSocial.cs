using System.Collections.Generic;

namespace SplatTagCore.Social
{
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
  }
}