using SplatTagCore.Social;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class Player : ISerializable, IReadonlySourceable
  {
    /// <summary>
    /// The database Id of the player.
    /// </summary>
    public readonly Guid Id = Guid.NewGuid();

    /// <summary>
    /// Back-store for the two-letter country abbreviation.
    /// </summary>
    private string? country;

    /// <summary>
    /// Back-store for player's FCs.
    /// </summary>
    private readonly List<FriendCode> friendCodes = new List<FriendCode>();

    /// <summary>
    /// Back-store for the names of this player. The this element is the current name.
    /// </summary>
    private readonly List<Name> names = new List<Name>();

    /// <summary>
    /// Back-store for the Sendou Profiles of this player.
    /// </summary>
    private readonly List<Sendou> sendouProfiles = new List<Sendou>();

    /// <summary>
    /// Back-store for the skill of this player.
    /// </summary>
    private readonly Skill skill = new Skill();

    /// <summary>
    /// Back-store for the team GUIDs for this player. The this element is the current team.
    /// No team represented by <see cref="Team.NoTeam.Id"/>.
    /// </summary>
    private readonly List<Guid> teams = new List<Guid>();

    /// <summary>
    /// Back-store for the Twitch Profiles of this player.
    /// </summary>
    private readonly List<Twitch> twitchProfiles = new List<Twitch>();

    /// <summary>
    /// Back-store for the Twitter Profiles of this player.
    /// </summary>
    private readonly List<Twitter> twitterProfiles = new List<Twitter>();

    /// <summary>
    /// Back-store for the weapons that the player uses (if any).
    /// </summary>
    private readonly List<string> weapons = new List<string>();

    /// <summary>
    /// Default construct a player
    /// </summary>
    public Player()
    {
    }

    /// <summary>
    /// Construct a player with their name and source
    /// </summary>
    /// <param name="ign"></param>
    /// <param name="source"></param>
    public Player(string ign, Source source)
    {
      this.names.Add(new Name(ign, source));
    }

    /// <summary>
    /// Construct a player with their name, teams, and source
    /// </summary>
    /// <param name="ign"></param>
    /// <param name="source"></param>
    public Player(string ign, IEnumerable<Guid> teams, Source source)
    {
      this.names.Add(new Name(ign, source));
      this.teams.AddRange(teams);
    }

    /// <summary>
    /// Get the player's Battlefy profile details.
    /// </summary>
    public Battlefy Battlefy { get; } = new Battlefy();

    /// <summary>
    /// Get or Set the Country.
    /// Null by default.
    /// To set this field, the value must be a two-letter abbreviation.
    /// </summary>
    public string? Country
    {
      get => country;
      set
      {
        if (value == null)
        {
          country = null;
        }
        else
        {
          value = value.Trim();
          if (value.Length == 2)
          {
            country = value.ToUpper();
          }
        }
      }
    }

    /// <summary>
    /// Get the emoji flag of the <see cref="Country"/> specified.
    /// </summary>
    /// <remarks>
    /// Magic number is the offset '🇦' - 'A'
    /// </remarks>
    public string? CountryFlag
    {
      get
      {
        if (Country == null) return null;
        return string.Concat(Country.Select(x => char.ConvertFromUtf32(x + 0x1F1A5)));
      }
    }

    /// <summary>
    /// The current team id this player plays for, or <see cref="Team.NoTeam.Id"/> if not set.
    /// </summary>
    public Guid CurrentTeam => teams.Count > 0 ? teams[0] : Team.NoTeam.Id;

    /// <summary>
    /// Get the player's Discord profile details.
    /// </summary>
    public Discord Discord { get; } = new Discord();

    /// <summary>
    /// The known Discord Ids of the player.
    /// </summary>
    public IReadOnlyList<Name> DiscordIds => Discord.Ids;

    /// <summary>
    /// The known Discord usernames of the player.
    /// </summary>
    public IReadOnlyList<Name> DiscordNames => Discord.Usernames;

    /// <summary>
    /// The known Friend Codes of the player.
    /// </summary>
    public IReadOnlyList<FriendCode> FriendCodes => friendCodes;

    /// <summary>
    /// The last known used name for the player
    /// </summary>
    public Name Name => names.Count > 0 ? names[0] : Builtins.UnknownPlayerName;

    /// <summary>
    /// The names this player is known by.
    /// </summary>
    public IReadOnlyList<Name> Names => names;

    /// <summary>
    /// Get the player's Sendou profile details.
    /// </summary>
    public IReadOnlyList<Sendou> SendouProfiles => sendouProfiles;

    public IReadOnlyList<Source> Sources =>
      names.SelectMany(n => n.Sources)
      .Concat(sendouProfiles.SelectMany(s => s.Sources))
      .Concat(twitchProfiles.SelectMany(s => s.Sources))
      .Concat(twitterProfiles.SelectMany(s => s.Sources))
      .Concat(Battlefy.PersistentIds.SelectMany(s => s.Sources))
      .Concat(Discord.Usernames.SelectMany(s => s.Sources))
      .Distinct()
      .OrderByDescending(s => s)
      .ToList()
      ;

    /// <summary>
    /// The Splatnet database Id of the player (a hex string).
    /// Null by default.
    /// </summary>
    public string? SplatnetId { get; set; }

    /// <summary>
    /// The teams this player is played for.
    /// No Team represented by <see cref="Team.NoTeam.Id"/>
    /// </summary>
    public IReadOnlyList<Guid> Teams => teams;

    /// <summary>
    /// Get or Set Top 500 flag.
    /// False by default.
    /// </summary>
    public bool Top500 { get; set; }

    /// <summary>
    /// The Names of this Player transformed.
    /// </summary>
    public IEnumerable<string> TransformedNames => Names.Select(n => n.Transformed);

    /// <summary>
    /// Get the player's Twitch profile details.
    /// </summary>
    public IReadOnlyList<Twitch> Twitch => twitchProfiles;

    /// <summary>
    /// Get the player's Twitter profile details.
    /// </summary>
    public IReadOnlyList<Twitter> Twitter => twitterProfiles;

    /// <summary>
    /// The weapons this player uses.
    /// </summary>
    public IReadOnlyList<string> Weapons => weapons;

    public void AddBattlefy(Battlefy value)
    {
      Battlefy.AddSlugs(value.Slugs);
      Battlefy.AddUsernames(value.Usernames);
      Battlefy.AddPersistentIds(value.PersistentIds);
    }

    public void AddBattlefyInformation(string slug, string username, string persistentId, Source source)
    {
      AddBattlefySlug(slug, source);
      AddBattlefyUsername(username, source);
      AddBattlefyPersistentId(persistentId, source);
    }

    public void AddBattlefySlug(string slug, Source source)
    {
      Battlefy.AddSlug(slug, source);
    }

    public void AddBattlefyUsername(string username, Source source)
    {
      Battlefy.AddUsername(username, source);
    }

    public void AddBattlefyPersistentId(string persistentId, Source source)
    {
      Battlefy.AddPersistentId(persistentId, source);
    }

    public void AddDiscord(Discord value)
    {
      Discord.AddIds(value.Ids);
      Discord.AddUsernames(value.Usernames);
    }

    public void AddDiscordId(string id, Source source)
    {
      Discord.AddId(id, source);
    }

    public void AddDiscordUsername(string username, Source source)
    {
      Discord.AddUsername(username, source);
    }

    public void AddFCs(IEnumerable<FriendCode> value)
    {
      if (value != null)
      {
        value = value.Where(fc => fc != FriendCode.NO_FRIEND_CODE);
        SplatTagCommon.InsertFrontUnique(value, friendCodes);
      }
    }

    public void AddName(string name, Source source)
    {
      SplatTagCommon.AddName(new Name(name, source), names);
    }

    public void AddNames(IEnumerable<Name> value)
    {
      SplatTagCommon.AddNames(value, names);
    }

    public void AddSendou(string handle, Source source)
    {
      SplatTagCommon.AddName(new Sendou(handle, source), sendouProfiles);
    }

    public void AddSendou(IEnumerable<Sendou> value)
    {
      SplatTagCommon.AddNames(value, sendouProfiles);
    }

    public void AddTeams(IEnumerable<Guid> value)
    {
      SplatTagCommon.InsertFrontUnique(value, teams);
    }

    public void AddTwitch(string handle, Source source)
    {
      SplatTagCommon.AddName(new Twitch(handle, source), twitchProfiles);
    }

    public void AddTwitch(IEnumerable<Twitch> value)
    {
      SplatTagCommon.AddNames(value, twitchProfiles);
    }

    public void AddTwitter(string handle, Source source)
    {
      SplatTagCommon.AddName(new Twitter(handle, source), twitterProfiles);
    }

    public void AddTwitter(IEnumerable<Twitter> value)
    {
      SplatTagCommon.AddNames(value, twitterProfiles);
    }

    public void AddWeapons(IEnumerable<string> value)
    {
      SplatTagCommon.AddStrings(value, weapons);
    }

    /// <summary>
    /// Correct the team ids for this player given a merge result (containing old id --> the replacement id)
    /// Returns if any work was done.
    /// </summary>
    public bool CorrectTeamIds(IDictionary<Guid, Guid> teamsMergeResult)
    {
      // Simple cases
      if (teams.Count == 0)
      {
        return false;
      }
      else if (teams.Count == 1)
      {
        if (teamsMergeResult.ContainsKey(teams[0]))
        {
          teams[0] = teamsMergeResult[teams[0]];
          return true;
        }
        else
        {
          return false;
        }
      }
      // Otherwise, for each team, correct the team id and de-dupe.
      else
      {
        bool workDone = false;
        var result = new HashSet<Guid>();

        foreach (var id in teams)
        {
          // If the merge result has this id changed
          if (teamsMergeResult.ContainsKey(id))
          {
            // Add the updated id (if not already present).
            result.Add(teamsMergeResult[id]);
            workDone = true;
          }
          else
          {
            // Otherwise take the previous id and add it (if not already present).
            result.Add(id);
          }
        }

        // Set our team ids if any changes were made.
        if (workDone)
        {
          teams.Clear();
          teams.AddRange(result);
        }

        return workDone;
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

      // Merge the teams.
      AddTeams(newerPlayer.teams);

      // Merge the player's name(s).
      AddNames(newerPlayer.names);

      // Merge the weapons.
      AddWeapons(newerPlayer.weapons);

      // Merge the Battlefy Slugs and usernames.
      AddBattlefy(newerPlayer.Battlefy);

      // Merge the Discord Slugs and usernames.
      AddDiscord(newerPlayer.Discord);

      // Merge the Social Data.
      AddSendou(newerPlayer.SendouProfiles);
      AddTwitch(newerPlayer.twitchProfiles);
      AddTwitter(newerPlayer.twitterProfiles);

      // Merge the misc data
      AddFCs(newerPlayer.friendCodes);

      if (!string.IsNullOrWhiteSpace(newerPlayer.Country))
      {
        this.Country = newerPlayer.Country;
      }

      if (newerPlayer.Top500)
      {
        this.Top500 = true;
      }
    }

    /// <summary>
    /// Overridden ToString.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return Name.Value;
    }

    #region Serialization

    // Deserialize
    protected Player(SerializationInfo info, StreamingContext context)
    {
      AddBattlefy(info.GetValueOrDefault("Battlefy", new Battlefy()));
      this.Country = info.GetValueOrDefault("Country", default(string));
      AddDiscord(info.GetValueOrDefault("Discord", new Discord()));
      AddFCs(info.GetValueOrDefault("FriendCode", Array.Empty<FriendCode>()));
      AddNames(info.GetValueOrDefault("Names", Array.Empty<Name>()));
      AddSendou(info.GetValueOrDefault("Sendou", Array.Empty<Sendou>()));

      Skill[] skills = info.GetValueOrDefault("Skill", Array.Empty<Skill>());
      this.skill = skills.Length == 1 ? skills[0] : new Skill();
      AddTeams(info.GetValueOrDefault("Teams", Array.Empty<Guid>()));
      this.Top500 = info.GetValueOrDefault("Top500", false);
      AddTwitch(info.GetValueOrDefault("Twitch", Array.Empty<Twitch>()));
      AddTwitter(info.GetValueOrDefault("Twitter", Array.Empty<Twitter>()));
      AddWeapons(info.GetValueOrDefault("Weapons", Array.Empty<string>()));

      this.Id = info.GetValueOrDefault("Id", Guid.Empty);
      if (this.Id == Guid.Empty)
      {
        throw new SerializationException("Guid cannot be empty for player: " + this.Name + " from source(s) [" + string.Join(", ", this.Sources) + "].");
      }
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (this.Battlefy.Slugs.Count > 0 || this.Battlefy.Usernames.Count > 0 || this.Battlefy.PersistentIds.Count > 0)
        info.AddValue("Battlefy", this.Battlefy);

      if (this.Country != null)
        info.AddValue("Country", this.Country);

      if (this.DiscordIds.Count > 0 || this.DiscordNames.Count > 0)
        info.AddValue("Discord", this.Discord);

      if (this.friendCodes.Count > 0)
        info.AddValue("FriendCode", this.friendCodes);

      info.AddValue("Id", this.Id);

      if (this.names.Count > 0)
        info.AddValue("Names", this.names);

      if (this.sendouProfiles.Count > 0)
        info.AddValue("Sendou", this.sendouProfiles);

      if (!this.skill.IsDefault)
        info.AddValue("Skill", this.skill);

      if (this.teams.Count > 0)
        info.AddValue("Teams", this.teams);

      if (this.Top500)
        info.AddValue("Top500", this.Top500);

      if (this.twitchProfiles.Count > 0)
        info.AddValue("Twitch", this.twitchProfiles);

      if (this.twitterProfiles.Count > 0)
        info.AddValue("Twitter", this.twitterProfiles);

      if (this.weapons.Count > 0)
        info.AddValue("Weapons", this.weapons);
    }

    [OnDeserialized]
    private void OnDeserialization(StreamingContext context)
    {
      // Nothing to do yet - versioning information and compatibility may
      // go here in the future.
    }

    #endregion Serialization
  }
}