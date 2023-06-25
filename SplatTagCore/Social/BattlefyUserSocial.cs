using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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

    [JsonConstructor]
    public BattlefyUserSocial(string battlefySlug, IEnumerable<Source> sources)
      : base(battlefySlug, sources, baseAddress)
    {
    }
  }
}