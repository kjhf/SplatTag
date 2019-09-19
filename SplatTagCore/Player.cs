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
    /// The names this player is known by
    /// </summary>
    public string[] Names { get; set; } = new string[0];

    /// <summary>
    /// The last known used name for the player
    /// </summary>
    public string MainPlayerName => Names.FirstOrDefault() ?? UNKNOWN_PLAYER;

    /// <summary>
    /// The teams this player has played for.
    /// </summary>
    public Team[] Teams { get; set; } = new Team[0];

    /// <summary>
    /// The current team this player plays for, or null if not set.
    /// </summary>
    public Team CurrentTeam => Teams.FirstOrDefault();

    /// <summary>
    /// The database Id of the player.
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    /// Overridden ToString.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return Id + ": " + MainPlayerName;
    }
  }
}