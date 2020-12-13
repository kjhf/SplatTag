using Newtonsoft.Json;
using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SplatTagDatabase.Importers
{
  internal class StatInkReader : IImporter
  {
    [Serializable]
    public class StatInkRoot
    {
      [JsonProperty("id")]
      public long Id { get; set; }

      [JsonProperty("splatnet_number")]
      public long SplatnetNumber { get; set; }

      [JsonProperty("url")]
      public Uri? Url { get; set; }

      [JsonProperty("user")]
      public User? User { get; set; }

      //[JsonProperty("lobby")]
      //public KeyNamePair Lobby { get; set; }

      //[JsonProperty("mode")]
      //public KeyNamePair Mode { get; set; }

      //[JsonProperty("rule")]
      //public KeyNamePair Rule { get; set; }

      //[JsonProperty("map")]
      //public Map Map { get; set; }

      //[JsonProperty("weapon")]
      //public StatInkReaderWeapon Weapon { get; set; }

      //[JsonProperty("freshness")]
      //public object Freshness { get; set; }

      //[JsonProperty("rank")]
      //public Rank Rank { get; set; }

      //[JsonProperty("rank_exp")]
      //public object RankExp { get; set; }

      //[JsonProperty("rank_after")]
      //public Rank RankAfter { get; set; }

      //[JsonProperty("rank_exp_after")]
      //public object RankExpAfter { get; set; }

      //[JsonProperty("x_power")]
      //public object XPower { get; set; }

      //[JsonProperty("x_power_after")]
      //public object XPowerAfter { get; set; }

      //[JsonProperty("estimate_x_power")]
      //public long EstimateXPower { get; set; }

      //[JsonProperty("level")]
      //public long Level { get; set; }

      //[JsonProperty("level_after")]
      //public long LevelAfter { get; set; }

      //[JsonProperty("star_rank")]
      //public long StarRank { get; set; }

      //[JsonProperty("result")]
      //public string Result { get; set; }

      //[JsonProperty("knock_out")]
      //public bool KnockOut { get; set; }

      //[JsonProperty("rank_in_team")]
      //public object RankInTeam { get; set; }

      //[JsonProperty("kill")]
      //public long Kill { get; set; }

      //[JsonProperty("death")]
      //public long Death { get; set; }

      //[JsonProperty("kill_or_assist")]
      //public long KillOrAssist { get; set; }

      //[JsonProperty("special")]
      //public long Special { get; set; }

      //[JsonProperty("kill_ratio")]
      //public long KillRatio { get; set; }

      //[JsonProperty("kill_rate")]
      //public double KillRate { get; set; }

      //[JsonProperty("max_kill_combo")]
      //public object MaxKillCombo { get; set; }

      //[JsonProperty("max_kill_streak")]
      //public object MaxKillStreak { get; set; }

      //[JsonProperty("death_reasons")]
      //public object[] DeathReasons { get; set; }

      //[JsonProperty("my_point")]
      //public long MyPoint { get; set; }

      //[JsonProperty("estimate_gachi_power")]
      //public object EstimateGachiPower { get; set; }

      //[JsonProperty("league_point")]
      //public object LeaguePoint { get; set; }

      //[JsonProperty("my_team_estimate_league_point")]
      //public object MyTeamEstimateLeaguePoint { get; set; }

      //[JsonProperty("his_team_estimate_league_point")]
      //public object HisTeamEstimateLeaguePoint { get; set; }

      //[JsonProperty("my_team_point")]
      //public object MyTeamPoint { get; set; }

      //[JsonProperty("his_team_point")]
      //public object HisTeamPoint { get; set; }

      //[JsonProperty("my_team_percent")]
      //public object MyTeamPercent { get; set; }

      //[JsonProperty("his_team_percent")]
      //public object HisTeamPercent { get; set; }

      //[JsonProperty("my_team_count")]
      //public long MyTeamCount { get; set; }

      //[JsonProperty("his_team_count")]
      //public long HisTeamCount { get; set; }

      //[JsonProperty("my_team_id")]
      //public object MyTeamId { get; set; }

      //[JsonProperty("his_team_id")]
      //public object HisTeamId { get; set; }

      //[JsonProperty("species")]
      //public object Species { get; set; }

      //[JsonProperty("gender")]
      //public object Gender { get; set; }

      //[JsonProperty("fest_title")]
      //public object FestTitle { get; set; }

      //[JsonProperty("fest_exp")]
      //public object FestExp { get; set; }

      //[JsonProperty("fest_title_after")]
      //public object FestTitleAfter { get; set; }

      //[JsonProperty("fest_exp_after")]
      //public object FestExpAfter { get; set; }

      //[JsonProperty("fest_power")]
      //public object FestPower { get; set; }

      //[JsonProperty("my_team_estimate_fest_power")]
      //public object MyTeamEstimateFestPower { get; set; }

      //[JsonProperty("his_team_my_team_estimate_fest_power")]
      //public object HisTeamMyTeamEstimateFestPower { get; set; }

      //[JsonProperty("my_team_fest_theme")]
      //public object MyTeamFestTheme { get; set; }

      //[JsonProperty("his_team_fest_theme")]
      //public object HisTeamFestTheme { get; set; }

      //[JsonProperty("my_team_nickname")]
      //public object MyTeamNickname { get; set; }

      //[JsonProperty("his_team_nickname")]
      //public object HisTeamNickname { get; set; }

      //[JsonProperty("clout")]
      //public object Clout { get; set; }

      //[JsonProperty("total_clout")]
      //public object TotalClout { get; set; }

      //[JsonProperty("total_clout_after")]
      //public object TotalCloutAfter { get; set; }

      //[JsonProperty("my_team_win_streak")]
      //public object MyTeamWinStreak { get; set; }

      //[JsonProperty("his_team_win_streak")]
      //public object HisTeamWinStreak { get; set; }

      //[JsonProperty("synergy_bonus")]
      //public object SynergyBonus { get; set; }

      //[JsonProperty("special_battle")]
      //public object SpecialBattle { get; set; }

      //[JsonProperty("image_judge")]
      //public object ImageJudge { get; set; }

      //[JsonProperty("image_result")]
      //public Uri ImageResult { get; set; }

      //[JsonProperty("image_gear")]
      //public object ImageGear { get; set; }

      //[JsonProperty("gears")]
      //public Gears Gears { get; set; }

      //[JsonProperty("period")]
      //public long Period { get; set; }

      //[JsonProperty("period_range")]
      //public string PeriodRange { get; set; }

      [JsonProperty("players")]
      public PlayerElement[]? Players { get; set; }

      //[JsonProperty("events")]
      //public object Events { get; set; }

      //[JsonProperty("splatnet_json")]
      //public SplatnetJson SplatnetJson { get; set; }

      //[JsonProperty("agent")]
      //public Agent Agent { get; set; }

      [JsonProperty("automated")]
      public bool Automated { get; set; }

      //[JsonProperty("environment")]
      //public string Environment { get; set; }

      //[JsonProperty("link_url")]
      //public object LinkUrl { get; set; }

      //[JsonProperty("note")]
      //public object Note { get; set; }

      //[JsonProperty("game_version")]
      //public string GameVersion { get; set; }

      //[JsonProperty("nawabari_bonus")]
      //public object NawabariBonus { get; set; }

      //[JsonProperty("start_at")]
      //public TimeNode StartAt { get; set; }

      [JsonProperty("end_at")]
      public TimeNode? EndAt { get; set; }

      //[JsonProperty("register_at")]
      //public TimeNode RegisterAt { get; set; }
    }

    public class TimeNode
    {
      [JsonProperty("time")]
      public long Time { get; set; }

      [JsonProperty("iso8601")]
      public DateTimeOffset? Iso8601 { get; set; }
    }

    public class TranslatableName
    {
      //[JsonProperty("de_DE")]
      //public string DeDe { get; set; }

      [JsonProperty("en_GB")]
      public string? EnGb { get; set; }

      //[JsonProperty("en_US")]
      //public string EnUs { get; set; }

      //[JsonProperty("es_ES")]
      //public string EsEs { get; set; }

      //[JsonProperty("es_MX")]
      //public string EsMx { get; set; }

      //[JsonProperty("fr_CA")]
      //public string FrCa { get; set; }

      //[JsonProperty("fr_FR")]
      //public string FrFr { get; set; }

      //[JsonProperty("it_IT")]
      //public string ItIt { get; set; }

      //[JsonProperty("ja_JP")]
      //public string JaJp { get; set; }

      //[JsonProperty("nl_NL")]
      //public string NlNl { get; set; }

      //[JsonProperty("ru_RU")]
      //public string RuRu { get; set; }

      //[JsonProperty("zh_CN")]
      //public string ZhCn { get; set; }

      //[JsonProperty("zh_TW")]
      //public string ZhTw { get; set; }
    }

    public class KeyNamePair
    {
      [JsonProperty("key")]
      public string? Key { get; set; }

      [JsonProperty("name")]
      public TranslatableName? Name { get; set; }
    }

    public class PlayerElement
    {
      [JsonProperty("team")]
      public string? Team { get; set; }

      [JsonProperty("is_me")]
      public bool IsMe { get; set; }

      [JsonProperty("weapon")]
      public StatInkReaderWeapon? Weapon { get; set; }

      [JsonProperty("level")]
      public long Level { get; set; }

      //[JsonProperty("rank")]
      //public Rank Rank { get; set; }

      [JsonProperty("star_rank")]
      public long StarRank { get; set; }

      //[JsonProperty("rank_in_team")]
      //public object RankInTeam { get; set; }

      //[JsonProperty("kill")]
      //public long Kill { get; set; }

      //[JsonProperty("death")]
      //public long Death { get; set; }

      //[JsonProperty("kill_or_assist")]
      //public long KillOrAssist { get; set; }

      //[JsonProperty("special")]
      //public long Special { get; set; }

      //[JsonProperty("my_kill")]
      //public object MyKill { get; set; }

      //[JsonProperty("point")]
      //public long Point { get; set; }

      [JsonProperty("name")]
      public string? Name { get; set; }

      //[JsonProperty("species")]
      //public object Species { get; set; }

      //[JsonProperty("gender")]
      //public object Gender { get; set; }

      //[JsonProperty("fest_title")]
      //public object FestTitle { get; set; }

      [JsonProperty("splatnet_id")]
      public string? SplatnetId { get; set; }

      [JsonProperty("top_500")]
      public bool? Top500 { get; set; }

      [JsonProperty("icon")]
      public string? Icon { get; set; }
    }

    public class StatInkReaderWeapon
    {
      //[JsonProperty("key")]
      //public string Key { get; set; }

      //[JsonProperty("splatnet")]
      //public long Splatnet { get; set; }

      //[JsonProperty("type")]
      //public TypeClass Type { get; set; }

      //[JsonProperty("name")]
      //public TranslatableName Name { get; set; }

      //[JsonProperty("sub")]
      //public KeyNamePair Sub { get; set; }

      //[JsonProperty("special")]
      //public KeyNamePair Special { get; set; }

      //[JsonProperty("reskin_of")]
      //public string ReskinOf { get; set; }

      [JsonProperty("main_ref")]
      public string? MainRef { get; set; }

      //[JsonProperty("main_power_up")]
      //public KeyNamePair MainPowerUp { get; set; }
    }

    //public class SplatnetJson
    //{
    //  [JsonProperty("rank")]
    //  public object Rank { get; set; }

    //  [JsonProperty("rule")]
    //  public Rule Rule { get; set; }

    //  [JsonProperty("type")]
    //  public string Type { get; set; }

    //  //[JsonProperty("stage")]
    //  //public Stage Stage { get; set; }

    //  [JsonProperty("x_power")]
    //  public object XPower { get; set; }

    //  [JsonProperty("game_mode")]
    //  public GameMode GameMode { get; set; }

    //  [JsonProperty("star_rank")]
    //  [JsonConverter(typeof(ParseStringConverter))]
    //  public long StarRank { get; set; }

    //  [JsonProperty("start_time")]
    //  [JsonConverter(typeof(ParseStringConverter))]
    //  public long StartTime { get; set; }

    //  [JsonProperty("player_rank")]
    //  [JsonConverter(typeof(ParseStringConverter))]
    //  public long PlayerRank { get; set; }

    //  [JsonProperty("elapsed_time")]
    //  [JsonConverter(typeof(ParseStringConverter))]
    //  public long ElapsedTime { get; set; }

    //  [JsonProperty("battle_number")]
    //  [JsonConverter(typeof(ParseStringConverter))]
    //  public long BattleNumber { get; set; }

    //  [JsonProperty("crown_players")]
    //  public string[] CrownPlayers { get; set; }

    //  [JsonProperty("my_team_count")]
    //  [JsonConverter(typeof(ParseStringConverter))]
    //  public long MyTeamCount { get; set; }

    //  //[JsonProperty("player_result")]
    //  //public PlayerResult PlayerResult { get; set; }

    //  [JsonProperty("my_team_result")]
    //  public GameMode MyTeamResult { get; set; }

    //  [JsonProperty("my_team_members")]
    //  public TeamMember[] MyTeamMembers { get; set; }

    //  [JsonProperty("estimate_x_power")]
    //  [JsonConverter(typeof(ParseStringConverter))]
    //  public long EstimateXPower { get; set; }

    //  [JsonProperty("other_team_count")]
    //  [JsonConverter(typeof(ParseStringConverter))]
    //  public long OtherTeamCount { get; set; }

    //  [JsonProperty("other_team_result")]
    //  public GameMode OtherTeamResult { get; set; }

    //  [JsonProperty("other_team_members")]
    //  public TeamMember[] OtherTeamMembers { get; set; }

    //  [JsonProperty("weapon_paint_point")]
    //  [JsonConverter(typeof(ParseStringConverter))]
    //  public long WeaponPaintPoint { get; set; }

    //  [JsonProperty("estimate_gachi_power")]
    //  public object EstimateGachiPower { get; set; }
    //}

    //public class GameMode
    //{
    //  [JsonProperty("key")]
    //  public string Key { get; set; }

    //  [JsonProperty("name")]
    //  public string Name { get; set; }
    //}

    //public class TeamMember
    //{
    //  [JsonProperty("player")]
    //  public MyTeamMemberPlayer Player { get; set; }

    //  [JsonProperty("kill_count")]
    //  [JsonConverter(typeof(ParseStringConverter))]
    //  public long KillCount { get; set; }

    //  [JsonProperty("sort_score")]
    //  [JsonConverter(typeof(ParseStringConverter))]
    //  public long SortScore { get; set; }

    //  [JsonProperty("death_count")]
    //  [JsonConverter(typeof(ParseStringConverter))]
    //  public long DeathCount { get; set; }

    //  [JsonProperty("assist_count")]
    //  [JsonConverter(typeof(ParseStringConverter))]
    //  public long AssistCount { get; set; }

    //  [JsonProperty("special_count")]
    //  [JsonConverter(typeof(ParseStringConverter))]
    //  public long SpecialCount { get; set; }

    //  [JsonProperty("game_paint_point")]
    //  [JsonConverter(typeof(ParseStringConverter))]
    //  public long GamePaintPoint { get; set; }
    //}

    //public class MyTeamMemberPlayer
    //{
    //  [JsonProperty("nickname")]
    //  public string Nickname { get; set; }

    //  [JsonProperty("star_rank")]
    //  [JsonConverter(typeof(ParseStringConverter))]
    //  public long StarRank { get; set; }

    //  [JsonProperty("player_rank")]
    //  [JsonConverter(typeof(ParseStringConverter))]
    //  public long PlayerRank { get; set; }

    //  [JsonProperty("principal_id")]
    //  public string PrincipalId { get; set; }
    //}

    //public class Rule
    //{
    //  [JsonProperty("key")]
    //  public string Key { get; set; }

    //  [JsonProperty("name")]
    //  public string Name { get; set; }

    //  [JsonProperty("multiline_name")]
    //  public string MultilineName { get; set; }
    //}

    public class User
    {
      [JsonProperty("id")]
      public long? Id { get; set; }

      [JsonProperty("name")]
      public string? Name { get; set; }

      [JsonProperty("screen_name")]
      public string? ScreenName { get; set; }

      [JsonProperty("url")]
      public Uri? Url { get; set; }

      //[JsonProperty("join_at")]
      //public TimeNode JoinAt { get; set; }

      [JsonProperty("profile")]
      public Profile? Profile { get; set; }

      //[JsonProperty("stat")]
      //public object Stat { get; set; }
    }

    public class Profile
    {
      //[JsonProperty("nnid")]
      //public string Nnid { get; set; }

      [JsonProperty("friend_code")]
      public string? FriendCode { get; set; }

      [JsonProperty("twitter")]
      public string? Twitter { get; set; }

      //[JsonProperty("ikanakama")]
      //public object Ikanakama { get; set; }

      //[JsonProperty("ikanakama2")]
      //public object Ikanakama2 { get; set; }

      //[JsonProperty("environment")]
      //public string Environment { get; set; }
    }

    internal class ParseStringConverter : JsonConverter
    {
      public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

      public override object? ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
      {
        if (reader.TokenType == JsonToken.Null) return null;
        var value = serializer.Deserialize<string>(reader);
        return long.Parse(value);
      }

      public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
      {
        if (untypedValue == null)
        {
          serializer.Serialize(writer, null);
          return;
        }
        var value = (long)untypedValue;
        serializer.Serialize(writer, value.ToString());
      }

      public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }

    private readonly string jsonFile;

    public StatInkReader(string jsonFile)
    {
      this.jsonFile = jsonFile ?? throw new ArgumentNullException(nameof(jsonFile));
    }

    public (Player[], Team[]) Load()
    {
      Debug.WriteLine("Loading " + jsonFile);
      string json = File.ReadAllText(jsonFile);
      StatInkRoot root = JsonConvert.DeserializeObject<StatInkRoot>(json);

      List<Player> players = new List<Player>();

      // Don't load the details if they are manual entries.
      if (!root.Automated || root.Players == null)
      {
        return (players.ToArray(), Array.Empty<Team>());
      }

      foreach (var p in root.Players)
      {
        Source source = new Source(Path.GetFileNameWithoutExtension(jsonFile))
        {
          Uri = root.Url
        };
        var newPlayer = new Player(p.Name ?? Builtins.UNKNOWN_PLAYER, source)
        {
          SplatnetId = p.SplatnetId,
          Top500 = p.Top500 == true,
        };
        if (p.IsMe)
        {
          if (root.User?.Profile?.Twitter != null)
          {
            newPlayer.AddTwitter(root.User.Profile.Twitter, source);
          }
          if (root.User?.Profile?.FriendCode != null)
          {
            if (FriendCode.TryParse(root.User.Profile.FriendCode, out FriendCode friendCode))
            {
              newPlayer.AddFCs(friendCode.AsEnumerable());
            }
          }
        }
        if (p.Weapon?.MainRef != null)
        {
          newPlayer.AddWeapons(new string[] { p.Weapon.MainRef });
        }
        players.Add(newPlayer);
      }

      return (players.ToArray(), Array.Empty<Team>());
    }

    public static bool AcceptsInput(string input)
    {
      // Must contain stat.ink
      return Path.GetFileName(input).Contains("stat.ink-") && Path.GetExtension(input).Equals(".json", StringComparison.OrdinalIgnoreCase);
    }
  }
}