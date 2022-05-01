using System.Collections.Generic;

namespace SplatTagCore
{
  public interface ISourceable : IReadonlySourceable
  {
    new public IList<Source> Sources { get; }
  }
}