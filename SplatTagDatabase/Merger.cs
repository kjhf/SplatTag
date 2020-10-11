using SplatTagCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SplatTagDatabase
{
  internal static class Merger
  {
    public static void FinalisePlayers(IDictionary<uint, Player> playersToMutate)
    {
      if (playersToMutate == null) return;

      var keys = playersToMutate.Keys.Reverse().ToArray();
      foreach (var playerId in keys)
      {
        var playersMinusCurrent = playersToMutate.Values.Where(p => p.Id != playerId);
        Debug.Assert(playersMinusCurrent.Count() == playersToMutate.Count - 1);
        var testPlayer = playersToMutate[playerId];
        Player foundPlayer = FindSamePlayer(playersMinusCurrent, testPlayer, null);
        if (foundPlayer != null)
        {
          Debug.Assert(foundPlayer != testPlayer, $"Player to merge is the same as the one being merged: {foundPlayer.Name} (Id {foundPlayer.Id}) equiv to {testPlayer.Name} (Id [{playerId}] == {testPlayer.Id})");
          foundPlayer.Merge(testPlayer);
          playersToMutate.Remove(playerId);
        }
      }
    }

    /// <summary>
    /// Merge the loaded players into the current players list.
    /// </summary>
    public static void MergePlayers(IDictionary<uint, Player> playersToMutate, IEnumerable<Player> incomingPlayers, IReadOnlyDictionary<long, Team> incomingTeams)
    {
      if (playersToMutate == null || incomingPlayers == null) return;

      // Add if the player is new (by name) and assign them with a new id
      // Otherwise, match the found team with its id, based on name.
      ConcurrentBag<Player> playersToAdd = new ConcurrentBag<Player>();
      ConcurrentBag<(Player, Player)> playersToMerge = new ConcurrentBag<(Player, Player)>();

      // Replace spaces because people adding tags or starting with space messes up same-name detection.
      Dictionary<Player, string[]> transformedPlayerNames =
        playersToMutate.ToDictionary(
          p => p.Value,
          p => p.Value.Names.Select(n => n.Replace(" ", "").TransformString().ToLowerInvariant()).ToArray());

      Parallel.ForEach(incomingPlayers, (importPlayer) =>
      {
        try
        {
          // Correct the ids
          if (incomingTeams != null)
          {
            importPlayer.Teams = importPlayer.Teams.Select(idToMerge => incomingTeams[idToMerge].Id);
          }

          Player foundPlayer = FindSamePlayer(playersToMutate.Values, importPlayer, transformedPlayerNames);

          if (foundPlayer == null)
          {
            playersToAdd.Add(importPlayer);
          }
          else
          {
            importPlayer.Id = foundPlayer.Id;
            playersToMerge.Add((foundPlayer, importPlayer));
          }
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine($"Error in importing player {importPlayer}: {ex}");
        }
      });

      foreach (Player importPlayer in playersToAdd)
      {
        uint key = playersToMutate.Keys.LastOrDefault() + 1;
        importPlayer.Id = key;
        playersToMutate.Add(key, importPlayer);
      }

      foreach (var (foundPlayer, importPlayer) in playersToMerge)
      {
        foundPlayer.Merge(importPlayer);
      }
    }

    /// <summary>
    /// Find a player that matches another instance.
    /// </summary>
    /// <param name="playersToMutate">The players to search</param>
    /// <param name="testPlayer">The player instance to try and find</param>
    /// <returns>The matched player, or null if new</returns>
    private static Player FindSamePlayer(IEnumerable<Player> playersToMutate, Player testPlayer, IEnumerable<KeyValuePair<Player, string[]>> transformedPlayerNames)
    {
      bool tryMatchFcs = !string.IsNullOrEmpty(testPlayer.FriendCode);
      bool tryMatchDiscord = !string.IsNullOrEmpty(testPlayer.DiscordName);
      bool tryMatchDiscordId = testPlayer.DiscordId != null;

      // Quick test all FCs and Discord names.
      Player foundSame = playersToMutate.FirstOrDefault(p =>
      {
        // Test if the Switch FC's match.
        if (tryMatchFcs && p.FriendCode?.Equals(testPlayer.FriendCode) == true)
        {
          // They do.
          return true;
        }

        // Test if the Discord Ids match.
        if (tryMatchDiscordId && p.DiscordId?.Equals(testPlayer.DiscordId) == true)
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

        return false;
      });
      if (foundSame != null) return foundSame;

      // Long test for names with transformation.
      HashSet<string> testPlayerTransformedNames = new HashSet<string>(testPlayer.Names.Select(n => n.Replace(" ", "").TransformString().ToLowerInvariant()));

      // Use the cached version if populated, else work out this player.
      if (transformedPlayerNames == null)
      {
        return playersToMutate.FirstOrDefault(p =>
        {
          // Replace spaces because people adding tags or starting with space messes up same-name detection.
          // Test if the name matches the names that we know this player by.
          foreach (string knownName in p.Names.Select(n => n.Replace(" ", "").TransformString().ToLowerInvariant()))
          {
            if ((!string.IsNullOrWhiteSpace(knownName)) && (testPlayerTransformedNames.Contains(knownName)))
            {
              return true;
            }
          }

          return false;
        });
      }
      else
      {
        return transformedPlayerNames.FirstOrDefault(tPair =>
        {
          // Test if the name matches the names that we know this player by.
          foreach (string knownName in tPair.Value)
          {
            if ((!string.IsNullOrWhiteSpace(knownName)) && (testPlayerTransformedNames.Contains(knownName)))
            {
              return true;
            }
          }

          return false;
        }).Key;
      }
    }

    /// <summary>
    /// Merge the loaded teams into the current teams list.
    /// </summary>
    public static void MergeTeams(IDictionary<long, Team> teamsToMutate, IEnumerable<Team> incomingTeams)
    {
      if (teamsToMutate.Count > 0)
      {
        Dictionary<string, Team> transformedTeamNames = teamsToMutate.Values.ToDictionary(t => t.SearchableName, t => t);

        // Add if the team is new (by name) and assign them with a new id
        // Otherwise, match the found team with its id, based on name.
        foreach (Team importTeam in incomingTeams)
        {
          // Replace spaces because people adding tags or starting with space messes up same-name detection.
          // Also transform the team name.
          transformedTeamNames.TryGetValue(importTeam.SearchableName, out Team foundTeam);

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