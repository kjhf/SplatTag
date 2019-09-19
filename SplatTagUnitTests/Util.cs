using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SplatTagUnitTests
{
  internal static class Util
  {
    public static object GetPrivateMember<T>(T instance, string memberName)
    {
      FieldInfo f = typeof(T).GetField(memberName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
      return f.GetValue(instance);
    }
  }
}
