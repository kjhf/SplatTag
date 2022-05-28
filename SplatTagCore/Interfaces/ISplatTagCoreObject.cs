using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  public interface ISplatTagCoreObject : IMatchable, IMergable, IReadonlySourceable, ISerializable
  {
    public Guid Id { get; }
  }
}