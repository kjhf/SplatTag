using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SplatTagCore
{
  [Serializable]
  public class Team
  {
    /// <summary>
    /// Displayed string for an unknown team.
    /// </summary>
    public const string UNKNOWN_TEAM = "(Unnamed Team)";

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

    /// <summary>
    /// Back-store for the names of this team. The first element is the current name.
    /// </summary>
    private List<string> names = new List<string>();

    /// <summary>
    /// Back-store for the transformed names of this team.
    /// </summary>
    /// <remarks>
    /// Though a HashSet may seem more performant, for collections with
    /// a small number of elements (under 20), List is actually better
    /// https://stackoverflow.com/questions/150750/hashset-vs-list-performance
    /// </remarks>
    private List<string> transformedNames = new List<string>();

    /// <summary>
    /// Back-store for the sources of this team.
    /// </summary>
    private List<string> sources = new List<string>();

    /// <summary>
    /// Back-store for the persistent ids of this team.
    /// </summary>
    /// <remarks>
    /// Though a HashSet may seem more performant, for collections with
    /// a small number of elements (under 20), List is actually better
    /// https://stackoverflow.com/questions/150750/hashset-vs-list-performance
    /// </remarks>
    private List<string> battlefyPersistentTeamIds = new List<string>();

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
    /// May be null.
    /// </summary>
    public string? BattlefyPersistentTeamId
    {
      get => battlefyPersistentTeamIds.Count > 0 ? battlefyPersistentTeamIds[0] : null;
      set
      {
        if (value != null && !string.IsNullOrWhiteSpace(value))
        {
          if (battlefyPersistentTeamIds.Count == 0)
          {
            battlefyPersistentTeamIds.Add(value);
          }
          else if (battlefyPersistentTeamIds[0].Equals(value))
          {
            // Nothing to do.
          }
          else
          {
            battlefyPersistentTeamIds.Remove(value);
            battlefyPersistentTeamIds.Insert(0, value);
          }
        }
      }
    }

    [JsonProperty("BattlefyPersistentTeamIds", Required = Required.Default)]
    /// <summary>
    /// The known Battlefy Persistent Ids of the team.
    /// </summary>
    public IList<string> BattlefyPersistentTeamIds
    {
      get => battlefyPersistentTeamIds.ToArray();
      set
      {
        battlefyPersistentTeamIds = new List<string>();
        foreach (string s in value)
        {
          if (!string.IsNullOrWhiteSpace(s) && !battlefyPersistentTeamIds.Contains(s))
          {
            battlefyPersistentTeamIds.Add(s);
          }
        }
      }
    }

    [JsonProperty("Names", Required = Required.Default)]
    /// <summary>
    /// The names this team is known by
    /// </summary>
    public IEnumerable<string> Names
    {
      get => names.ToArray();
      set
      {
        names = new List<string>();
        foreach (string s in value)
        {
          if (!string.IsNullOrWhiteSpace(s) && !names.Contains(s))
          {
            names.Add(s);
          }
        }
        transformedNames.Clear(); // Invalidate searchable names.
      }
    }

    [JsonIgnore]
    /// <summary>
    /// The names this team is known by transformed into searchable query.
    /// </summary>
    public IReadOnlyCollection<string> TransformedNames
    {
      get
      {
        if (transformedNames == null)
        {
          transformedNames = new List<string>();
          foreach (var name in names)
          {
            transformedNames.Add(name.Replace(" ", "").TransformString().ToLowerInvariant());
          }
        }
        return transformedNames;
      }
    }

    [JsonProperty("Name", Required = Required.Default)]
    /// <summary>
    /// The name of the team
    /// </summary>
    public string Name
    {
      get => names.Count > 0 ? names[0] : UNKNOWN_TEAM;
      set
      {
        if (!string.IsNullOrWhiteSpace(value))
        {
          if (names.Count == 0)
          {
            names.Add(value);
          }
          else if (names[0].Equals(value))
          {
            // Nothing to do.
          }
          else
          {
            names.Remove(value);
            names.Insert(0, value);
          }
          transformedNames.Clear();
        }
      }
    }

    [JsonProperty("Sources", Required = Required.Default)]
    /// <summary>
    /// Get or Set the current sources that make up this Player instance.
    /// </summary>
    public IList<string> Sources
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
    /// Null by default.
    /// </summary>
    public string? Twitter { get; set; }

    public Team()
    {
      Div = new Division();
      clanTags = new Stack<string>();
      sources = new List<string>();
    }

    /// <summary>
    /// Filter all players to return only those in this team.
    /// </summary>
    public IEnumerable<Player> GetPlayers(IEnumerable<Player> allPlayers)
    {
      return allPlayers.Where(p => p.Teams.Contains(this.Id));
    }

    /// <summary>
    /// Merge this team with another (newer) team instance
    /// </summary>
    /// <param name="newerTeam"></param>
    public void Merge(Team newerTeam)
    {
      // Merge the tags.
      if (clanTags.Count == 0)
      {
        ClanTags = newerTeam.ClanTags;
      }
      else
      {
        // Iterates the other stack in reverse order so older tags are pushed first
        // so the most recent end up first in the stack.
        foreach (string tag in newerTeam.clanTags.Reverse())
        {
          if (string.IsNullOrWhiteSpace(tag)) continue;

          string foundTag = this.clanTags.FirstOrDefault(teamTags => teamTags.Equals(tag, StringComparison.OrdinalIgnoreCase));

          if (foundTag == null)
          {
            clanTags.Push(tag);

            // The tag has changed, update the tag option.
            this.ClanTagOption = newerTeam.ClanTagOption;
          }
        }
      }

      // Merge Twitter
      if (!string.IsNullOrWhiteSpace(newerTeam.Twitter))
      {
        this.Twitter = newerTeam.Twitter;
      }

      // Update the div if the other div is known.
      if (newerTeam.Div.Value != Division.UNKNOWN)
      {
        this.Div = newerTeam.Div;
      }

      // Merge the team's name(s).
      if (names.Count == 0)
      {
        Names = newerTeam.names;
      }
      else
      {
        // Iterates the other stack in reverse order so older names are pushed first
        // so the most recent end up first in the stack.
        var reverseTeamNames = newerTeam.names.ToList();
        reverseTeamNames.Reverse();
        foreach (string n in reverseTeamNames.Where(s => !string.IsNullOrWhiteSpace(s)))
        {
          string foundName = this.names.Find(teamNames => teamNames.Equals(n, StringComparison.OrdinalIgnoreCase));

          if (foundName == null)
          {
            names.Insert(0, n);
          }
          else
          {
            names.Remove(foundName);
            names.Insert(0, n);
          }
        }
      }

      // Merge the team's persistent battlefy id(s).
      if (battlefyPersistentTeamIds.Count == 0)
      {
        BattlefyPersistentTeamIds = newerTeam.battlefyPersistentTeamIds;
      }
      else
      {
        // Iterates the other stack in reverse order so older names are pushed first
        // so the most recent end up first in the stack.
        var reverseBattlefyIds = newerTeam.BattlefyPersistentTeamIds.ToList();
        reverseBattlefyIds.Reverse();
        foreach (string n in reverseBattlefyIds.Where(s => !string.IsNullOrWhiteSpace(s)))
        {
          string foundId = this.battlefyPersistentTeamIds.Find(ids => ids.Equals(n, StringComparison.OrdinalIgnoreCase));

          if (foundId == null)
          {
            battlefyPersistentTeamIds.Insert(0, n);
          }
          else
          {
            battlefyPersistentTeamIds.Remove(foundId);
            battlefyPersistentTeamIds.Insert(0, n);
          }
        }
      }

      // Merge the sources.
      if (sources.Count == 0)
      {
        Sources = newerTeam.sources;
      }
      else
      {
        foreach (string source in newerTeam.Sources.Where(s => !string.IsNullOrWhiteSpace(s)))
        {
          string foundSource = this.sources.Find(sources => sources.Equals(source, StringComparison.OrdinalIgnoreCase));

          if (foundSource == null)
          {
            sources.Add(source);
          }
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

    /// <summary>
    /// Calculates the <see cref="ClanTagOption"/> based on the tag and the example player name.
    /// Does NOT set the tag.
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="examplePlayerName"></param>
    public void SetTagOption(string tag, string examplePlayerName)
    {
      string transformedTag = tag.TransformString();
      if (string.IsNullOrWhiteSpace(transformedTag))
      {
        // Nothing to do, no tag
      }
      else
      {
        if (examplePlayerName.StartsWith(tag, StringComparison.OrdinalIgnoreCase) || examplePlayerName.StartsWith(transformedTag, StringComparison.OrdinalIgnoreCase))
        {
          this.ClanTagOption = TagOption.Front;
        }
        else if (examplePlayerName.EndsWith(tag, StringComparison.OrdinalIgnoreCase) || examplePlayerName.EndsWith(transformedTag, StringComparison.OrdinalIgnoreCase))
        {
          // Tag is at the back.
          this.ClanTagOption = TagOption.Back;
        }

        if (transformedTag.Length == 2)
        {
          char first = transformedTag[0];
          char second = transformedTag[1];
          // If the tag has 2 characters, check 'surrounding' criteria which is take the
          // first character of the tag and check if the captain's name begins with this character,
          // then take the last character of the tag and check if the captain's name ends with this character.
          // e.g. Tag: //, Captain's name: /captain/
          if (examplePlayerName.StartsWith(first.ToString(), StringComparison.OrdinalIgnoreCase)
          && examplePlayerName.EndsWith(second.ToString(), StringComparison.OrdinalIgnoreCase))
          {
            this.ClanTagOption = TagOption.Surrounding;
          }
        }
      }
    }
  }
}