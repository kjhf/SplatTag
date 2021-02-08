namespace SplatTagCore
{
  public interface IImporter
  {
    /// <summary>
    /// Load the importer to return its Source.
    /// </summary>
    Source Load();
  }
}