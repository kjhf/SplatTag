using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagCore;
using SplatTagDatabase.Importers;
using System;
using System.IO;
using System.Linq;

namespace SplatTagUnitTests
{
  /// <summary>
  /// LUTI JSON import unit test
  /// </summary>
  [TestClass]
  public class LUTIImporterUnitTests
  {
    /// <summary>
    /// Test the LUTI Importer
    /// </summary>
    [TestMethod]
    public void TestLUTIImporter()
    {
      string filePath = Path.GetTempFileName() + ".json";
      const string JSON =
        @"[
  {
    ""Team Name"": ""Example Team"",
    ""Division"": ""X"",
    ""Tag"": ""ex"",
    ""Team Captain"": ""Cap 1 ex"",
    ""Player 2"": ""P2 ex"",
    ""Player 3"": ""TagAgainstNameex "",
    ""Player 4"": "" P4 ex"",
    ""Player 5"": "" SubNoTag "",
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
    ""Player 3"": ""AT A3 "",
    ""Player 4"": "" AT A4"",
    ""Player 5"": "" ATAlpha "",
    ""Player 6"": ""AT  Bravo"",
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
    ""Player 2"": ""/ We/"",
    ""Player 3"": ""/ Dropped /"",
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
        Source s = reader.Load();
        var loadedTeams = s.Teams;
        var loadedPlayers = s.Players;

        Assert.AreEqual(3, loadedTeams.Length);
        Assert.AreEqual(18, loadedPlayers.Length);

        int indexOfExampleTeam = Array.IndexOf(loadedTeams, loadedTeams.First(t => t.Name.Value == "Example Team"));
        Assert.AreEqual("ex", loadedTeams[indexOfExampleTeam].ClanTagInformation.MostRecent?.Value);

        int indexOfAnotherTeam = Array.IndexOf(loadedTeams, loadedTeams.First(t => t.Name.Value == "Another Team"));
        Assert.AreEqual("AT", loadedTeams[indexOfAnotherTeam].ClanTagInformation.MostRecent?.Value);

        int indexOfOhNoTeam = Array.IndexOf(loadedTeams, loadedTeams.First(t => t.Name.Value == "Oh No"));
        Assert.AreEqual("/", loadedTeams[indexOfOhNoTeam].ClanTagInformation.MostRecent?.Value);

        Assert.AreEqual(TagOption.Back, loadedTeams[indexOfExampleTeam].Tag?.LayoutOption);
        Assert.AreEqual(TagOption.Front, loadedTeams[indexOfAnotherTeam].Tag?.LayoutOption);
        Assert.AreEqual(TagOption.Surrounding, loadedTeams[indexOfOhNoTeam].Tag?.LayoutOption);

        Assert.IsTrue(loadedPlayers.Any(p => p.Name.Value == "Cap 1")); // Assert name was loaded without the tag.
        Assert.IsTrue(loadedPlayers.Any(p => p.Name.Value == "P2")); // Assert name was loaded without the tag.
        Assert.IsTrue(loadedPlayers.Any(p => p.Name.Value == "TagAgainstName")); // Assert name was loaded without the tag.
        Assert.IsTrue(loadedPlayers.Any(p => p.Name.Value == "P4")); // Assert name was loaded without the tag.
        Assert.IsTrue(loadedPlayers.Any(p => p.Name.Value == "SubNoTag")); // Assert name was loaded.
        Assert.IsTrue(loadedPlayers.Any(p => p.Name.Value == "CAP")); // Assert name was loaded without the tag.
        Assert.IsTrue(loadedPlayers.Any(p => p.Name.Value == "A2")); // Assert name was loaded without the tag.
        Assert.IsTrue(loadedPlayers.Any(p => p.Name.Value == "A3")); // Assert name was loaded without the tag.
        Assert.IsTrue(loadedPlayers.Any(p => p.Name.Value == "A4")); // Assert name was loaded without the tag.
        Assert.IsTrue(loadedPlayers.Any(p => p.Name.Value == "Alpha")); // Assert name was loaded without the tag.
        Assert.IsTrue(loadedPlayers.Any(p => p.Name.Value == "Bravo")); // Assert name was loaded without the tag.
        Assert.IsTrue(loadedPlayers.Any(p => p.Name.Value == "Charlie")); // Assert name was loaded without the tag.
        Assert.IsTrue(loadedPlayers.Any(p => p.Name.Value == "Delta")); // Assert name was loaded without the tag.
        Assert.IsTrue(loadedPlayers.Any(p => p.Name.Value == "Echo")); // Assert name was loaded without the tag.
        Assert.IsTrue(loadedPlayers.Any(p => p.Name.Value == "Foxtrot")); // Assert name was loaded without the tag.
        Assert.IsTrue(loadedPlayers.Any(p => p.Name.Value == "Oops")); // Assert name was loaded without the tag.
        Assert.IsTrue(loadedPlayers.Any(p => p.Name.Value == "We")); // Assert name was loaded without the tag.
        Assert.IsTrue(loadedPlayers.Any(p => p.Name.Value == "Dropped")); // Assert name was loaded without the tag.

        for (int i = 0; i < loadedPlayers.Length; i++)
        {
          Assert.IsTrue(loadedPlayers[i].CurrentTeam != Team.NoTeam.Id, $"Current team not set for {loadedPlayers[i].Name} ({i})"); // Test Current Team is set
        }
      }
      finally
      {
        File.Delete(filePath);
      }
    }
  }
}