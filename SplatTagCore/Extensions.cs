using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SplatTagCore
{
  public static class Extensions
  {
    /// <summary>
    /// Add an element to the list if it does not already contain the element.
    /// </summary>
    public static void AddUnique<T>(this IList<T> list, T element)
    {
      if (!list.Contains(element))
      {
        list.Add(element);
      }
    }

    /// <summary>
    /// Get if a string contains another by <see cref="StringComparison"/>.
    /// </summary>
    public static bool Contains(this string s, string other, StringComparison comp)
    {
      return s.IndexOf(other, comp) != -1;
    }

    /// <summary>
    /// Get if a string[] contains a string by <see cref="StringComparison"/>.
    /// </summary>
    public static bool Contains(this IEnumerable<string> s, string other, StringComparison comp)
    {
      return s.Any(str => str.IndexOf(other, comp) != -1);
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified
    /// predicate, and returns the first occurrence within the entire List.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ilist"></param>
    /// <param name="match">The System.Predicate delegate that defines the conditions of the element to search for.</param>
    /// <returns>
    /// The first element that matches the conditions defined by the specified predicate,
    /// if found; otherwise, the default value for type T.
    /// </returns>
    /// <exception cref="ArgumentNullException">match is null</exception>
    public static T Find<T>(this IList<T> ilist, Predicate<T> match)
    {
      if (ilist is List<T> list)
      {
        return list.Find(match);
      }
      else if (ilist is T[] array)
      {
        return Array.Find(array, match);
      }
      else
      {
        return ilist.FirstOrDefault(i => match(i));
      }
    }

    /// <summary>
    /// Convert an object to an enumerable (containing only itself)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static IEnumerable<T> AsEnumerable<T>(this T obj)
    {
      yield return obj;
    }

    public static T? GetValue<T>(this JToken jToken, string key, T? defaultValue = default)
    {
      var ret = jToken[key];
      if (ret == null) return defaultValue;
      if (ret is JObject)
      {
        return JsonConvert.DeserializeObject<T>(ret.ToString());
      }
      else
      {
        return ret.Value<T>();
      }
    }
  }
}