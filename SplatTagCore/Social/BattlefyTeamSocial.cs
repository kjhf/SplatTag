using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SplatTagCore.Social
{
  public class BattlefyTeamSocial : Social
  {
    private const string baseAddress = "battlefy.com/teams";

    public BattlefyTeamSocial(string teamPersistentId, Source source)
      : base(teamPersistentId, source, baseAddress)
    {
    }

    [JsonConstructor]
    public BattlefyTeamSocial(string teamPersistentId, IEnumerable<Source> sources)
      : base(teamPersistentId, sources, baseAddress)
    {
    }
  }
}