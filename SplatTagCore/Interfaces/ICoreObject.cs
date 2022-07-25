using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  /// <summary>
  /// Basic record of data used by the <see cref="SplatTagCore"/> back-end.
  /// </summary>
  public interface ICoreObject : IEquatable<ICoreObject>, ISerializable
  {
    /// <summary>
    /// Get the object's value in textual form.
    /// </summary>
    public string GetDisplayValue();
  }
}