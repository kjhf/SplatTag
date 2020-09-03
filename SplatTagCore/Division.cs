using System.Collections.Generic;

namespace SplatTagCore
{
  /// <summary>
  /// A division.
  /// </summary>
  public interface IDivision
  {
    /// <summary>
    /// Get a string name or representation of this div.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Get the value of this div.
    /// </summary>
    int Value { get; }
  }
}