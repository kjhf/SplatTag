using System;

namespace SplatTagCore
{
  /// <summary>
  /// Basic record of data used by the <see cref="SplatTagCore"/> back-end that is compliant with sourcing.
  /// </summary>
  public interface ISourcedCoreObject : ICoreObject, IReadonlySourceable
  {
  }
}