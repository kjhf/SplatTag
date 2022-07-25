using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  public interface IIdentifiableCoreObject : ISelfMatchable, ISelfMergable, ISourcedCoreObject, ISerializable
  {
    public Guid Id { get; }
  }
}