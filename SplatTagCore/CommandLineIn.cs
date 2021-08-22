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
    public bool Verbose { get; set; }
    public bool QueryIsClanTag { get; set; }
    public bool QueryIsTeam { get; set; }
    public bool QueryIsPlayer { get; set; }
  }
}