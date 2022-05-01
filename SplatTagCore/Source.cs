using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace SplatTagCore
{
  [Serializable]
  public class Source : ISerializable, IComparable<Source>, IEquatable<Source?>
  {
    private const string SOURCE_DATE_FORMAT = "yyyy-mm-dd-";
    private static readonly int SOURCE_DATE_FORMAT_LEN = SOURCE_DATE_FORMAT.Length;
    private static readonly Regex TOURNAMENT_ID_REGEX = new("-+([0-9a-fA-F]{18,})$");

    private string? battlefyId;
    private string? strippedTournamentName;

    /// <summary>
    /// Construct a source with a name and optional date.
    /// Date will be inferred if not specified, or set to Builtins.UnknownDateTime.
    /// </summary>
    public Source(string name = Builtins.UNKNOWN_SOURCE, DateTime? start = null)
    {
      Name = name;

      if (Name.Length > SOURCE_DATE_FORMAT_LEN && Name.Count('-') > 2)
      {
        var dateStr = Name[..SOURCE_DATE_FORMAT_LEN].Trim('-');

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
    public Bracket[] Brackets { get; set; } = Array.Empty<Bracket>();

    /// <summary>
    /// The source identifier, which is its name.
    /// </summary>
    public string Id => Name;

    /// <summary>
    /// The filename for the source
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The players that this source represents
    /// e.g. all players that have signed up to this tournament
    /// </summary>
    public Player[] Players { get; set; } = Array.Empty<Player>();

    /// <summary>
    /// Get the start time of the source, e.g. the tourney time or when the source was created.
    /// Used in figuring out 'current' order.
    /// Defaults to <see cref="Builtins.UnknownDateTime"/>
    /// </summary>
    public DateTime Start { get; set; }

    /// <summary>
    /// Get the stripped tournament name, which excludes the id and date.
    /// </summary>
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
    public Team[] Teams { get; set; } = Array.Empty<Team>();

    /// <summary>
    /// Relevant URI(s) for the source
    /// </summary>
    public Uri[] Uris { get; set; } = Array.Empty<Uri>();

    /// <summary>
    /// Compare start time to another Source's start time.
    /// 0 is same, &lt; 0 is earlier, &gt; 0 is later.
    /// </summary>
    public int CompareTo(Source other) => Start.CompareTo(other.Start);

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

    public override string ToString()
    {
      return Name ?? base.ToString();
    }

    private void LazyCalculateSourceName()
    {
      if (Name.Length > SOURCE_DATE_FORMAT_LEN && Name.Count('-') > 2)
      {
        strippedTournamentName = Name[SOURCE_DATE_FORMAT_LEN..].Trim('-');
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

    #region Serialization

    // Deserialize
    protected Source(SerializationInfo info, StreamingContext context)
    {
      this.Brackets = info.GetValueOrDefault("Brackets", Array.Empty<Bracket>());
      this.Name = info.GetString("Name");
      this.Players = info.GetValueOrDefault("Players", Array.Empty<Player>());
      this.Start = new DateTime(info.GetValueOrDefault("Start", Builtins.UNKNOWN_DATE_TIME_TICKS));
      this.Teams = info.GetValueOrDefault("Teams", Array.Empty<Team>());
      this.Uris = info.GetValueOrDefault("Uris", Array.Empty<Uri>());
    }

    // Serialize

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (this.Brackets.Length > 0)
        info.AddValue("Brackets", this.Brackets);

      if (this.Name != null)
        info.AddValue("Name", this.Name);

      if (this.Players.Length > 0)
        info.AddValue("Players", this.Players);

      if (this.Start != Builtins.UnknownDateTime)
        info.AddValue("Start", this.Start.Ticks);

      if (this.Teams.Length > 0)
        info.AddValue("Teams", this.Teams);

      if (this.Uris.Length > 0)
        info.AddValue("Uris", this.Uris);
    }

    public class SourceStringConverter
    {
      public static readonly Func<string, Source> ConstructSource = (name) => new Source(name);
      public static readonly Func<string, Source> UseBuiltIn = (_) => Builtins.BuiltinSource;
      public static readonly Func<string, Source> UseManual = (_) => Builtins.ManualSource;
      private readonly Dictionary<string, Source> lookup;

      public SourceStringConverter(Dictionary<string, Source>? lookup = null)
      {
        this.lookup = lookup ?? new Dictionary<string, Source>();
      }

      /// <summary>
      /// Convert multiple source strings into their Source objects.
      /// Optionally specify the sourceResolverFunction to change the resolving of non-existent Sources (by default it uses the Built-in Source).
      /// </summary>
      /// <param name="names"></param>
      /// <param name="sourceResolverFunction"></param>
      /// <returns></returns>
      [return: NotNullIfNotNull("sourceResolverFunction")]
      public IEnumerable<Source> Convert(IEnumerable<string> names, Func<string, Source>? sourceResolverFunction = null)
      {
        if (sourceResolverFunction == null)
          sourceResolverFunction = UseBuiltIn;

        foreach (var id in names)
          yield return Convert(id, sourceResolverFunction);
      }

      [return: NotNullIfNotNull("sourceResolverFunction")]
      public Source? Convert(string name, Func<string, Source>? sourceResolverFunction)
      {
        var source = lookup.Get(name);
        if (source == null && sourceResolverFunction != null)
        {
          source = sourceResolverFunction(name);
        }
        return source;
      }
    }

    #endregion Serialization
  }
}