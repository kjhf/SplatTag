using System.Collections.Generic;
using System.Linq;

namespace SplatTagCore
{
  public interface ISourceable : IReadonlySourceable
  {
    new public IList<Source> Sources { get; }
  }
}