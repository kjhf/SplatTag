using NLog;
using System;
using System.Collections.Generic;
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
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    ///// <summary>
    ///// Get if two Players match.
    ///// </summary>
    ///// <param name="first">First Player to match</param>
    ///// <param name="second">Second Player to match</param>
    ///// <param name="matchOptions">How to match</param>
    /////
    ///// <returns>Players are equal based on the match options</returns>
    //public static bool PlayersMatch(Player first, Player second, FilterOptions matchOptions)
    //{
    //  // Quick out if they're literally the same.
    //  if (first.Id == second.Id) return true;

    //  // Test if any of the Battlefy Persistent Ids match.
    //  if ((matchOptions & FilterOptions.BattlefyPersistentIds) != 0 && first.BattlefyInformation.MatchPersistent(second.BattlefyInformation))
    //  {
    //    // They do.
    //    logger.ConditionalDebug($"{nameof(PlayersMatch)}: Matched player {first} (Id {first.Id}) with Battlefy Persistent Id(s) [{string.Join(", ", first.BattlefyInformation.PersistentIds)}] from player {second} (Id {second.Id}).");
    //    return true;
    //  }

    //  // Test if the Battlefy Usernames match.
    //  if ((matchOptions & FilterOptions.BattlefyUsername) != 0 && first.BattlefyInformation.MatchUsernames(second.BattlefyInformation))
    //  {
    //    // They do.
    //    logger.ConditionalDebug($"{nameof(PlayersMatch)}: Matched player {first} (Id {first.Id}) with BattlefyUsername(s) [{string.Join(", ", first.BattlefyInformation.Usernames)}] from player {second} (Id {second.Id}).");
    //    return true;
    //  }

    //  // Test if any of the Battlefy Slugs match.
    //  if ((matchOptions & FilterOptions.BattlefySlugs) != 0 && first.BattlefyInformation.MatchSlugs(second.BattlefyInformation))
    //  {
    //    // They do.
    //    logger.ConditionalDebug($"{nameof(PlayersMatch)}: Matched player {first} (Id {first.Id}) with Battlefy Slug(s) [{string.Join(", ", first.BattlefyInformation.Slugs)}] from player {second} (Id {second.Id}).");
    //    return true;
    //  }

    //  // Test if the Switch FC's match.
    //  if ((matchOptions & FilterOptions.FriendCode) != 0 && first.FCInformation.ItemsMatch(second.FriendCodesInformation))
    //  {
    //    // They do.
    //    logger.ConditionalDebug($"{nameof(PlayersMatch)}: Matched player {first} (Id {first.Id}) with Friend Code(s) [{string.Join(", ", first.FCInformation.GetCodesUnordered())}] from player {second} (Id {second.Id}).");
    //    return true;
    //  }

    //  // Test if the Twitches match.
    //  if ((matchOptions & FilterOptions.Twitch) != 0 && first.TwitchInformation.ItemsMatch(second.TwitchInformation))
    //  {
    //    // They do.
    //    logger.ConditionalDebug($"{nameof(PlayersMatch)}: Matched player {first} (Id {first.Id}) with Twitch(es) [{string.Join(", ", first.TwitchInformation.GetItemsUnordered())}] from player {second} (Id {second.Id}).");
    //    return true;
    //  }

    //  // Test if the Twitters match.
    //  if ((matchOptions & FilterOptions.Twitter) != 0 && first.TwitterInformation.ItemsMatch(second.TwitterInformation))
    //  {
    //    // They do.
    //    logger.ConditionalDebug($"{nameof(PlayersMatch)}: Matched player {first} (Id {first.Id}) with Twitter(es) [{string.Join(", ", first.TwitterInformation.GetItemsUnordered())}] from player {second} (Id {second.Id}).");
    //    return true;
    //  }

    //  // Test if the Sendous match.
    //  if ((matchOptions & FilterOptions.PlayerSendou) != 0 && first.SendouInformation.ItemsMatch(second.SendouInformation))
    //  {
    //    // They do.
    //    logger.ConditionalDebug($"{nameof(PlayersMatch)}: Matched player {first} (Id {first.Id}) with Sendou(s) [{string.Join(", ", first.SendouInformation.GetItemsUnordered())}] from player {second} (Id {second.Id}).");
    //    return true;
    //  }

    //  // Test if the Discord Ids match.
    //  if ((matchOptions & FilterOptions.DiscordId) != 0 && first.DiscordInformation.MatchPersistent(second.DiscordInformation))
    //  {
    //    // They do.
    //    logger.ConditionalDebug($"{nameof(PlayersMatch)}: Matched player {first} (Id {first.Id}) with Discord Id(s) [{string.Join(", ", first.DiscordIds)}] from player {second} (Id {second.Id}).");
    //    return true;
    //  }

    //  // Test if the Discord names match.
    //  if ((matchOptions & FilterOptions.DiscordName) != 0 && first.DiscordInformation.MatchUsernames(second.DiscordInformation))
    //  {
    //    // They do.
    //    logger.ConditionalDebug($"{nameof(PlayersMatch)}: Matched player {first} (Id {first.Id}) with Discord Name(s) [{string.Join(", ", first.DiscordNames)}] from player {second} (Id {second.Id}).");
    //    return true;
    //  }

    //  // Test if the weapons match.
    //  if ((matchOptions & FilterOptions.Weapon) != 0 && first.WeaponsInformation.ItemsMatch(second.WeaponsInformation))
    //  {
    //    // They do.
    //    logger.ConditionalDebug($"{nameof(PlayersMatch)}: Matched player {first} (Id {first.Id}) with Weapon(s) [{string.Join(", ", first.WeaponsInformation.GetItemsUnordered())}] from player {second} (Id {second.Id}).");
    //    return true;
    //  }

    //  // If we're matching by name, then the player must also have a matching battlefy slug, or matching team.
    //  if ((matchOptions & FilterOptions.PlayerName) != 0
    //    && (first.TeamInformation.ItemsMatch(second.TeamInformation) || first.BattlefyInformation.MatchSlugs(second.BattlefyInformation))
    //    && first.AllKnownNames.TransformedNamesMatch(second.AllKnownNames))
    //  {
    //    if (logger != null)
    //    {
    //      var teamMatch = first.TeamInformation.ItemsMatch(second.TeamInformation);
    //      var battlefyMatch = first.BattlefyInformation.MatchSlugs(second.BattlefyInformation);

    //      logger.ConditionalDebug($"{nameof(PlayersMatch)}: Matched player {first} (Id {first.Id}) with TransformedName(s) [{string.Join(", ", first.NamesInformation.TransformedNames)}] " +
    //        $"using teamMatch={teamMatch} battlefyMatch={battlefyMatch} " +
    //        $"from player {second} (Id {second.Id}) with TransformedName(s) [{string.Join(", ", second.NamesInformation.TransformedNames)}]. ");
    //    }
    //    return true;
    //  }

    //  return false;
    //}

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
    ///
    /// <returns>Teams match</returns>
    public static bool TeamsMatchById(Team first, Team second)
    {
      // Quick out if they're literally the same.
      if (first.Id == second.Id) return true;

      // Get if the Battlefy Ids match.
      if (NamesMatch(first.BattlefyPersistentTeamIds, second.BattlefyPersistentTeamIds))
      {
        // They do.
        logger.ConditionalDebug($"{nameof(TeamsMatchById)}: Matched team {first} (Id {first.Id}) with Battlefy Persistent Id(s) " +
          $"[{string.Join(", ", first.BattlefyPersistentTeamIds)}] " +
          $"from team {second} (Id {second.Id}).");
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
    ///
    /// <returns>Teams match</returns>
    public static bool TeamsMatch(IReadOnlyCollection<Player> allPlayers, Team first, Team second)
    {
      // Get if ids or BattlefyPersistentIds match first.
      if (TeamsMatchById(first, second)) return true;

      // Otherwise, test if players match.
      if (TransformedNamesMatch(first.Names, second.Names))
      {
        // They do.
        logger.ConditionalDebug($"{nameof(TeamsMatch)}: Matched team {first} (Id {first.Id}) with TransformedNames(s) " +
          $"[{string.Join(", ", first.TeamNamesInformation?.TransformedNames ?? Array.Empty<string>())}] from team {second} (Id {second.Id}).");

        int sharedPlayersCount = 0;
        var firstPlayers = first.GetPlayers(allPlayers);
        if (firstPlayers.Count() <= 1)
        {
          logger.ConditionalDebug($"Shared players requirement NOT met.\nFirst: {first} does not have enough players: {string.Join(", ", firstPlayers)}  [{string.Join(", ", first.Sources)}]");
          return false;
        }
        var secondPlayers = second.GetPlayers(allPlayers);
        if (secondPlayers.Count() <= 1)
        {
          logger.ConditionalDebug($"Shared players requirement NOT met.\nSecond: {second} does not have enough players: {string.Join(", ", secondPlayers)}  [{string.Join(", ", second.Sources)}]");
          return false;
        }

        foreach (var firstPlayer in firstPlayers)
        {
          string[] firstPlayerNamesWithTag = first.ClanTags.SelectMany(tag =>
            firstPlayer.NamesInformation?.TransformedNames.Select(n => tag.CombineToPlayer(n)) ?? Array.Empty<string>()).ToArray();

          foreach (var secondPlayer in secondPlayers)
          {
            var secondPlayerNamesWithTag = second.ClanTags.SelectMany(tag =>
              secondPlayer.NamesInformation?.TransformedNames.Select(n => tag.CombineToPlayer(n)) ?? Array.Empty<string>()).ToArray();

            if (StringMatch(firstPlayer.NamesInformation?.TransformedNames.Concat(firstPlayerNamesWithTag) ?? Array.Empty<string>(),
                            secondPlayer.NamesInformation?.TransformedNames.Concat(secondPlayerNamesWithTag) ?? Array.Empty<string>(),
                            StringComparer.OrdinalIgnoreCase))
            {
              ++sharedPlayersCount;

              if (sharedPlayersCount >= 2)
              {
                logger.ConditionalDebug("Shared players requirement met.");
                return true;
              }
            }
          }
        }

        logger.ConditionalDebug($"Shared players requirement NOT met.\nFirst: {first}, with players: {string.Join(", ", firstPlayers)}  [{string.Join(", ", first.Sources)}]\nSecond: {second}, with players: {string.Join(", ", secondPlayers)}  [{string.Join(", ", second.Sources)}]");
      }

      return false;
    }
  }
}