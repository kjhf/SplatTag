﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SplatTagCore
{
  /// <summary>
  /// Matching and equality functions
  /// </summary>
  /// <remarks>
  /// Note that IEnuerable has a optimisation where First() will use [0] if the collection is an IList.
  /// </remarks>
  public static class Matcher
  {
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

      // Test if any of the Battlefy Persistent Ids match.
      if ((matchOptions & FilterOptions.BattlefyPersistentIds) != 0 && first.Battlefy.MatchPersistent(second.Battlefy))
      {
        // They do.
        if (logger != null)
        {
          logger.Write(nameof(PlayersMatch));
          logger.Write(": Matched player ");
          logger.Write(first.ToString());
          logger.Write(" (Id ");
          logger.Write(first.Id);
          logger.Write(") with Battlefy Persistent Id(s) [");
          logger.Write(string.Join(", ", first.Battlefy.PersistentIds));
          logger.Write("] from player ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.WriteLine(").");
        }
        return true;
      }

      // Test if the Battlefy Usernames match.
      if ((matchOptions & FilterOptions.BattlefyUsername) != 0 && first.Battlefy.MatchUsernames(second.Battlefy))
      {
        // They do.
        if (logger != null)
        {
          logger.Write(nameof(PlayersMatch));
          logger.Write(": Matched player ");
          logger.Write(first.ToString());
          logger.Write(" (Id ");
          logger.Write(first.Id);
          logger.Write(") with BattlefyUsername(s) [");
          logger.Write(string.Join(", ", first.Battlefy.Usernames));
          logger.Write("] from player ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.WriteLine(").");
        }
        return true;
      }

      // Test if any of the Battlefy Slugs match.
      if ((matchOptions & FilterOptions.BattlefySlugs) != 0 && first.Battlefy.MatchSlugs(second.Battlefy))
      {
        // They do.
        if (logger != null)
        {
          logger.Write(nameof(PlayersMatch));
          logger.Write(": Matched player ");
          logger.Write(first.ToString());
          logger.Write(" (Id ");
          logger.Write(first.Id);
          logger.Write(") with Battlefy Slugs(s) [");
          logger.Write(string.Join(", ", first.Battlefy.Slugs));
          logger.Write("] from player ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.WriteLine(").");
        }
        return true;
      }

      // Test if the Switch FC's match.
      if ((matchOptions & FilterOptions.FriendCode) != 0 && first.FCInformation.Match(second.FCInformation))
      {
        // They do.
        if (logger != null)
        {
          logger.Write(nameof(PlayersMatch));
          logger.Write(": Matched player ");
          logger.Write(first.ToString());
          logger.Write(" (Id ");
          logger.Write(first.Id);
          logger.Write(") with Friend Code(s) [");
          logger.Write(string.Join(", ", first.FCInformation.GetCodesUnordered()));
          logger.Write("] from player ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.WriteLine(").");
        }
        return true;
      }

      // Test if the Twitches match.
      if ((matchOptions & FilterOptions.Twitch) != 0 && first.TwitchInformation.Match(second.TwitchInformation))
      {
        // They do.
        if (logger != null)
        {
          logger.Write(nameof(PlayersMatch));
          logger.Write(": Matched player ");
          logger.Write(first.ToString());
          logger.Write(" (Id ");
          logger.Write(first.Id);
          logger.Write(") with Twitch(es) [");
          logger.Write(string.Join(", ", first.TwitchInformation.GetItemsUnordered()));
          logger.Write("] from player ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.WriteLine(").");
        }
        return true;
      }

      // Test if the Twitters match.
      if ((matchOptions & FilterOptions.Twitter) != 0 && first.TwitterInformation.Match(second.TwitterInformation))
      {
        // They do.
        if (logger != null)
        {
          logger.Write(nameof(PlayersMatch));
          logger.Write(": Matched player ");
          logger.Write(first.ToString());
          logger.Write(" (Id ");
          logger.Write(first.Id);
          logger.Write(") with Twitter(s) [");
          logger.Write(string.Join(", ", first.TwitterInformation.GetItemsUnordered()));
          logger.Write("] from player ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.WriteLine(").");
        }
        return true;
      }

      // Test if the Discord Ids match.
      if ((matchOptions & FilterOptions.DiscordId) != 0 && first.Discord.MatchPersistent(second.Discord))
      {
        // They do.
        if (logger != null)
        {
          logger.Write(nameof(PlayersMatch));
          logger.Write(": Matched player ");
          logger.Write(first.ToString());
          logger.Write(" (Id ");
          logger.Write(first.Id);
          logger.Write(") with Discord Id(s) [");
          logger.Write(string.Join(", ", first.DiscordIds));
          logger.Write("] from player ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.WriteLine(").");
        }
        return true;
      }

      // Test if the Discord names match.
      if ((matchOptions & FilterOptions.DiscordName) != 0 && first.Discord.MatchUsernames(second.Discord))
      {
        // They do.
        if (logger != null)
        {
          logger.Write(nameof(PlayersMatch));
          logger.Write(": Matched player ");
          logger.Write(first.ToString());
          logger.Write(" (Id ");
          logger.Write(first.Id);
          logger.Write(") with Discord Names [");
          logger.Write(string.Join(", ", first.DiscordNames));
          logger.Write("] from player ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.WriteLine(").");
        }
        return true;
      }

      // If we're matching by name, then the player must also have a matching battlefy slug, or matching team.
      if ((matchOptions & FilterOptions.PlayerName) != 0
        && (first.TeamInformation.Match(second.TeamInformation) || first.Battlefy.MatchSlugs(second.Battlefy))
        && first.AllKnownNames.TransformedNamesMatch(second.AllKnownNames))
      {
        if (logger != null)
        {
          var teamMatch = first.TeamInformation.Match(second.TeamInformation);
          var battlefyMatch = first.Battlefy.MatchSlugs(second.Battlefy);

          logger.Write(nameof(PlayersMatch));
          logger.Write(": Matched player ");
          logger.Write(first.ToString());
          logger.Write(" (Id ");
          logger.Write(first.Id);
          logger.Write(") with TransformedNames [");
          logger.Write(string.Join(", ", first.NamesInformation.TransformedNames));
          logger.Write("] using");
          logger.Write(" teamMatch=");
          logger.Write(teamMatch);
          logger.Write(" battlefyMatch=");
          logger.Write(battlefyMatch);
          logger.Write(" from player ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.Write(") with TransformedNames [");
          logger.Write(string.Join(", ", second.NamesInformation.TransformedNames));
          logger.WriteLine("].");
        }
        return true;
      }

      return false;
    }

    /// <summary>
    /// Get if this collection matches a second by the Name.
    /// Matches by Ordinal Ignore Case by default.
    /// </summary>
    public static bool NamesMatch(this IReadOnlyCollection<Name> first, IReadOnlyCollection<Name> second, StringComparer? stringComparison = null)
    {
      int firstCount = first.Count;
      int secondCount = second.Count;

      if (firstCount == 0)
      {
        return false;
      }
      else if (secondCount == 0)
      {
        return false;
      }
      else
      {
        stringComparison ??= StringComparer.OrdinalIgnoreCase;

        if (firstCount == 1 && secondCount == 1)
        {
          return stringComparison.Equals(first.First().Value, second.First().Value);
        }
        else if (firstCount == 1)
        {
          return second.Select(n => n.Value).Contains(first.First().Value, stringComparison);
        }
        else if (secondCount == 1)
        {
          return first.Select(n => n.Value).Contains(second.First().Value, stringComparison);
        }
        else
        {
          return first.Select(n => n.Value).Intersect(second.Select(n => n.Value), stringComparison).Any();
        }
      }
    }

    /// <summary>
    /// Get if this collection matches a second by the transformed names.
    /// Matches by Ordinal by default.
    /// </summary>
    /// <remarks>
    /// Highly optimised method as this is called literally millions of times during matching
    /// </remarks>
    public static bool TransformedNamesMatch(this IReadOnlyCollection<Name> first, IReadOnlyCollection<Name> second, StringComparer? stringComparison = null)
    {
      int firstCount = first.Count;
      int secondCount = second.Count;

      if (firstCount == 0)
      {
        return false;
      }
      else if (secondCount == 0)
      {
        return false;
      }
      else
      {
        stringComparison ??= StringComparer.OrdinalIgnoreCase;

        if (firstCount == 1 && secondCount == 1)
        {
          return stringComparison.Equals(first.First().Transformed, second.First().Transformed);
        }
        else if (firstCount == 1)
        {
          return second.Select(n => n.Transformed).Contains(first.First().Transformed, stringComparison);
        }
        else if (secondCount == 1)
        {
          return first.Select(n => n.Transformed).Contains(second.First().Transformed, stringComparison);
        }
        else
        {
          return first.Select(n => n.Transformed).Intersect(second.Select(n => n.Transformed), stringComparison).Any();
        }
      }
    }

    /// <summary>
    /// Count matches between <see cref="Name"/>s of first and second.
    /// Matches by Ordinal Ignore Case by default.
    /// </summary>
    public static int NamesMatchCount(IReadOnlyCollection<Name> first, IReadOnlyCollection<Name> second, StringComparer? stringComparison = null)
    {
      if (first.Count == 0 || second.Count == 0) return 0;

      stringComparison ??= StringComparer.OrdinalIgnoreCase;
      if (first.Count == 1)
      {
        return (second.Count == 1)
          ? stringComparison.Equals(first.First().Value, second.First().Value) ? 1 : 0
          : second.Select(n => n.Value).Contains(first.First().Value, stringComparison) ? 1 : 0;
      }
      else if (second.Count == 1)
      {
        return first.Select(n => n.Value).Contains(second.First().Value, stringComparison) ? 1 : 0;
      }
      else
      {
        return first.Select(n => n.Value).Intersect(second.Select(n => n.Value), stringComparison).Count();
      }
    }

    /// <summary>
    /// Get if matches between lists of first and second with a default comparison.
    /// </summary>
    /// <remarks>
    /// This is a lot more performant for lists with 0 or 1 object in it rather than creating Intersect HashMaps.
    /// </remarks>
    public static bool GenericMatch<T>(this IReadOnlyCollection<T> first, IReadOnlyCollection<T> second) where T : notnull
    {
      int firstCount = first.Count;
      int secondCount = second.Count;

      if (firstCount == 0 || secondCount == 0)
      {
        return false;
      }
      else if (firstCount == 1 && secondCount == 1)
      {
        return first.First().Equals(second.First());
      }
      else if (firstCount == 1)
      {
        return second.Contains(first.First());
      }
      else if (secondCount == 1)
      {
        return first.Contains(second.First());
      }
      else if (firstCount > 4 || secondCount > 4)
      {
        return first.Intersect(second).Any();
      }
      else
      {
        foreach (var obj in first)
        {
          if (second.Contains(obj))
          {
            return true;
          }
        }
        return false;
      }
    }

    /// <summary>
    /// Get if any matches between <see cref="string"/>s of first and second.
    /// Matches by OrdinalIgnoreCase by default.
    /// </summary>
    public static bool StringMatch(this IReadOnlyCollection<string> first, IReadOnlyCollection<string> second, StringComparer? stringComparison = null)
    {
      if (first.Count == 0 || second.Count == 0) return false;

      stringComparison ??= StringComparer.OrdinalIgnoreCase;
      if (first.Count == 1)
      {
        return (second.Count == 1)
          ? stringComparison.Equals(first.First(), second.First())
          : second.Contains(first.First(), stringComparison);
      }
      else if (second.Count == 1)
      {
        return first.Contains(second.First(), stringComparison);
      }
      else if (first.Count > 4 || second.Count > 4)
      {
        return first.Intersect(second, stringComparison).Any();
      }
      else
      {
        foreach (var obj in first)
        {
          if (second.Contains(obj, stringComparison))
          {
            return true;
          }
        }
        return false;
      }
    }

    /// <summary>
    /// Get if any matches between <see cref="string"/>s of first and second.
    /// </summary>
    public static bool StringMatch(IEnumerable<string> first, IEnumerable<string> second, StringComparer stringComparison) => first.Intersect(second, stringComparison).Any();

    /// <summary>
    /// Get if two Teams match through ids.
    /// This is implemented as:
    /// - Ids match or BattlefyPersistentIds match.
    /// </summary>
    /// <param name="first">First Team to match</param>
    /// <param name="second">Second Team to match</param>
    /// <param name="logger">Logger to write to (or null to not write)</param>
    /// <returns>Teams match</returns>
    public static bool TeamsMatch(Team first, Team second, TextWriter? logger = null)
    {
      // Quick out if they're literally the same.
      if (first.Id == second.Id) return true;

      // Get if the Battlefy Ids match.
      if (first.BattlefyPersistentTeamId != null && first.BattlefyPersistentTeamIds.NamesMatch(second.BattlefyPersistentTeamIds))
      {
        // They do.
        if (logger != null)
        {
          logger.Write(nameof(TeamsMatch));
          logger.Write(": Matched team ");
          logger.Write(first.ToString());
          logger.Write(" (Id ");
          logger.Write(first.Id);
          logger.Write(") with BattlefyPersistentTeamIds e.g. ");
          logger.Write(first.BattlefyPersistentTeamId);
          logger.Write(" from team ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.WriteLine(").");
        }
        return true;
      }

      return false;
    }

    /// <summary>
    /// Get if two Teams match.
    /// This is implemented as:
    /// - BattlefyPersistentIds match, or
    /// - The names are (roughly) the same AND the teams have AT LEAST TWO players that match.
    /// </summary>
    /// <param name="allPlayers">Collection of all players to help base team equality</param>
    /// <param name="first">First Team to match</param>
    /// <param name="second">Second Team to match</param>
    /// <param name="logger">Logger to write to (or null to not write)</param>
    /// <returns>Teams match</returns>
    public static bool TeamsMatch(IReadOnlyCollection<Player> allPlayers, Team first, Team second, TextWriter? logger = null)
    {
      // Get if ids match first.
      if (TeamsMatch(first, second, logger)) return true;

      // Otherwise, test if players match.
      if (first.NamesInformation.TransformedNamesMatch(second.NamesInformation))
      {
        // They do.
        if (logger != null)
        {
          logger.Write(nameof(TeamsMatch));
          logger.Write(": Matched team ");
          logger.Write(first.ToString());
          logger.Write(" (Id ");
          logger.Write(first.Id);
          logger.Write(") from its TransformedNames from team ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.WriteLine(")");
        }

        int sharedPlayersCount = 0;
        var firstPlayers = first.GetPlayers(allPlayers);
        var secondPlayers = second.GetPlayers(allPlayers);
        foreach (var firstPlayer in firstPlayers)
        {
          string[] firstPlayerNamesWithTag = first.ClanTags.SelectMany(tag => firstPlayer.NamesInformation.TransformedNames.Select(n => tag.CombineToPlayer(n))).ToArray();

          foreach (var secondPlayer in secondPlayers)
          {
            var secondPlayerNamesWithTag = second.ClanTags.SelectMany(tag => secondPlayer.NamesInformation.TransformedNames.Select(n => tag.CombineToPlayer(n)));

            if (StringMatch(firstPlayer.NamesInformation.TransformedNames.Concat(firstPlayerNamesWithTag), secondPlayer.NamesInformation.TransformedNames.Concat(secondPlayerNamesWithTag), StringComparer.OrdinalIgnoreCase))
            {
              ++sharedPlayersCount;

              if (sharedPlayersCount >= 2)
              {
                logger?.WriteLine("Shared players requirement met.");
                return true;
              }
            }
          }
        }

        logger?.WriteLine($"Shared players requirement NOT met.\nFirst: {first}, with players: {string.Join(", ", firstPlayers)}  [{string.Join(", ", first.Sources)}]\nSecond: {second}, with players: {string.Join(", ", secondPlayers)}  [{string.Join(", ", second.Sources)}]");
      }

      return false;
    }
  }
}