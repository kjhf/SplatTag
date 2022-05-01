using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  public interface ISplatTagCoreObject : IMatchable, IReadonlySourceable, ISerializable
  {
    public Guid Id { get; }
  }
}