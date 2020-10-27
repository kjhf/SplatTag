using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SplatTagDatabase.Importers
{
  internal class SendouReader : IImporter
  {
    private readonly string jsonFile;

    public SendouReader(string jsonFile)
    {
      this.jsonFile = jsonFile ?? throw new ArgumentNullException(nameof(jsonFile));
    }

    public (Player[], Team[]) Load()
    {
      if (jsonFile == null)
      {
        throw new InvalidOperationException(nameof(jsonFile) + " is not set.");
      }
      Debug.WriteLine("Loading " + jsonFile);
      JObject json = JObject.Parse(File.ReadAllText(jsonFile));

      List<Player> players = new List<Player>();
      foreach (JToken userToken in json["users"])
      {
        Player player = new Player();
        var name = userToken["twitch_name"].Value<string>();
        if (name != null)
        {
          player.Name = name;
        }
        name = userToken["twitter_name"].Value<string>();
        if (name != null)
        {
          player.Name = name;
          player.Twitter = name;
        }
        name = userToken["username"].Value<string>();
        if (name != null)
        {
          player.Name = name;
        }
        var sendouId = userToken["id"].Value<string>();
        if (sendouId != null && ulong.TryParse(sendouId, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out ulong id))
        {
          player.SendouId = id;
        }
        var country = userToken["country"].Value<string>();
        if (name != null)
        {
          player.Country = country;
        }
        var weapons = userToken["weapons"].HasValues ? userToken["weapons"].Values<string>() : null;
        if (weapons != null)
        {
          player.Weapons = weapons;
        }
        var top500 = userToken["top500"].Value<bool?>();
        if (top500 != null)
        {
          player.Top500 = top500.Value;
        }
        JToken discord = userToken["discord"];
        if (discord != null)
        {
          player.DiscordName = $"{discord["username"].Value<string>()}#{discord["discriminator"].Value<string>()}";
          var discordId = discord["id"].Value<string>();
          if (discordId != null && ulong.TryParse(discordId, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out ulong parsedId))
          {
            player.DiscordId = parsedId;
          }
        }

        player.Sources = new string[] { Path.GetFileNameWithoutExtension(jsonFile) };
        players.Add(player);
      }

      return (players.ToArray(), new Team[0]);
    }

    public static bool AcceptsInput(string input)
    {
      // Is named Sendou.json
      return Path.GetFileName(input).Equals("Sendou.json");
    }
  }
}