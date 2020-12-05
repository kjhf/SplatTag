using System;
using System.Collections.Generic;

namespace SplatTagCore.Social
{
  public class Sendou : Social
  {
    protected override string SocialBaseAddress => "sendou.ink/u/";

    public Sendou(string handle, Source source)
      : base(handle, source)
    {
    }

    public Sendou(string handle, IEnumerable<Source> sources)
      : base(handle, sources)
    {
    }
  }
}