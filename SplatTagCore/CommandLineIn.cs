using System;

namespace SplatTagCore
{
  public class CommandLineIn
  {
    public string? B64 { get; set; }
    public string? Query { get; set; }
    public string? SlappId { get; set; }
    public bool ExactCase { get; set; }
    public bool ExactCharacterRecognition { get; set; }
    public bool QueryIsRegex { get; set; }
    public string? Rebuild { get; set; }
    public string? Patch { get; set; }
    public bool KeepOpen { get; set; }
    public int Limit { get; set; }
    public bool Verbose { get; set; }
    public bool QueryIsClanTag { get; set; }
    public bool QueryIsTeam { get; set; }
    public bool QueryIsPlayer { get; set; }
  }

  public static class ConsoleOptions
  {
    public static (Type optionType, string flagName, string description, Func<object>? getDefaultValue)[] GetOptionsAsTuple()
    {
      return new (Type, string, string, Func<object>?)[] {
        (typeof(string), "--b64", "The query as a base 64 string", null),
        (typeof(string), "--query", "The query as a string. If no queryIsX flags are set, this searches everything.", null),
        (typeof(string), "--slappId", "Get the internal Slapp Id object", null),
        (typeof(bool), "--exactCase", "The query should be treated as case-sensitive. (Default: false; insensitive)", () => false),
        (typeof(bool), "--exactCharacterRecognition", "The query should disable the near-character recognition. (Default: false; use near chars)", () => false),
        (typeof(string), "--rebuild", "Rebuilds the database with the specified file. (Default: usual searching)", null),
        (typeof(string), "--patch", "Patches the database with the specified file. (Default: usual searching) ", null),
        (typeof(bool), "--keepOpen", "The process should not terminate after the query has been serviced. (Default: false; terminates)", () => false),
        (typeof(int), "--limit", "The maximum number of results to retrieve. (Default: 20)", () => 20),
        (typeof(bool), "--verbose", "Print verbose output. (Default: false)", () => false),
        (typeof(bool), "--queryIsRegex", "The query should be treated as Regex. (Default: false; interpret as text)", () => false),
        (typeof(bool), "--queryIsClanTag", "I'm looking for a Tag. (Default: false; search all unless another flag is specified)", () => false),
        (typeof(bool), "--queryIsTeam", "I'm looking for a Team. (Default: false; search all unless another flag is specified)", () => false),
        (typeof(bool), "--queryIsPlayer", "I'm looking for a Player. (Default: false; search all unless another flag is specified)", () => false)
      };
    }
  }
}