using System;
using System.Collections.Generic;

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
  }
}