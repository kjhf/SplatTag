using Newtonsoft.Json;
using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SplatTagDatabase.Importers
{
  internal class TwitterReader : IImporter
  {
    private readonly string jsonFile;
    private readonly Source source;

    public TwitterReader(string jsonFile)
    {
      this.jsonFile = jsonFile ?? throw new ArgumentNullException(nameof(jsonFile));
      this.source = new Source(Path.GetFileNameWithoutExtension(jsonFile));
    }

    public override bool Equals(object? obj)
    {
      return obj is TwitterReader reader &&
             source.Equals(reader.source);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(nameof(TwitterReader), source);
    }

    public Source Load()
    {
      Debug.WriteLine("Loading " + jsonFile);
      string json = File.ReadAllText(jsonFile);

      var teams = new List<Team>();
      var teamTwitters = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
      foreach (var pair in teamTwitters)
      {
        var newTeam = new Team(pair.Key, source);
        newTeam.AddTwitter(pair.Value, source);
        teams.Add(newTeam);
      }

      source.Teams = teams.ToArray();
      return source;
    }

    public static bool AcceptsInput(string input)
    {
      // Twitter.json
      return Path.GetFileName(input).EndsWith("Twitter.json");
    }
  }
}