using NLog;
using SplatTagCore;
using SplatTagCore.Social;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

    public static T GetRandomCoreObject<T>() where T : BaseSplatTagCoreObject<T>, new()
    {
      T ob = new();
      ob.PopulateWithRandomValues();
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
      return new FriendCode(GetRandomIEnumerable<short>(3).ToArray());
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
      if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
      {
        return GetRandomValue(type.GetGenericArguments()[0]);
      }
      if (type == typeof(string))
      {
        return GetRandomString();
      }
      if (type == typeof(short))
      {
        return GetRandomPositiveShort();
      }
      if (type == typeof(int))
      {
        return GetRandomIntSigned();
      }
      if (type == typeof(double))
      {
        return GetRandomDouble();
      }
      if (type == typeof(bool))
      {
        return GetRandomBool();
      }
      if (type == typeof(DateTime))
      {
        return GetRandomDateTime();
      }
      if (type == typeof(Guid))
      {
        return GetRandomGuid();
      }
      if (type == typeof(Uri))
      {
        return GetRandomUri();
      }
      if (type == typeof(Bracket))
      {
        return GetRandomBracket();
      }
      if (type == typeof(Division))
      {
        return GetRandomDivision();
      }
      if (type == typeof(Game))
      {
        return GetRandomGame();
      }
      if (type == typeof(Player))
      {
        return GetRandomCoreObject<Player>();
      }
      if (type == typeof(Team))
      {
        return GetRandomCoreObject<Team>();
      }
      if (type == typeof(Source))
      {
        return GetRandomSource();
      }
      if (type == typeof(Placement))
      {
        return GetRandomPlacement();
      }
      if (type == typeof(Pronoun))
      {
        return GetRandomPronoun();
      }
      if (type == typeof(Skill))
      {
        return new Skill();
      }
      if (type == typeof(Name))
      {
        return GetRandomName();
      }
      if (type == typeof(BattlefyTeamSocial))
      {
        return new BattlefyTeamSocial(GetRandomString(), GetRandomSource());
      }
      if (type == typeof(BattlefyUserSocial))
      {
        return new BattlefyUserSocial(GetRandomString(), GetRandomSource());
      }
      if (type == typeof(PlusMembership))
      {
        return new PlusMembership(GetRandomString(), GetRandomSource());
      }
      if (type == typeof(Sendou))
      {
        return new Sendou(GetRandomString(), GetRandomSource());
      }
      if (type == typeof(Twitch))
      {
        return new Twitch(GetRandomString(), GetRandomSource());
      }
      if (type == typeof(Twitter))
      {
        return new Twitter(GetRandomString(), GetRandomSource());
      }
      if (type == typeof(ClanTag))
      {
        return new ClanTag(GetRandomString(), GetRandomSource(), GetRandomEnum<TagOption>());
      }
      throw new NotImplementedException("GetRandomValue for type " + type);
    }

    private static T GetRandomEnum<T>() where T : struct, Enum
    {
      var values = Enum.GetValues<T>();
      return (T)(values.GetValue(rand.Next(values.Length)) ?? throw new InvalidOperationException("Random enum value is null for type " + typeof(T)));
    }

    public static T GetRandomValue<T>() where T : notnull => (T)GetRandomValue(typeof(T));

    // Honestly this is shit code and I'm sorry :')
    public static void PopulateWithRandomValues<T>(this BaseHandler<T> handler) where T : notnull
    {
      if (handler is SingleValueHandler<T> singleValueHandler)
      {
        singleValueHandler.Value = GetRandomValue<T>();
      }
      else if (handler is BaseSourcedItemHandler<T> itemHandler)
      {
        for (var i = 0; i < rand.Next(1, 10); i++)
        {
          itemHandler.items.Add(GetRandomValue<T>(), GetRandomIEnumerable<Source>().ToList());
        }
      }
      else if (handler is DivisionsHandler dh)
      {
        for (var i = 0; i < rand.Next(1, 10); i++)
        {
          dh.items.Add(GetRandomValue<Division>(), GetRandomIEnumerable<Source>().ToList());
        }
      }
      else if (handler is FriendCodesHandler fch)
      {
        for (var i = 0; i < rand.Next(1, 10); i++)
        {
          fch.items.Add(GetRandomFriendCode(), GetRandomIEnumerable<Source>().ToList());
        }
      }
      else if (handler is PronounsHandler ph)
      {
        for (var i = 0; i < rand.Next(1, 10); i++)
        {
          ph.items.Add(GetRandomPronoun(), GetRandomIEnumerable<Source>().ToList());
        }
      }
      else if (handler is TeamsHandler th)
      {
        for (var i = 0; i < rand.Next(1, 10); i++)
        {
          th.items.Add(GetRandomGuid(), GetRandomIEnumerable<Source>().ToList());
        }
      }
      else if (handler is WeaponsHandler wh)
      {
        for (var i = 0; i < rand.Next(1, 10); i++)
        {
          wh.items.Add(GetRandomIEnumerable<string>().ToList(), GetRandomIEnumerable<Source>().ToList());
        }
      }
      else if (handler is IDictionary<string, BaseHandler> handlerCollection)
      {
        foreach (var pair in handlerCollection)
        {
          var child = (dynamic)pair.Value;
          PopulateWithRandomValues(child);
        }
      }
      else if (handler is IEnumerable collection)
      {
        if (collection.GetType().GenericTypeArguments.Length != 1)
        {
          throw new NotImplementedException($"Handler type not implemented as collection is not generic/too many type args ({collection.GetType().GenericTypeArguments.Length}): {handler.GetType()}");
        }
        Type itemType = collection.GetType().GetGenericArguments()[0];
        var dyn = (dynamic)handler;
        for (var i = 0; i < rand.Next(1, 10); i++)
        {
          dynamic data = GetRandomValue(itemType);
          dyn.Add(data);
        }
      }
      else
      {
        throw new NotImplementedException("Handler type not implemented: " + handler.GetType());
      }
    }
  }
}