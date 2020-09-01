using Newtonsoft.Json;
using System;

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
  }
}