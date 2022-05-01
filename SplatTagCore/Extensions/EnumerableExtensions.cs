using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace SplatTagCore
{
  public static class EnumerableExtensions
  {
    /// <summary>
    /// Add an entry to the dictionary key's collection or begin the key's collection with the value like a setdefault dict.
    /// </summary>
    public static void AddOrAppend<TKey, TValueCollection, TValue>(this IDictionary<TKey, TValueCollection> dictionary, TKey key, TValue value) where TValueCollection : ICollection<TValue>, new()
    {
      if (dictionary.ContainsKey(key) && dictionary[key] != null)
      {
        dictionary[key].AddUnique(value);
        return;
      }

      dictionary[key] = new TValueCollection { value };
    }

    /// <summary>
    /// Add an entry to the dictionary key's collection or begin the key's collection with the value(s) like a setdefault dict.
    /// </summary>
    public static void AddOrAppend<TKey, TValueCollection, TValue>(this IDictionary<TKey, TValueCollection> dictionary, TKey key, ICollection<TValue> values) where TValueCollection : ICollection<TValue>, new()
    {
      if (dictionary.ContainsKey(key) && dictionary[key] != null)
      {
        dictionary[key].AddUnique(values);
        return;
      }

      dictionary[key] = new TValueCollection();
      foreach (TValue value in values)
      {
        dictionary[key].Add(value);
      }
    }

    /// <summary>
    /// Add an element to the list if it does not already contain the element.
    /// </summary>
    public static void AddUnique<T>(this ICollection<T> list, T element)
    {
      if (!list.Contains(element))
      {
        list.Add(element);
      }
    }

    /// <summary>
    /// Add an element to the list if it does not already contain the element.
    /// </summary>
    public static void AddUnique<T>(this ICollection<T> list, IEnumerable<T> elements)
    {
      foreach (T element in elements)
      {
        AddUnique(list, element);
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

    /// <summary>
    /// Get if a string enumerable contains a string by <see cref="StringComparison"/>.
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
    public static T? Find<T>(this IReadOnlyList<T> ilist, Predicate<T> match)
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
        for (int i = 0; i < ilist.Count; i++)
        {
          if (match(ilist[i]))
          {
            return ilist[i];
          }
        }
        return default;
      }
    }

    /// <summary>
    /// Wrap a ForEach loop in a lambda.
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
    {
      foreach (var item in items)
      {
        action(item);
      }
    }

    /// <summary>
    /// Do an action on a pair of items from two sequences.
    /// Like <see cref="Enumerable.Zip{TFirst, TSecond, TResult}(IEnumerable{TFirst}, IEnumerable{TSecond}, Func{TFirst, TSecond, TResult})"/> but with no return.
    /// </summary>
    /// <typeparam name="TFirst"></typeparam>
    /// <typeparam name="TSecond"></typeparam>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <param name="action"></param>
    public static void ForPair<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second,
    Action<TFirst, TSecond> action)
    {
      using var enumFirst = first.GetEnumerator();
      using var enumSecond = second.GetEnumerator();
      while (enumFirst.MoveNext() && enumSecond.MoveNext())
      {
        action(enumFirst.Current, enumSecond.Current);
      }
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    [return: NotNullIfNotNull("defaultValue")]
    public static TValue? Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue? defaultValue = default)
      => key == null || !dictionary.TryGetValue(key, out var value) ? defaultValue : value;

    /// <summary>
    /// Gets the value associated with the specified key and box/un-box correctly.
    /// </summary>
    [return: NotNullIfNotNull("defaultValue")]
    public static TTarget? GetWithConversion<TTarget, TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TTarget? defaultValue = default) where TTarget : TValue
      => (TTarget?)Convert.ChangeType(Get(dictionary, key, defaultValue), typeof(TTarget?), CultureInfo.InvariantCulture);

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

    /// <summary>
    /// Get if the KeyValuePair is null or default.
    /// </summary>
    public static bool IsNullOrDefault<KT, VT>(this KeyValuePair<KT, VT> value) => default(KeyValuePair<string, object>).Equals(value);

    /// <summary>
    /// Get if the KeyValuePair is null or default.
    /// </summary>
    public static bool IsNullOrDefault<KT, VT>(this KeyValuePair<KT, VT>? value) => value == null || IsNullOrDefault(value.Value);

    /// <summary>
    /// Remove multiple indices
    /// With thanks to https://stackoverflow.com/questions/63495264/how-can-i-efficiently-remove-elements-by-index-from-a-very-large-list
    /// </summary>
    internal static void RemoveAtRange<T>(this List<T> values, List<int> indices)
    {
      if (indices.Count == 0)
      {
        return;
      }

      if (indices.Count == 1)
      {
        values.RemoveAt(indices[0]);
        return;
      }

      // Otherwise
      indices.Sort();

      int sourceStartIndex = 0;
      int skipCount = 0;

      int destStartIndex;
      int spanLength;

      // Copy items up to last index to be skipped
      foreach (var skipIndex in indices)
      {
        spanLength = skipIndex - sourceStartIndex;
        destStartIndex = sourceStartIndex - skipCount;

        for (int i = sourceStartIndex; i < sourceStartIndex + spanLength; i++)
        {
          values[destStartIndex] = values[i];
          destStartIndex++;
        }

        sourceStartIndex = skipIndex + 1;
        skipCount++;
      }

      // Copy remaining items (between last index to be skipped and end of list)
      spanLength = values.Count - sourceStartIndex;
      destStartIndex = sourceStartIndex - skipCount;

      for (int i = sourceStartIndex; i < sourceStartIndex + spanLength; i++)
      {
        values[destStartIndex] = values[i];
        destStartIndex++;
      }

      // This uses/rebuilds the list's array so needs a List (rather than IList) object.
      values.RemoveRange(destStartIndex, indices.Count);
    }

    /// <summary>
    /// Returns the list after sorting.
    /// Sorts the elements in the entire List using the default comparer.
    /// </summary>
    public static List<T> SortInline<T>(this List<T> list, bool reverse = false)
    {
      if (reverse)
      {
        list.Sort();
        list.Reverse();
      }
      else
      {
        list.Sort();
      }
      return list;
    }
  }
}