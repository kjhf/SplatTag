using System;
using System.Collections.Generic;

namespace SplatTagCore.Social
{
  [Serializable]
  public class Twitch : Social
  {
    private const string baseAddress = "twitch.tv";

    public Twitch(string handle, Source source)
      : base(handle, source, baseAddress)
    {
    }

    public Twitch(string handle, IEnumerable<Source> sources)
      : base(handle, sources, baseAddress)
    {
    }
  }
}