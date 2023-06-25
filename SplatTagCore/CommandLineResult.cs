using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SplatTagCore
{
  public class CommandLineResult
  {
    [JsonPropertyName("Message")]
    public string Message { get; set; } = "Message not set!";

    [JsonPropertyName("Query")]
    public string Query { get; set; } = string.Empty;

    [JsonPropertyName("Options")]
    public MatchOptions Options { get; set; } = new MatchOptions();

    [JsonPropertyName("Players")]
    public Player[] Players { get; set; } = Array.Empty<Player>();

    [JsonPropertyName("Teams")]
    public Team[] Teams { get; set; } = Array.Empty<Team>();

    [JsonPropertyName("AdditionalTeams")]
    public Dictionary<Guid, Team> AdditionalTeams { get; set; } = new Dictionary<Guid, Team>();

    [JsonPropertyName("PlayersForTeams")]
    public Dictionary<Guid, (Player, bool)[]> PlayersForTeams { get; set; } = new Dictionary<Guid, (Player, bool)[]>();

    [JsonPropertyName("Sources")]
    public string[] Sources { get; set; } = Array.Empty<string>();

    [JsonPropertyName("PlacementsForPlayers")]
    public Dictionary<Guid, Dictionary<string, Bracket[]>> PlacementsForPlayers { get; set; } = new Dictionary<Guid, Dictionary<string, Bracket[]>>();
  }
}