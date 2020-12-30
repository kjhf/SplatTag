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

        Assert.AreEqual<string>("Example Team", loadedTeams[0].Name.Value);
        Assert.AreEqual<string>("ex", loadedTeams[0].ClanTags[0].Value);
        Assert.AreEqual<string>("Another Team", loadedTeams[1].Name.Value);
        Assert.AreEqual<string>("AT", loadedTeams[1].ClanTags[0].Value);
        Assert.AreEqual<string>("Oh No", loadedTeams[2].Name.Value);
        Assert.AreEqual<string>("//", loadedTeams[2].ClanTags[0].Value);

        Assert.AreEqual(TagOption.Back, loadedTeams[0].ClanTagOption);
        Assert.AreEqual(TagOption.Front, loadedTeams[1].ClanTagOption);
        Assert.AreEqual(TagOption.Surrounding, loadedTeams[2].ClanTagOption);

        Assert.AreEqual<string>("Cap 1", loadedPlayers[0].Name.Value); // Assert name was loaded without the tag.
        Assert.AreEqual<string>("P2", loadedPlayers[1].Name.Value); // Assert name was loaded without the tag.
        Assert.AreEqual<string>("TagAgainstName", loadedPlayers[2].Name.Value); // Assert name was loaded without the tag.
        Assert.AreEqual<string>("P4", loadedPlayers[3].Name.Value); // Assert name was loaded without the tag.
        Assert.AreEqual<string>("SubNoTag", loadedPlayers[4].Name.Value); // Assert name was loaded.
        Assert.AreEqual<string>("CAP", loadedPlayers[5].Name.Value); // Assert name was loaded without the tag.
        Assert.AreEqual<string>("A2", loadedPlayers[6].Name.Value); // Assert name was loaded without the tag.
        Assert.AreEqual<string>("A3", loadedPlayers[7].Name.Value); // Assert name was loaded without the tag.
        Assert.AreEqual<string>("A4", loadedPlayers[8].Name.Value); // Assert name was loaded without the tag.
        Assert.AreEqual<string>("Alpha", loadedPlayers[9].Name.Value); // Assert name was loaded without the tag.
        Assert.AreEqual<string>("Bravo", loadedPlayers[10].Name.Value); // Assert name was loaded without the tag.
        Assert.AreEqual<string>("Charlie", loadedPlayers[11].Name.Value); // Assert name was loaded without the tag.
        Assert.AreEqual<string>("Delta", loadedPlayers[12].Name.Value); // Assert name was loaded without the tag.
        Assert.AreEqual<string>("Echo", loadedPlayers[13].Name.Value); // Assert name was loaded without the tag.
        Assert.AreEqual<string>("Foxtrot", loadedPlayers[14].Name.Value); // Assert name was loaded without the tag.
        Assert.AreEqual<string>("Oops", loadedPlayers[15].Name.Value); // Assert name was loaded without the tag.
        Assert.AreEqual<string>("We", loadedPlayers[16].Name.Value); // Assert name was loaded without the tag.
        Assert.AreEqual<string>("Dropped", loadedPlayers[17].Name.Value); // Assert name was loaded without the tag.

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