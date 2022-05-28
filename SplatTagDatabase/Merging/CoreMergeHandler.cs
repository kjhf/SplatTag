using NLog;
using SplatTagCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SplatTagDatabase.Merging
{
  public class CoreMergeHandler : ICoreMergeHandler
  {
    internal const int PARALLEL_THRESHOLD = 15;
    private const int MAX_FINALISE_LOOPS = 10;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private readonly List<Player> _players = new();
    private readonly List<Team> _teams = new();
    private readonly IdMigrationHandler _migrationHandler = new();

    public CoreMergeResults MergeSource(Source source) => MergeOnce(source.Players, source.Teams);

    public CoreMergeResults[] MergeKnown()
    {
      List<CoreMergeResults> finalResults = new(capacity: MAX_FINALISE_LOOPS);
      logger.ConditionalTrace($"{nameof(MergeKnown)} called with {_players.Count} players and {_teams.Count} teams.");
      try
      {
        bool loop = true;
        for (int iteration = 1; loop; iteration++)
        {
          if (iteration >= MAX_FINALISE_LOOPS)
          {
            string error = $"{nameof(MergeKnown)}: Too many iterations. Aborting.";
            logger.Error(error);
            throw new NotImplementedException(error);
          }

          logger.Info($"Performing final merge (iteration #{iteration})...");
          var results = MergeOnce(_players.ToArray(), _teams.ToArray());
          finalResults.Add(results);
          loop = results.AnyMerged;
          logger.Trace($"{nameof(MergeKnown)}: AnyMerged={results.AnyMerged}, Count={results.Count}, Added={results.AddedRecords.Count()}, Merged={results.MergedRecords.Count()}.");
        }
      }
      catch (Exception ex)
      {
        logger.Warn(ex, $"Failed {nameof(MergeKnown)}. Continuing anyway. {ex}");
      }

      return finalResults.ToArray();
    }

    internal CoreMergeResults AddPlayers(ICollection<Player> players) => MergeOnce(players.ToArray(), null);

    internal CoreMergeResults AddTeams(ICollection<Team> teams) => MergeOnce(null, teams.ToArray());

    private CoreMergeResults MergeOnce(ICollection<Player>? players, ICollection<Team>? teams)
    {
      CoreMergeResults? playerResults = null;
      CoreMergeResults? teamResults = null;

      if (players != null)
      {
        playerResults = PrepMergePlayers(players);
        PerformPreppedMerge(playerResults);
      }

      if (teams != null)
      {
        teamResults = PrepMergeTeams(teams);
        PerformPreppedMerge(teamResults);
      }

      return new CoreMergeResults(playerResults, teamResults);
    }

    private CoreMergeResults PrepMergePlayers(ICollection<Player> players)
    {
      if (players.Count == 0)
      {
        return new CoreMergeResults(Array.Empty<MergeRecord>());
      }

      return players.Count > PARALLEL_THRESHOLD ?
        PrepMergePlayersParallel(players) :
        PrepMergePlayersSerial(players);
    }

    private CoreMergeResults PrepMergePlayersSerial(IEnumerable<Player> players)
    {
      var records = new List<MergeRecord>();

      foreach (var player in players)
      {
        records.Add(TryFindPlayer(player, _players));
      }

      return new CoreMergeResults(records);
    }

    private CoreMergeResults PrepMergePlayersParallel(IEnumerable<Player> players)
    {
      var records = new ConcurrentBag<MergeRecord>();

      Parallel.ForEach(players, player =>
      {
        records.Add(TryFindPlayer(player, _players));
      });

      return new CoreMergeResults(records);
    }

    private CoreMergeResults PrepMergeTeams(ICollection<Team> teams)
    {
      if (teams.Count == 0)
      {
        return new CoreMergeResults(Array.Empty<MergeRecord>());
      }

      return teams.Count > PARALLEL_THRESHOLD ?
        PrepMergeTeamsParallel(teams) :
        PrepMergeTeamsSerial(teams);
    }

    private CoreMergeResults PrepMergeTeamsSerial(IEnumerable<Team> teams)
    {
      var records = new List<MergeRecord>();

      foreach (var team in teams)
      {
        records.Add(TryFindTeam(team, _teams, _players));
      }

      return new CoreMergeResults(records);
    }

    private CoreMergeResults PrepMergeTeamsParallel(IEnumerable<Team> teams)
    {
      var records = new ConcurrentBag<MergeRecord>();

      Parallel.ForEach(teams, team =>
      {
        records.Add(TryFindTeam(team, _teams, _players));
      });

      return new CoreMergeResults(records);
    }

    private bool PerformPreppedMerge(CoreMergeResults prep)
    {
      // First, prep the migrations (and the final ids for the results)
      _migrationHandler.PrepMigrations(prep);

      foreach (var record in prep.MergedRecords)
      {
        record.PerformMerge();

        var discardedItem = record.MergedItem;
        if (discardedItem is Player player)
        {
          _players.Remove(player);
        }
        else if (discardedItem is Team team)
        {
          _teams.Remove(team);
        }
        else
        {
          throw new InvalidOperationException("Unknown item type: " + discardedItem?.GetType().Name);
        }
      }

      foreach (var item in prep.AddedItems)
      {
        AddItem(item);
      }

      bool hasTeamMigration = _migrationHandler.PerformMigration(_players);
      logger.ConditionalDebug($"{nameof(PerformPreppedMerge)}: Finished prepped merge -- " +
        $"{prep.DiscardedItems.Count()} discarded items, " +
        $"{prep.MergedRecords.Count()} merged items, " +
        $"{prep.AddedItems.Count()} added items. " +
        $"hasTeamMigration={hasTeamMigration}. " +
        $"Now {_players.Count} players, {_teams.Count} teams.");
      return hasTeamMigration;

      void AddItem(ISplatTagCoreObject item)
      {
        if (item is Player player)
        {
          _players.Add(player);
        }
        else if (item is Team team)
        {
          _teams.Add(team);
        }
        else
        {
          throw new InvalidOperationException("Unknown item type: " + item.GetType().Name);
        }
      }
    }

    /// <summary>
    /// Get the merge record for the incoming Player. The MergeRecord has IsMerge set if successful.
    /// </summary>
    private static MergeRecord TryFindPlayer(Player incomingPlayer, IReadOnlyCollection<Player> reference) => TryFindMergable(incomingPlayer, reference, null);

    /// <summary>
    /// Get the merge record for the incoming Player. The MergeRecord has IsMerge set if successful.
    /// </summary>
    private static MergeRecord TryFindTeam(Team incomingTeam, IReadOnlyCollection<Team> reference, IReadOnlyCollection<Player>? knownPlayers) => TryFindMergable(incomingTeam, reference, knownPlayers);

    /// <summary>
    /// Get the merge record for the incoming item. The MergeRecord has IsMerge set if successful.
    /// </summary>
    private static MergeRecord TryFindMergable(ISplatTagCoreObject incomingItem, IReadOnlyCollection<ISplatTagCoreObject> reference, IReadOnlyCollection<Player>? knownPlayers)
    {
      // Shortcut if nothing to reference.
      if (reference.Count <= 1)
      {
        logger.ConditionalTrace($"Not merging the record {incomingItem.Id} because reference is not populated.");
        return MergeRecord.CreateMergeRecordForAddedItem(incomingItem);
      }

      var orderedResults = reference.Where(i => incomingItem.Id != i.Id).GroupBy(i => i.MatchWithReason(incomingItem).ToSummedWeight()).OrderByDescending(group => group.Key);
      var bestResultGroup = orderedResults.First();
      var weighting = bestResultGroup.Key;
      foreach (var bestResult in bestResultGroup)
      {
        var bestReason = bestResult.MatchWithReason(incomingItem);
        if (weighting > FilterOptionWeights.MergableThresholdWeight)
        {
          return MergeRecord.CreateMergeRecordForMergedItems(bestResult, incomingItem, bestReason);
        }

        if (knownPlayers != null && bestReason.HasFlag(FilterOptions.TeamName) && incomingItem is Team t1 && bestResult is Team t2)
        {
          bool matched = Matcher.TeamsMatch(knownPlayers, t1, t2);
          if (matched)
          {
            return MergeRecord.CreateMergeRecordForMergedItems(bestResult, incomingItem, bestReason);
          }
          // else loop
        }
      }
      logger.ConditionalTrace($"Not merging the record {incomingItem} ({incomingItem.Id}) because the best item(s) for it, " +
        $"e.g. {bestResultGroup.FirstOrDefault()} scoring {weighting} did not match the threshold (< {FilterOptionWeights.MergableThresholdWeight}) or Team Name.");
      return MergeRecord.CreateMergeRecordForAddedItem(incomingItem);
    }
  }
}