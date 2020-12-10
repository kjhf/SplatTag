using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SplatTagCore;
using SplatTagCore.Social;
using System;
using System.IO;
using System.Linq;

namespace SplatTagUnitTests
{
  /// <summary>
  /// Serialization unit tests
  /// </summary>
  [TestClass]
  public class SerializationUnitTests
  {
    private static string Serialize(object obj)
    {
      if (JsonConvert.DefaultSettings == null)
      {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings();
      }
      var settings = JsonConvert.DefaultSettings();
      settings.DefaultValueHandling = DefaultValueHandling.Ignore;
      var serializer = JsonSerializer.Create(settings);
      StringWriter sw = new StringWriter();
      serializer.Serialize(sw, obj);
      return sw.ToString();
    }

    private static T Deserialize<T>(string json)
    {
      return JsonConvert.DeserializeObject<T>(json);
    }

    [TestMethod]
    public void SerializeBattlefy()
    {
      Battlefy battlefy = new Battlefy();
      // Remember adding first = back of the list
      battlefy.AddSlug("kjhf", new Source("h2"));
      battlefy.AddUsername("username2", new Source("u2"));
      battlefy.AddSlug("handle1", new Source("h1"));
      battlefy.AddUsername("username1", new Source("u1"));

      string json = Serialize(battlefy);
      Console.WriteLine(nameof(SerializeBattlefy) + ": ");
      Console.WriteLine(json);
      Battlefy deserialized = Deserialize<Battlefy>(json);

      Assert.AreEqual(2, deserialized.Slugs.Count, "Unexpected number of slugs");
      Assert.AreEqual(2, deserialized.Usernames.Count, "Unexpected number of usernames");
      Assert.AreEqual("handle1", deserialized.Slugs[0].Value, "Slug [0] unexpected handle");
      Assert.AreEqual("h1", deserialized.Slugs[0].Sources.First().Name, "Slug [0] unexpected source");
      Assert.AreEqual("kjhf", deserialized.Slugs[1].Value, "Slug [1] unexpected handle");
      Assert.AreEqual("h2", deserialized.Slugs[1].Sources.First().Name, "Slug [1] unexpected source");
      Assert.AreEqual("https://battlefy.com/users/kjhf", deserialized.Slugs[1].Uri?.AbsoluteUri, "Slug [1] unexpected uri");
      Assert.AreEqual("username1", deserialized.Usernames[0].Value, "Usernames [0] unexpected handle");
      Assert.AreEqual("u1", deserialized.Usernames[0].Sources.First().Name, "Usernames [0] unexpected source");
      Assert.AreEqual("username2", deserialized.Usernames[1].Value, "Usernames [1] unexpected handle");
      Assert.AreEqual("u2", deserialized.Usernames[1].Sources.First().Name, "Usernames [1] unexpected source");
    }

    [TestMethod]
    public void SerializeDiscord()
    {
      Discord discord = new Discord();
      // Remember adding first = back of the list
      discord.AddId("123456789", new Source("source2"));
      discord.AddUsername("username2", new Source("u2"));
      discord.AddId("4444", new Source("source1"));
      discord.AddUsername("username1", new Source("u1"));

      string json = Serialize(discord);
      Console.WriteLine(nameof(SerializeDiscord) + ": ");
      Console.WriteLine(json);
      Discord deserialized = Deserialize<Discord>(json);

      Assert.AreEqual(2, deserialized.Ids.Count, "Unexpected number of ids");
      Assert.AreEqual(2, deserialized.Usernames.Count, "Unexpected number of usernames");
      Assert.AreEqual("4444", deserialized.Ids[0].Value, "Id [0] unexpected handle");
      Assert.AreEqual("source1", deserialized.Ids[0].Sources.First().Name, "Id [0] unexpected source");
      Assert.AreEqual("123456789", deserialized.Ids[1].Value, "Id [1] unexpected handle");
      Assert.AreEqual("source2", deserialized.Ids[1].Sources.First().Name, "Id [1] unexpected source");
      Assert.AreEqual("username1", deserialized.Usernames[0].Value, "Usernames [0] unexpected handle");
      Assert.AreEqual("u1", deserialized.Usernames[0].Sources.First().Name, "Usernames [0] unexpected source");
      Assert.AreEqual("username2", deserialized.Usernames[1].Value, "Usernames [1] unexpected handle");
      Assert.AreEqual("u2", deserialized.Usernames[1].Sources.First().Name, "Usernames [1] unexpected source");
    }

    [TestMethod]
    public void SerializeSendou()
    {
      const string handle = "slate";
      var source = new Source("s1");
      Sendou sendou = new Sendou(handle, source);

      string json = Serialize(sendou);
      Console.WriteLine(nameof(SerializeSendou) + ": ");
      Console.WriteLine(json);
      Sendou deserialized = Deserialize<Sendou>(json);

      Assert.AreEqual("https://sendou.ink/u/slate", deserialized.Uri?.AbsoluteUri, "Unexpected Uri");
    }

    [TestMethod]
    public void SerializePlayer()
    {
      Player player = new Player();
      // Remember adding first = back of the list
      player.AddBattlefySlug("kjhf", new Source("h2"));
      player.AddBattlefyUsername("username2", new Source("u2"));
      player.AddBattlefySlug("handle1", new Source("h1"));
      player.AddBattlefyUsername("username1", new Source("u1"));

      player.AddDiscordId("123456789", new Source("source2"));
      player.AddDiscordUsername("username2", new Source("u2"));
      player.AddDiscordId("4444", new Source("source1"));
      player.AddDiscordUsername("username1", new Source("u1"));

      player.AddSendou("slate", new Source("s1"));

      string json = Serialize(player);
      Console.WriteLine(nameof(SerializePlayer) + ": ");
      Console.WriteLine(json);
      Player deserialized = Deserialize<Player>(json);

      var battlefy = deserialized.Battlefy;
      Assert.AreEqual(2, battlefy.Slugs.Count, "Unexpected number of slugs");
      Assert.AreEqual(2, battlefy.Usernames.Count, "Unexpected number of usernames");
      Assert.AreEqual("handle1", battlefy.Slugs[0].Value, "Slug [0] unexpected handle");
      Assert.AreEqual("h1", battlefy.Slugs[0].Sources.First().Name, "Slug [0] unexpected source");
      Assert.AreEqual("kjhf", battlefy.Slugs[1].Value, "Slug [1] unexpected handle");
      Assert.AreEqual("h2", battlefy.Slugs[1].Sources.First().Name, "Slug [1] unexpected source");
      Assert.AreEqual("https://battlefy.com/users/kjhf", battlefy.Slugs[1].Uri?.AbsoluteUri, "Slug [1] unexpected uri");
      Assert.AreEqual("username1", battlefy.Usernames[0].Value, "Usernames [0] unexpected handle");
      Assert.AreEqual("u1", battlefy.Usernames[0].Sources.First().Name, "Usernames [0] unexpected source");
      Assert.AreEqual("username2", battlefy.Usernames[1].Value, "Usernames [1] unexpected handle");
      Assert.AreEqual("u2", battlefy.Usernames[1].Sources.First().Name, "Usernames [1] unexpected source");

      var discord = deserialized.Discord;
      Assert.AreEqual(2, discord.Ids.Count, "Unexpected number of ids");
      Assert.AreEqual(2, discord.Usernames.Count, "Unexpected number of usernames");
      Assert.AreEqual("4444", discord.Ids[0].Value, "Id [0] unexpected handle");
      Assert.AreEqual("source1", discord.Ids[0].Sources.First().Name, "Id [0] unexpected source");
      Assert.AreEqual("123456789", discord.Ids[1].Value, "Id [1] unexpected handle");
      Assert.AreEqual("source2", discord.Ids[1].Sources.First().Name, "Id [1] unexpected source");
      Assert.AreEqual("username1", discord.Usernames[0].Value, "Usernames [0] unexpected handle");
      Assert.AreEqual("u1", discord.Usernames[0].Sources.First().Name, "Usernames [0] unexpected source");
      Assert.AreEqual("username2", discord.Usernames[1].Value, "Usernames [1] unexpected handle");
      Assert.AreEqual("u2", discord.Usernames[1].Sources.First().Name, "Usernames [1] unexpected source");

      var sendou = deserialized.SendouProfiles[0];
      Assert.AreEqual("https://sendou.ink/u/slate", sendou.Uri?.AbsoluteUri, "Unexpected Uri");
    }

    [TestMethod]
    public void SerializeTeam()
    {
      Team team = new Team();
      // Remember adding first = back of the list
      team.AddBattlefyId("2teamid2", new Source("u2"));
      team.AddBattlefyId("1teamid1", new Source("u1"));

      team.AddClanTag("old", new Source("source2"));
      team.AddClanTag("new", new Source("source3"));

      team.AddDivision(new Division(10, DivType.LUTI, "S9"));
      team.AddDivision(new Division(8, DivType.LUTI, "SX"));

      team.AddName("team2", new Source("t2"));
      team.AddName("team1", new Source("t1"));

      string json = Serialize(team);
      Console.WriteLine(nameof(SerializeTeam) + ": ");
      Console.WriteLine(json);
      Team deserialized = Deserialize<Team>(json);

      var battlefy = deserialized.BattlefyPersistentTeamIds;
      Assert.AreEqual(2, battlefy.Count, "Unexpected number of team battlefy slugs");
      Assert.AreEqual("1teamid1", battlefy[0].Value, "Slug [0] unexpected handle");
      Assert.AreEqual("2teamid2", battlefy[1].Value, "Slug [1] unexpected handle");
      Assert.AreEqual("u1", battlefy[0].Sources.First().Name, "Slug [0] unexpected source");
      Assert.AreEqual("u2", battlefy[1].Sources.First().Name, "Slug [1] unexpected source");

      var clanTags = deserialized.ClanTags;
      Assert.AreEqual(2, clanTags.Count, "Unexpected number of clanTags");
      Assert.AreEqual("new", clanTags[0].Value, "clanTags[0] Unexpected Value");
      Assert.AreEqual("old", clanTags[1].Value, "clanTags[1] Unexpected Value");
      Assert.AreEqual("source3", clanTags[0].Sources.First().Name, "clanTags[0] unexpected source");
      Assert.AreEqual("source2", clanTags[1].Sources.First().Name, "clanTags[1] unexpected source");

      var divisions = deserialized.Divisions;
      Assert.AreEqual(2, divisions.Count, "Unexpected number of divisions");
      Assert.AreEqual(DivType.LUTI, divisions[0].DivType, "Unexpected DivType");
      Assert.AreEqual(DivType.LUTI, divisions[1].DivType, "Unexpected DivType");
      Assert.AreEqual("SX", divisions[0].Season, "Unexpected Season");
      Assert.AreEqual("S9", divisions[1].Season, "Unexpected Season");
      Assert.AreEqual(8, divisions[0].Value, "Unexpected Value");
      Assert.AreEqual(10, divisions[1].Value, "Unexpected Value");

      var names = deserialized.Names;
      Assert.AreEqual(2, names.Count, "Unexpected number of team names");
      Assert.AreEqual("team1", names[0].Value, "Names [0] unexpected handle");
      Assert.AreEqual("team2", names[1].Value, "Names [1] unexpected handle");
      Assert.AreEqual("t1", names[0].Sources.First().Name, "Names [0] unexpected source");
      Assert.AreEqual("t2", names[1].Sources.First().Name, "Names [1] unexpected source");
    }
  }
}