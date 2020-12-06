using SplatTagCore.Social;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SplatTagCore
{
  /// <summary>
  /// Common functions for SplatTag objects
  /// </summary>
  public static class SplatTagCommon
  {
    /// <summary>
    /// Add a name or handle and its source to the private list.
    /// </summary>
    /// <typeparam name="T">The type, which must be a Name (including Social)</typeparam>
    /// <param name="name">The name or handle</param>
    /// <param name="sources">The sources</param>
    /// <param name="privateList">Reference to the player's private list</param>
    internal static Battlefy? AddBattlefy(string slug, IEnumerable<string> usernames, IEnumerable<Source> sources, List<Battlefy> privateList)
    {
      if (!string.IsNullOrWhiteSpace(slug))
      {
        if (privateList.Count == 0)
        {
          privateList.Add(new Battlefy(slug, usernames, sources));
        }
        else if (privateList[0].Value.Equals(slug))
        {
          privateList[0].AddSources(sources);
          privateList[0].AddUsernames(usernames);
        }
        else
        {
          var foundName = privateList.Find(n => n.Value.Equals(slug));
          if (foundName != null)
          {
            privateList.Remove(foundName);
            privateList.Insert(0, foundName);
            privateList[0].AddSources(sources);
            privateList[0].AddUsernames(usernames);
          }
          else
          {
            privateList.Insert(0, new Battlefy(slug, usernames, sources));
          }
        }

        return privateList[0];
      }
      return null;
    }

    /// <summary>
    /// Add Ids to the list if not currently contained.
    /// </summary>
    internal static void AddIds(IEnumerable<Guid> value, List<Guid> privateList)
    {
      if (value != null)
      {
        if (privateList.Count == 0)
        {
          // Shortcut, just set the teams.
          privateList.AddRange(value.Distinct());
        }
        else
        {
          // Iterates the other stack in reverse order so older teams are pushed first
          // so the most recent end up first in the stack.
          var reverseTeams = value.Distinct().ToList();
          reverseTeams.Reverse();
          foreach (Guid t in reverseTeams)
          {
            // If this team is already first, there's nothing to do.
            if (privateList[0] != t)
            {
              privateList.Remove(t); // If the team isn't found, this just returns false.
              privateList.Insert(0, t);
            }
          }
        }
      }
    }

    /// <summary>
    /// Add a name or handle and its source to the private list.
    /// </summary>
    /// <typeparam name="T">The type, which must be a Name (including Social)</typeparam>
    /// <param name="name">The name or handle</param>
    /// <param name="source">The source</param>
    /// <param name="privateList">Reference to the player's private list</param>
    internal static T? AddName<T>(string name, Source source, List<T> privateList) where T : Name
    {
      return AddName(name, source.AsEnumerable(), privateList);
    }

    /// <summary>
    /// Add a name or handle and its sources to the private list.
    /// </summary>
    /// <typeparam name="T">The type, which must be a Name (including Social)</typeparam>
    /// <param name="name">The name or handle</param>
    /// <param name="sources">The sources</param>
    /// <param name="privateList">Reference to the player's private list</param>
    internal static T? AddName<T>(string name, IEnumerable<Source> sources, List<T> privateList) where T : Name
    {
      if (!string.IsNullOrWhiteSpace(name))
      {
        if (privateList.Count == 0)
        {
          privateList.Add((T)Activator.CreateInstance(typeof(T), name, sources));
        }
        else if (privateList[0].Value.Equals(name))
        {
          privateList[0].AddSources(sources);
        }
        else
        {
          var foundName = privateList.Find(n => n.Value.Equals(name));
          if (foundName != null)
          {
            privateList.Remove(foundName);
            privateList.Insert(0, foundName);
            privateList[0].AddSources(sources);
          }
          else
          {
            privateList.Insert(0, (T)Activator.CreateInstance(typeof(T), name, sources));
          }
        }

        return privateList[0];
      }
      return null;
    }

    /// <summary>
    /// Add known names to the front of a list, or move the current name to the front.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="privateList"></param>
    internal static void AddNames<T>(IEnumerable<T> value, List<T> privateList) where T : Name
    {
      if (value != null)
      {
        foreach (T name in value)
        {
          AddName(name.Value, name.Sources, privateList);
        }
      }
    }

    /// <summary>
    /// Add sources to the list if not currently contained.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="privateList"></param>
    internal static void AddSource(Source value, List<Source> privateList)
    {
      AddSources(value.AsEnumerable(), privateList);
    }

    /// <summary>
    /// Add sources to the list if not currently contained.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="privateList"></param>
    internal static void AddSources(IEnumerable<Source> value, List<Source> privateList)
    {
      if (value != null)
      {
        foreach (Source source in value)
        {
          Source? foundSource = privateList.Find(s => s.Id == source.Id);

          if (foundSource == null)
          {
            privateList.Add(source);
          }
        }
      }
    }

    /// <summary>
    /// Add strings to the list if not currently contained.
    /// </summary>
    internal static void AddStrings(IEnumerable<string> value, List<string> privateList)
    {
      if (value != null)
      {
        foreach (string w in value)
        {
          if (!string.IsNullOrWhiteSpace(w))
          {
            if (privateList.Count == 0)
            {
              privateList.Add(w);
            }
            else if (privateList[0].Equals(w, StringComparison.OrdinalIgnoreCase))
            {
              // Nothing to do.
            }
            else
            {
              privateList.Remove(w);
              privateList.Insert(0, w);
            }
          }
        }
      }
    }
  }
}