using System;

namespace SplatTagCore
{
  public interface ITeamResolver
  {
    /// <summary>
    /// Match a Team by its id.
    /// Never returns null.
    /// </summary>
    public Team GetTeamById(Guid id);
  }
}