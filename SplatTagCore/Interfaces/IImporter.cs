namespace SplatTagCore
{
  public interface IImporter
  {
    /// <summary>
    /// Load the importer to return its Source.
    /// </summary>
    Source Load();

    /// <summary>
    /// Get the hashcode for this importer to allow for duplication testing.
    /// </summary>
    int GetHashCode();
  }
}