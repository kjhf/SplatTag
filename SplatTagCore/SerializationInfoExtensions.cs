using System;
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
    /// Get the value at the name or the default value of T.
    /// </summary>
    /// <typeparam name="T">Return type of the deserialized object</typeparam>
    /// <param name="serializationInfo">Serialization context</param>
    /// <param name="name">Name of the object</param>
    /// <returns>The object or default(T)</returns>
    public static T? GetValueOrDefault<T>(this SerializationInfo serializationInfo, string name)
    {
      return (T?)getValueNoThrow.Invoke(serializationInfo, new object[] { name, typeof(T) });
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
      if (value == null)
      {
        return defaultValue;
      }
      else
      {
        return (T)value;
      }
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
      else
      {
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
}