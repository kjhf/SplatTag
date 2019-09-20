﻿namespace SplatTagCore
{
  public class Team
  {
    /// <summary>
    /// The name of the team
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The tag(s) of the team
    /// </summary>
    public string[] ClanTags { get; set; }

    /// <summary>
    /// The placement of the tag
    /// </summary>
    public TagOption ClanTagOption { get; set; } = TagOption.Unknown;

    /// <summary>
    /// The database Id of the team.
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    /// Overridden ToString.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return Id + ": " + Name;
    }
  }
}