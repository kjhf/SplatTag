using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  public static class SerializationInfoExtensions
  {
    public static T? GetValueOrDefault<T>(this SerializationInfo serializationInfo, string name)
    {
      try
      {
        return (T)serializationInfo.GetValue(name, typeof(T));
      }
      catch (SerializationException)
      {
        return default;
      }
    }

    public static T GetValueOrDefault<T>(this SerializationInfo serializationInfo, string name, T defaultValue)
    {
      try
      {
        return (T)serializationInfo.GetValue(name, typeof(T));
      }
      catch (SerializationException)
      {
        return defaultValue;
      }
    }

    public static T? GetEnumOrDefault<T>(this SerializationInfo serializationInfo, string name, T defaultValue) where T : Enum
    {
      try
      {
        return (T)Enum.Parse(typeof(T), serializationInfo.GetString(name));
      }
      catch (Exception ex) when (ex is SerializationException || ex is SystemException)
      {
        return defaultValue;
      }
    }
  }
}