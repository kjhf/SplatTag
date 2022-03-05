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

    public static string GetBestTeamPlayerDivString(this Team t, ITeamResolver splatTagController)
    {
      var playersForTeam = splatTagController.GetPlayersForTeam(t);
      Division highestDiv = t.CurrentDiv;
      Player? bestPlayer = null;
      foreach (var (player, mostRecent) in playersForTeam)
      {
        if (mostRecent && player.TeamInformation.Count > 1)
        {
          foreach (Team playerTeam in player.TeamInformation.GetTeamsUnordered().Select(id => splatTagController.GetTeamById(id)))
          {
            if (playerTeam.CurrentDiv < highestDiv)
            {
              highestDiv = playerTeam.CurrentDiv;
              bestPlayer = player;
            }
          }
        }
      }

      if (bestPlayer == null || highestDiv == Division.Unknown)
      {
        // Don't show anything it's pointless.
        return "";
      }
      else if (highestDiv.Value == t.CurrentDiv.Value)
      {
        return "No higher div players.";
      }
      else
      {
        return $"Highest div'd player is {bestPlayer.Name} at {highestDiv}.";
      }
    }
  }
}