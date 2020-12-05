using System;

namespace SplatTagCore.Social
{
  public class Sendou : Social
  {
    protected override string SocialBaseAddress => "sendou.ink/u/";

    public Sendou(string handle, Source source)
      : base(handle, source)
    {
    }
  }
}