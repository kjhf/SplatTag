using SplatTagCore.Social;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class Player : IMergable<Player>, IReadonlySourceable
  {
    /// <summary>
    /// The database Id of the player.
    /// </summary>
    [JsonPropertyName("Id")]
    [JsonRequired]
    public readonly Guid Id = Guid.NewGuid();

    /// <summary>
    /// Back-store for the weapons that the player uses (if any).
    /// </summary>
    [JsonPropertyName("Weapons")]
    private readonly List<string> weapons = new();

    /// <summary>
    /// Back-store for the two-letter country abbreviation.
    /// </summary>
    [JsonIgnore]
    private string? country;

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
      this.NamesInformation.Add(new Name(ign, source));
    }

    /// <summary>
    /// Construct a player with their name, teams, and source
    /// </summary>
    /// <param name="ign"></param>
    /// <param name="source"></param>
    public Player(string ign, IList<Guid> teams, Source source)
    {
      this.NamesInformation.Add(new Name(ign, source));
      this.TeamInformation.Add(teams, source);
    }

    /// <summary>
    /// Any names (social or IGN) this player is known by; does NOT include Battlefy.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<Name> AllKnownNames
      => new List<Name>(Names.Concat(SendouProfiles).Concat(Discord.Ids).Concat(Discord.Usernames).Concat(TwitchProfiles).Concat(TwitterProfiles).Distinct());

    /// <summary>
    /// Get the player's Battlefy profile details.
    /// </summary>
    [JsonPropertyName("Battlefy")]
    public Battlefy Battlefy { get; } = new Battlefy();

    /// <summary>
    /// Get or Set the Country.
    /// Null by default.
    /// To set this field, the value must be a two-letter abbreviation.
    /// </summary>
    [JsonPropertyName("Country")]
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
    [JsonIgnore]
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
    [JsonIgnore]
    public Guid CurrentTeam => TeamInformation.CurrentTeam ?? Team.NoTeam.Id;

    /// <summary>
    /// Get the player's Discord profile details.
    /// </summary>
    [JsonPropertyName("Discord")]
    public Discord Discord { get; } = new Discord();

    /// <summary>
    /// The known Discord Ids of the player.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyCollection<Name> DiscordIds => Discord.Ids;

    /// <summary>
    /// The known Discord usernames of the player.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyCollection<Name> DiscordNames => Discord.Usernames;

    /// <summary>
    /// Get the information regarding the friend codes for this player.
    /// </summary>
    [JsonPropertyName("FCs")]
    public FriendCodesHandler FCInformation { get; } = new FriendCodesHandler();

    /// <summary>
    /// The last known used name for the player
    /// </summary>
    [JsonIgnore]
    public Name Name => NamesInformation.MostRecent ?? Builtins.UnknownPlayerName;

    /// <summary>
    /// The in-game or registered names this player is known by.
    /// </summary>
    [JsonPropertyName("N")]
    [JsonRequired]
    public IReadOnlyCollection<Name> Names
    {
      get => NamesInformation.GetItemsUnordered();
      set => NamesInformation.Add(value);
    }

    /// <summary>
    /// The in-game or registered names this player is known by.
    /// </summary>
    [JsonIgnore]
    public NamesHandler<Name> NamesInformation { get; } = new();

    /// <summary>
    /// The plus membership(s) this player belongs to.
    /// </summary>
    [JsonPropertyName("Plus")]
    public IReadOnlyCollection<PlusMembership> PlusMembership
    {
      get => PlusMembershipInformation.GetItemsUnordered();
      protected set => PlusMembershipInformation.Add(value);
    }

    /// <summary>
    /// The plus membership(s) this player belongs to.
    /// </summary>
    [JsonIgnore]
    public NamesHandler<PlusMembership> PlusMembershipInformation { get; } = new();

    /// <summary>
    /// Player's pronoun(s)
    /// </summary>
    [JsonPropertyName("Pro")]
    public PronounsHandler PronounInformation { get; } = new();

    /// <summary>
    /// The Sendou social information this player belongs to.
    /// </summary>
    [JsonIgnore]
    public NamesHandler<Sendou> SendouInformation { get; } = new();

    /// <summary>
    /// Get the player's Sendou profile details.
    /// </summary>
    [JsonPropertyName("Sendou")]
    public IReadOnlyCollection<Sendou> SendouProfiles
    {
      get => SendouInformation.GetItemsUnordered();
      protected set => SendouInformation.Add(value);
    }

    /// <summary>
    /// The Twitch social information this player belongs to.
    /// </summary>
    [JsonIgnore]
    public NamesHandler<Twitch> TwitchInformation { get; } = new();

    /// <summary>
    /// The Twitch social information this player belongs to.
    /// </summary>
    [JsonPropertyName("Twitch")]
    public IReadOnlyCollection<Twitch> TwitchProfiles
    {
      get => TwitchInformation.GetItemsUnordered();
      protected set => TwitchInformation.Add(value);
    }

    /// <summary>
    /// The Twitter social information this player belongs to.
    /// </summary>
    [JsonIgnore]
    public NamesHandler<Twitter> TwitterInformation { get; } = new();

    /// <summary>
    /// The Twitter social information this player belongs to.
    /// </summary>
    [JsonPropertyName("Twitter")]
    public IReadOnlyCollection<Twitter> TwitterProfiles
    {
      get => TwitterInformation.GetItemsUnordered();
      protected set => TwitterInformation.Add(value);
    }

    /// <summary>
    /// Get the player's Skill/clout.
    /// </summary>
    [JsonPropertyName("Skill")]
    public Skill Skill { get; } = new Skill();

    [JsonIgnore]
    public IReadOnlyList<Source> Sources =>
      NamesInformation.Sources
        .Concat(Battlefy.Sources)
        .Concat(Discord.Sources)
        .Concat(FCInformation.Sources)
        .Concat(PlusMembershipInformation.Sources)
        .Concat(PronounInformation.Sources)
        .Concat(SendouInformation.Sources)
        .Concat(TeamInformation.Sources)
        .Concat(TwitchInformation.Sources)
        .Concat(TwitterInformation.Sources)
        .Distinct()
        .OrderByDescending(s => s)
        .ToList()
        ;

    /// <summary>
    /// The Splatnet database Id of the player (a hex string).
    /// Null by default.
    /// </summary>
    [JsonPropertyName("SplatnetId")]
    public string? SplatnetId { get; set; }

    /// <summary>
    /// Get the information regarding teams for this player.
    /// </summary>
    [JsonPropertyName("Teams")]
    public TeamsHandler TeamInformation { get; set; } = new TeamsHandler();

    /// <summary>
    /// Get or Set Top 500 flag.
    /// False by default.
    /// </summary>
    [JsonPropertyName("Top500")]
    public bool Top500 { get; set; }

    /// <summary>
    /// The weapons this player uses.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<string> Weapons => weapons;

    public void AddBattlefyInformation(string slug, string username, string persistentId, Source source)
    {
      AddBattlefySlug(slug, source);
      AddBattlefyUsername(username, source);
      AddBattlefyPersistentId(persistentId, source);
    }

    public void AddBattlefyPersistentId(string persistentId, Source source)
    {
      Battlefy.AddPersistentId(persistentId, source);
    }

    public void AddBattlefySlug(string slug, Source source)
    {
      Battlefy.AddSlug(slug, source);
    }

    public void AddBattlefyUsername(string username, Source source)
    {
      Battlefy.AddUsername(username, source);
    }

    public void AddDiscordId(string id, Source source)
    {
      Discord.AddId(id, source);
    }

    public void AddDiscordUsername(string discordNameIncludingDiscrim, Source source)
    {
      Debug.WriteLineIf(!discordNameIncludingDiscrim.Contains("#"), $"Added Discord name to player {this.Name} but it does not have a #!");
      Discord.AddUsername(discordNameIncludingDiscrim, source);
    }

    public void AddFCs(FriendCode value, Source source)
      => FCInformation.Add(value, source);

    public void AddFCs(IList<FriendCode> value, Source source)
      => FCInformation.Add(value, source);

    public void AddName(string name, Source source)
      => NamesInformation.Add(new Name(name, source));

    public void AddPlusServerMembership(int? plusLevel, Source source)
      => PlusMembershipInformation.Add(new PlusMembership(plusLevel, source));

    public void AddSendou(string handle, Source source)
      => SendouInformation.Add(new Sendou(handle, source));

    public void AddTeams(Guid value, Source source)
      => TeamInformation.Add(value, source);

    public void AddTwitch(string handle, Source source)
      => TwitchInformation.Add(new Twitch(handle, source));

    public void AddTwitter(string handle, Source source)
      => TwitterInformation.Add(new Twitter(handle, source));

    public void AddWeapons(IEnumerable<string> incoming)
    {
      weapons.AddUnique(incoming.Distinct());
    }

    /// <summary>
    /// Merge this player with another player instance.
    /// Chronology safe.
    /// </summary>
    public void Merge(Player other)
    {
      if (other == null) throw new ArgumentNullException(nameof(other));
      if (ReferenceEquals(this, other)) return;

      // Merge the teams.
      TeamInformation.Merge(other.TeamInformation);

      // Merge the player's name(s).
      NamesInformation.Merge(other.NamesInformation);

      // Merge the weapons.
      AddWeapons(other.weapons);

      // Merge the Battlefy Slugs and usernames.
      Battlefy.Merge(other.Battlefy);

      // Merge the Discord Slugs and usernames.
      Discord.Merge(other.Discord);

      // Merge the Social Data.
      PlusMembershipInformation.Merge(other.PlusMembershipInformation);
      SendouInformation.Merge(other.SendouInformation);
      TwitchInformation.Merge(other.TwitchInformation);
      TwitterInformation.Merge(other.TwitterInformation);

      // Merge the misc data
      FCInformation.Merge(other.FCInformation);
      PronounInformation.Merge(other.PronounInformation);

      if (!string.IsNullOrWhiteSpace(other.Country))
      {
        this.Country = other.Country;
      }

      if (other.Top500)
      {
        this.Top500 = true;
      }
    }

    /// <summary>
    /// Overridden ToString, returns the player's name.
    /// </summary>
    public override string ToString()
    {
      return Name.Value;
    }
  }
}