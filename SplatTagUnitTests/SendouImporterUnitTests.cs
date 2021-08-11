using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagCore;
using SplatTagDatabase.Importers;
using System;
using System.IO;
using System.Linq;

namespace SplatTagUnitTests
{
  /// <summary>
  /// Sendou JSON import unit test
  /// </summary>
  [TestClass]
  public class SendouImporterUnitTests
  {
    /// <summary>
    /// Test the Sendou Importer
    /// </summary>
    [TestMethod]
    public void TestSendouImporter_Old()
    {
      string filePath = Path.GetTempFileName() + ".Sendou.json";
      const string JSON =
        @"{
            ""users"": [
            {
              ""username"": ""!Username!"",
              ""id"": ""5cff40aefeacd363157b3962"",
              ""twitch_name"": ""Twitch_Name"",
              ""twitter_name"": ""Twitter_Name"",
              ""country"": ""us"",
              ""weapons"": [
                ""Kensa Dynamo Roller"",
                ""Gold Dynamo Roller"",
                ""Nautilus 79"",
                ""Custom Explosher"",
                ""Tenta Camo Brella""
              ],
              ""top500"": false,
              ""discord"": {
                ""discordId"": ""85179121671364608"",
                ""username"": ""!A_DISCORD_USERNAME!"",
                ""avatar"": ""3a6d339e5395a07ffee9fd5cd079782d"",
                ""discriminator"": ""2227"",
                ""public_flags"": 0
              }
            },
            {
              ""username"": ""AnotherPlayer"",
              ""id"": ""5f36d677a6558f2e8fadfdd8"",
              ""twitch_name"": null,
              ""twitter_name"": ""AnotherPlayerTwitter"",
              ""country"": ""fr"",
              ""weapons"": [
                ""Splatterscope"",
                ""Splat Charger"",
                ""Splat Brella"",
                ""Sorella Brella""
              ],
              ""top500"": true,
              ""discord"": {
                ""discordId"": ""346383172986208256"",
                ""username"": ""!AnotherPlayerDiscord"",
                ""avatar"": ""482c0dde65d22f31d405efeec6b12b57"",
                ""discriminator"": ""1783"",
                ""public_flags"": 0
              }
            }
          ]
        }";

      try
      {
        File.WriteAllText(filePath, JSON);
        SendouReader reader = new SendouReader(filePath);
        Source s = reader.Load();
        var loadedTeams = s.Teams;
        var loadedPlayers = s.Players;

        Assert.AreEqual(0, loadedTeams.Length);
        Assert.AreEqual(2, loadedPlayers.Length);

        Assert.IsTrue(loadedPlayers.Any(p => p.Name.Value == "!Username!"));
        Assert.IsTrue(loadedPlayers.Any(p => p.Discord.Ids.FirstOrDefault()?.Value == "85179121671364608"));
        Assert.IsTrue(loadedPlayers.Any(p => p.Discord.Usernames.FirstOrDefault()?.Value == "!A_DISCORD_USERNAME!#2227"));
        Assert.IsTrue(loadedPlayers.Any(p => p.Twitter.FirstOrDefault()?.Value == "Twitter_Name"));
        Assert.IsTrue(loadedPlayers.Any(p => p.Twitch.FirstOrDefault()?.Value == "Twitch_Name"));
        Assert.IsTrue(loadedPlayers.Any(p => p.Name.Value == "AnotherPlayer"));
        Assert.IsTrue(loadedPlayers.Any(p => p.Discord.Ids.FirstOrDefault()?.Value == "346383172986208256"));
        Assert.IsTrue(loadedPlayers.Any(p => p.Discord.Usernames.FirstOrDefault()?.Value == "!AnotherPlayerDiscord#1783"));
        Assert.IsTrue(loadedPlayers.Any(p => p.Twitter.FirstOrDefault()?.Value == "AnotherPlayerTwitter"));
        Assert.IsTrue(loadedPlayers.Count(p => p.Top500) == 1);
      }
      finally
      {
        File.Delete(filePath);
      }
    }

    /// <summary>
    /// Test the Sendou Importer
    /// </summary>
    [TestMethod]
    public void TestSendouImporter_New()
    {
      string filePath = Path.GetTempFileName() + ".Sendou.json";
      const string JSON =
        @"[
  {
    ""id"": 8001,
    ""username"": ""Player1"",
    ""discriminator"": ""3122"",
    ""discordAvatar"": ""1c4720e249fd1f425839fc06b23b38bb"",
    ""discordId"": ""342369454719631361"",
    ""profile"": null
  },
  {
    ""id"": 8002,
    ""username"": ""AnotherPlayer"",
    ""discriminator"": ""6667"",
    ""discordAvatar"": ""1895e5ca0ebb705ee92add96808f1f0e"",
    ""discordId"": ""98746624112599040"",
    ""profile"": {
      ""twitterName"": ""ATwitter""
    }
  }
]";

      try
      {
        File.WriteAllText(filePath, JSON);
        SendouReader reader = new SendouReader(filePath);
        Source s = reader.Load();
        var loadedTeams = s.Teams;
        var loadedPlayers = s.Players;

        Assert.AreEqual(0, loadedTeams.Length);
        Assert.AreEqual(2, loadedPlayers.Length);

        Assert.IsTrue(loadedPlayers.Any(p => p.Name.Value == "Player1"));
        Assert.IsTrue(loadedPlayers.Any(p => p.Discord.Ids.FirstOrDefault()?.Value == "342369454719631361"));
        Assert.IsTrue(loadedPlayers.Any(p => p.Discord.Usernames.FirstOrDefault()?.Value == "Player1#3122"));
        Assert.IsTrue(loadedPlayers.Any(p => p.Name.Value == "AnotherPlayer"));
        Assert.IsTrue(loadedPlayers.Any(p => p.Discord.Ids.FirstOrDefault()?.Value == "98746624112599040"));
        Assert.IsTrue(loadedPlayers.Any(p => p.Discord.Usernames.FirstOrDefault()?.Value == "AnotherPlayer#6667"));
        Assert.IsTrue(loadedPlayers.Any(p => p.Twitter.FirstOrDefault()?.Value == "ATwitter"));
      }
      finally
      {
        File.Delete(filePath);
      }
    }
  }
}