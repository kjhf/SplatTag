using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SplatTagCore
{
  [Serializable]
  public class Player
  {
    /// <summary>
    /// Displayed string for an unknown player.
    /// </summary>
    public const string UNKNOWN_PLAYER = "(Unnamed Player)";

    public static readonly Regex DISCORD_NAME_REGEX = new Regex(@"\(?.*#[0-9]{4}\)?", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

    /// <summary>
    /// Back-store for the names of this player. The first element is the current name.
    /// </summary>
    private List<string> names = new List<string>();

    /// <summary>
    /// Back-store for the sources of this player.
    /// </summary>
    private List<string> sources = new List<string>();

    /// <summary>
    /// Back-store for the team GUIDs for this player. The first element is the current team.
    /// No team represented by <see cref="Team.NoTeam.Id"/>.
    /// </summary>
    private List<Guid> teams = new List<Guid>();

    /// <summary>
    /// Back-store for the weapons that the player uses (if any).
    /// </summary>
    private List<string> weapons = new List<string>();

    /// <summary>
    /// Back-store for player's Twitch.
    /// </summary>
    private string twitch;

    /// <summary>
    /// Back-store for player's Twitter.
    /// </summary>
    private string twitter;

    /// <summary>
    /// Back-store for player's Battlefy Slugs.
    /// </summary>
    private List<string> battlefySlugs = new List<string>();

    /// <summary>
    /// Back-store for the transformed names of this player.
    /// </summary>
    /// <remarks>
    /// Though a HashSet may seem more performant, for collections with
    /// a small number of elements (under 20), List is actually better
    /// https://stackoverflow.com/questions/150750/hashset-vs-list-performance
    /// </remarks>
    private List<string> transformedNames = new List<string>();

    [JsonProperty("Names", Required = Required.Always)]
    /// <summary>
    /// The names this player is known by.
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
        transformedNames = null;
      }
    }

    [JsonIgnore]
    /// <summary>
    /// The names this player is known by transformed into searchable query.
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

    [JsonIgnore]
    /// <summary>
    /// The last known used name for the player
    /// </summary>
    public string Name
    {
      get => names.Count > 0 ? names[0] : UNKNOWN_PLAYER;
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
          transformedNames = null;
        }
      }
    }

    [JsonProperty("Teams", Required = Required.Always)]
    /// <summary>
    /// The teams this player is played for.
    /// No Team represented by <see cref="Team.NoTeam.Id"/>
    /// </summary>
    public ICollection<Guid> Teams
    {
      get => teams.ToArray();
      set => teams = value?.Distinct().ToList() ?? new List<Guid>();
    }

    [JsonIgnore]
    /// <summary>
    /// The old teams this player has played for.
    /// </summary>
    public IEnumerable<Guid> OldTeams
    {
      get => teams?.Skip(1).ToArray();
    }

    [JsonIgnore]
    /// <summary>
    /// The current team id this player plays for, or NoTeam.Id if not set.
    /// </summary>
    public Guid CurrentTeam
    {
      get => teams.Count > 0 ? teams[0] : Team.NoTeam.Id;
      set
      {
        if (teams.Count == 0)
        {
          teams.Add(value);
        }
        else if (teams[0].Equals(value))
        {
          // Nothing to do.
        }
        else
        {
          teams.Remove(value);
          teams.Insert(0, value);
        }
      }
    }

    [JsonProperty("Weapons", Required = Required.Default)]
    /// <summary>
    /// The weapons this player uses.
    /// </summary>
    public IList<string> Weapons
    {
      get => weapons.ToArray();
      set
      {
        weapons = new List<string>();
        foreach (string s in value)
        {
          if (!string.IsNullOrWhiteSpace(s) && !weapons.Contains(s, StringComparison.OrdinalIgnoreCase))
          {
            weapons.Add(s);
          }
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

    [JsonProperty("Id", Required = Required.Always)]
    /// <summary>
    /// The database Id of the player.
    /// </summary>
    public readonly Guid Id = Guid.NewGuid();

    [JsonProperty("SendouId", Required = Required.Default)]
    /// <summary>
    /// The Sendou database Id of the player.
    /// Null by default.
    /// </summary>
    public Guid? SendouId { get; set; }

    [JsonProperty("SplatnetId", Required = Required.Default)]
    /// <summary>
    /// The Splatnet database Id of the player (a hex string).
    /// Null by default.
    /// </summary>
    public string SplatnetId { get; set; }

    [JsonProperty("DiscordId", Required = Required.Default)]
    /// <summary>
    /// The Discord database Id of the player.
    /// Null by default.
    /// </summary>
    public ulong? DiscordId { get; set; }

    [JsonProperty("DiscordName", Required = Required.Default)]
    /// <summary>
    /// Get or Set the Discord Name.
    /// Null by default.
    /// </summary>
    public string DiscordName { get; set; }

    [JsonProperty("FriendCode", Required = Required.Default)]
    /// <summary>
    /// Get or Set the Friend Code.
    /// Null by default.
    /// </summary>
    public string FriendCode { get; set; }

    [JsonProperty("Country", Required = Required.Default)]
    /// <summary>
    /// Get or Set the Country.
    /// Null by default.
    /// </summary>
    public string Country { get; set; }

    [JsonIgnore]
    /// <summary>
    /// Get the emoji flag of the <see cref="Country"/> specified.
    /// </summary>
    public string CountryFlag
    {
      get
      {
        if (Country == null) return null;
        return string.Concat(Country.ToUpper().Select(x => char.ConvertFromUtf32(x + 0x1F1A5)));
      }
    }

    [JsonProperty("Top500", Required = Required.Default)]
    /// <summary>
    /// Get or Set Top 500 flag.
    /// False by default.
    /// </summary>
    public bool Top500 { get; set; }

    [JsonProperty("BattlefyUsername", Required = Required.Default)]
    /// <summary>
    /// Get or Set a BattlefyUsername.
    /// </summary>
    public string BattlefyUsername { get; set; }

    [JsonProperty("BattlefySlugs", Required = Required.Default)]
    /// <summary>
    /// Get or Set BattlefySlugs.
    /// </summary>
    public IList<string> BattlefySlugs
    {
      get => battlefySlugs;
      set
      {
        battlefySlugs = new List<string>();
        foreach (string s in value)
        {
          if (!string.IsNullOrWhiteSpace(s) && !battlefySlugs.Contains(s))
          {
            battlefySlugs.Add(s);
          }
        }
      }
    }

    [JsonProperty("Twitch", Required = Required.Default)]
    /// <summary>
    /// Get or Set the player's Twitch link.
    /// </summary>
    public string Twitch
    {
      get => twitch;
      set
      {
        if (string.IsNullOrWhiteSpace(value))
        {
          twitch = null;
        }
        else if (value.Contains("twitch.tv"))
        {
          twitch = value;
        }
        else
        {
          if (value.StartsWith("@"))
          {
            value = value.Substring(1);
          }
          twitch = "https://twitch.tv/" + value;
        }
      }
    }

    [JsonProperty("Twitter", Required = Required.Default)]
    /// <summary>
    /// Get or Set the player's twitter link.
    /// </summary>
    public string Twitter
    {
      get => twitter;
      set
      {
        if (string.IsNullOrWhiteSpace(value))
        {
          twitter = null;
        }
        else if (value.Contains("twitter.com"))
        {
          twitter = value;
        }
        else
        {
          if (value.StartsWith("@"))
          {
            value = value.Substring(1);
          }
          twitter = "https://twitter.com/" + value;
        }
      }
    }

    /// <summary>
    /// Merge this player with another (newer) player instance
    /// </summary>
    /// <param name="newerPlayer">The new import record</param>
    /// <exception cref="ArgumentNullException"><paramref name="newerPlayer"/> is <c>null</c>.</exception>
    public void Merge(Player newerPlayer)
    {
      if (newerPlayer == null) throw new ArgumentNullException(nameof(newerPlayer));
      if (ReferenceEquals(this, newerPlayer)) return;

      // Merge the players.
      if (teams.Count == 0)
      {
        // Shortcut, just set the teams.
        Teams = newerPlayer.teams;
      }
      else
      {
        // Iterates the other stack in reverse order so older teams are pushed first
        // so the most recent end up first in the stack.
        var reverseTeams = newerPlayer.teams.Distinct().ToList();
        reverseTeams.Reverse();
        foreach (Guid t in reverseTeams)
        {
          // If this team is already first, there's nothing to do.
          if (teams[0] != t)
          {
            teams.Remove(t); // If the team isn't found, this just returns false.
            teams.Insert(0, t);
          }
        }
      }

      // Merge the player's name(s).
      if (names.Count == 0)
      {
        Names = newerPlayer.names;
      }
      else
      {
        // Iterates the other stack in reverse order so older names are pushed first
        // so the most recent end up first in the stack.
        var reversePlayers = newerPlayer.names.ToList();
        reversePlayers.Reverse();
        foreach (string n in reversePlayers.Where(s => !string.IsNullOrWhiteSpace(s)))
        {
          string foundName = this.names.Find(playerNames => playerNames.Equals(n, StringComparison.OrdinalIgnoreCase));

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

      // Merge the sources.
      if (sources.Count == 0)
      {
        Sources = newerPlayer.sources;
      }
      else
      {
        foreach (string source in newerPlayer.Sources.Where(s => !string.IsNullOrWhiteSpace(s)))
        {
          string foundSource = this.sources.Find(sources => sources.Equals(source, StringComparison.OrdinalIgnoreCase));

          if (foundSource == null)
          {
            sources.Add(source);
          }
        }
      }

      // Merge the weapons.
      if (weapons.Count == 0)
      {
        Weapons = newerPlayer.weapons;
      }
      else
      {
        foreach (string weapon in newerPlayer.Weapons.Where(s => !string.IsNullOrWhiteSpace(s)))
        {
          string foundWeapon = weapons.Find(wep => weapon.Equals(wep, StringComparison.OrdinalIgnoreCase));

          if (foundWeapon == null)
          {
            weapons.Add(weapon);
          }
        }
      }

      // Merge the BattlefySlugs.
      if (battlefySlugs.Count == 0)
      {
        BattlefySlugs = newerPlayer.battlefySlugs;
      }
      else
      {
        foreach (string slug in newerPlayer.BattlefySlugs.Where(s => !string.IsNullOrWhiteSpace(s)))
        {
          string found = this.battlefySlugs.Find(slugs => slugs.Equals(slug, StringComparison.OrdinalIgnoreCase));

          if (found == null)
          {
            battlefySlugs.Add(slug);
          }
        }
      }

      // Merge the misc data
      if (!string.IsNullOrWhiteSpace(newerPlayer.FriendCode))
      {
        this.FriendCode = newerPlayer.FriendCode;
      }

      if (!string.IsNullOrWhiteSpace(newerPlayer.DiscordName))
      {
        this.DiscordName = newerPlayer.DiscordName;
      }

      if (newerPlayer.DiscordId != null)
      {
        this.DiscordId = newerPlayer.DiscordId;
      }

      if (!string.IsNullOrWhiteSpace(newerPlayer.Country))
      {
        this.Country = newerPlayer.Country;
      }

      if (newerPlayer.SendouId != null)
      {
        this.SendouId = newerPlayer.SendouId;
      }

      if (newerPlayer.Top500)
      {
        this.Top500 = true;
      }

      if (!string.IsNullOrWhiteSpace(newerPlayer.Twitch))
      {
        this.Twitch = newerPlayer.Twitch;
      }

      if (!string.IsNullOrWhiteSpace(newerPlayer.Twitter))
      {
        this.Twitter = newerPlayer.Twitter;
      }

      if (!string.IsNullOrWhiteSpace(newerPlayer.BattlefyUsername))
      {
        this.BattlefyUsername = newerPlayer.BattlefyUsername;
      }
    }

    /// <summary>
    /// Overridden ToString.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return Name;
    }
  }
}