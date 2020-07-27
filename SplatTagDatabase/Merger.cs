using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SplatTagDatabase
{
  internal static class Merger
  {
    /// <summary>
    /// Merge the loaded players into the current players list.
    /// </summary>
    public static void MergePlayers(IDictionary<uint, Player> playersToMutate, IEnumerable<Player> incomingPlayers)
    {
      if (playersToMutate.Count > 0)
      {
        // Add if the player is new (by name) and assign them with a new id
        // Otherwise, match the found team with its id, based on name.
        foreach (Player importPlayer in incomingPlayers)
        {
          Player foundPlayer = playersToMutate.Values.FirstOrDefault(p => p.Name.Equals(importPlayer.Name, StringComparison.OrdinalIgnoreCase));

          if (foundPlayer == null)
          {
            uint key = playersToMutate.Keys.LastOrDefault() + 1;
            importPlayer.Id = key;
            playersToMutate.Add(key, importPlayer);
          }
          else
          {
            foundPlayer.Merge(importPlayer);
          }
        }
      }
      else
      {
        // No merge required, just take as-is.
        foreach (Player importPlayer in incomingPlayers)
        {
          uint key = playersToMutate.Keys.LastOrDefault() + 1;
          importPlayer.Id = key;
          playersToMutate.Add(key, importPlayer);
        }
      }
    }

    /// <summary>
    /// Merge the loaded teams into the current teams list.
    /// </summary>
    public static void MergeTeams(IDictionary<uint, Team> teamsToMutate, IEnumerable<Team> incomingTeams)
    {
      if (teamsToMutate.Count > 0)
      {
        // Add if the team is new (by name) and assign them with a new id
        // Otherwise, match the found team with its id, based on name.
        foreach (Team importTeam in incomingTeams)
        {
          Team foundTeam = teamsToMutate.Values.FirstOrDefault(t => t.Name.Equals(importTeam.Name));

          if (foundTeam == null)
          {
            uint key = teamsToMutate.Keys.LastOrDefault() + 1;
            importTeam.Id = key;
            teamsToMutate.Add(key, importTeam);
          }
          else
          {
            foundTeam.Merge(importTeam);
          }
        }
      }
      else
      {
        // No merge required, just take as-is.
        foreach (Team importTeam in incomingTeams)
        {
          uint key = teamsToMutate.Keys.LastOrDefault() + 1;
          importTeam.Id = key;
          teamsToMutate.Add(key, importTeam);
        }
      }
    }
  }
}