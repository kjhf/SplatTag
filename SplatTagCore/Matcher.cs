using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SplatTagCore
{
  /// <summary>
  /// Matching and equality functions
  /// </summary>
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

      // Test if the Discord Ids match.
      if ((matchOptions & FilterOptions.DiscordId) != 0 && NamesMatch(first.DiscordIds, second.DiscordIds) > 0)
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

      // Test if the Battlefy Usernames match.
      if ((matchOptions & FilterOptions.BattlefyUsername) != 0 && GenericMatch(first.BattlefyUsernames, second.BattlefyUsernames) > 0)
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
          logger.Write(string.Join(", ", first.BattlefyUsernames));
          logger.Write("] from player ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.WriteLine(").");
        }
        return true;
      }

      // Test if any of the Battlefy Slugs match.
      if ((matchOptions & FilterOptions.BattlefySlugs) != 0 && NamesMatch(first.Battlefy.Slugs, second.Battlefy.Slugs) > 0)
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
      if ((matchOptions & FilterOptions.FriendCode) != 0 && GenericMatch<FriendCode>(first.FriendCodes, second.FriendCodes) > 0)
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
          logger.Write(string.Join(", ", first.FriendCodes));
          logger.Write("] from player ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.WriteLine(").");
        }
        return true;
      }

      // Test if the Twitches match.
      if ((matchOptions & FilterOptions.Twitch) != 0 && NamesMatch(first.Twitch, second.Twitch) > 0)
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
          logger.Write(string.Join(", ", first.Twitch));
          logger.Write("] from player ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.WriteLine(").");
        }
        return true;
      }

      // Test if the Twitters match.
      if ((matchOptions & FilterOptions.Twitter) != 0 && NamesMatch(first.Twitter, second.Twitter) > 0)
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
          logger.Write(string.Join(", ", first.Twitter));
          logger.Write("] from player ");
          logger.Write(second);
          logger.Write(" (Id ");
          logger.Write(second.Id);
          logger.WriteLine(").");
        }
        return true;
      }

      // Test if the Discord names match.
      if ((matchOptions & FilterOptions.DiscordName) != 0 && second.DiscordNames?.Equals(first.DiscordNames) == true)
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
    /// Count number of matches between <see cref="Name"/>s of first and second.
    /// </summary>
    public static int NamesMatch(IEnumerable<Name> first, IEnumerable<Name> second)
    {
      return second.Select(n => n.Value).Intersect(first.Select(n => n.Value)).Count();
    }

    /// <summary>
    /// Count number of matches between lists of first and second with a default comparison.
    /// </summary>
    public static int GenericMatch<T>(IEnumerable<T> first, IEnumerable<T> second)
    {
      return second.Intersect(first).Count();
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
      if (first.BattlefyPersistentTeamId != null && NamesMatch(first.BattlefyPersistentTeamIds, second.BattlefyPersistentTeamIds) > 0)
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
  }
}