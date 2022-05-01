using NLog;
using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  public abstract class BaseSplatTagCoreObject<T> : BaseHandlerCollectionSourced<T>, ISplatTagCoreObject<T>, IMatchable<T> where T : BaseHandlerCollectionSourced<T>
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    protected readonly IdHandler IdHandler = new();

    /// <summary>
    /// The database ID of the object
    /// </summary>
    public Guid Id => IdHandler.Id;

    public override FilterOptions MatchWithReason(IMatchable other)
      => MatchWithReason((T)other);

    public bool Matches(T other, FilterOptions matchOptions)
      => (MatchWithReason(other) & matchOptions) != 0;

    public override void Merge(IMergable other) => Merge((T)other);

    protected BaseSplatTagCoreObject()
      : base()
    {
    }

    protected BaseSplatTagCoreObject(SerializationInfo info, StreamingContext context)
      : this()
    {
      try
      {
        this.IdHandler = new(info, context);
      }
      catch (SerializationException)
      {
        logger.Error($"GUID cannot be empty for this BaseSplatTagCoreObject: {this.GetType().Name} from source(s) [{string.Join(", ", this.Sources)}].");
        throw;
      }
    }
  }
}