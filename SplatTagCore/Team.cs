using System;
using System.Collections.Generic;
using System.Linq;

namespace SplatTagCore
{
  public class Team
  {
    /// <summary>
    /// The tag(s) of the team, first is the current tag.
    /// </summary>
    private Stack<string> clanTags = new Stack<string>();

    /// <summary>
    /// The name of the team
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The division of the team
    /// </summary>
    public Division Div { get; set; } = Division.Unknown;

    /// <summary>
    /// The tag(s) of the team
    /// </summary>
    public string[] ClanTags
    {
      get => clanTags.ToArray();
      set => clanTags = new Stack<string>(value);
    }

    /// <summary>
    /// The placement of the tag
    /// </summary>
    public TagOption ClanTagOption { get; set; } = TagOption.Unknown;

    /// <summary>
    /// The database Id of the team.
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    /// The most recent tag of the team
    /// </summary>
    public string Tag => ClanTags.Length > 0 ? ClanTags[0] : "";

    /// <summary>
    /// Merge this team with another (newer) team instance
    /// </summary>
    /// <param name="otherPlayer"></param>
    public void Merge(Team otherTeam)
    {
      // Merge the tags.
      // Iterates the other stack in reverse order so older tags are pushed first
      // so the most recent end up first in the stack.
      foreach (string tag in otherTeam.clanTags.Reverse())
      {
        string foundTag = this.clanTags.FirstOrDefault(teamTags => teamTags.Equals(tag, StringComparison.OrdinalIgnoreCase));

        if (foundTag == null)
        {
          clanTags.Push(tag);

          // The tag has changed, update the tag option.
          this.ClanTagOption = otherTeam.ClanTagOption;
        }
      }

      // Update the div if the other div is known.
      if (otherTeam.Div != Division.UNKNOWN)
      {
        this.Div = otherTeam.Div;
      }
    }

    /// <summary>
    /// Overridden ToString.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return $"{Tag} {Name} (Div {Div})";
    }
  }
}