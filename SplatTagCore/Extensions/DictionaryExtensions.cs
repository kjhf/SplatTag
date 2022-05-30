using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace SplatTagCore.Extensions
{
  public static class DictionaryExtensions
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
    /// Gets the value associated with the specified key.
    /// </summary>
    [return: NotNullIfNotNull("defaultValue")]
    public static TValue? Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue? defaultValue = default)
      => key == null || !dictionary.TryGetValue(key, out var value) ? defaultValue : value;

    /// <summary>
    /// Gets the value associated with the specified key, or returns a default after adding it.
    /// </summary>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueAdder)
    {
      if (key == null || !dictionary.TryGetValue(key, out var value))
      {
        return dictionary[key] = valueAdder();
      }
      // else
      return value;
    }

    /// <summary>
    /// Gets the value associated with the specified key and box/un-box correctly.
    /// </summary>
    [return: NotNullIfNotNull("defaultValue")]
    public static TTarget? GetWithConversion<TTarget, TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TTarget? defaultValue = default) where TTarget : TValue
      => (TTarget?)Convert.ChangeType(Get(dictionary, key, defaultValue), typeof(TTarget?), CultureInfo.InvariantCulture);

    /// <summary>
    /// Adds a key/value pair to the <see cref="ConcurrentDictionary{TKey,TValue}"/> if the key does not already
    /// exist, or updates a key/value pair in the <see cref="ConcurrentDictionary{TKey,TValue}"/> if the key
    /// already exists.
    /// </summary>
    /// <param name="key">The key to be added or whose value should be updated</param>
    /// <param name="addValue">The value to be added for an absent key</param>
    /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on
    /// the key's existing value</param>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is a null reference
    /// (Nothing in Visual Basic).</exception>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="updateValueFactory"/> is a null reference
    /// (Nothing in Visual Basic).</exception>
    /// <exception cref="T:System.OverflowException">The dictionary contains too many
    /// elements.</exception>
    /// <returns>The new value for the key.  This will be either be the result of addValueFactory (if the key was
    /// absent) or the result of updateValueFactory (if the key was present).</returns>
    public static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
    {
      if (dictionary is ConcurrentDictionary<TKey, TValue> concurrent)
      {
        return concurrent.AddOrUpdate(key, addValue, updateValueFactory);
      }

      if (key == null) throw new ArgumentNullException(nameof(key));
      if (updateValueFactory == null) throw new ArgumentNullException(nameof(updateValueFactory));

      // If the key exists, try to update, otherwise add it.
      TValue newValue = (dictionary.TryGetValue(key, out TValue oldValue)) ?
        updateValueFactory(key, oldValue) :
        addValue;
      dictionary[key] = newValue;
      return newValue;
    }

    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
    {
      foreach (var kvp in keyValuePairs)
      {
        dictionary.Add(kvp.Key, kvp.Value);
      }
    }

    public static void AddRangeFromKeys<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys, Func<TKey, TValue> valueSelector)
    {
      foreach (var key in keys)
      {
        dictionary.Add(key, valueSelector(key));
      }
    }

    public static void AddRangeFromValues<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<TValue> values, Func<TValue, TKey> keySelector)
    {
      foreach (var value in values)
      {
        dictionary.Add(keySelector(value), value);
      }
    }
  }
}