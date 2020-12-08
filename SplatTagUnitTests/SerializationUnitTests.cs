using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SplatTagCore;
using SplatTagCore.Social;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
      battlefy.AddSlug("handle2", new Source("h2"));
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
      Assert.AreEqual("handle2", deserialized.Slugs[1].Value, "Slug [1] unexpected handle");
      Assert.AreEqual("h2", deserialized.Slugs[1].Sources.First().Name, "Slug [1] unexpected source");
      Assert.AreEqual("username1", deserialized.Usernames[0].Value, "Usernames [0] unexpected handle");
      Assert.AreEqual("u1", deserialized.Usernames[0].Sources.First().Name, "Usernames [0] unexpected source");
      Assert.AreEqual("username2", deserialized.Usernames[1].Value, "Usernames [1] unexpected handle");
      Assert.AreEqual("u2", deserialized.Usernames[1].Sources.First().Name, "Usernames [1] unexpected source");
    }
  }
}