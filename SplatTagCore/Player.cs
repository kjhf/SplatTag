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
    public const string UNKNOWN_PLAYER = "(unknown)";

    public static readonly Regex FRIEND_CODE_REGEX = new Regex(@"\(?\d{4}(-| )\d{4}(-| )\d{4}\)?", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
    public static readonly Regex DISCORD_NAME_REGEX = new Regex(@"\(?.*#[0-9]{4}\)?", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

    /// <summary>
    /// Back-store for the names of this player. The first element is the current name.
    /// </summary>
    private List<string> names = new List<string>();

    /// <summary>
    /// Back-store for the team ids for this player. The first element is the current team.
    /// A 0 represents no team.
    /// </summary>
    private List<long> teams = new List<long>();

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
      }
    }

    /// <summary>
    /// The last known used name for the player
    /// </summary>
    public string Name
    {
      get => names.Count > 0 ? names[0] : UNKNOWN_PLAYER;
      set
      {
        if (!string.IsNullOrWhiteSpace(value) && !names.Contains(value))
        {
          names.Insert(0, value);
        }
      }
    }

    [JsonProperty("Teams", Required = Required.Always)]
    /// <summary>
    /// The teams this player is played for.
    /// A 0 represents no team.
    /// </summary>
    public IEnumerable<long> Teams
    {
      get => teams.ToArray();
      set => teams = new List<long>(value ?? new long[0]);
    }

    /// <summary>
    /// The current team id this player plays for, or 0 if not set.
    /// </summary>
    public long CurrentTeam
    {
      get => teams.Count > 0 ? teams[0] : 0;
      set
      {
        if (teams.Contains(value))
        {
          teams.Remove(value);
        }
        teams.Insert(0, value);
      }
    }

    [JsonProperty("Sources", Required = Required.Default)]
    /// <summary>
    /// Get or Set the current sources that make up this Player instance.
    /// </summary>
    public List<string> Sources { get; set; } = new List<string>();

    [JsonProperty("Id", Required = Required.Always)]
    /// <summary>
    /// The database Id of the player.
    /// </summary>
    public uint Id { get; set; }

    [JsonProperty("DiscordName", Required = Required.Default)]
    /// <summary>
    /// Get or Set the Discord Name.
    /// Null by default.
    /// </summary>
    public string DiscordName { get; set; } = null;

    [JsonProperty("FriendCode", Required = Required.Default)]
    /// <summary>
    /// Get or Set the Friend Code.
    /// Null by default.
    /// </summary>
    public string FriendCode { get; set; } = null;

    /// <summary>
    /// Merge this player with another (newer) player instance
    /// </summary>
    /// <param name="otherPlayer"></param>
    public void Merge(Player otherPlayer)
    {
      // Merge the players.
      // Iterates the other stack in reverse order so older teams are pushed first
      // so the most recent end up first in the stack.
      var reverseTeams = otherPlayer.teams;
      reverseTeams.Reverse();
      foreach (uint t in reverseTeams)
      {
        if (this.teams.Contains(t))
        {
          if (teams[0] != t)
          {
            teams.Remove(t);
            teams.Insert(0, t);
          }
        }
        else
        {
          teams.Insert(0, t);
        }
      }

      // Merge the player's name(s).
      // Iterates the other stack in reverse order so older names are pushed first
      // so the most recent end up first in the stack.
      var reversePlayers = otherPlayer.names;
      reversePlayers.Reverse();
      foreach (string n in reversePlayers)
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

      // Merge the sources.
      foreach (string source in otherPlayer.Sources)
      {
        string foundSource = Sources.Find(sources => sources.Equals(source, StringComparison.OrdinalIgnoreCase));

        if (foundSource == null)
        {
          Sources.Add(source);
        }
      }

      // Merge the misc data
      if (otherPlayer.FriendCode != null)
      {
        this.FriendCode = otherPlayer.FriendCode;
      }

      if (otherPlayer.DiscordName != null)
      {
        this.DiscordName = otherPlayer.DiscordName;
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