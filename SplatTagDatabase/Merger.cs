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
      Trace.WriteLine($"Beginning FinalisePlayers on {keys.Length} entries.");
      foreach (var playerId in keys)
      {
        var playersMinusCurrent = playersToMutate.Values.Where(p => p.Id != playerId);
        Debug.Assert(playersMinusCurrent.Count() == playersToMutate.Count - 1);
        var testPlayer = playersToMutate[playerId];

        // Find an equivalent player.
        Player foundPlayer = FindSamePlayerPersistent(playersMinusCurrent, testPlayer)
          ?? FindSamePlayerByTeamAndName(playersMinusCurrent, testPlayer);

        if (foundPlayer != null)
        {
          Debug.Assert(foundPlayer != testPlayer, $"Player to merge is the same as the one being merged: {foundPlayer.Name} (Id {foundPlayer.Id}) equiv to {testPlayer.Name} (Id [{playerId}] == {testPlayer.Id})");
          foundPlayer.Merge(testPlayer);
          playersToMutate.Remove(playerId);
        }
      }
      Trace.WriteLine($"Finished FinalisePlayers with {playersToMutate.Count} entries.");
    }

    public static void CorrectPlayerIds(ICollection<Player> incomingPlayers, IDictionary<long, long> teamsMergeResult)
    {
      if (incomingPlayers == null || teamsMergeResult == null || incomingPlayers.Count == 0 || teamsMergeResult.Count == 0) return;

      Debug.WriteLine($"CorrectPlayerIds called with {incomingPlayers.Count} entries.");
      Debug.WriteLine("Entries: ");
      foreach (var resultPair in teamsMergeResult)
      {
        Debug.WriteLine($"[{resultPair.Key}] --> {resultPair.Value}");
      }

      Parallel.ForEach(incomingPlayers, (importPlayer) =>
      {
        try
        {
          // Correct the ids
          importPlayer.Teams = importPlayer.Teams.Select(idToMerge => teamsMergeResult[idToMerge]);
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine($"Error in CorrectPlayerIds, failed for player {importPlayer}. Teams: {string.Join(", ", importPlayer.Teams)}. {ex}");
          Debug.WriteLine(ex);
          importPlayer.Teams = null;
        }
      });
    }

    /// <summary>
    /// Merge the loaded players into the current players list.
    /// </summary>
    public static void MergePlayers(IDictionary<uint, Player> playersToMutate, IEnumerable<Player> incomingPlayers)
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
          // First, try match through persistent information only.
          // If that doesn't work, try and match a name and same team.
          Player foundPlayer = FindSamePlayerPersistent(playersToMutate.Values, importPlayer)
            ?? FindSamePlayerByTeamAndName(playersToMutate.Values, importPlayer);

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
    /// Find a player that matches another instance through their persistent information.
    /// </summary>
    /// <param name="playersToMutate">The players to search</param>
    /// <param name="testPlayer">The player instance to try and find</param>
    /// <returns>The matched player, or null if new</returns>
    private static Player FindSamePlayerPersistent(IEnumerable<Player> playersToMutate, Player testPlayer)
    {
      bool tryMatchFcs = !string.IsNullOrEmpty(testPlayer.FriendCode);
      bool tryMatchDiscord = !string.IsNullOrEmpty(testPlayer.DiscordName);
      bool tryMatchDiscordId = testPlayer.DiscordId != null;
      bool tryMatchTwitch = testPlayer.Twitch != null;
      bool tryMatchTwitter = testPlayer.Twitter != null;
      bool tryMatchBattlefySlug = testPlayer.BattlefySlugs?.Count > 0;
      bool tryMatchBattlefyUsername = testPlayer.BattlefyUsername != null;

      return playersToMutate.FirstOrDefault(p =>
      {
        // Test if the Switch FC's match.
        if (tryMatchFcs && p.FriendCode?.Equals(testPlayer.FriendCode) == true)
        {
          // They do.
          Debug.WriteLine($"FindSamePlayerPersistent: Matched player {testPlayer} (Id {testPlayer.Id}) with FC {p.FriendCode} from player {p} (Id {p.Id}).");
          return true;
        }

        // Test if the Discord Ids match.
        if (tryMatchDiscordId && p.DiscordId?.Equals(testPlayer.DiscordId) == true)
        {
          // They do.
          Debug.WriteLine($"FindSamePlayerPersistent: Matched player {testPlayer} (Id {testPlayer.Id}) with Discord Id {p.DiscordId} from player {p} (Id {p.Id}).");
          return true;
        }

        // Test if the Discord names match.
        if (tryMatchDiscord && p.DiscordName?.Equals(testPlayer.DiscordName) == true)
        {
          // They do.
          Debug.WriteLine($"FindSamePlayerPersistent: Matched player {testPlayer} (Id {testPlayer.Id}) with Discord name {p.DiscordName} from player {p} (Id {p.Id}).");
          return true;
        }

        // Test if the Twitches match.
        if (tryMatchTwitch && p.Twitch?.Equals(testPlayer.Twitch) == true)
        {
          // They do.
          Debug.WriteLine($"FindSamePlayerPersistent: Matched player {testPlayer} (Id {testPlayer.Id}) with Twitch {p.Twitch} from player {p} (Id {p.Id}).");
          return true;
        }

        // Test if the Twitters match.
        if (tryMatchTwitter && p.Twitter?.Equals(testPlayer.Twitter) == true)
        {
          // They do.
          Debug.WriteLine($"FindSamePlayerPersistent: Matched player {testPlayer} (Id {testPlayer.Id}) with Twitter {p.Twitter} from player {p} (Id {p.Id}).");
          return true;
        }

        // Test if the Battlefy Usernames match.
        if (tryMatchBattlefyUsername && p.BattlefyUsername?.Equals(testPlayer.BattlefyUsername) == true)
        {
          // They do.
          Debug.WriteLine($"FindSamePlayerPersistent: Matched player {testPlayer} (Id {testPlayer.Id}) with BattlefyUsername {p.BattlefyUsername} from player {p} (Id {p.Id}).");
          return true;
        }

        // Test if any of the Battlefy Slugs match.
        if (tryMatchBattlefySlug && p.BattlefySlugs.Intersect(testPlayer.BattlefySlugs).Any())
        {
          // They do.
          Debug.WriteLine($"FindSamePlayerPersistent: Matched player {testPlayer} (Id {testPlayer.Id}) with test BattlefySlug(s) e.g. {testPlayer.BattlefySlugs.First()} with player {p} (Id {p.Id}).");
          return true;
        }

        return false;
      });
    }

    /// <summary>
    /// Find a player that matches another instance by matching a team and name.
    /// Does not search other information.
    /// </summary>
    /// <param name="playersToMutate">The players to search</param>
    /// <param name="testPlayer">The player instance to try and find</param>
    /// <returns>The matched player, or null if new</returns>
    private static Player FindSamePlayerByTeamAndName(IEnumerable<Player> playersToMutate, Player testPlayer)
    {
      // Long test for names with transformation.
      HashSet<string> testPlayerTransformedNames = new HashSet<string>(testPlayer.Names.Select(n => n.Replace(" ", "").TransformString().ToLowerInvariant()));

      return playersToMutate.FirstOrDefault(p =>
      {
        if (p.Teams.Intersect(testPlayer.Teams).Any())
        {
          // Replace spaces because people adding tags or starting with space messes up same-name detection.
          // Test if the name matches the names that we know this player by.
          foreach (string knownName in p.Names.Select(n => n.Replace(" ", "").TransformString().ToLowerInvariant()))
          {
            if ((!string.IsNullOrWhiteSpace(knownName)) && (testPlayerTransformedNames.Contains(knownName)))
            {
              Debug.WriteLine($"FindSamePlayerThroughNames: Matched uncached player {testPlayer} (Id {testPlayer.Id}) with known name {knownName} from player {p} (Id {p.Id}).");
              return true;
            }
          }
        }

        return false;
      });
    }

    /// <summary>
    /// Find a player that matches another instance by any of their known names.
    /// Does not search other information. Use <see cref="FindSamePlayerPersistent(IEnumerable{Player}, Player)"/> if you want this.
    /// </summary>
    /// <param name="playersToMutate">The players to search</param>
    /// <param name="testPlayer">The player instance to try and find</param>
    /// <param name="transformedPlayerNames">Cached player names. Can be null to calculate.</param>
    /// <returns>The matched player, or null if new</returns>
    private static Player FindSamePlayerByNames(IEnumerable<Player> playersToMutate, Player testPlayer, IEnumerable<KeyValuePair<Player, string[]>> transformedPlayerNames)
    {
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
              Debug.WriteLine($"FindSamePlayerThroughNames: Matched uncached player {testPlayer} (Id {testPlayer.Id}) with known name {knownName} from player {p} (Id {p.Id}).");
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
              Debug.WriteLine($"FindSamePlayerThroughNames: Matched cached player {testPlayer} (Id {testPlayer.Id}) with known name {knownName} from player {tPair.Key} (Id {tPair.Key.Id}).");
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
    /// <returns>
    /// A dictionary of merged team ids keyed by initial with values of the new id.
    /// </returns>
    public static IDictionary<long, long> MergeTeams(IDictionary<long, Team> teamsToMutate, IEnumerable<Team> incomingTeams)
    {
      ConcurrentDictionary<long, long> mergeResult = new ConcurrentDictionary<long, long>();

      if (incomingTeams == null)
      {
        return mergeResult;
      }

      if (teamsToMutate == null)
      {
        teamsToMutate = new Dictionary<long, Team>();
      }

      if (teamsToMutate?.Count > 0)
      {
        // Construct a SearchableNames lookup of teams.
        Dictionary<string, Team> transformedTeamNames = new Dictionary<string, Team>();
        ConcurrentBag<long> mergedTeams = new ConcurrentBag<long>();
        foreach (var teamPair in teamsToMutate)
        {
          var id = teamPair.Key;
          var team = teamPair.Value;
          try
          {
            transformedTeamNames.Add(team.SearchableName, team);
          }
          catch (ArgumentException)
          {
            // If this team name already exists, merge the teams.
            var existingTeam = transformedTeamNames[team.SearchableName];
            existingTeam.Merge(team);
            mergedTeams.Add(id);
            mergeResult.TryAdd(id, existingTeam.Id);
          }
        }

        foreach (var teamKey in mergedTeams)
        {
          teamsToMutate.Remove(teamKey);
        }

        // Add if the team is new (by name) and assign them with a new id
        // Otherwise, match the found team with its id, based on name.
        foreach (Team importTeam in incomingTeams)
        {
          // Replace spaces because people adding tags or starting with space messes up same-name detection.
          // Also transform the team name.
          if (transformedTeamNames.TryGetValue(importTeam.SearchableName, out Team foundTeam))
          {
            mergeResult.TryAdd(importTeam.Id, foundTeam.Id);
            importTeam.Id = foundTeam.Id;
            foundTeam.Merge(importTeam);
          }
          else
          {
            long key = teamsToMutate.Keys.LastOrDefault() + 1;
            mergeResult.TryAdd(importTeam.Id, key);
            importTeam.Id = key;
            teamsToMutate.Add(key, importTeam);
          }
        }
      }
      else
      {
        // No merge required, just take as-is.
        foreach (Team importTeam in incomingTeams)
        {
          long key = teamsToMutate.Keys.LastOrDefault() + 1;
          mergeResult.TryAdd(importTeam.Id, key);
          importTeam.Id = key;
          teamsToMutate.Add(key, importTeam);
        }
      }

      return mergeResult;
    }
  }
}