using System;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using static SplatTagCore.JSONConverters;

namespace SplatTagCore
{
  public class Source : IComparable<Source>, IEquatable<Source?>
  {
    [JsonIgnore]
    private static readonly Regex TOURNAMENT_ID_REGEX = new("-+([0-9a-fA-F]{18,})$");

    [JsonIgnore]
    private string? battlefyId;

    [JsonIgnore]
    private string? strippedTournamentName;

    /// <summary>
    /// Construct a source with a name and optional date.
    /// Date will be inferred if not specified, or set to Builtins.UnknownDateTime.
    /// </summary>
    public Source(string name = Builtins.UNKNOWN_SOURCE, DateTime? start = null)
    {
      Name = name;

      int dataStrLength = "yyyy-mm-dd-".Length;
      if (Name.Length > dataStrLength && Name.Count('-') > 2)
      {
        var dateStr = Name.Substring(0, dataStrLength).Trim('-');

        // If start wasn't specified, set from the name.
        if (start == null && DateTime.TryParse(dateStr, out var temp))
        {
          start = temp;
        }
      }

      Start = start ?? Builtins.UnknownDateTime;
    }

    /// <summary>
    /// The Battlefy Id, if applicable.
    /// </summary>
    [JsonIgnore]
    public string? BattlefyId
    {
      get
      {
        if (battlefyId == null)
        {
          LazyCalculateSourceName();
        }
        return battlefyId?.Length == 0 ? null : battlefyId;
      }
    }

    /// <summary>
    /// Get the Battlefy URL for this tournament, if it has a battlefy id associated with it.
    /// </summary>
    [JsonIgnore]
    public string? BattlefyUri
    {
      get
      {
        if (battlefyId == null)
        {
          LazyCalculateSourceName();
        }
        return battlefyId?.Length == 0 ? null : $"https://battlefy.com/_/_/{BattlefyId}/info";
      }
    }

    /// <summary>
    /// The brackets that make up the source.
    /// </summary>
    [JsonPropertyName("Brackets")]
    public Bracket[] Brackets { get; set; } = Array.Empty<Bracket>();

    /// <summary>
    /// The source identifier, which is its name.
    /// </summary>
    [JsonIgnore]
    public string Id => Name;

    /// <summary>
    /// The filename for the source
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; }

    /// <summary>
    /// The players that this source represents
    /// e.g. all players that have signed up to this tournament
    /// </summary>
    [JsonPropertyName("Players")]
    public Player[] Players { get; set; } = Array.Empty<Player>();

    /// <summary>
    /// Get the start time of the source, e.g. the tourney time or when the source was created.
    /// Used in figuring out 'current' order.
    /// Defaults to <see cref="Builtins.UnknownDateTime"/>
    /// </summary>
    [JsonPropertyName("Start")]
    [JsonConverter(typeof(DateTimeTicksConverter))]
    public DateTime Start { get; set; } = Builtins.UnknownDateTime;

    /// <summary>
    /// Get the stripped tournament name, which excludes the id and date.
    /// </summary>
    [JsonIgnore]
    public string StrippedTournamentName
    {
      get
      {
        if (strippedTournamentName == null)
        {
          LazyCalculateSourceName();
        }
        return strippedTournamentName!;
      }
    }

    /// <summary>
    /// The teams that this source represents
    /// e.g. all teams that have signed up to this tournament
    /// </summary>
    [JsonPropertyName("Teams")]
    public Team[] Teams { get; set; } = Array.Empty<Team>();

    /// <summary>
    /// Relevant URI(s) for the source
    /// </summary>
    [JsonPropertyName("Uris")]
    public Uri[] Uris { get; set; } = Array.Empty<Uri>();

    /// <summary>
    /// Compare start time to another Source's start time.
    /// 0 is same, &lt; 0 is earlier, &gt; 0 is later.
    /// </summary>
    public int CompareTo(Source other) => Start.CompareTo(other.Start);

    public override string ToString()
    {
      return Name ?? base.ToString();
    }

    private void LazyCalculateSourceName()
    {
      int dataStrLength = "yyyy-mm-dd-".Length;
      if (Name.Length > dataStrLength && Name.Count('-') > 2)
      {
        strippedTournamentName = Name[dataStrLength..].Trim('-');
      }
      else
      {
        strippedTournamentName = Name;
      }

      // Too many tourneys have this at the start of the name and it's completely redundant and screws up the grouping majority calc.
      if (strippedTournamentName.StartsWith("splatoon-2-"))
      {
        strippedTournamentName = strippedTournamentName["splatoon-2-".Length..];
      }

      var idAtNameEndMatch = TOURNAMENT_ID_REGEX.Match(Name);
      if (idAtNameEndMatch.Success)
      {
        // We always want to grab the last match as the id is at the end of the source name.
        battlefyId = idAtNameEndMatch.Groups[^1].Value;
        strippedTournamentName = strippedTournamentName[..TOURNAMENT_ID_REGEX.Match(strippedTournamentName).Index];
      }
      else
      {
        battlefyId = "";
      }
    }

    public override bool Equals(object? obj)
    {
      return Equals(obj as Source);
    }

    public bool Equals(Source? other)
    {
      return other != null &&
             Id == other.Id;
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(Id);
    }
  }
}