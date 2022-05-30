using System;

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

    /// <summary>
    /// Displayed string for an unknown source.
    /// </summary>
    public const string UNKNOWN_SOURCE = "(Unnamed Source)";

    /// <summary>
    /// Displayed string for an unknown bracket.
    /// </summary>
    public const string UNKNOWN_BRACKET = "(Unnamed Bracket)";

    /// <summary>
    /// The built-in source, for use in objects that are pre-defined by the program code.
    /// </summary>
    public static readonly Source BuiltinSource = new("0001-01-02-Built-in-Source");

    /// <summary>
    /// The manual entry source, for use in objects that are defined by manual user entry.
    /// </summary>
    public static readonly Source ManualSource = new("0001-01-03-Manual-Entry-Source");

    /// <summary>
    /// The <see cref="Name"/> object for an <see cref="UNKNOWN_PLAYER"/>.
    /// </summary>
    public static readonly Name UnknownPlayerName = new(UNKNOWN_PLAYER, BuiltinSource);

    /// <summary>
    /// The <see cref="Name"/> object for an <see cref="UNKNOWN_TEAM"/>.
    /// </summary>
    public static readonly Name UnknownTeamName = new(UNKNOWN_TEAM, BuiltinSource);

    /// <summary>
    /// Unknown or not set datetime ticks.
    /// </summary>
    public const long UNKNOWN_DATE_TIME_TICKS = 0;

    /// <summary>
    /// Unknown or not set datetime.
    /// </summary>
    public static readonly DateTime UnknownDateTime = new(UNKNOWN_DATE_TIME_TICKS);

    /// <summary>
    /// The count before a parallel operation is done instead of in-line.
    /// </summary>
    public const int PARALLEL_THRESHOLD = 15;
  }
}