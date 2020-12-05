using System;

namespace SplatTagCore.Social
{
  public class Twitch : Social
  {
    protected override string SocialBaseAddress => "twitch.tv";

    public Twitch(string handle, Source source)
      : base(handle, source)
    {
    }
  }
}