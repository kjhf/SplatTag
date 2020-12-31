using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SplatTagCore
{
  [Serializable]
  public class CommandLineResult
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
    public Dictionary<Guid, string> Sources { get; set; } = new Dictionary<Guid, string>();
  }
}