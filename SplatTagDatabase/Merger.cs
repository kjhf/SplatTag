using SplatTagCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SplatTagDatabase
{
  internal static class Merger
  {
    /// <summary>
    /// Merge the loaded players into the current players list.
    /// </summary>
    public static void MergePlayers(IDictionary<uint, Player> playersToMutate, IEnumerable<Player> incomingPlayers, IReadOnlyDictionary<long, Team> incomingTeams)
    {
      // Add if the player is new (by name) and assign them with a new id
      // Otherwise, match the found team with its id, based on name.
      ConcurrentBag<Player> playersToAdd = new ConcurrentBag<Player>();
      Parallel.ForEach(incomingPlayers, (importPlayer) =>
      {
        List<long> correctedTeamIds = new List<long>();
        foreach (long idToMerge in importPlayer.Teams)
        {
          if (!incomingTeams.ContainsKey(idToMerge))
          {
            throw new ArgumentException($"incomingTeams does not contain the merged id, {idToMerge}");
          }
          else
          {
            var id = incomingTeams[idToMerge].Id;
            correctedTeamIds.Add(id);
          }
        }
        importPlayer.Teams = correctedTeamIds;

        Player foundPlayer = playersToMutate.Values.FirstOrDefault(p =>
        {
          // Replace spaces because people adding tags or starting with space messes up same-name detection.
          string currentName = p.Name.Replace(" ", "");
          string incomingName = importPlayer.Name.Replace(" ", "");

          if (currentName.Length < incomingName.Length)
          {
            return currentName.StartsWith(incomingName, StringComparison.InvariantCultureIgnoreCase);
          }
          else
          {
            return incomingName.StartsWith(currentName, StringComparison.InvariantCultureIgnoreCase);
          }
        });

        if (foundPlayer == null)
        {
          playersToAdd.Add(importPlayer);
        }
        else
        {
          importPlayer.Id = foundPlayer.Id;
          foundPlayer.Merge(importPlayer);
        }
      });

      foreach (Player importPlayer in playersToAdd)
      {
        uint key = playersToMutate.Keys.LastOrDefault() + 1;
        importPlayer.Id = key;
        playersToMutate.Add(key, importPlayer);
      }
    }

    /// <summary>
    /// Merge the loaded teams into the current teams list.
    /// </summary>
    public static void MergeTeams(IDictionary<long, Team> teamsToMutate, IEnumerable<Team> incomingTeams)
    {
      if (teamsToMutate.Count > 0)
      {
        // Add if the team is new (by name) and assign them with a new id
        // Otherwise, match the found team with its id, based on name.
        foreach (Team importTeam in incomingTeams)
        {
          Team foundTeam = teamsToMutate.Values.FirstOrDefault(t =>
          {
            // Replace spaces because people adding tags or starting with space messes up same-name detection.
            string currentName = t.Name.Replace(" ", "");
            string incomingName = importTeam.Name.Replace(" ", "");

            if (currentName.Length < incomingName.Length)
            {
              return incomingName.StartsWith(currentName, StringComparison.InvariantCultureIgnoreCase);
            }
            else
            {
              return currentName.StartsWith(incomingName, StringComparison.InvariantCultureIgnoreCase);
            }
          });

          if (foundTeam == null)
          {
            long key = teamsToMutate.Keys.LastOrDefault() + 1;
            importTeam.Id = key;
            teamsToMutate.Add(key, importTeam);
          }
          else
          {
            importTeam.Id = foundTeam.Id;
            foundTeam.Merge(importTeam);
          }
        }
      }
      else
      {
        // No merge required, just take as-is.
        foreach (Team importTeam in incomingTeams)
        {
          long key = teamsToMutate.Keys.LastOrDefault() + 1;
          importTeam.Id = key;
          teamsToMutate.Add(key, importTeam);
        }
      }
    }
  }
}