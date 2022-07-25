using System;
using System.Collections.Generic;

namespace SplatTagCore
{
  public interface ITeamResolver
  {
    /// <summary>
    /// Match a Team by its id.
    /// </summary>
    /// <returns>
    /// True if resolved, else false and team is set to <see cref="Team.UnknownTeam"/>
    /// </returns>
    public bool GetTeamById(Guid id, out Team team);

    /// <summary>
    /// Match a Team by its id.
    /// </summary>
    /// <returns>
    /// Non-null team, which defaults to <see cref="Team.UnknownTeam"/> if not found.
    /// </returns>
    public Team GetTeamById(Guid id);

    /// <summary>
    /// Get the players that played on team <paramref name="t"/>, as a list of tuples, containing the player and if
    /// that player still plays for the team (true) or is no longer the most recent team (false).
    /// </summary>
    public IReadOnlyList<(Player player, bool mostRecent)> GetPlayersForTeam(Team t);
  }
}