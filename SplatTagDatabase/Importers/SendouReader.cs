using Newtonsoft.Json.Linq;
using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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

    public override bool Equals(object? obj)
    {
      return obj is SendouReader reader &&
             source.Equals(reader.source);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(nameof(SendouReader), source);
    }

    public Source Load()
    {
      Debug.WriteLine("Loading " + jsonFile);
      JToken json = JToken.Parse(File.ReadAllText(jsonFile));
      if (json is not JArray)
      {
        json = json?.GetValue<JToken>("users", null) ?? new JArray();
      }

      var players = new List<Player>();
      foreach (JToken userToken in json)
      {
        var player = new Player();
        var name = userToken.GetValue<string>("username");
        if (name != null)
        {
          player.AddName(name, source);
        }
        name = userToken.GetValue<string>("twitch_name");
        if (name != null)
        {
          player.AddTwitch(name, source);
        }
        name = userToken.GetValue<string>("twitter_name");
        if (name != null)
        {
          player.AddTwitter(name, source);
        }
        var sendouId = userToken.GetValue<string>("id");
        if (sendouId != null)
        {
          player.AddSendou(sendouId, source);
        }
        var country = userToken.GetValue<string>("country");
        if (name != null)
        {
          player.Country = country;
        }
        var weapons = userToken["weapons"]?.HasValues == true ? userToken["weapons"]?.Values<string>() : null;
        if (weapons != null)
        {
          player.AddWeapons(weapons!);
        }
        var top500 = userToken.GetValue("top500", false);
        if (top500)
        {
          player.Top500 = true;
        }
        JToken discord = userToken["discord"] ?? userToken;
        if (discord != null)
        {
          player.AddDiscordUsername($"{discord["username"]?.Value<string>()}#{discord["discriminator"]?.Value<string>()}", source);
          var discordId = discord["discordId"]?.Value<string>();
          if (discordId != null)
          {
            player.AddDiscordId(discordId, source);
          }
        }
        JToken profile = userToken["profile"] ?? userToken;
        if (profile?.HasValues == true)
        {
          name = profile.GetValue<string>("twitchName");
          if (name != null)
          {
            player.AddTwitch(name, source);
          }
          name = profile.GetValue<string>("twitterName");
          if (name != null)
          {
            player.AddTwitter(name, source);
          }
        }
        var plusServerMembership = userToken.GetValue<int?>("membershipTier");
        if (plusServerMembership != null)
        {
          player.AddPlusServerMembership(plusServerMembership, source);
        }
        players.Add(player);
      }

      source.Players = players.ToArray();
      return source;
    }

    public static bool AcceptsInput(string input)
    {
      // Is named Sendou.json
      return Path.GetFileName(input).EndsWith("Sendou.json");
    }
  }
}