using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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

    [JsonConstructor]
    public Twitch(string handle, IEnumerable<Source> sources)
      : base(handle, sources, baseAddress)
    {
    }
  }
}