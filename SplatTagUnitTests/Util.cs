using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace SplatTagUnitTests
{
  internal static class Util
  {
    public static object? GetPrivateMember<T>(T instance, string memberName)
    {
      return typeof(T)
        .GetField(memberName, BindingFlags.Instance | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public)
        ?.GetValue(instance);
    }
  }
}