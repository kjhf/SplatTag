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

    public (Player[], Team[]) Load()
    {
      Debug.WriteLine("Loading " + jsonFile);
      string json = File.ReadAllText(jsonFile);

      List<Team> teams = new List<Team>();
      foreach (var pair in JsonConvert.DeserializeObject<Dictionary<string, string>>(json))
      {
        Team newTeam = new Team(pair.Key, source);
        newTeam.AddTwitter(pair.Value, source);
        teams.Add(newTeam);
      }

      return (new Player[0], teams.ToArray());
    }

    public static bool AcceptsInput(string input)
    {
      // Twitter.json
      return Path.GetFileName(input).EndsWith("Twitter.json");
    }
  }
}