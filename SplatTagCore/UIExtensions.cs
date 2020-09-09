using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SplatTagCore
{
  public static class UIExtensions
  {
    /// <summary>
    /// Get a string[] detailing the current and ex players on the team.
    /// </summary>
    public static string[] GetTeamPlayersStrings(this Team t, SplatTagController controller)
    {
      return controller.GetPlayersForTeam(t).Select(tuple => tuple.Item1.Name + " " + (tuple.Item2 ? "(Current)" : "(Ex)")).ToArray();
    }

    /// <summary>
    /// Get a string detailing the old teams in the enumerable.
    /// Old teams start at index 1, not first.
    /// </summary>
    public static string GetOldTeamsStrings(this IEnumerable<Team> teams)
    {
      StringBuilder sb = new StringBuilder();
      teams = teams.Skip(1);
      if (teams.Any())
      {
        sb.Append("(Old teams: ");
        sb.Append(string.Join(", ", teams.Select(t => t.Tag + " " + t.Name)));
        sb.Append(")");
      }

      return sb.ToString();
    }

    public static string GetBestTeamPlayerDivString(this Team t, SplatTagController splatTagController)
    {
      (Player, bool)[] playersForTeam = splatTagController.GetPlayersForTeam(t);
      IDivision highestDiv = t.Div;
      Player bestPlayer = null;
      foreach ((Player, bool) pair in playersForTeam)
      {
        if (pair.Item2 && pair.Item1.Teams.Count() > 1)
        {
          foreach (Team playerTeam in pair.Item1.Teams.Select(id => splatTagController.GetTeamById(id)))
          {
            if (playerTeam.Div.Value < highestDiv.Value)
            {
              highestDiv = playerTeam.Div;
              bestPlayer = pair.Item1;
            }
          }
        }
      }

      if (highestDiv == LUTIDivision.Unknown)
      {
        return "Their div is unknown.";
      }
      else if (highestDiv.Value == t.Div.Value)
      {
        return "No higher div players.";
      }
      else
      {
        return $"Highest Div'd player is {bestPlayer.Name} at {highestDiv}.";
      }
    }
  }
}
