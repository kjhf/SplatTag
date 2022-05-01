using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SplatTagCore
{
  public static class JsonExtensions
  {
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