using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NLog;
using SplatTagCore;
using SplatTagCore.Social;
using SplatTagDatabase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace SplatTagUnitTests
{
  /// <summary>
  /// Serialization unit tests
  /// </summary>
  [TestClass]
  public class SerializationUnitTests
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    [TestInitialize]
    public void TestInitialize()
    {
      LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(PathUtils.FindFileUpToRoot("nlog.config"));
      logger.Trace("TestInitialize");
    }

    private static string Serialize(object obj)
    {
      JsonConvert.DefaultSettings ??= SplatTagJsonSnapshotDatabase.JsonConvertDefaultSettings;
      var serializer = JsonSerializer.Create(JsonConvert.DefaultSettings());
      StringWriter sw = new();
      serializer.Serialize(sw, obj);
      return sw.ToString();
    }

    private static JsonSerializerSettings DeserializeGetSettings(Dictionary<string, Source>? lookup)
    {
      if (JsonConvert.DefaultSettings == null)
      {
        JsonConvert.DefaultSettings = SplatTagJsonSnapshotDatabase.JsonConvertDefaultSettings;
      }
      var settings = JsonConvert.DefaultSettings();
      if (lookup != null)
      {
        settings.Context = new StreamingContext(StreamingContextStates.All, new Source.SourceStringConverter(lookup));
      }
      return settings;
    }

    private static object Deserialize(string json, Dictionary<string, Source>? lookup)
    {
      var settings = DeserializeGetSettings(lookup);
      return JsonConvert.DeserializeObject(json, settings) ?? throw new InvalidOperationException($"JsonConvert failed to Deserialize Object (json.Length={json.Length})");
    }

    private static T Deserialize<T>(string json, Dictionary<string, Source>? lookup)
    {
      var settings = DeserializeGetSettings(lookup);
      return JsonConvert.DeserializeObject<T>(json, settings) ?? throw new InvalidOperationException($"JsonConvert failed to Deserialize Object of type {typeof(T).Name} (json.Length={json.Length})");
    }

    [TestMethod]
    public void SerializeBattlefy()
    {
      var sources = new Dictionary<string, Source>();
      var h1 = new Source("h1", DateTime.Now.AddDays(1));
      var u1 = new Source("u1", DateTime.Now.AddDays(1));
      var h2 = new Source("h2", DateTime.Now);
      var u2 = new Source("u2", DateTime.Now);
      sources.Add(h1.Id, h1);
      sources.Add(u1.Id, u1);
      sources.Add(h2.Id, h2);
      sources.Add(u2.Id, u2);

      BattlefyHandler battlefy = new();
      battlefy.AddSlug("handle1", h1);
      battlefy.AddUsername("username1", u1);
      battlefy.AddSlug("anonymous", h2);
      battlefy.AddUsername("username2", u2);

      string json = Serialize(battlefy);
      Console.WriteLine(nameof(SerializeBattlefy) + ": ");
      Console.WriteLine(json);
      BattlefyHandler deserialized = Deserialize<BattlefyHandler>(json, sources);

      var orderedSlugs = deserialized.SlugsOrdered;
      var orderedUsernames = deserialized.UsernamesOrdered;
      Assert.AreEqual(2, orderedSlugs.Count, "Unexpected number of slugs");
      Assert.AreEqual(2, orderedUsernames.Count, "Unexpected number of usernames");
      Assert.AreEqual("handle1", orderedSlugs[0].Value, "Slug [0] unexpected handle");
      Assert.AreEqual("h1", orderedSlugs[0].Sources[0].Name, "Slug [0] unexpected source");
      Assert.AreEqual("anonymous", orderedSlugs[1].Value, "Slug [1] unexpected handle");
      Assert.AreEqual("h2", orderedSlugs[1].Sources[0].Name, "Slug [1] unexpected source");
      Assert.AreEqual("https://battlefy.com/users/anonymous", orderedSlugs[1].Uri?.AbsoluteUri, "Slug [1] unexpected uri");
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

      DiscordHandler discord = new();
      discord.AddId("123456789", source2);
      discord.AddUsername("username2", u2);
      discord.AddId("4444", source1);
      discord.AddUsername("username1", u1);

      string json = Serialize(discord);
      Console.WriteLine(nameof(SerializeDiscord) + ": ");
      Console.WriteLine(json);
      DiscordHandler deserialized = Deserialize<DiscordHandler>(json, sources);
      var orderedIds = deserialized.IdsOrdered;
      var orderedUsernames = deserialized.UsernamesOrdered;

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
      Sendou sendou = new(handle, source1);

      string json = Serialize(sendou);
      Console.WriteLine(nameof(SerializeSendou) + ": ");
      Console.WriteLine(json);
      Sendou deserialized = Deserialize<Sendou>(json, sources);

      Assert.AreEqual("https://sendou.ink/u/slate", deserialized.Uri?.AbsoluteUri, "Unexpected Uri");
    }

    [TestMethod]
    public void SerializePlayerDeterministic()
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

      Player player = new();
      player.AddBattlefySlug("anonymous", h2);
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
      Console.WriteLine(nameof(SerializePlayerDeterministic) + ": ");
      Console.WriteLine(json);
      Player deserialized = Deserialize<Player>(json, sources);

      var orderedSlugs = deserialized.BattlefySlugsOrdered;
      var orderedUsernames = deserialized.BattlefyNamesOrdered;
      var orderedPesistentIds = deserialized.BattlefyIdsOrdered;
      Assert.AreEqual(2, orderedSlugs.Count, "Unexpected number of slugs");
      Assert.AreEqual(2, orderedUsernames.Count, "Unexpected number of usernames");
      Assert.AreEqual("handle1", orderedSlugs[0].Value, "Slug [0] unexpected handle");
      Assert.AreEqual("h1", orderedSlugs[0].Sources[0].Name, "Slug [0] unexpected source");
      Assert.AreEqual("anonymous", orderedSlugs[1].Value, "Slug [1] unexpected handle");
      Assert.AreEqual("h2", orderedSlugs[1].Sources[0].Name, "Slug [1] unexpected source");
      Assert.AreEqual("https://battlefy.com/users/anonymous", orderedSlugs[1].Uri?.AbsoluteUri, "Slug [1] unexpected uri");
      Assert.AreEqual("username1", orderedUsernames[0].Value, "Usernames [0] unexpected handle");
      Assert.AreEqual("u1", orderedUsernames[0].Sources[0].Name, "Usernames [0] unexpected source");
      Assert.AreEqual("username2", orderedUsernames[1].Value, "Usernames [1] unexpected handle");
      Assert.AreEqual("u2", orderedUsernames[1].Sources[0].Name, "Usernames [1] unexpected source");
      Assert.AreEqual("0000-1111-2222-3333", orderedPesistentIds[0].Value, "PersistentIds [0] unexpected id");

      var discord = deserialized.DiscordInformationNoCreate;
      Assert.IsNotNull(discord, "Discord information not found");

      var orderedDiscordIds = discord.IdsOrdered;
      var orderedDiscordUsernames = discord.UsernamesOrdered;
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
    public void SerializePlayerRandomly()
    {
      Player p = ArbitraryDataExtensions.GetRandomCoreObject<Player>();
      Assert.IsTrue(p.Name.Value.Length > 0 && p.Name != Builtins.UnknownPlayerName, $"Name not set ({p.Name})");

      string json = Serialize(p);
      Console.WriteLine(nameof(SerializePlayerRandomly) + ": ");
      Console.WriteLine(json);
      Player deserialized = Deserialize<Player>(json, p.Sources.ToDictionary(s => s.Id));
      Assert.AreEqual(p.Name, deserialized.Name, "Name not equal");
      AssertSerializeAndDeserialize(json);

      var reasons = p.MatchWithReason(deserialized);
      Assert.IsTrue(reasons.HasFlag(FilterOptions.SlappId));
      Assert.IsTrue(reasons.HasFlag(FilterOptions.PlayerName));
      Assert.AreEqual(p.Id, deserialized.Id, "Id not equal");
      Assert.IsNotNull(deserialized.CurrentTeam, "CurrentTeam not set");
      Assert.IsNotNull(deserialized.Name, "Name not set");
      Assert.AreEqual(p.CurrentTeam, deserialized.CurrentTeam, "CurrentTeam not equal");
      Assert.AreEqual(p.Name, deserialized.Name, "Name not equal");
      Assert.IsTrue(p.Sources.GenericMatch(deserialized.Sources), "Sources not equal");
    }

    [TestMethod]
    public void SerializeIdRandomly()
    {
      IdHandler idHandler = new();

      string json = Serialize(idHandler);
      Console.WriteLine(nameof(SerializeIdRandomly) + ": ");
      Console.WriteLine(json);
      IdHandler deserialized = Deserialize<IdHandler>(json, null);
      string json2 = Serialize(deserialized);
      Assert.AreEqual(idHandler.Id, deserialized.Id, "Id not equal");

      AssertSerializeAndDeserialize(json);
    }

    [TestMethod]
    public void SerializeAllSupportedHandlersRandomly()
    {
      Dictionary<string, bool> results = new();
      foreach (var handler in ((IBaseHandlerCollectionSourced)new Player()).SupportedHandlers.Concat(((IBaseHandlerCollectionSourced)new Team()).SupportedHandlers))
      {
        var constructedHandler = handler.Value.Item2();
        constructedHandler.PopulateWithRandomValues();
        string json = Serialize(constructedHandler);
        Console.WriteLine(nameof(SerializeAllSupportedHandlersRandomly) + ": " + handler.Value.Item1.Name);
        Console.WriteLine(json);
        var deserialized = Deserialize(json, null);
        string json2 = Serialize(deserialized);
        Console.WriteLine(nameof(SerializeAllSupportedHandlersRandomly) + ": (deserialized):");
        Console.WriteLine(json2);
        results[handler.Key] = json == json2;
      }

      if (results.Any(r => !r.Value))
      {
        Console.WriteLine("Failed to de/serialize all handlers");
        foreach (var result in results)
        {
          Console.WriteLine($"{result.Key}: {result.Value}");
        }
        Assert.Fail();
      }
    }

    [TestMethod]
    public void SerializeTeamRandomly()
    {
      Team t = ArbitraryDataExtensions.GetRandomCoreObject<Team>();
      Assert.IsTrue(t.Name.Value.Length > 0 && t.Name != Builtins.UnknownTeamName, $"Name not set ({t.Name})");

      string json = Serialize(t);
      Console.WriteLine(nameof(SerializeTeamRandomly) + ": ");
      Console.WriteLine(json);
      Team deserialized = Deserialize<Team>(json, t.Sources.ToDictionary(s => s.Id));
      Assert.AreEqual(t.Name, deserialized.Name, "Name not equal");
      var reasons = t.MatchWithReason(deserialized);
      Assert.IsTrue(reasons.HasFlag(FilterOptions.SlappId));
      Assert.IsTrue(reasons.HasFlag(FilterOptions.TeamName));
      Assert.AreEqual(t.Id, deserialized.Id, "Id not equal");
      Assert.IsNotNull(deserialized.CurrentDiv, "CurrentDiv not set");
      Assert.IsNotNull(deserialized.Name, "Name not set");
      Assert.AreEqual(t.CurrentDiv, deserialized.CurrentDiv, "CurrentDiv not equal");
      Assert.AreEqual(t.Name, deserialized.Name, "Name not equal");
      Assert.IsTrue(t.Sources.GenericMatch(deserialized.Sources), "Sources not equal");
    }

    [TestMethod]
    public void SerializeTeam()
    {
      var sources = new Dictionary<string, Source>();
      var oldestSource = new Source("old_source", DateTime.Now.AddDays(-6));
      var newestSource = new Source("new_source", DateTime.Now);
      sources.Add(newestSource.Id, newestSource);
      sources.Add(oldestSource.Id, oldestSource);

      Team team = new();
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

      var battlefy = deserialized.BattlefyPersistentTeamIdsOrdered;
      Assert.AreEqual(2, battlefy.Count, "Unexpected number of team battlefy slugs");
      Assert.AreEqual("1teamid1", battlefy[0].Value, "Slug [0] unexpected handle");
      Assert.AreEqual("2teamid2", battlefy[1].Value, "Slug [1] unexpected handle");
      Assert.AreEqual("new_source", battlefy[0].Sources[0].Name, "Slug [0] unexpected source");
      Assert.AreEqual("old_source", battlefy[1].Sources[0].Name, "Slug [1] unexpected source");

      var clanTags = deserialized.ClanTagsOrdered;
      Assert.AreEqual(2, clanTags.Count, "Unexpected number of clanTags");
      Assert.AreEqual("new", clanTags[0].Value, "clanTags[0] Unexpected Value");
      Assert.AreEqual("old", clanTags[1].Value, "clanTags[1] Unexpected Value");
      Assert.AreEqual("new_source", clanTags[0].Sources[0].Name, "clanTags[0] unexpected source");
      Assert.AreEqual("old_source", clanTags[1].Sources[0].Name, "clanTags[1] unexpected source");

      var divisions = deserialized.DivsOrdered;
      Assert.AreEqual(2, divisions.Count, "Unexpected number of divisions");
      // First - most recent
      Assert.AreEqual(8, divisions[0].Value, "Unexpected Value");
      Assert.AreEqual(DivType.LUTI, divisions[0].DivType, "Unexpected DivType");
      Assert.AreEqual("SX", divisions[0].Season, "Unexpected Season");
      // Next
      Assert.AreEqual(10, divisions[1].Value, "Unexpected Value");
      Assert.AreEqual(DivType.DSB, divisions[1].DivType, "Unexpected DivType");
      Assert.AreEqual("S9", divisions[1].Season, "Unexpected Season");

      var names = deserialized.Names.ToList();
      Assert.AreEqual(2, names.Count, "Unexpected number of team names");
      Assert.IsTrue(names.Find(n => n.Value == "team1")?.Sources[0].Name == newestSource.Name, "Names [0] unexpected handle/source combo");
      Assert.IsTrue(names.Find(n => n.Value == "team2")?.Sources[0].Name == oldestSource.Name, "Names [1] unexpected handle/source combo");
    }

    [TestMethod]
    public void SerializeBrackets()
    {
      var sources = new Dictionary<string, Source>();
      var source1 = new Source("source1");

      Player player1 = new();
      player1.AddSendou("slate", source1);
      Player player2 = new();
      player2.AddSendou("wug", source1);
      Team team1 = new("Team One", source1);
      player1.AddTeams(team1.Id, source1);
      Team team2 = new("Team Two", source1);
      player2.AddTeams(team2.Id, source1);
      source1.Players = new[] { player1, player2 };
      source1.Teams = new[] { team1, team2 };
      sources.Add(source1.Id, source1);

      Score s1 = new(new[] { 1, 3 });
      Game g1 = new(s1, new[] { player1.Id, player2.Id }, new[] { team1.Id, team2.Id });
      Dictionary<int, Guid[]> placementByPlayers = new()
      {
        [1] = new[] { player2.Id },
        [2] = new[] { player1.Id }
      };
      Dictionary<int, Guid[]> placementByTeams = new()
      {
        [1] = new[] { team2.Id },
        [2] = new[] { team1.Id }
      };

      Placement placement = new(placementByPlayers, placementByTeams);
      Bracket b1 = new("bracket_name", new[] { g1 }, new[] { player1.Id, player2.Id }, new[] { team1.Id, team2.Id }, placement);
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
    public void SerializeSourceRandomly()
    {
      Source source = ArbitraryDataExtensions.GetRandomSource(true);
      string json = Serialize(source);
      Console.WriteLine(nameof(SerializeSourceRandomly) + ": ");
      Console.WriteLine(json);
      AssertSerializeAndDeserialize(json);

      Source deserialized = Deserialize<Source>(json, null);
      Assert.AreNotEqual(deserialized.Start, DateTime.MinValue);
    }

    [TestMethod]
    public void DeserializeFriendCodes()
    {
      const string json = @"[[6653,9220,3527],[6653,9220,3527],[6653,9220,3527],[6653,9220,3527]]";
      FriendCode[] fcs = Deserialize<FriendCode[]>(json, new Dictionary<string, Source>());
      Assert.IsNotNull(fcs);
      Assert.AreEqual(4, fcs.Length, "Expected 4 friend codes parsed.");

      Player player = new();
      player.AddFCs(fcs, Builtins.ManualSource);
      Assert.AreEqual(1, player.FCs.Count, "Expected only 1 FC as the values are equal.");
    }

    [TestMethod]
    public void SerializationInfoAsKeyPairs()
    {
      var testCases = new Dictionary<string, KeyPairTestClass>();
      var source1 = new KeyPairTestClass
      {
        Name = "test_name",
        Value = 10
      };

      string json = Serialize(source1);
      Console.WriteLine(nameof(SerializationInfoAsKeyPairs) + ": ");
      Console.WriteLine(json);
      KeyPairTestClass deserialized = JsonConvert.DeserializeObject<KeyPairTestClass>(json) ?? throw new InvalidOperationException($"JsonConvert failed to Deserialize Object of type {typeof(KeyPairTestClass).Name} (json.Length={json.Length})");
      Assert.IsNotNull(deserialized);
      Assert.AreEqual(source1.Name, deserialized.Name);
      Assert.AreEqual(source1.Value, deserialized.Value);
    }

    /// <summary>
    /// This serializes and deserializes again to ensure that the serialization info is preserved.
    /// </summary>
    /// <param name="serializedJson"></param>
    /// <param name="testName"></param>
    private static void AssertSerializeAndDeserialize(string serializedJson, [CallerMemberName] string testName = "")
    {
      var deserialized = Deserialize(serializedJson, null);
      string json2 = Serialize(deserialized);
      Console.WriteLine(nameof(testName) + ": (deserialized):");
      Console.WriteLine(json2);
      Assert.AreEqual(serializedJson, json2);
    }

    [Serializable]
    private class KeyPairTestClass : ISerializable
    {
      private const string NameSerialization = "K";
      private const string ValueSerialization = "V";

      public string Name { get; internal set; } = "name";
      public int Value { get; internal set; } = 1;

      public KeyPairTestClass()
      {
      }

      protected KeyPairTestClass(SerializationInfo info, StreamingContext context)
      {
        var kvs = info.AsKeyValuePairs();
        foreach (var pair in kvs)
        {
          switch (pair.Key)
          {
            case NameSerialization: Name = Convert.ToString(pair.Value) ?? ""; break;
            case ValueSerialization: Value = Convert.ToInt32(pair.Value); break;
            default: throw new SerializationException($"Unknown key: {pair.Key}");
          }
        }
      }

      public void GetObjectData(SerializationInfo info, StreamingContext context)
      {
        info.AddValue(NameSerialization, Name);
        info.AddValue(ValueSerialization, Value);
      }
    }
  }
}