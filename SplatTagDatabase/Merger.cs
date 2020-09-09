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
        Player foundPlayer = FindSamePlayer(playersToMutate.Values, importPlayer);

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
    /// Find a player that matches another instance.
    /// </summary>
    /// <param name="playersToMutate">The players to search</param>
    /// <param name="testPlayer">The player instance to try and find</param>
    /// <returns>The matched player, or null if new</returns>
    private static Player FindSamePlayer(ICollection<Player> playersToMutate, Player testPlayer)
    {
      bool tryMatchFcs = testPlayer.FriendCode != null;
      bool tryMatchDiscord = testPlayer.DiscordName != null;
      HashSet<string> testPlayerTransformedNames = new HashSet<string>(testPlayer.Names.Select(n => n.Replace(" ", "").TransformString().ToLowerInvariant()));

      return playersToMutate.FirstOrDefault(p =>
      {
        // Test if the Switch FC's match.
        if (tryMatchFcs && p.FriendCode == testPlayer.FriendCode)
        {
          // They do.
          return true;
        }

        // Test if the Discord names match.
        if (tryMatchDiscord && p.DiscordName?.Equals(testPlayer.DiscordName, StringComparison.OrdinalIgnoreCase) == true)
        {
          // They do.
          return true;
        }

        // Test if the name matches the names that we know this player by.
        foreach (string knownName in p.Names)
        {
          // Replace spaces because people adding tags or starting with space messes up same-name detection.
          string transformedKnownName = knownName.Replace(" ", "").TransformString().ToLowerInvariant();
          if (testPlayerTransformedNames.Contains(transformedKnownName))
          {
            return true;
          }
        }

        return false;
      });
    }

    /// <summary>
    /// Merge the loaded teams into the current teams list.
    /// </summary>
    public static void MergeTeams(IDictionary<long, Team> teamsToMutate, IEnumerable<Team> incomingTeams)
    {
      if (teamsToMutate.Count > 0)
      {
        Dictionary<string, Team> transformedTeamNames =
          teamsToMutate.Values.ToDictionary(t => t.Name.Replace(" ", "").TransformString().ToLowerInvariant(), t => t);

        // Add if the team is new (by name) and assign them with a new id
        // Otherwise, match the found team with its id, based on name.
        foreach (Team importTeam in incomingTeams)
        {
          // Replace spaces because people adding tags or starting with space messes up same-name detection.
          // Also transform the team name.
          string incomingName = importTeam.Name.Replace(" ", "").TransformString();

          transformedTeamNames.TryGetValue(incomingName.ToLowerInvariant(), out Team foundTeam);

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