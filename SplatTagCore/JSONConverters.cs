using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SplatTagCore
{
  public static class JSONConverters
  {
    public class GuidToSourceConverter
    {
      public static GuidToSourceConverter? Instance { get; private set; }
      private readonly Dictionary<string, Source> lookup;

      public GuidToSourceConverter(Dictionary<string, Source> lookup)
      {
        this.lookup = lookup;
        Instance = this;
      }

      public IEnumerable<Source> Convert(IEnumerable<string> names)
      {
        foreach (var id in names)
          yield return Convert(id);
      }

      public Source Convert(string name)
      {
        return lookup.ContainsKey(name) ? lookup[name] : Builtins.BuiltinSource;
      }
    }

    public class GuidArrayConverter : JsonConverter<Dictionary<int, Guid[]>>
    {
      public override Dictionary<int, Guid[]> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, Guid[]>>(ref reader, options);

        if (dictionary != null)
        {
          var result = new Dictionary<int, Guid[]>();
          foreach (var item in dictionary)
          {
            if (int.TryParse(item.Key, out int key))
            {
              result.Add(key, item.Value);
            }
          }
          return result;
        }
        return new();
      }

      public override void Write(Utf8JsonWriter writer, Dictionary<int, Guid[]> value, JsonSerializerOptions options)
      {
        var dictionary = new Dictionary<string, Guid[]>();
        foreach (var item in value)
        {
          dictionary.Add(item.Key.ToString(), item.Value);
        }

        JsonSerializer.Serialize(writer, dictionary, options);
      }
    }

    public class SourceIdConverter : JsonConverter<Source>
    {
      public override Source Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        var sourceId = JsonSerializer.Deserialize<string>(ref reader, options);

        if (sourceId != null)
        {
          if (GuidToSourceConverter.Instance != null)
          {
            return GuidToSourceConverter.Instance.Convert(sourceId);
          }
          else
          {
            return new Source(sourceId);
          }
        }
        else
        {
          return Builtins.ManualSource;
        }
      }

      public override void Write(Utf8JsonWriter writer, Source value, JsonSerializerOptions options)
      {
        JsonSerializer.Serialize(writer, value.Id, options);
      }
    }

    public class SourceIdsConverter : JsonConverter<IList<Source>>
    {
      public override IList<Source> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        var sourceIds = JsonSerializer.Deserialize<IList<string>>(ref reader, options);

        if (sourceIds != null)
        {
          if (GuidToSourceConverter.Instance != null)
          {
            return GuidToSourceConverter.Instance.Convert(sourceIds).Distinct().ToList();
          }
          else
          {
            return sourceIds.Select(s => new Source(s)).Distinct().ToList();
          }
        }
        else
        {
          return Array.Empty<Source>();
        }
      }

      public override void Write(Utf8JsonWriter writer, IList<Source> value, JsonSerializerOptions options)
      {
        JsonSerializer.Serialize(writer, new List<string>(value.Select(s => s.Id)), options);
      }
    }

    public class DateTimeTicksConverter : JsonConverter<DateTime>
    {
      public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        var ticks = JsonSerializer.Deserialize<string>(ref reader, options);

        if (ticks != null)
        {
          return new DateTime(long.Parse(ticks));
        }
        else
        {
          return Builtins.UnknownDateTime;
        }
      }

      public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
      {
        JsonSerializer.Serialize(writer, value.Ticks, options);
      }
    }
  }
}