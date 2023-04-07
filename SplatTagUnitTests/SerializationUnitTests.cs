using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagCore;
using SplatTagCore.Social;
using SplatTagDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using static SplatTagCore.JSONConverters;

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
      return JsonSerializer.Serialize(obj, SplatTagJsonSnapshotDatabase.jsonSerializerOptions);
    }

    private static T Deserialize<T>(string json, Dictionary<string, Source> lookup)
    {
      _ = new GuidToSourceConverter(lookup); // Set instance
      return JsonSerializer.Deserialize<T>(json, SplatTagJsonSnapshotDatabase.jsonSerializerOptions) ?? throw new InvalidOperationException($"JsonConvert failed to Deserialize Object of type {typeof(T).Name} (json.Length={json.Length})");
    }

    [TestMethod]
    public void SerializeBattlefy()
    {
      Dictionary<string, Source> sources = new Dictionary<string, Source>();
      var h1 = new Source("h1", DateTime.Now.AddDays(1));
      var u1 = new Source("u1", DateTime.Now.AddDays(1));
      var h2 = new Source("h2", DateTime.Now);
      var u2 = new Source("u2", DateTime.Now);
      sources.Add(h1.Id, h1);
      sources.Add(u1.Id, u1);
      sources.Add(h2.Id, h2);
      sources.Add(u2.Id, u2);

      Battlefy battlefy = new Battlefy();
      battlefy.AddSlug("handle1", h1);
      battlefy.AddUsername("username1", u1);
      battlefy.AddSlug("kjhf", h2);
      battlefy.AddUsername("username2", u2);

      string json = Serialize(battlefy);
      Console.WriteLine(nameof(SerializeBattlefy) + ": ");
      Console.WriteLine(json);
      Battlefy deserialized = Deserialize<Battlefy>(json, sources);

      var orderedSlugs = deserialized.SlugsHandler.GetItemsOrdered();
      var orderedUsernames = deserialized.UsernamesHandler.GetItemsOrdered();
      Assert.AreEqual(2, orderedSlugs.Count, "Unexpected number of slugs");
      Assert.AreEqual(2, orderedUsernames.Count, "Unexpected number of usernames");
      Assert.AreEqual("handle1", orderedSlugs[0].Value, "Slug [0] unexpected handle");
      Assert.AreEqual("h1", orderedSlugs[0].Sources[0].Name, "Slug [0] unexpected source");
      Assert.AreEqual("kjhf", orderedSlugs[1].Value, "Slug [1] unexpected handle");
      Assert.AreEqual("h2", orderedSlugs[1].Sources[0].Name, "Slug [1] unexpected source");
      Assert.AreEqual("https://battlefy.com/users/kjhf", orderedSlugs[1].Uri?.AbsoluteUri, "Slug [1] unexpected uri");
      Assert.AreEqual("username1", orderedUsernames[0].Value, "Usernames [0] unexpected handle");
      Assert.AreEqual("u1", orderedUsernames[0].Sources[0].Name, "Usernames [0] unexpected source");
      Assert.AreEqual("username2", orderedUsernames[1].Value, "Usernames [1] unexpected handle");
      Assert.AreEqual("u2", orderedUsernames[1].Sources[0].Name, "Usernames [1] unexpected source");
    }

    [TestMethod]
    public void SerializeDiscord()
    {
      var sources = new Dictionary<string, Source>();
      var source2 = new Source("source2", DateTime.Now);
      var u2 = new Source("u2", DateTime.Now);
      var source1 = new Source("source1", DateTime.Now.AddDays(1));
      var u1 = new Source("u1", DateTime.Now.AddDays(1));
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
      var orderedIds = deserialized.IdsHandler.GetItemsOrdered();
      var orderedUsernames = deserialized.UsernamesHandler.GetItemsOrdered();

      Assert.AreEqual(2, orderedIds.Count, "Unexpected number of ids");
      Assert.AreEqual(2, orderedUsernames.Count, "Unexpected number of usernames");
      Assert.AreEqual("4444", orderedIds[0].Value, "Id [0] unexpected handle");
      Assert.AreEqual("source1", orderedIds[0].Sources[0].Name, "Id [0] unexpected source");
      Assert.AreEqual("123456789", orderedIds[1].Value, "Id [1] unexpected handle");
      Assert.AreEqual("source2", orderedIds[1].Sources[0].Name, "Id [1] unexpected source");
      Assert.AreEqual("username1", orderedUsernames[0].Value, "Usernames [0] unexpected handle");
      Assert.AreEqual("u1", orderedUsernames[0].Sources[0].Name, "Usernames [0] unexpected source");
      Assert.AreEqual("username2", orderedUsernames[1].Value, "Usernames [1] unexpected handle");
      Assert.AreEqual("u2", orderedUsernames[1].Sources[0].Name, "Usernames [1] unexpected source");
    }

    [TestMethod]
    public void SerializeSendou()
    {
      var sources = new Dictionary<string, Source>();
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
      var sources = new Dictionary<string, Source>();
      var source1 = new Source("source1", DateTime.Now.AddDays(1));
      var source2 = new Source("source2");
      var u1 = new Source("u1", DateTime.Now.AddDays(1));
      var u2 = new Source("u2");
      var h1 = new Source("h1", DateTime.Now.AddDays(1));
      var h2 = new Source("h2");
      var s1 = new Source("s1", DateTime.Now.AddDays(1));
      sources.Add(h1.Id, h1);
      sources.Add(h2.Id, h2);
      sources.Add(source1.Id, source1);
      sources.Add(source2.Id, source2);
      sources.Add(u1.Id, u1);
      sources.Add(u2.Id, u2);
      sources.Add(s1.Id, s1);

      Player player = new Player();
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

      var orderedSlugs = deserialized.Battlefy.SlugsHandler.GetItemsOrdered();
      var orderedUsernames = deserialized.Battlefy.UsernamesHandler.GetItemsOrdered();
      var orderedPesistentIds = deserialized.Battlefy.PersistentIdsHandler.GetItemsOrdered();
      Assert.AreEqual(2, orderedSlugs.Count, "Unexpected number of slugs");
      Assert.AreEqual(2, orderedUsernames.Count, "Unexpected number of usernames");
      Assert.AreEqual("handle1", orderedSlugs[0].Value, "Slug [0] unexpected handle");
      Assert.AreEqual("h1", orderedSlugs[0].Sources[0].Name, "Slug [0] unexpected source");
      Assert.AreEqual("kjhf", orderedSlugs[1].Value, "Slug [1] unexpected handle");
      Assert.AreEqual("h2", orderedSlugs[1].Sources[0].Name, "Slug [1] unexpected source");
      Assert.AreEqual("https://battlefy.com/users/kjhf", orderedSlugs[1].Uri?.AbsoluteUri, "Slug [1] unexpected uri");
      Assert.AreEqual("username1", orderedUsernames[0].Value, "Usernames [0] unexpected handle");
      Assert.AreEqual("u1", orderedUsernames[0].Sources[0].Name, "Usernames [0] unexpected source");
      Assert.AreEqual("username2", orderedUsernames[1].Value, "Usernames [1] unexpected handle");
      Assert.AreEqual("u2", orderedUsernames[1].Sources[0].Name, "Usernames [1] unexpected source");
      Assert.AreEqual("0000-1111-2222-3333", orderedPesistentIds[0].Value, "PersistentIds [0] unexpected id");

      var discord = deserialized.Discord;
      var orderedDiscordIds = discord.IdsHandler.GetItemsOrdered();
      var orderedDiscordUsernames = discord.UsernamesHandler.GetItemsOrdered();
      Assert.AreEqual(2, orderedDiscordIds.Count, "Unexpected number of discord ids");
      Assert.AreEqual(2, orderedDiscordUsernames.Count, "Unexpected number of discord usernames");
      Assert.AreEqual("4444", orderedDiscordIds[0].Value, "Id [0] unexpected handle");
      Assert.AreEqual("source1", orderedDiscordIds[0].Sources[0].Name, "Id [0] unexpected source");
      Assert.AreEqual("123456789", orderedDiscordIds[1].Value, "Id [1] unexpected handle");
      Assert.AreEqual("source2", orderedDiscordIds[1].Sources[0].Name, "Id [1] unexpected source");
      Assert.AreEqual("username1", orderedDiscordUsernames[0].Value, "Usernames [0] unexpected handle");
      Assert.AreEqual("u1", orderedDiscordUsernames[0].Sources[0].Name, "Usernames [0] unexpected source");
      Assert.AreEqual("username2", orderedDiscordUsernames[1].Value, "Usernames [1] unexpected handle");
      Assert.AreEqual("u2", orderedDiscordUsernames[1].Sources[0].Name, "Usernames [1] unexpected source");

      var sendou = deserialized.SendouProfiles.FirstOrDefault();
      Assert.AreEqual("https://sendou.ink/u/slate", sendou?.Uri?.AbsoluteUri, "Unexpected Uri");
    }

    [TestMethod]
    public void SerializeTeam()
    {
      var sources = new Dictionary<string, Source>();
      var oldestSource = new Source("old_source", DateTime.Now.AddDays(-6));
      var newestSource = new Source("new_source", DateTime.Now);
      sources.Add(newestSource.Id, newestSource);
      sources.Add(oldestSource.Id, oldestSource);

      Team team = new Team();
      team.AddBattlefyId("2teamid2", oldestSource);
      team.AddBattlefyId("1teamid1", newestSource);

      team.AddClanTag("old", oldestSource);
      team.AddClanTag("new", newestSource);

      team.AddDivision(new Division(10, DivType.DSB, "S9"), oldestSource);
      team.AddDivision(new Division(8, DivType.LUTI, "SX"), newestSource);

      team.AddName("team2", oldestSource);
      team.AddName("team1", newestSource);

      string json = Serialize(team);
      Console.WriteLine(nameof(SerializeTeam) + ": ");
      Console.WriteLine(json);
      Team deserialized = Deserialize<Team>(json, sources);

      var battlefy = deserialized.BattlefyPersistentTeamIdInformation.GetItemsOrdered();
      Assert.AreEqual(2, battlefy.Count, "Unexpected number of team battlefy slugs");
      Assert.AreEqual("1teamid1", battlefy[0].Value, "Slug [0] unexpected handle");
      Assert.AreEqual("2teamid2", battlefy[1].Value, "Slug [1] unexpected handle");
      Assert.AreEqual("new_source", battlefy[0].Sources[0].Name, "Slug [0] unexpected source");
      Assert.AreEqual("old_source", battlefy[1].Sources[0].Name, "Slug [1] unexpected source");

      var clanTags = deserialized.ClanTagInformation.GetItemsOrdered();
      Assert.AreEqual(2, clanTags.Count, "Unexpected number of clanTags");
      Assert.AreEqual("new", clanTags[0].Value, "clanTags[0] Unexpected Value");
      Assert.AreEqual("old", clanTags[1].Value, "clanTags[1] Unexpected Value");
      Assert.AreEqual("new_source", clanTags[0].Sources[0].Name, "clanTags[0] unexpected source");
      Assert.AreEqual("old_source", clanTags[1].Sources[0].Name, "clanTags[1] unexpected source");

      var divisions = deserialized.DivisionInformation.GetDivisionsOrdered();
      Assert.AreEqual(2, divisions.Count, "Unexpected number of divisions");
      // First - most recent
      Assert.AreEqual(8, divisions[0].Value, "Unexpected Value");
      Assert.AreEqual(DivType.LUTI, divisions[0].DivType, "Unexpected DivType");
      Assert.AreEqual("SX", divisions[0].Season, "Unexpected Season");
      // Next
      Assert.AreEqual(10, divisions[1].Value, "Unexpected Value");
      Assert.AreEqual(DivType.DSB, divisions[1].DivType, "Unexpected DivType");
      Assert.AreEqual("S9", divisions[1].Season, "Unexpected Season");

      var names = deserialized.NamesInformation.GetItemsOrdered();
      Assert.AreEqual(2, names.Count, "Unexpected number of team names");
      Assert.AreEqual("team1", names[0].Value, "Names [0] unexpected handle");
      Assert.AreEqual("team2", names[1].Value, "Names [1] unexpected handle");
      Assert.AreEqual("new_source", names[0].Sources[0].Name, "Names [0] unexpected source");
      Assert.AreEqual("old_source", names[1].Sources[0].Name, "Names [1] unexpected source");
    }

    [TestMethod]
    public void SerializeBrackets()
    {
      var sources = new Dictionary<string, Source>();
      var source1 = new Source("source1");

      Player player1 = new Player();
      player1.AddSendou("slate", source1);
      Player player2 = new Player();
      player2.AddSendou("wug", source1);
      Team team1 = new Team("Team One", source1);
      player1.AddTeams(team1.Id, source1);
      Team team2 = new Team("Team Two", source1);
      player2.AddTeams(team2.Id, source1);
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
      const string json = "[[6653,9220,3527],[6653,9220,3527],[6653,9220,3527],[6653,9220,3527]]";
      FriendCode[] fcs = Deserialize<FriendCode[]>(json, new Dictionary<string, Source>());
      Assert.IsNotNull(fcs);
      Assert.AreEqual(4, fcs.Length, "Expected 4 friend codes parsed.");

      Player player = new Player();
      player.AddFCs(fcs, Builtins.ManualSource);
      Assert.AreEqual(1, player.FCInformation.Count, "Expected only 1 FC as the values are equal.");
    }
  }
}