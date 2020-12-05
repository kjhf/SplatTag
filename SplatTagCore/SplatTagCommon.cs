using System;
using System.Collections.Generic;
using System.Text;

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
    /// <param name="source">The source</param>
    /// <param name="privateList">Reference to the player's private list</param>
    internal static T? AddName<T>(string name, Source source, List<T> privateList) where T : Name
    {
      if (!string.IsNullOrWhiteSpace(name))
      {
        if (privateList.Count == 0)
        {
          privateList.Add((T)Activator.CreateInstance(typeof(T), name, source));
        }
        else if (privateList[0].Value.Equals(name))
        {
          privateList[0].AddSource(source);
        }
        else
        {
          var foundName = privateList.Find(n => n.Value.Equals(name));
          if (foundName != null)
          {
            privateList.Remove(foundName);
            privateList.Insert(0, foundName);
            foundName.AddSource(source);
          }
          else
          {
            privateList.Insert(0, (T)Activator.CreateInstance(typeof(T), name, source));
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
          if (!string.IsNullOrWhiteSpace(name.Value))
          {
            if (privateList.Count == 0)
            {
              privateList.Add(name);
            }
            else if (privateList[0].Equals(value))
            {
              // Nothing to do.
            }
            else
            {
              privateList.Remove(name);
              privateList.Insert(0, name);
            }
          }
        }
      }
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
  }
}