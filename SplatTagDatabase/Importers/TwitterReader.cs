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

    public TwitterReader(string jsonFile)
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
      string json = File.ReadAllText(jsonFile);

      List<Team> teams = new List<Team>();
      foreach (var pair in JsonConvert.DeserializeObject<Dictionary<string, string>>(json))
      {
        Team newTeam = new Team
        {
          Name = pair.Key,
          Twitter = pair.Value
        };

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