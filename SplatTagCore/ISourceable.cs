using System.Collections.Generic;

namespace SplatTagCore
{
  internal interface ISourceable
  {
    public IList<Source> Sources { get; }
  }
}