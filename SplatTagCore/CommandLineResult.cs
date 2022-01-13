using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SplatTagCore
{
  [Serializable]
  public record CommandLineResult
  {
    [JsonProperty("Message", Required = Required.Always)]
    public string Message { get; set; } = "Message not set!";

    [JsonProperty("Query", Required = Required.Always)]
    public string Query { get; set; } = string.Empty;

    [JsonProperty("Options", Required = Required.Always)]
    public MatchOptions Options { get; set; } = new MatchOptions();

    [JsonProperty("Players", Required = Required.Always)]
    public Player[] Players { get; set; } = Array.Empty<Player>();

    [JsonProperty("Teams", Required = Required.Always)]
    public Team[] Teams { get; set; } = Array.Empty<Team>();

    [JsonProperty("AdditionalTeams", Required = Required.Always)]
    public Dictionary<Guid, Team> AdditionalTeams { get; set; } = new Dictionary<Guid, Team>();

    [JsonProperty("PlayersForTeams", Required = Required.Always)]
    public Dictionary<Guid, (Player, bool)[]> PlayersForTeams { get; set; } = new Dictionary<Guid, (Player, bool)[]>();

    [JsonProperty("Sources", Required = Required.Always)]
    public string[] Sources { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Dictionary keyed by Player id, of value
    /// Dictionary keyed by Source id of value Bracket array
    /// </summary>
    [JsonProperty("PlacementsForPlayers", Required = Required.Always)]
    public Dictionary<Guid, Dictionary<string, Bracket[]>> PlacementsForPlayers { get; set; } = new Dictionary<Guid, Dictionary<string, Bracket[]>>();
  }
}