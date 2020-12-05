using System;
using System.Collections.Generic;

namespace SplatTagCore.Social
{
  public class Twitter : Social
  {
    protected override string SocialBaseAddress => "twitter.com";

    public Twitter(string handle, Source source)
      : base(handle, source)
    {
    }

    public Twitter(string handle, IEnumerable<Source> sources)
      : base(handle, sources)
    {
    }
  }
}