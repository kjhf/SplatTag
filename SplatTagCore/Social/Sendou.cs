using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SplatTagCore.Social
{
  [Serializable]
  public class Sendou : Social
  {
    private const string baseAddress = "sendou.ink/u";

    public Sendou(string handle, Source source)
      : base(handle, source, baseAddress)
    {
    }

    public Sendou(string handle, IEnumerable<Source> sources)
      : base(handle, sources, baseAddress)
    {
    }

    #region Serialization

    // Deserialize
    protected Sendou(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion Serialization
  }
}