namespace SplatTagCore
{
  public interface IImporter
  {
    /// <summary>
    /// Load the importer to return its Players and Teams.
    /// </summary>
    /// <returns></returns>
    (Player[], Team[]) Load();
  }
}