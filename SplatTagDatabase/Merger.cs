using SplatTagCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplatTagDatabase
{
  internal static class Merger
  {
    /// <summary>
    /// Final time-consuming call to look at all player entries and merge where appropriate.
    /// </summary>
    /// <param name="playersToMutate"></param>
    public static void FinalisePlayers(IList<Player> playersToMutate, TextWriter? logger = null)
    {
      if (playersToMutate == null) return;

      string logMessage = $"Beginning {nameof(FinalisePlayers)} on {playersToMutate.Count} entries.";
      logger?.WriteLine(logMessage);

      string lastProgressBar = "";
      for (int i = playersToMutate.Count - 1; i >= 0; --i)
      {
        var newerPlayerRecord = playersToMutate[i];

        // First, try match through persistent information only.
        Player? foundPlayer = null;
        for (int j = 0; j < i; ++j)
        {
          var olderPlayerRecord = playersToMutate[j];
          if (PlayersMatch(olderPlayerRecord, newerPlayerRecord, FilterOptions.Persistent, logger))
          {
            foundPlayer = olderPlayerRecord;
            break;
          }
        }

        // If that doesn't work, try and match a name and same team.
        if (foundPlayer == null)
        {
          for (int j = 0; j < i; ++j)
          {
            var olderPlayerRecord = playersToMutate[j];
            if (PlayersMatch(olderPlayerRecord, newerPlayerRecord, FilterOptions.Name, logger))
            {
              foundPlayer = olderPlayerRecord;
              break;
            }
          }
        }

        // If a player has now been found, merge it.
        if (foundPlayer != null)
        {
          logger?.WriteLine($"Merging player {newerPlayerRecord} with teams [{string.Join(", ", newerPlayerRecord.Teams)}] into {foundPlayer} with teams [{string.Join(", ", foundPlayer.Teams)}].");
          foundPlayer.Merge(newerPlayerRecord);
          playersToMutate.RemoveAt(i); // remove the newer record
        }

        string progressBar = Util.GetProgressBar(playersToMutate.Count - i, playersToMutate.Count, 100);
        if (!progressBar.Equals(lastProgressBar))
        {
          logger?.WriteLine(progressBar);
          lastProgressBar = progressBar;
        }
      }

      logMessage = $"Finished {nameof(FinalisePlayers)} with {playersToMutate.Count} entries.";
      logger?.WriteLine(logMessage);
    }

    /// <summary>
    /// Final time-consuming call to look at all team entries and merge where appropriate.
    /// </summary>
    /// <returns>
    /// A dictionary of merged team ids keyed by initial with values of the new id.
    /// </returns>
    public static IDictionary<Guid, Guid> FinaliseTeams(IReadOnlyCollection<Player> allPlayers, IList<Team> teamsToMutate, TextWriter? logger = null)
    {
      ConcurrentDictionary<Guid, Guid> mergeResult = new ConcurrentDictionary<Guid, Guid>();

      logger?.WriteLine($"Beginning {nameof(FinaliseTeams)} on {teamsToMutate.Count} entries.");

      string lastProgressBar = "";
      for (int i = teamsToMutate.Count - 1; i >= 0; --i)
      {
        var newerTeamRecord = teamsToMutate[i];

        // Try match teams.
        Team? foundTeam = null;
        for (int j = 0; j < i; ++j)
        {
          var olderTeamRecord = teamsToMutate[j];
          if (TeamsMatch(allPlayers, olderTeamRecord, newerTeamRecord, logger))
          {
            foundTeam = olderTeamRecord;
            break;
          }
        }

        // If a teams has now been found, merge it.
        if (foundTeam != null)
        {
          MergeExistingTeam(mergeResult, newerTeamRecord, foundTeam);
          teamsToMutate.RemoveAt(i);
        }

        string progressBar = Util.GetProgressBar(teamsToMutate.Count - i, teamsToMutate.Count, 100);
        if (!progressBar.Equals(lastProgressBar))
        {
          logger?.WriteLine(progressBar);
          lastProgressBar = progressBar;
        }
      }

      logger?.WriteLine($"Finished {nameof(FinaliseTeams)} with {teamsToMutate.Count} entries.");
      return mergeResult;
    }

    public static void CorrectTeamIdsForPlayers(ICollection<Player> incomingPlayers, IDictionary<Guid, Guid> teamsMergeResult, TextWriter? logger = null)
    {
      if (incomingPlayers == null || teamsMergeResult == null || incomingPlayers.Count == 0 || teamsMergeResult.Count == 0) return;

      if (logger != null)
      {
        logger.Write(nameof(CorrectTeamIdsForPlayers));
        logger.Write(" called with ");
        logger.Write(incomingPlayers.Count);
        logger.WriteLine(" entries.");
        logger.WriteLine("Entries: ");
        foreach (var resultPair in teamsMergeResult)
        {
          logger.Write('[');
          logger.Write(resultPair.Key);
          logger.Write("] --> ");
          logger.Write(resultPair.Value);
          logger.WriteLine();
        }
      }

      // For each team, correct the id as specified.
      Parallel.ForEach(incomingPlayers, (importPlayer) =>
      {
        bool hasChanges = false;
        Guid[] teams = importPlayer.Teams.ToArray();
        for (int i = 0; i < teams.Length; i++)
        {
          var thisTeam = teams[i];
          if (teamsMergeResult.ContainsKey(thisTeam))
          {
            teams[i] = teamsMergeResult[thisTeam];
            hasChanges = true;
          }
        }

        if (hasChanges)
        {
          importPlayer.Teams = teams;
        }
      });
    }

    /// <summary>
    /// Merge the loaded players into the current players list.
    /// </summary>
    public static void MergePlayers(ICollection<Player> playersToMutate, IEnumerable<Player> incomingPlayers, TextWriter? logger = null)
    {
      if (playersToMutate == null || incomingPlayers == null) return;

      // Add if the player is new (by name) and assign them with a new id
      // Otherwise, match the found team with its id, based on name.
      ConcurrentBag<Player> playersToAdd = new ConcurrentBag<Player>();
      ConcurrentBag<(Player, Player)> playersToMerge = new ConcurrentBag<(Player, Player)>();

      Parallel.ForEach(incomingPlayers, (importPlayer) =>
      {
        try
        {
          // First, try match through persistent information only.
          // If that doesn't work, try and match a name and same team.
          Player foundPlayer =
            playersToMutate.FirstOrDefault(p => PlayersMatch(importPlayer, p, FilterOptions.Persistent, logger))
            ?? playersToMutate.FirstOrDefault(p => PlayersMatch(importPlayer, p, FilterOptions.Name, logger));

          if (foundPlayer == null)
          {
            playersToAdd.Add(importPlayer);
          }
          else
          {
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
        playersToMutate.Add(importPlayer);
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
    private static Player FindSamePlayerPersistent(IEnumerable<Player> playersToMutate, Player testPlayer, TextWriter? logger = null)
    {
      FilterOptions matchOptions = FilterOptions.None;
      if (!string.IsNullOrEmpty(testPlayer.FriendCode))
      {
        matchOptions |= FilterOptions.FriendCode;
      }
      if (!string.IsNullOrEmpty(testPlayer.DiscordName))
      {
        matchOptions |= FilterOptions.DiscordName;
      }
      if (testPlayer.DiscordId != null)
      {
        matchOptions |= FilterOptions.DiscordId;
      }
      if (testPlayer.Twitch != null)
      {
        matchOptions |= FilterOptions.Twitch;
      }
      if (testPlayer.Twitter != null)
      {
        matchOptions |= FilterOptions.Twitter;
      }
      if (testPlayer.BattlefySlugs?.Count > 0)
      {
        matchOptions |= FilterOptions.BattlefySlugs;
      }
      if (testPlayer.BattlefyUsername != null)
      {
        matchOptions |= FilterOptions.BattlefyUsername;
      }

      return playersToMutate.FirstOrDefault(p => PlayersMatch(testPlayer, p, matchOptions, logger));
    }

    /// <summary>
    /// Merge the loaded teams into the current teams list.
    /// </summary>
    /// <returns>
    /// A dictionary of merged team ids keyed by initial with values of the new id.
    /// </returns>
    public static IDictionary<Guid, Guid> MergeTeamsByPersistentIds(ICollection<Team> teamsToMutate, IEnumerable<Team> incomingTeams)
    {
      ConcurrentDictionary<Guid, Guid> mergeResult = new ConcurrentDictionary<Guid, Guid>();

      if (incomingTeams == null)
      {
        return mergeResult;
      }

      if (teamsToMutate == null)
      {
        teamsToMutate = new List<Team>();
      }

      if (teamsToMutate.Count > 0)
      {
        // Merge teams based on the Battlefy Persistent Id.
        foreach (Team importTeam in incomingTeams)
        {
          if (importTeam.BattlefyPersistentTeamId != null)
          {
            var foundTeam = teamsToMutate.FirstOrDefault(t => importTeam.BattlefyPersistentTeamId.Equals(t?.BattlefyPersistentTeamId));
            if (foundTeam != null)
            {
              MergeExistingTeam(mergeResult, importTeam, foundTeam);
            }
            else
            {
              teamsToMutate.Add(importTeam);
            }
          }
          else
          {
            teamsToMutate.Add(importTeam);
          }
        }
      }
      else
      {
        // No merge required, just take as-is.
        foreach (Team importTeam in incomingTeams)
        {
          teamsToMutate.Add(importTeam);
        }
      }

      return mergeResult;
    }

    private static void MergeExistingTeam(ConcurrentDictionary<Guid, Guid> mergeResult, Team newerTeam, Team olderTeam)
    {
      mergeResult.TryAdd(newerTeam.Id, olderTeam.Id);
      olderTeam.Merge(newerTeam);
    }

    /// <summary>
    /// Get if two Players match.
    /// </summary>
    /// <param name="first">First Player to match</param>
    /// <param name="second">Second Player to match</param>
    /// <param name="matchOptions">How to match</param>
    /// <param name="logger">Logger to write to (or null to not write)</param>
    /// <returns>Players are equal based on the match options</returns>
    public static bool PlayersMatch(Player first, Player second, FilterOptions matchOptions, TextWriter? logger = null)
    {
      // Quick out if they're literally the same.
      if (first.Id == second.Id) return true;

      // Test if the Discord Ids match.
      if ((matchOptions & FilterOptions.DiscordId) != 0 && second.DiscordId?.Equals(first.DiscordId) == true)
      {
        // They do.
        if (logger != null)
        {
          logger.Write(nameof(PlayersMatch));
          logger.Write(": Matched player ");
          logger.Write(first.ToString());
          logger.Write(" (Id ");
          logger.Write(first.Id);
          logger.Write(") with Discord Id ");
          logger.Write(second.DiscordId);
          logger.Write(" from player ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.WriteLine(").");
        }
        return true;
      }

      // Test if the Battlefy Usernames match.
      if ((matchOptions & FilterOptions.BattlefyUsername) != 0 && second.BattlefyUsername?.Equals(first.BattlefyUsername) == true)
      {
        // They do.
        if (logger != null)
        {
          logger.Write(nameof(PlayersMatch));
          logger.Write(": Matched player ");
          logger.Write(first.ToString());
          logger.Write(" (Id ");
          logger.Write(first.Id);
          logger.Write(") with BattlefyUsername ");
          logger.Write(first.BattlefyUsername);
          logger.Write(" from player ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.WriteLine(").");
        }
        return true;
      }

      // Test if any of the Battlefy Slugs match.
      if ((matchOptions & FilterOptions.BattlefySlugs) != 0 && second.BattlefySlugs.Intersect(first.BattlefySlugs).Any())
      {
        // They do.
        if (logger != null)
        {
          logger.Write(nameof(PlayersMatch));
          logger.Write(": Matched player ");
          logger.Write(first.ToString());
          logger.Write(" (Id ");
          logger.Write(first.Id);
          logger.Write(") with test BattlefySlug(s) e.g. ");
          logger.Write(first.BattlefySlugs.FirstOrDefault());
          logger.Write(" with player ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.WriteLine(").");
        }
        return true;
      }

      // Test if the Switch FC's match.
      if ((matchOptions & FilterOptions.FriendCode) != 0 && second.FriendCode?.Equals(first.FriendCode) == true)
      {
        // They do.
        if (logger != null)
        {
          logger.Write(nameof(PlayersMatch));
          logger.Write(": Matched player ");
          logger.Write(first.ToString());
          logger.Write(" (Id ");
          logger.Write(first.Id);
          logger.Write(") with FC ");
          logger.Write(first.FriendCode);
          logger.Write(" from player ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.WriteLine(").");
        }
        return true;
      }

      // Test if the Twitches match.
      if ((matchOptions & FilterOptions.Twitch) != 0 && second.Twitch?.Equals(first.Twitch) == true)
      {
        // They do.
        if (logger != null)
        {
          logger.Write(nameof(PlayersMatch));
          logger.Write(": Matched player ");
          logger.Write(first.ToString());
          logger.Write(" (Id ");
          logger.Write(first.Id);
          logger.Write(") with Twitch ");
          logger.Write(first.Twitch);
          logger.Write(" from player ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.WriteLine(").");
        }
        return true;
      }

      // Test if the Twitters match.
      if ((matchOptions & FilterOptions.Twitter) != 0 && second.Twitter?.Equals(first.Twitter) == true)
      {
        // They do.
        if (logger != null)
        {
          logger.Write(nameof(PlayersMatch));
          logger.Write(": Matched player ");
          logger.Write(first.ToString());
          logger.Write(" (Id ");
          logger.Write(first.Id);
          logger.Write(") with Twitter ");
          logger.Write(first.Twitter);
          logger.Write(" from player ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.WriteLine(").");
        }
        return true;
      }

      // Test if the Discord names match.
      if ((matchOptions & FilterOptions.DiscordName) != 0 && second.DiscordName?.Equals(first.DiscordName) == true)
      {
        // They do.
        if (logger != null)
        {
          logger.Write(nameof(PlayersMatch));
          logger.Write(": Matched player ");
          logger.Write(first.ToString());
          logger.Write(" (Id ");
          logger.Write(first.Id);
          logger.Write(") with Discord name ");
          logger.Write(first.DiscordName);
          logger.Write(" from player ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.WriteLine(").");
        }
        return true;
      }

      if ((matchOptions & FilterOptions.Name) != 0
        && first.Teams.Intersect(second.Teams).Any()
        && first.TransformedNames.Intersect(second.TransformedNames).Any())
      {
        if (logger != null)
        {
          logger.Write(nameof(PlayersMatch));
          logger.Write(": Matched player ");
          logger.Write(first.ToString());
          logger.Write(" (Id ");
          logger.Write(first.Id);
          logger.Write(") with transformed names [");
          logger.Write(string.Join(", ", first.TransformedNames));
          logger.Write("] from player ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.Write(") with transformed names [");
          logger.Write(string.Join(", ", second.TransformedNames));
          logger.WriteLine("].");
        }
        return true;
      }

      return false;
    }

    /// <summary>
    /// Get if two Teams match.
    /// This is implemented as:
    /// - BattlefyPersistentIds match, or
    /// - The names are (roughly) the same AND the teams have AT LEAST TWO players the same.
    /// </summary>
    /// <param name="first">First Team to match</param>
    /// <param name="second">Second Team to match</param>
    /// <param name="logger">Logger to write to (or null to not write)</param>
    /// <returns>Teams match</returns>
    public static bool TeamsMatch(IReadOnlyCollection<Player> allPlayers, Team first, Team second, TextWriter? logger = null)
    {
      // Quick out if they're literally the same.
      if (first.Id == second.Id) return true;

      // Get if the Battlefy Ids match.
      if (first.BattlefyPersistentTeamId != null && first.BattlefyPersistentTeamId == second.BattlefyPersistentTeamId)
      {
        // They do.
        if (logger != null)
        {
          logger.Write(nameof(TeamsMatch));
          logger.Write(": Matched team ");
          logger.Write(first.ToString());
          logger.Write(" (Id ");
          logger.Write(first.Id);
          logger.Write(") with BattlefyPersistentTeamId ");
          logger.Write(first.BattlefyPersistentTeamId);
          logger.Write(" from team ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.WriteLine(").");
        }
        return true;
      }

      var matchedTransformedNames = first.TransformedNames.Intersect(second.TransformedNames);
      if (matchedTransformedNames.Any())
      {
        // They do.
        int sharedPlayersCount = 0;
        var firstPlayers = first.GetPlayers(allPlayers);
        var secondPlayers = second.GetPlayers(allPlayers);
        foreach (var firstPlayer in firstPlayers)
        {
          foreach (var secondPlayer in secondPlayers)
          {
            if (PlayersMatch(firstPlayer, secondPlayer, FilterOptions.Default))
            {
              ++sharedPlayersCount;
            }
          }
        }

        if (logger != null)
        {
          logger.Write(nameof(TeamsMatch));
          logger.Write(": Matched team ");
          logger.Write(first.ToString());
          logger.Write(" (Id ");
          logger.Write(first.Id);
          logger.Write(") with TransformedNames [");
          logger.Write(string.Join(", ", matchedTransformedNames));
          logger.Write("] from team ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.Write(") with ");
          logger.Write(sharedPlayersCount);
          logger.WriteLine(" shared player(s).");
        }

        if (sharedPlayersCount >= 2)
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Adds a new player to the players list with respect to their team and its tag.
    /// Returns the player that was added (or null if playername was null or empty).
    /// </summary>
    /// <param name="playerName">The player's name on the roster</param>
    /// <param name="teamTag">The team's tag</param>
    /// <param name="transformedTag"></param>
    /// <param name="newTeam"></param>
    /// <param name="players"></param>
    /// <param name="source"></param>
    public static Player? AddPlayerFromTag(string? playerName, string teamTag, string transformedTag, Team newTeam, List<Player> players, string source)
    {
      if (playerName != null && !string.IsNullOrWhiteSpace(playerName))
      {
        playerName = playerName.Trim();
        switch (newTeam.ClanTagOption)
        {
          case TagOption.Front:
            if (playerName.StartsWith(teamTag, StringComparison.OrdinalIgnoreCase))
            {
              playerName = playerName.Substring(teamTag.Length).Trim();
            }
            else if (playerName.StartsWith(transformedTag, StringComparison.OrdinalIgnoreCase))
            {
              playerName = playerName.Substring(transformedTag.Length).Trim();
            }
            break;

          case TagOption.Back:
            if (playerName.EndsWith(teamTag, StringComparison.OrdinalIgnoreCase))
            {
              playerName = playerName.Substring(0, playerName.Length - teamTag.Length).Trim();
            }
            else if (playerName.EndsWith(transformedTag, StringComparison.OrdinalIgnoreCase))
            {
              playerName = playerName.Substring(0, playerName.Length - transformedTag.Length).Trim();
            }
            break;

          case TagOption.Surrounding:
            if (playerName.StartsWith(teamTag[0].ToString(), StringComparison.OrdinalIgnoreCase) && playerName.EndsWith(teamTag[1].ToString(), StringComparison.OrdinalIgnoreCase))
            {
              playerName = playerName.Substring(1, playerName.Length - 2).Trim();
            }
            else if (playerName.StartsWith(transformedTag[0].ToString(), StringComparison.OrdinalIgnoreCase) && playerName.EndsWith(transformedTag[1].ToString(), StringComparison.OrdinalIgnoreCase))
            {
              playerName = playerName.Substring(1, playerName.Length - 2).Trim();
            }
            break;

          default:
            // Just leave it.
            break;
        }

        var p = new Player
        {
          CurrentTeam = newTeam.Id,
          Name = playerName,
          Sources = new string[] { source }
        };
        players.Add(p);
        return p;
      }
      else
      {
        return null;
      }
    }
  }
}