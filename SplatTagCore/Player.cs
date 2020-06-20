using System;
using System.Collections.Generic;
using System.Linq;

namespace SplatTagCore
{
  public class Player
  {
    /// <summary>
    /// Displayed string for an unknown player.
    /// </summary>
    public const string UNKNOWN_PLAYER = "(unknown)";

    /// <summary>
    /// Back-store for the names of this player. The first element is the current name.
    /// </summary>
    private Stack<string> names = new Stack<string>();

    /// <summary>
    /// Back-store for the teams for this player. The first element is the current team.
    /// </summary>
    private Stack<Team> teams = new Stack<Team>();

    /// <summary>
    /// The names this player is known by.
    /// </summary>
    public string[] Names
    {
      get => names.ToArray();
      set => names = new Stack<string>(value);
    }

    /// <summary>
    /// The last known used name for the player
    /// </summary>
    public string Name
    {
      get => names.Count > 0 ? names.Peek() : UNKNOWN_PLAYER;
      set => names.Push(value);
    }

    /// <summary>
    /// The teams this player is played for.
    /// </summary>
    public Team[] Teams
    {
      get => teams.ToArray();
      set => teams = new Stack<Team>(value);
    }

    /// <summary>
    /// The current team this player plays for, or null if not set.
    /// </summary>
    public Team CurrentTeam
    {
      get => teams.Count > 0 ? teams.Peek() : null;
      set => teams.Push(value);
    }

    /// <summary>
    /// The database Id of the player.
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    /// Merge this player with another (newer) player instance
    /// </summary>
    /// <param name="otherPlayer"></param>
    public void Merge(Player otherPlayer)
    {
      // Merge the teams.
      // Iterates the other stack in reverse order so older teams are pushed first
      // so the most recent end up first in the stack.
      foreach (Team t in otherPlayer.teams.Reverse())
      {
        Team foundTeam = this.teams.FirstOrDefault(playerTeams => playerTeams.Name.Equals(t.Name, StringComparison.OrdinalIgnoreCase));

        if (foundTeam == null)
        {
          teams.Push(t);
        }
      }

      // Merge the player's name(s).
      // Iterates the other stack in reverse order so older names are pushed first
      // so the most recent end up first in the stack.
      foreach (string n in otherPlayer.names.Reverse())
      {
        string foundName = this.names.FirstOrDefault(playerNames => playerNames.Equals(n, StringComparison.OrdinalIgnoreCase));

        if (foundName == null)
        {
          names.Push(n);
        }
      }
    }

    /// <summary>
    /// Overridden ToString.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return Name + (CurrentTeam == null ? null : $" (Plays for {CurrentTeam})");
    }
  }
}