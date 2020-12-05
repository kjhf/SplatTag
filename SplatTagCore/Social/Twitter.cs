using System;

namespace SplatTagCore.Social
{
  public class Twitter : Social
  {
    protected override string SocialBaseAddress => "twitter.com";

    public Twitter(string handle, Source source)
      : base(handle, source)
    {
    }
  }
}