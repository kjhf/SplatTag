using Newtonsoft.Json.Linq;
using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SplatTagDatabase.Importers
{
  internal class SendouReader : IImporter
  {
    private readonly string jsonFile;
    private readonly Source source;

    public SendouReader(string jsonFile)
    {
      this.jsonFile = jsonFile ?? throw new ArgumentNullException(nameof(jsonFile));
      this.source = new Source(Path.GetFileNameWithoutExtension(jsonFile));
    }

    public (Player[], Team[]) Load()
    {
      Debug.WriteLine("Loading " + jsonFile);
      JObject json = JObject.Parse(File.ReadAllText(jsonFile));

      List<Player> players = new List<Player>();
      foreach (JToken userToken in json["users"])
      {
        Player player = new Player();
        var name = userToken["username"].Value<string>();
        if (name != null)
        {
          player.AddName(name, source);
        }
        name = userToken["twitch_name"].Value<string>();
        if (name != null)
        {
          player.AddTwitch(name, source);
        }
        name = userToken["twitter_name"].Value<string>();
        if (name != null)
        {
          player.AddTwitter(name, source);
        }
        var sendouId = userToken["id"].Value<string>();
        if (sendouId != null)
        {
          player.AddSendou(sendouId, source);
        }
        var country = userToken["country"].Value<string>();
        if (name != null)
        {
          player.Country = country;
        }
        var weapons = userToken["weapons"].HasValues ? userToken["weapons"].Values<string>() : null;
        if (weapons != null)
        {
          player.AddWeapons(weapons);
        }
        var top500 = userToken["top500"].Value<bool?>();
        if (top500 != null)
        {
          player.Top500 = top500.Value;
        }
        JToken discord = userToken["discord"];
        if (discord != null)
        {
          player.AddDiscordName($"{discord["username"].Value<string>()}#{discord["discriminator"].Value<string>()}", source);
          var discordId = discord["id"].Value<string>();
          if (discordId != null)
          {
            player.AddDiscordId(discordId, source);
          }
        }

        player.AddSources(source.AsEnumerable());
        players.Add(player);
      }

      return (players.ToArray(), new Team[0]);
    }

    public static bool AcceptsInput(string input)
    {
      // Is named Sendou.json
      return Path.GetFileName(input).EndsWith("Sendou.json");
    }
  }
}