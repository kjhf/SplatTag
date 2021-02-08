namespace SplatTagCore
{
  /// <summary>
  /// How the <see cref="ClanTag"/> should be rendered around a <see cref="Player"/>'s name.
  /// </summary>
  public enum TagOption
  {
    /// <summary>
    /// The placement of the tag hasn't been determined
    /// </summary>
    Unknown,

    /// <summary>
    /// Tag goes before the name
    /// </summary>
    Front,

    /// <summary>
    /// Tag goes after the name
    /// </summary>
    Back,

    /// <summary>
    /// The tag surrounds the name, e.g. "_name_"
    /// </summary>
    Surrounding,

    /// <summary>
    /// Players just do what they want sometimes
    /// </summary>
    Variable
  }
}