using Newtonsoft.Json;
using SplatTagCore;
using System;
using System.Collections.Generic;
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
        throw new InvalidOperationException("jsonFile is not set.");
      }
      string json = File.ReadAllText(jsonFile);

      List<Team> teams = new List<Team>();
      foreach (var pair in JsonConvert.DeserializeObject<Dictionary<string, string>>(json))
      {
        Team newTeam = new Team
        {
          Id = -teams.Count - 1,  // This will be updated when the merge happens.
          Name = pair.Key,
          Twitter = pair.Value
        };

        teams.Add(newTeam);
      }

      return (new Player[0], teams.ToArray());
    }

    public bool AcceptsInput(string input)
    {
      // Is named Twitter.json
      return Path.GetFileName(input).Equals("Twitter.json");
    }
  }
}