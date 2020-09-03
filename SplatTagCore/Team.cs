using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SplatTagCore
{
  [Serializable]
  public class Team
  {
    public static readonly Team NoTeam = new Team()
    {
      ClanTagOption = TagOption.Unknown,
      ClanTags = new string[] { "FA" },
      Div = new LUTIDivision(),
      Id = 0,
      Name = "(Free Agent)",
      Sources = new List<string>()
    };

    /// <summary>
    /// The tag(s) of the team, first is the current tag.
    /// </summary>
    private Stack<string> clanTags = new Stack<string>();

    [JsonProperty("Names", Required = Required.Always)]
    /// <summary>
    /// The name of the team
    /// </summary>
    public string Name { get; set; }

    [JsonProperty("Div", Required = Required.Default)]
    /// <summary>
    /// The division of the team
    /// </summary>
    public IDivision Div { get; set; } = LUTIDivision.Unknown;

    [JsonProperty("ClanTags", Required = Required.Always)]
    /// <summary>
    /// The tag(s) of the team
    /// </summary>
    public string[] ClanTags
    {
      get => clanTags.ToArray();
      set => clanTags = new Stack<string>(value);
    }

    [JsonProperty("ClanTagOption", Required = Required.Default)]
    /// <summary>
    /// The placement of the tag
    /// </summary>
    public TagOption ClanTagOption { get; set; } = TagOption.Unknown;

    [JsonProperty("Id", Required = Required.Always)]
    /// <summary>
    /// The database Id of the team.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// The most recent tag of the team
    /// </summary>
    public string Tag => ClanTags.Length > 0 ? ClanTags[0] : "";

    [JsonProperty("Sources", Required = Required.Always)]
    /// <summary>
    /// Get or Set the current sources that make up this Team instance.
    /// </summary>
    public List<string> Sources { get; set; } = new List<string>();

    [JsonProperty("Twitter", Required = Required.Default)]
    /// <summary>
    /// Get or Set the team's twitter link.
    /// </summary>
    public string Twitter { get; set; }

    /// <summary>
    /// Merge this team with another (newer) team instance
    /// </summary>
    /// <param name="otherTeam"></param>
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

      // Merge Twitter
      if (!string.IsNullOrWhiteSpace(otherTeam.Twitter))
      {
        this.Twitter = otherTeam.Twitter;
      }

      // Update the div if the other div is known.
      if (otherTeam.Div.Value != LUTIDivision.UNKNOWN)
      {
        this.Div = otherTeam.Div;
      }

      // Merge the sources.
      foreach (string source in otherTeam.Sources)
      {
        string foundSource = this.Sources.Find(sources => sources.Equals(source, StringComparison.OrdinalIgnoreCase));

        if (foundSource == null)
        {
          Sources.Add(source);
        }
      }
    }

    /// <summary>
    /// Overridden ToString.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return $"{Tag} {Name} ({Div})";
    }
  }
}