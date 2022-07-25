using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using SplatTagCore;
using SplatTagCore.Social;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SplatTagUnitTests
{
  internal static class ArbitraryDataExtensions
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static readonly Random rand = new();

    public static bool GetRandomBool()
    {
      return rand.Next(2) == 0;
    }

    public static Bracket GetRandomBracket()
    {
      return new Bracket(
        GetRandomString(),
        GetRandomIEnumerable<Game>().ToList(),
        GetRandomIEnumerable<Guid>().ToList(),
        GetRandomIEnumerable<Guid>().ToList(),
        GetRandomValue<Placement>()
        );
    }

    public static Division GetRandomDivision()
    {
      return new Division(
        GetRandomString(),
        GetRandomEnum<DivType>(),
        GetRandomString()
        );
    }

    public static T GetRandomCoreObject<T>() where T : IdentifiableObjectHandler<T>, IIdentifiableCoreObject, new()
    {
      T ob = new();
      ob.PopulateSourcedHandlerCollectionWithRandomValuesT();
      return ob;
    }

    public static DateTime GetRandomDateTime()
    {
      return new(GetRandomPositiveInt());
    }

    public static double GetRandomDouble()
    {
      return rand.NextDouble();
    }

    public static Game GetRandomGame()
    {
      return new Game(new Score(GetRandomIEnumerable<int>().ToList()), GetRandomIEnumerable<Guid>().ToList());
    }

    public static Guid GetRandomGuid()
    {
      return Guid.NewGuid();
    }

    public static IEnumerable<T> GetRandomIEnumerable<T>(int? count = null) where T : notnull
    {
      count ??= rand.Next(2, 10);
      for (int i = 0; i < count; i++)
      {
        yield return GetRandomValue<T>();
      }
    }

    public static int GetRandomPositiveInt()
    {
      return rand.Next();
    }

    public static int GetRandomIntSigned()
    {
      return rand.Next() - (int.MaxValue / 2);
    }

    public static short GetRandomPositiveShort()
    {
      return (short)(rand.Next() % (short.MaxValue + 1));
    }

    public static FriendCode GetRandomFriendCode()
    {
      const int COUNT = 3;
      short[] shorts = new short[COUNT];
      for (int i = 0; i < COUNT; i++)
      {
        shorts[i] = (short)(GetRandomPositiveShort() % 10000);
      }

      return new FriendCode(shorts);
    }

    public static Name GetRandomName()
    {
      return new Name(GetRandomString(), GetRandomSource());
    }

    public static Placement GetRandomPlacement()
    {
      var players = new Dictionary<int, Guid[]>();
      for (var i = 0; i < rand.Next(1, 10); i++)
      {
        players.Add(i, GetRandomIEnumerable<Guid>().ToArray());
      }
      var teams = new Dictionary<int, Guid[]>();
      for (var i = 0; i < rand.Next(1, 10); i++)
      {
        teams.Add(i, GetRandomIEnumerable<Guid>().ToArray());
      }
      return new Placement(players, teams);
    }

    public static Pronoun GetRandomPronoun()
    {
      return new Pronoun(GetRandomEnum<PronounFlags>(), GetRandomSource());
    }

    public static Source GetRandomSource(bool getFull = false)
    {
      if (getFull)
      {
        return new Source(GetRandomString(), GetRandomDateTime())
        {
          Brackets = GetRandomIEnumerable<Bracket>().ToArray(),
          Players = GetRandomIEnumerable<Player>().ToArray(),
          Teams = GetRandomIEnumerable<Team>().ToArray(),
          Uris = GetRandomIEnumerable<Uri>().ToArray()
        };
      }
      // else
      return new Source(GetRandomString(), GetRandomDateTime());
    }

    public static Skill GetRandomSkill()
    {
      return new(GetRandomDivision());
    }

    public static string GetRandomString()
    {
      const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
      var stringChars = new char[8];
      for (int i = 0; i < stringChars.Length; i++)
      {
        stringChars[i] = chars[rand.Next(chars.Length)];
      }
      return new(stringChars);
    }

    public static Uri GetRandomUri()
    {
      return new("https://www.example.com/" + GetRandomString());
    }

    public static object GetRandomValue(this Type type)
    {
      if (type.IsGenericType)
      {
        var nullableType = type.GetTypeAssignableToGenericType(typeof(Nullable<>));
        if (nullableType != null)
        {
          return GetRandomValue(nullableType.GetGenericArguments()[0]);
        }

        var enumerableType = type.GetTypeAssignableToGenericType(typeof(IEnumerable<>));
        if (enumerableType != null)
        {
          var elementType = enumerableType.GetGenericArguments()[0];
          object? ob = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
          Assert.IsNotNull(ob);
          var list = (IList)ob;
          for (int i = 0; i < rand.Next(2, 10); i++)
          {
            list.Add(GetRandomValue(elementType));
          }
          return list;
        }
      }

      return type switch
      {
        Type t when t == typeof(bool) => GetRandomBool(),
        Type t when t == typeof(Bracket) => GetRandomBracket(),
        Type t when t == typeof(Division) => GetRandomDivision(),
        Type t when t == typeof(DateTime) => GetRandomDateTime(),
        Type t when t == typeof(double) => GetRandomDouble(),
        Type t when t == typeof(Game) => GetRandomGame(),
        Type t when t == typeof(Guid) => GetRandomGuid(),
        Type t when t == typeof(int) => GetRandomPositiveInt(),
        Type t when t == typeof(short) => GetRandomPositiveShort(),
        Type t when t == typeof(FriendCode) => GetRandomFriendCode(),
        Type t when t == typeof(Name) => GetRandomName(),
        Type t when t == typeof(Placement) => GetRandomPlacement(),
        Type t when t == typeof(Player) => GetRandomCoreObject<Player>(),
        Type t when t == typeof(Team) => GetRandomCoreObject<Team>(),
        Type t when t == typeof(Pronoun) => GetRandomPronoun(),
        Type t when t == typeof(Skill) => GetRandomSkill(),
        Type t when t == typeof(Source) => GetRandomSource(),
        Type t when t == typeof(string) => GetRandomString(),
        Type t when t == typeof(TeamId) => (TeamId)GetRandomGuid(),
        Type t when t == typeof(Uri) => GetRandomUri(),
        Type t when t == typeof(WeaponsContainer) => new WeaponsContainer(GetRandomIEnumerable<string>()),
        Type t when t == typeof(BattlefyTeamSocial) => new BattlefyTeamSocial(GetRandomString(), GetRandomSource()),
        Type t when t == typeof(BattlefyUserSocial) => new BattlefyUserSocial(GetRandomString(), GetRandomSource()),
        Type t when t == typeof(PlusMembership) => new PlusMembership(GetRandomString(), GetRandomSource()),
        Type t when t == typeof(Sendou) => new Sendou(GetRandomString(), GetRandomSource()),
        Type t when t == typeof(Twitch) => new Twitch(GetRandomString(), GetRandomSource()),
        Type t when t == typeof(Twitter) => new Twitter(GetRandomString(), GetRandomSource()),
        Type t when t == typeof(ClanTag) => new ClanTag(GetRandomString(), GetRandomSource(), GetRandomEnum<TagOption>()),
        _ => throw new NotImplementedException("GetRandomValue for type " + type)
      };
    }

    private static T GetRandomEnum<T>() where T : struct, Enum
    {
      var values = Enum.GetValues<T>();
      return (T)(values.GetValue(rand.Next(values.Length)) ?? throw new InvalidOperationException("Random enum value is null for type " + typeof(T)));
    }

    public static T GetRandomValue<T>() where T : notnull => (T)GetRandomValue(typeof(T));

    public static BaseHandler PopulateSingleValueHandlerWithRandomValuesT<T>(this SingleValueHandler<T> handler) where T : notnull
    {
      handler.Value = GetRandomValue<T>();
      return handler;
    }

    public static T PopulateSourcedItemHandlerWithRandomValuesT<T>(this T handler) where T : BaseHandlerSourced
    {
      var itemType = handler.GetType().GetTypeAssignableToGenericType(typeof(BaseSourcedItemHandler<>));
      if (itemType != null)
      {
        var elementType = itemType.GetGenericArguments()[0];

        for (var i = 0; i < rand.Next(1, 10); i++)
        {
          if (elementType is IReadonlySourceable)
          {
            ((dynamic)handler).Add((dynamic)GetRandomValue(elementType));  // Sources not needed.
          }
          else
          {
            ((dynamic)handler).Add((dynamic)GetRandomValue(elementType), GetRandomIEnumerable<Source>().ToList());
          }
        }
      }
      else
      {
        throw new InvalidCastException("Handler type not implemented for getting generics in BaseHandlerSourced: " + handler.GetType());
      }
      return handler;
    }

    private static T PopulateSourcedHandlerCollectionWithRandomValuesT<T>(this T handler) where T : BaseHandlerCollectionSourced, IBaseHandlerCollectionSourced
    {
      foreach (var supported in handler.SupportedHandlers)
      {
        var childHandler = handler.GetHandler<BaseHandler>(supported.Key);
        PopulateWithRandomValues((dynamic)childHandler);
      }
      return handler;
    }

    public static BaseHandler PopulateWithRandomValues(this BaseHandler handler)
    {
      var type = handler.GetType();
      return type switch
      {
        var _ when type.CanBeType(typeof(BaseHandlerCollectionSourced)) => PopulateSourcedHandlerCollectionWithRandomValuesT((dynamic)handler),
        var _ when type.CanBeType(typeof(BaseHandlerSourced)) => PopulateSourcedItemHandlerWithRandomValuesT((dynamic)handler),
        var _ when type.CanBeType(typeof(SingleValueHandler<>)) => PopulateSingleValueHandlerWithRandomValuesT((dynamic)handler),
        _ => throw new NotImplementedException("Handler type not implemented: " + handler.GetType())
      };
    }

    public static Type? GetTypeAssignableToGenericType(this Type type, Type generic)
      => new[] { type }
      .Concat(type.GetTypeInfo().ImplementedInterfaces)
      .FirstOrDefault(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == generic) ??
      (type.GetTypeInfo().BaseType?.GetTypeAssignableToGenericType(generic));

    public static bool CanBeType(this Type type, Type otherType)
    {
      var isGeneric = otherType.IsGenericType;
      var isEqual = type == otherType;

      if (isEqual) return true;

      return isGeneric
        ? GetTypeAssignableToGenericType(type, otherType) != null
        : type.GetInterface(nameof(otherType)) != null || type.IsAssignableTo(otherType);
    }
  }
}