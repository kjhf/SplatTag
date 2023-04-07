using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SplatTagCore.Social
{
  [Serializable]
  public class Twitter : Social
  {
    private const string baseAddress = "twitter.com";

    public Twitter(string handle, Source source)
      : base(handle, source, baseAddress)
    {
    }

    [JsonConstructor]
    public Twitter(string handle, IEnumerable<Source> sources)
      : base(handle, sources, baseAddress)
    {
    }
  }
}