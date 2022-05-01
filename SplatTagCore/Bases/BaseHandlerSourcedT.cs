using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  /// <summary>
  /// Base class for <see cref="BaseHandler{T}"/> classes that are also sourced.
  /// </summary>
  public abstract class BaseHandlerSourced<T> : BaseHandlerSourced, IMatchable<T>, IMergable<T>
  {
    protected BaseHandlerSourced()
    { }

    public abstract FilterOptions MatchWithReason(T other);

    public override FilterOptions MatchWithReason(IMatchable other) => MatchWithReason((T)other);

    public abstract void Merge(T other);

    public override void Merge(IMergable other) => Merge((T)other);
  }
}