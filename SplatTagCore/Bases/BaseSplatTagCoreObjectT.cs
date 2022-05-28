using System;

namespace SplatTagCore
{
  public abstract class BaseSplatTagCoreObject<T> :
    BaseHandlerCollectionSourced<T>,
    ISplatTagCoreObject<T>, IMatchable<T>
    where T : BaseHandlerCollectionSourced<T>
  {
    protected IdHandler IdInformation => GetHandler<IdHandler>(IdHandler.SerializationName);

    /// <summary>
    /// The database ID of the object
    /// </summary>
    public Guid Id => IdInformation.Id;

    public bool Matches(T other, FilterOptions matchOptions)
      => (MatchWithReason(other) & matchOptions) != 0;

    protected BaseSplatTagCoreObject()
    {
    }
  }
}