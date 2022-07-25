using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  public static class SerializationInfoExtensions
  {
    /// <summary>
    /// The private GetValueNoThrow method in SerializationInfo.
    /// Needed as there is no Try equivalent and to prevent the <see cref="SerializationException"/> throw which is extremely expensive.
    /// </summary>
    private static readonly MethodInfo getValueNoThrow = typeof(SerializationInfo)
      .GetMethod("GetValueNoThrow", BindingFlags.Instance | BindingFlags.NonPublic);

    /// <summary>
    /// Yield-return the SerializationInfo values as key value pairs.
    /// </summary>
    /// <param name="serializationInfo">Serialization context</param>
    /// <returns>The serialized objects as KV pairs</returns>
    public static IEnumerable<KeyValuePair<string, object>> AsKeyValuePairs(this SerializationInfo serializationInfo)
    {
      var e = serializationInfo.GetEnumerator();
      while (e.MoveNext())
      {
        switch (e.Value)
        {
          case JValue v:
            if (v.Value is null) continue;
            yield return new KeyValuePair<string, object>(e.Name, v.Value);
            break;

          case string v:
            yield return new KeyValuePair<string, object>(e.Name, v);
            break;

          default:
            yield return new KeyValuePair<string, object>(e.Name, e.Value);
            break;
        }
      }
    }

    /// <summary>
    /// Return the first KV pair in the SerializationInfo context, or default(KeyValuePair<string, object>).
    /// </summary>
    /// <param name="serializationInfo">Serialization context</param>
    public static KeyValuePair<string, object> GetFirstOrDefault(this SerializationInfo serializationInfo)
    {
      return serializationInfo.AsKeyValuePairs().FirstOrDefault();
    }

    /// <summary>
    /// Get if the key exists.
    /// </summary>
    /// <param name="serializationInfo">Serialization context</param>
    /// <param name="name">Name of the object</param>
    /// <returns>If the key exists.</returns>
    public static bool Contains(this SerializationInfo serializationInfo, string name)
      => GetValueOrDefault(serializationInfo, name, typeof(object)) != null;

    /// <summary>
    /// Get the value at the name or the default value of T.
    /// </summary>
    /// <typeparam name="T">Return type of the deserialized object</typeparam>
    /// <param name="serializationInfo">Serialization context</param>
    /// <param name="name">Name of the object</param>
    /// <returns>The object or default(T)</returns>
    public static T? GetValueOrDefault<T>(this SerializationInfo serializationInfo, string name) where T : class
    {
      return (T?)getValueNoThrow.Invoke(serializationInfo, new object[] { name, typeof(T) });
    }

    /// <summary>
    /// Get the value at the name or the default value of T.
    /// </summary>
    /// <param name="serializationInfo">Serialization context</param>
    /// <param name="name">Name of the object</param>
    /// <param name="expectedType">The type of the object to deserialize</param>
    /// <returns>The object or default(T)</returns>
    public static object? GetValueOrDefault(this SerializationInfo serializationInfo, string name, Type expectedType)
    {
      return getValueNoThrow.Invoke(serializationInfo, new object[] { name, expectedType });
    }

    /// <summary>
    /// Get the value at the name or <paramref name="defaultValue"/>.
    /// </summary>
    /// <typeparam name="T">Return type of the deserialized object</typeparam>
    /// <param name="serializationInfo">Serialization context</param>
    /// <param name="name">Name of the object</param>
    /// <param name="defaultValue">Default to return if deserialization was unsuccessful.</param>
    /// <returns>The object or <paramref name="defaultValue"/></returns>
    public static T GetValueOrDefault<T>(this SerializationInfo serializationInfo, string name, T defaultValue)
    {
      object? value = getValueNoThrow.Invoke(serializationInfo, new object[] { name, typeof(T) });
      return value == null ? defaultValue : (T)value;
    }

    /// <summary>
    /// Get the value at the name or <paramref name="defaultValue"/>, automatically converting enum names.
    /// </summary>
    /// <typeparam name="T">Return type of the deserialized object</typeparam>
    /// <param name="serializationInfo">Serialization context</param>
    /// <param name="name">Name of the object</param>
    /// <param name="defaultValue">Default to return if deserialization was unsuccessful.</param>
    /// <returns>The object or <paramref name="defaultValue"/></returns>
    public static T GetEnumOrDefault<T>(this SerializationInfo serializationInfo, string name, T defaultValue) where T : Enum
    {
      object? value = getValueNoThrow.Invoke(serializationInfo, new object[] { name, typeof(string) });
      if (value == null)
      {
        return defaultValue;
      }
      // else
      try
      {
        return (T)Enum.Parse(typeof(T), (string)value);
      }
      catch (SystemException)
      {
        return defaultValue;
      }
    }
  }
}