using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

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

    #region Serialization

    // Deserialize
    protected Twitch(SerializationInfo info, StreamingContext context)
      : base(info, context, baseAddress)
    {
    }

    #endregion Serialization
  }
}