namespace SplatTagCore
{
  /// <summary>
  /// Built-in statics that <see cref="SplatTagCore"/> needs for error handling.
  /// </summary>
  public static class Builtins
  {
    /// <summary>
    /// Displayed string for an unknown player.
    /// </summary>
    public const string UNKNOWN_PLAYER = "(Unnamed Player)";

    /// <summary>
    /// Displayed string for an unknown team.
    /// </summary>
    public const string UNKNOWN_TEAM = "(Unnamed Team)";

    public static readonly Source BuiltinSource = new Source("builtin")
    {
      Brackets = new Bracket[0],
      Placements = new Placements[0],
      Players = new Player[0],
      Teams = new Team[0]
    };

    public static readonly Source ManualSource = new Source("Manual Entry")
    {
      Brackets = new Bracket[0],
      Placements = new Placements[0],
      Players = new Player[0],
      Teams = new Team[0]
    };

    public static readonly Name UnknownPlayerName = new Name(UNKNOWN_PLAYER, BuiltinSource);
    public static readonly Name UnknownTeamName = new Name(UNKNOWN_TEAM, BuiltinSource);
  }
}