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
      ClanTagOption = TagOption.Variable,
      ClanTags = new string[] { "FA" },
      Div = new Division(),
      Name = "(Free Agent)",
      sources = new List<string>()
    };

    public static readonly Team UnlinkedTeam = new Team()
    {
      ClanTagOption = TagOption.Variable,
      ClanTags = new string[] { nameof(SplatTagCore) + " ERROR" },
      Div = new Division(),
      Name = "UNLINKED TEAM",
      sources = new List<string>()
    };

    /// <summary>
    /// The tag(s) of the team, first is the current tag.
    /// </summary>
    private Stack<string> clanTags;

    private string name;
    private string searchableName;

    /// <summary>
    /// Back-store for the sources of this team.
    /// </summary>
    private List<string> sources = new List<string>();

    [JsonProperty("ClanTagOption", Required = Required.Default)]
    /// <summary>
    /// The placement of the tag
    /// </summary>
    public TagOption ClanTagOption { get; set; }

    [JsonProperty("ClanTags", Required = Required.Always)]
    /// <summary>
    /// The tag(s) of the team
    /// </summary>
    public string[] ClanTags
    {
      get => clanTags.ToArray();
      set => clanTags = new Stack<string>(value.Where(s => !string.IsNullOrEmpty(s)));
    }

    [JsonProperty("Div", Required = Required.Always)]
    /// <summary>
    /// The division of the team
    /// </summary>
    public Division Div { get; set; }

    [JsonProperty("Id", Required = Required.Always)]
    /// <summary>
    /// The GUID of the team.
    /// </summary>
    public readonly Guid Id = Guid.NewGuid();

    [JsonProperty("BattlefyPersistentTeamId", Required = Required.Default)]
    /// <summary>
    /// The Battlefy Persistent Id of the team (or null if not set).
    /// Should be a hex string but may not be a ulong.
    /// </summary>
    public string BattlefyPersistentTeamId { get; set; }

    [JsonProperty("Name", Required = Required.Always)]
    /// <summary>
    /// The name of the team
    /// </summary>
    public string Name
    {
      get => name;
      set
      {
        if (string.IsNullOrWhiteSpace(value)) return;

        name = value;
        searchableName = null; // Invalidate searchable name.
      }
    }

    /// <summary>
    /// Get the searchable name for this team (i.e. the transformed lower-case team name).
    /// </summary>
    public string SearchableName => searchableName ?? (searchableName = Name.Replace(" ", "").TransformString().ToLowerInvariant());

    [JsonProperty("Sources", Required = Required.Default)]
    /// <summary>
    /// Get or Set the current sources that make up this Player instance.
    /// </summary>
    public string[] Sources
    {
      get => sources.ToArray();
      set
      {
        sources = new List<string>();
        foreach (string s in value)
        {
          if (!string.IsNullOrWhiteSpace(s) && !sources.Contains(s))
          {
            sources.Add(s);
          }
        }
      }
    }

    [JsonIgnore]
    /// <summary>
    /// The most recent tag of the team
    /// </summary>
    public string Tag => ClanTags.Length > 0 ? ClanTags[0] : "";

    [JsonProperty("Twitter", Required = Required.Default)]
    /// <summary>
    /// Get or Set the team's twitter link.
    /// </summary>
    public string Twitter { get; set; }

    public Team()
    {
      Div = new Division();
      clanTags = new Stack<string>();
      sources = new List<string>();
    }

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
        if (string.IsNullOrWhiteSpace(tag)) continue;

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
      if (otherTeam.Div.Value != Division.UNKNOWN)
      {
        this.Div = otherTeam.Div;
      }

      // Merge the sources.
      foreach (string source in otherTeam.Sources)
      {
        if (string.IsNullOrWhiteSpace(source)) continue;

        string foundSource = this.sources.Find(sources => sources.Equals(source, StringComparison.OrdinalIgnoreCase));

        if (foundSource == null)
        {
          sources.Add(source);
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

    public void SetTagOption(string tag, string examplePlayerName)
    {
      string transformedTag = tag?.TransformString();
      if (string.IsNullOrWhiteSpace(transformedTag))
      {
        // Nothing to do, no tag
      }
      else if (examplePlayerName.StartsWith(transformedTag, StringComparison.OrdinalIgnoreCase) || examplePlayerName.StartsWith(tag, StringComparison.OrdinalIgnoreCase))
      {
        this.ClanTagOption = TagOption.Front;
      }
      else if (examplePlayerName.EndsWith(transformedTag, StringComparison.OrdinalIgnoreCase) || examplePlayerName.EndsWith(tag, StringComparison.OrdinalIgnoreCase))
      {
        // Tag is at the back.
        this.ClanTagOption = TagOption.Back;
      }
      else
      {
        // If the tag has 2 or more characters, check 'surrounding' criteria which is take the
        // first character of the tag and check if the captain's name begins with this character,
        // then take the last character of the tag and check if the captain's name ends with this character.
        // e.g. Tag: //, Captain's name: /captain/
        if (tag.Length >= 2)
        {
          if (examplePlayerName.StartsWith(tag[0].ToString(), StringComparison.OrdinalIgnoreCase)
          && examplePlayerName.EndsWith(tag[tag.Length - 1].ToString(), StringComparison.OrdinalIgnoreCase))
          {
            this.ClanTagOption = TagOption.Surrounding;
          }
        }
        if (this.ClanTagOption != TagOption.Surrounding && transformedTag.Length >= 2)
        {
          if (examplePlayerName.StartsWith(transformedTag[0].ToString(), StringComparison.OrdinalIgnoreCase)
          && examplePlayerName.EndsWith(transformedTag[transformedTag.Length - 1].ToString(), StringComparison.OrdinalIgnoreCase))
          {
            this.ClanTagOption = TagOption.Surrounding;
          }
        }
      }
    }
  }
}