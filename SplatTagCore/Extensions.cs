using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
    /// Get if a string enumerable contains a string by <see cref="StringComparison"/>.
    /// </summary>
    public static bool Contains(this IEnumerable<string> s, string other, StringComparison comp)
    {
      return s.Any(str => str.IndexOf(other, comp) != -1);
    }

    /// <summary>
    /// Get the number of times a string contains another string.
    /// </summary>
    public static int Count(this string s, string other) => Regex.Matches(s, Regex.Escape(other)).Count;

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

    /// <summary>
    /// Inserts a new value into a sorted collection.
    /// </summary>
    /// <typeparam name="T">The type of collection values, where the type implements IComparable of itself</typeparam>
    /// <param name="collection">The source collection</param>
    /// <param name="item">The item being inserted</param>
    public static void InsertSorted<T>(this IList<T> collection, T item)
        where T : IComparable<T>
    {
      InsertSorted(collection, item, Comparer<T>.Create((x, y) => x.CompareTo(y)));
    }

    /// <summary>
    /// Inserts a new value into a sorted collection.
    /// </summary>
    /// <typeparam name="T">The type of collection values</typeparam>
    /// <param name="collection">The source collection</param>
    /// <param name="item">The item being inserted</param>
    /// <param name="comparerFunction">An IComparer to comparer T values, e.g. Comparer&lt;T&gt;.Create((x, y) =&gt; (x.Property &lt; y.Property) ? -1 : (x.Property &gt; y.Property) ? 1 : 0)</param>
    public static void InsertSorted<T>(this IList<T> collection, T item, IComparer<T> comparerFunction)
    {
      if (collection.Count == 0)
      {
        // Simple add
        collection.Add(item);
      }
      else if (comparerFunction.Compare(item, collection[collection.Count - 1]) >= 0)
      {
        // Add to the end as the item being added is greater than the last item by comparison.
        collection.Add(item);
      }
      else if (comparerFunction.Compare(item, collection[0]) <= 0)
      {
        // Add to the front as the item being added is less than the first item by comparison.
        collection.Insert(0, item);
      }
      else
      {
        // Otherwise, search for the place to insert.
        int index = 0;
        if (collection is List<T> list)
        {
          index = list.BinarySearch(item, comparerFunction);
        }
        else if (collection is T[] arr)
        {
          index = Array.BinarySearch(arr, item, comparerFunction);
        }
        else
        {
          for (int i = 0; i < collection.Count; i++)
          {
            if (comparerFunction.Compare(collection[i], item) <= 0)
            {
              // If the item is the same or before, then the insertion point is here.
              index = i;
              break;
            }

            // Otherwise loop. We're already tested the last element for greater than count.
          }
        }

        if (index < 0)
        {
          // The zero-based index of item if item is found,
          // otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than item or, if there is no larger element, the bitwise complement of Count.
          index = ~index;
        }

        collection.Insert(index, item);
      }
    }
  }
}