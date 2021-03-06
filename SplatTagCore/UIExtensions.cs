﻿using System;
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

    public static string GetBestTeamPlayerDivString(this Team t, SplatTagController splatTagController)
    {
      (Player, bool)[] playersForTeam = splatTagController.GetPlayersForTeam(t);
      Division highestDiv = t.CurrentDiv;
      Player? bestPlayer = null;
      foreach ((Player, bool) pair in playersForTeam)
      {
        if (pair.Item2 && pair.Item1.Teams.Count > 1)
        {
          foreach (Team playerTeam in pair.Item1.Teams.Select(id => splatTagController.GetTeamById(id)))
          {
            if (playerTeam.CurrentDiv < highestDiv)
            {
              highestDiv = playerTeam.CurrentDiv;
              bestPlayer = pair.Item1;
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