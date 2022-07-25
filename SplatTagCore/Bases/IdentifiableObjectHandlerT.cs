using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  public abstract class IdentifiableObjectHandler<T> :
    BaseHandlerCollectionSourced,
    IEquatable<ICoreObject>
    where T : IIdentifiableCoreObject
  {
    protected IdHandler IdInformation => GetHandler<IdHandler>(IdHandler.SerializationName);

    /// <summary>
    /// The database ID of the object
    /// </summary>
    public Guid Id => IdInformation.Id;

    public string GetDisplayValue() => ToString();

    public bool Equals(ICoreObject other)
    {
      if (other is null) return false;
      if (ReferenceEquals(this, other)) return true;
      if (other is IdentifiableObjectHandler<T> otherHandler)
      {
        return Id == otherHandler.Id;
      }
      return false;
    }

    // public so derived classes can implement ISerializable
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      // Generate an Id if one has not yet.
      var _ = Id;

      // Pass off to the base.
      base.GetObjectData(info, context);
    }

    protected IdentifiableObjectHandler()
    {
    }
  }
}