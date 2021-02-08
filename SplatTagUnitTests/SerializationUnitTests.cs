using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SplatTagCore;
using SplatTagCore.Social;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

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

    private static T Deserialize<T>(string json, Dictionary<Guid, Source> lookup)
    {
      var settings = new JsonSerializerSettings
      {
        DefaultValueHandling = DefaultValueHandling.Ignore
      };
      settings.Context = new StreamingContext(StreamingContextStates.All, new Source.GuidToSourceConverter(lookup));

      return JsonConvert.DeserializeObject<T>(json, settings);
    }

    [TestMethod]
    public void SerializeBattlefy()
    {
      Dictionary<Guid, Source> sources = new Dictionary<Guid, Source>();
      var h2 = new Source("h2");
      var u2 = new Source("u2");
      var h1 = new Source("h1");
      var u1 = new Source("u1");
      sources.Add(h2.Id, h2);
      sources.Add(u2.Id, u2);
      sources.Add(h1.Id, h1);
      sources.Add(u1.Id, u1);

      Battlefy battlefy = new Battlefy();
      // Remember adding first = back of the list
      battlefy.AddSlug("kjhf", h2);
      battlefy.AddUsername("username2", u2);
      battlefy.AddSlug("handle1", h1);
      battlefy.AddUsername("username1", u1);

      string json = Serialize(battlefy);
      Console.WriteLine(nameof(SerializeBattlefy) + ": ");
      Console.WriteLine(json);
      Battlefy deserialized = Deserialize<Battlefy>(json, sources);

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
      Dictionary<Guid, Source> sources = new Dictionary<Guid, Source>();
      var source2 = new Source("source2");
      var u2 = new Source("u2");
      var source1 = new Source("source1");
      var u1 = new Source("u1");
      sources.Add(source2.Id, source2);
      sources.Add(u2.Id, u2);
      sources.Add(source1.Id, source1);
      sources.Add(u1.Id, u1);

      Discord discord = new Discord();
      // Remember adding first = back of the list
      discord.AddId("123456789", source2);
      discord.AddUsername("username2", u2);
      discord.AddId("4444", source1);
      discord.AddUsername("username1", u1);

      string json = Serialize(discord);
      Console.WriteLine(nameof(SerializeDiscord) + ": ");
      Console.WriteLine(json);
      Discord deserialized = Deserialize<Discord>(json, sources);

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
      Dictionary<Guid, Source> sources = new Dictionary<Guid, Source>();
      var source2 = new Source("source2");
      var u2 = new Source("u2");
      var source1 = new Source("source1");
      var u1 = new Source("u1");
      sources.Add(source2.Id, source2);
      sources.Add(u2.Id, u2);
      sources.Add(source1.Id, source1);
      sources.Add(u1.Id, u1);

      const string handle = "slate";
      Sendou sendou = new Sendou(handle, source1);

      string json = Serialize(sendou);
      Console.WriteLine(nameof(SerializeSendou) + ": ");
      Console.WriteLine(json);
      Sendou deserialized = Deserialize<Sendou>(json, sources);

      Assert.AreEqual("https://sendou.ink/u/slate", deserialized.Uri?.AbsoluteUri, "Unexpected Uri");
    }

    [TestMethod]
    public void SerializePlayer()
    {
      Dictionary<Guid, Source> sources = new Dictionary<Guid, Source>();
      var source2 = new Source("source2");
      var u2 = new Source("u2");
      var source1 = new Source("source1");
      var u1 = new Source("u1");
      var h2 = new Source("h2");
      var h1 = new Source("h1");
      var s1 = new Source("s1");
      sources.Add(h2.Id, h2);
      sources.Add(h1.Id, h1);
      sources.Add(source2.Id, source2);
      sources.Add(u2.Id, u2);
      sources.Add(source1.Id, source1);
      sources.Add(u1.Id, u1);
      sources.Add(s1.Id, s1);

      Player player = new Player();
      // Remember adding first = back of the list
      player.AddBattlefySlug("kjhf", h2);
      player.AddBattlefyUsername("username2", u2);
      player.AddBattlefySlug("handle1", h1);
      player.AddBattlefyUsername("username1", u1);
      player.AddBattlefyPersistentId("0000-1111-2222-3333", h1);

      player.AddDiscordId("123456789", source2);
      player.AddDiscordUsername("username2", u2);
      player.AddDiscordId("4444", source1);
      player.AddDiscordUsername("username1", u1);

      player.AddSendou("slate", s1);

      string json = Serialize(player);
      Console.WriteLine(nameof(SerializePlayer) + ": ");
      Console.WriteLine(json);
      Player deserialized = Deserialize<Player>(json, sources);

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
      Assert.AreEqual("0000-1111-2222-3333", battlefy.PersistentIds[0].Value, "PersistentIds [0] unexpected id");

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
      Dictionary<Guid, Source> sources = new Dictionary<Guid, Source>();
      var source3 = new Source("source3");
      var source2 = new Source("source2");
      var u2 = new Source("u2");
      var source1 = new Source("source1");
      var u1 = new Source("u1");
      var t2 = new Source("t2");
      var t1 = new Source("t1");
      sources.Add(t2.Id, t2);
      sources.Add(t1.Id, t1);
      sources.Add(source3.Id, source3);
      sources.Add(source2.Id, source2);
      sources.Add(u2.Id, u2);
      sources.Add(source1.Id, source1);
      sources.Add(u1.Id, u1);

      Team team = new Team();
      // Remember adding first = back of the list
      team.AddBattlefyId("2teamid2", u2);
      team.AddBattlefyId("1teamid1", u1);

      team.AddClanTag("old", source2);
      team.AddClanTag("new", source3);

      team.AddDivision(new Division(10, DivType.LUTI, "S9"));
      team.AddDivision(new Division(8, DivType.LUTI, "SX"));

      team.AddName("team2", t2);
      team.AddName("team1", t1);

      string json = Serialize(team);
      Console.WriteLine(nameof(SerializeTeam) + ": ");
      Console.WriteLine(json);
      Team deserialized = Deserialize<Team>(json, sources);

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

    [TestMethod]
    public void SerializeBrackets()
    {
      Dictionary<Guid, Source> sources = new Dictionary<Guid, Source>();
      var source1 = new Source("source1");

      Player player1 = new Player();
      player1.AddSendou("slate", source1);
      Player player2 = new Player();
      player2.AddSendou("wug", source1);
      Team team1 = new Team("Team One", source1);
      player1.AddTeams(new[] { team1.Id });
      Team team2 = new Team("Team Two", source1);
      player2.AddTeams(new[] { team2.Id });
      source1.Players = new[] { player1, player2 };
      source1.Teams = new[] { team1, team2 };
      sources.Add(source1.Id, source1);

      Score s1 = new Score(new[] { 1, 3 });
      Game g1 = new Game(s1, new[] { player1.Id, player2.Id }, new[] { team1.Id, team2.Id });
      Dictionary<int, Guid[]> placementByPlayers = new Dictionary<int, Guid[]>
      {
        [1] = new[] { player2.Id },
        [2] = new[] { player1.Id }
      };
      Dictionary<int, Guid[]> placementByTeams = new Dictionary<int, Guid[]>
      {
        [1] = new[] { team2.Id },
        [2] = new[] { team1.Id }
      };

      Placement placement = new Placement(placementByPlayers, placementByTeams);
      Bracket b1 = new Bracket("bracket_name", new[] { g1 }, new[] { player1.Id, player2.Id }, new[] { team1.Id, team2.Id }, placement);
      source1.Brackets = new[] { b1 };

      string json = Serialize(source1);
      Console.WriteLine(nameof(SerializeBrackets) + ": ");
      Console.WriteLine(json);
      Source deserialized = Deserialize<Source>(json, sources);

      Assert.AreEqual("1-3", deserialized.Brackets[0].Matches[0].Score.Description);
      Assert.AreEqual(player2.Id, deserialized.Brackets[0].Placements.PlayersByPlacement[1][0]);
      Assert.AreEqual(player1.Id, deserialized.Brackets[0].Placements.PlayersByPlacement[2][0]);
      Assert.AreEqual(team2.Id, deserialized.Brackets[0].Placements.TeamsByPlacement[1][0]);
      Assert.AreEqual(team1.Id, deserialized.Brackets[0].Placements.TeamsByPlacement[2][0]);
    }

    [TestMethod]
    public void DeserializeFriendCodes()
    {
      string json = @"[{""FC"":[6653,9220,3527]},{""FC"":[6653,9220,3527]},{""FC"":[6653,9220,3527]},{""FC"":[6653,9220,3527]}]";
      List<FriendCode> fcs = Deserialize<List<FriendCode>>(json, new Dictionary<Guid, Source>());

      Player player = new Player();
      player.AddFCs(fcs);
      Assert.AreEqual(1, player.FriendCodes.Count, "Expected only 1 FC as the values are equal.");
    }
  }
}