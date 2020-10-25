using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SplatTagCore
{
  [Serializable]
  public class CommandLineResult
  {
    [JsonProperty("Message", Required = Required.Always)]
    public string Message { get; set; }

    [JsonProperty("Players", Required = Required.Always)]
    public Player[] Players { get; set; }

    [JsonProperty("Teams", Required = Required.Always)]
    public Team[] Teams { get; set; }

    [JsonProperty("AdditionalTeams", Required = Required.Always)]
    public Dictionary<long, Team> AdditionalTeams { get; set; }

    [JsonProperty("PlayersForTeams", Required = Required.Always)]
    public Dictionary<long, (Player, bool)[]> PlayersForTeams { get; set; }
  }
}