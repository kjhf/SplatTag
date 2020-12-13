using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SplatTagCore.Social
{
  [Serializable]
  public class BattlefySocial : Social
  {
    private const string baseAddress = "battlefy.com/users";

    public BattlefySocial(string handle, Source source)
      : base(handle, source, baseAddress)
    {
    }

    public BattlefySocial(string handle, IEnumerable<Source> sources)
      : base(handle, sources, baseAddress)
    {
    }

    #region Serialization

    // Deserialize
    protected BattlefySocial(SerializationInfo info, StreamingContext context)
      : base(info, context, baseAddress)
    {
    }

    #endregion Serialization
  }
}