using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagCore;
using SplatTagDatabase.Importers;
using System.IO;

namespace SplatTagUnitTests
{
  /// <summary>
  /// LUTI JSON import unit test
  /// </summary>
  [TestClass]
  public class LUTIImporterUnitTests
  {
    /// <summary>
    /// Test match a player by (no match).
    /// </summary>
    [TestMethod]
    public void MatchPlayerNoMatchTest()
    {
      UnitTestDatabase database = new UnitTestDatabase();
      SplatTagController controller = new SplatTagController(database);
      controller.Initialise(new string[] { "P.exe" });

      string filePath = Path.GetTempFileName() + ".json";
      const string JSON =
        @"[
  {
    ""Team Name"": ""Example Team"",
    ""Division"": ""X"",
    ""Tag"": ""ex"",
    ""Team Captain"": ""Cap 1 ex"",
    ""Player 2"": ""P2 ex"",
    ""Player 3"": ""P3 ex"",
    ""Player 4"": ""P4 ex"",
    ""Player 5"": ""Sub ex"",
    ""Player 6"": """",
    ""Player 7"": """",
    ""Player 8"": """",
    ""Player 9"": """",
    ""Player 10"": """"
  },
  {
    ""Team Name"": ""Another Team"",
    ""Division"": ""1"",
    ""Tag"": ""AT"",
    ""Team Captain"": ""AT CAP"",
    ""Player 2"": ""AT A2"",
    ""Player 3"": ""AT A3"",
    ""Player 4"": ""AT A4"",
    ""Player 5"": ""AT Alpha"",
    ""Player 6"": ""AT Beta"",
    ""Player 7"": ""AT Charlie"",
    ""Player 8"": ""AT Delta"",
    ""Player 9"": ""AT Echo"",
    ""Player 10"": ""AT Foxtrot""
  },
  {
    ""Team Name"": ""Oh No"",
    ""Division"": ""D"",
    ""Tag"": ""//"",
    ""Team Captain"": ""/Oops/"",
    ""Player 2"": ""/We/"",
    ""Player 3"": ""/Dropped/"",
    ""Player 4"": """",
    ""Player 5"": """",
    ""Player 6"": """",
    ""Player 7"": """",
    ""Player 8"": """",
    ""Player 9"": """",
    ""Player 10"": """"
  }
]";

      try
      {
        File.WriteAllText(filePath, JSON);
        LUTIJsonReader reader = new LUTIJsonReader(filePath);
        var loaded = reader.Load();

        Assert.AreEqual(3, loaded.Item2.Length);
        Assert.AreEqual(18, loaded.Item1.Length);

        Assert.AreEqual("Example Team", loaded.Item2[0].Name);
        Assert.AreEqual("ex", loaded.Item2[0].ClanTags[0]);
        Assert.AreEqual("Another Team", loaded.Item2[1].Name);
        Assert.AreEqual("AT", loaded.Item2[1].ClanTags[0]);
        Assert.AreEqual("Oh No", loaded.Item2[2].Name);
        Assert.AreEqual("//", loaded.Item2[2].ClanTags[0]);

        Assert.AreEqual(TagOption.Back, loaded.Item2[0].ClanTagOption);
        Assert.AreEqual(TagOption.Front, loaded.Item2[1].ClanTagOption);
        Assert.AreEqual(TagOption.Surrounding, loaded.Item2[2].ClanTagOption);

        Assert.AreEqual("Cap 1", loaded.Item1[0].Name); // Assert name was loaded without the tag.
        Assert.AreEqual("P2", loaded.Item1[1].Name); // Assert name was loaded without the tag.
        Assert.AreEqual("Example Team", loaded.Item1[1].CurrentTeam.Name); // Test Current Team is set

        Assert.AreEqual("CAP", loaded.Item1[5].Name); // Assert name was loaded without the tag.
        Assert.AreEqual("A2", loaded.Item1[6].Name); // Assert name was loaded without the tag.
        Assert.AreEqual("Another Team", loaded.Item1[6].CurrentTeam.Name); // Test Current Team is set

        Assert.AreEqual("Oh No", loaded.Item1[15].CurrentTeam.Name); // Test Current Team is set
      }
      finally
      {
        File.Delete(filePath);
      }
    }
  }
}