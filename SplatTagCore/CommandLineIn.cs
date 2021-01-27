namespace SplatTagCore
{
  public class CommandLineIn
  {
    public string? B64 { get; set; }
    public string? Query { get; set; }
    public bool ExactCase { get; set; }
    public bool ExactCharacterRecognition { get; set; }
    public bool QueryIsRegex { get; set; }
    public bool Rebuild { get; set; }
    public bool KeepOpen { get; set; }
  }
}