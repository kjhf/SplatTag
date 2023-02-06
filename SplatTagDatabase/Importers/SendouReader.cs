using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Nodes;

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
      string jsonString = File.ReadAllText(jsonFile);
      JsonNode? root = JsonNode.Parse(jsonString);
      if (root is not JsonArray jsonArray)
      {
        if ((root as JsonObject)?.TryGetPropertyValue("users", out var node) == true)
        {
          jsonArray = node as JsonArray ?? new JsonArray();
        }
        else
        {
          jsonArray = new JsonArray();
        }
      }

      var players = new List<Player>();
      foreach (JsonNode? userToken in jsonArray)
      {
        if (userToken is null) continue;

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
        var weapons = userToken.GetValue<string[]>("weapons", null);
        if (weapons != null)
        {
          player.AddWeapons(weapons!);
        }
        var top500 = userToken.GetValue<bool>("top500");
        if (top500)
        {
          player.Top500 = true;
        }
        JsonNode discord = userToken.GetValue<JsonObject>("discord") ?? userToken;
        if (discord != null)
        {
          player.AddDiscordUsername($"{discord.GetValue<string>("username")}#{discord.GetValue<string>("discriminator")}", source);
          var discordId = discord.GetValue<string>("discordId");
          if (discordId != null)
          {
            player.AddDiscordId(discordId, source);
          }
        }
        JsonNode profile = userToken.GetValue<JsonObject>("profile") ?? userToken;

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