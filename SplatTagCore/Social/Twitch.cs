using System;
using System.Collections.Generic;

namespace SplatTagCore.Social
{
  public class Twitch : Social
  {
    protected override string SocialBaseAddress => "twitch.tv";

    public Twitch(string handle, Source source)
      : base(handle, source)
    {
    }

    public Twitch(string handle, IEnumerable<Source> sources)
      : base(handle, sources)
    {
    }
  }
}