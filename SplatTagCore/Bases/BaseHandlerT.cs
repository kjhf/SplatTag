using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  /// <summary>
  /// Base class for all handlers that have data that can be matched, merged, and serialized.
  /// </summary>
  public abstract class BaseHandler<T> : BaseHandler, IMatchable<T>, IMergable<T>
  {
    protected BaseHandler()
    {
    }

    public abstract FilterOptions MatchWithReason(T other);

    public override FilterOptions MatchWithReason(IMatchable other) => MatchWithReason((T)other);

    public abstract void Merge(T other);

    public override void Merge(IMergable other) => Merge((T)other);
  }
}