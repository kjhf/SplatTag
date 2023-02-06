using System;
using System.Collections.Generic;

namespace SplatTagCore.Social
{
  [Serializable]
  public class PlusMembership : Social
  {
    private const string baseAddress = "sendou.ink/plus/history/";

    public int? Level
    {
      get
      {
        try
        {
          // Handle is in form p/yyyy/M where p is the plus membership (1/2/3/null), yyyy is the year (2020-), M is the month (1-12)
          return int.Parse(Handle.Split('/')[0]);
        }
        catch (SystemException)
        {
          return null;
        }
      }
    }

    public DateTime? Date
    {
      get
      {
        try
        {
          // Handle is in form p/yyyy/M where p is the plus membership (1/2/3/null), yyyy is the year (2020-), M is the month (1-12)
          var parts = Handle.Split('/');
          return new DateTime(year: int.Parse(parts[1]), month: int.Parse(parts[2]), day: 1);
        }
        catch (SystemException)
        {
          return null;
        }
      }
    }

    public PlusMembership(string handle, Source source)
      : base(handle, source, baseAddress)
    {
    }

    public PlusMembership(string handle, IEnumerable<Source> sources)
      : base(handle, sources, baseAddress)
    {
    }

    public PlusMembership(int? plusLevel, Source source)
      : base($"{plusLevel}/{source.Start.Year}/{source.Start.Month}", source, baseAddress)
    {
    }
  }
}