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
    /// Add an object to the front of a list if it is not currently contained in the list.
    /// Note that this does not affect sources.
    /// </summary>
    /// <typeparam name="T">The generic type</typeparam>
    /// <param name="value">The object(s) to add</param>
    /// <param name="privateList">The list to mutate</param>
    internal static void InsertFrontUnique<T>(T value, List<T> privateList) where T : notnull
    {
      if (privateList.Count == 0)
      {
        // Shortcut, just set the value.
        privateList.Add(value);
      }
      else
      {
        // If this item is already first, there's nothing to do.
        if (!value.Equals(privateList[0]))
        {
          privateList.Remove(value); // If item isn't found, this just returns false.
          privateList.Insert(0, value);
        }
      }
    }

    /// <summary>
    /// Add objects to the front of a list if it is not currently contained in the list.
    /// </summary>
    /// <typeparam name="T">The generic type</typeparam>
    /// <param name="value">The object(s) to add</param>
    /// <param name="privateList">The list to mutate</param>
    internal static void InsertFrontUnique<T>(IEnumerable<T> value, List<T> privateList) where T : notnull
    {
      if (privateList.Count == 0)
      {
        // Shortcut, just set the values.
        value = value.Distinct();
        privateList.AddRange(value);
      }
      else
      {
        // Iterates the other stack in reverse order so older objects are pushed first
        // so the most recent end up first in the stack.
        var reversed = value.Distinct().ToList();
        reversed.Reverse();
        foreach (var item in reversed)
        {
          // If this item is already first, there's nothing to do.
          if (!item.Equals(privateList[0]))
          {
            privateList.Remove(item); // If the item isn't found, this just returns false.
            privateList.Insert(0, item);
          }
        }
      }
    }

    /// <summary>
    /// Add an object to the front of a list if it is not currently contained in the list.
    /// Affects sources.
    /// </summary>
    /// <typeparam name="T">The generic type</typeparam>
    /// <param name="value">The object(s) to add</param>
    /// <param name="privateList">The list to mutate</param>
    internal static T InsertFrontUniqueSourced<T>(T value, List<T> privateList) where T : notnull, ISourceable
    {
      if (privateList.Count == 0)
      {
        // Shortcut, just set the value.
        privateList.Add(value);
      }
      else if (privateList[0].Equals(value))
      {
        // Item already exists, add incoming sources to it.
        AddSources(value.Sources, privateList[0].Sources);
      }
      else
      {
        var foundObj = privateList.Find(n => n.Equals(value));
        if (foundObj != null)
        {
          privateList.Remove(foundObj);
          privateList.Insert(0, foundObj);
          AddSources(foundObj.Sources, privateList[0].Sources);
        }
        else
        {
          // New item, insert as-is.
          privateList.Insert(0, value);
        }
      }
      return privateList[0];
    }

    /// <summary>
    /// Add a known name to the front of a list, or move the current name to the front.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="privateList"></param>
    internal static T AddName<T>(T value, List<T> privateList) where T : Name
    {
      return InsertFrontUniqueSourced(value, privateList);
    }

    /// <summary>
    /// Add known names to the front of a list, or move the current name to the front.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="privateList"></param>
    internal static void AddNames<T>(IEnumerable<T> value, List<T> privateList) where T : Name
    {
      foreach (T name in value.Reverse())
      {
        InsertFrontUniqueSourced(name, privateList);
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
    /// <param name="sourceValue"></param>
    /// <param name="privateDestinationList"></param>
    internal static void AddSources(IEnumerable<Source> sourceValue, IList<Source> privateDestinationList)
    {
      foreach (Source source in sourceValue)
      {
        Source? foundSource = privateDestinationList.Find(s => s.Id == source.Id);

        if (foundSource == null)
        {
          privateDestinationList.Add(source);
        }
      }
    }

    /// <summary>
    /// Add strings to the list if not currently contained.
    /// </summary>
    internal static void AddStrings(IEnumerable<string> value, List<string> privateList)
    {
      foreach (string w in value.Reverse())
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