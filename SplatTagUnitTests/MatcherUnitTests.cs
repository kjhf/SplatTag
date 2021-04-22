using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagCore;
using System;
using System.Linq;

namespace SplatTagUnitTests
{
  /// <summary>
  /// <see cref="Matcher"/> unit tests
  /// </summary>
  [TestClass]
  public class MatcherUnitTests
  {
    [TestMethod]
    public void TestNamesMatching()
    {
      var n1 = new Name("n1", Builtins.ManualSource);
      var n2 = new Name("n2", Builtins.ManualSource);
      var n3 = new Name("n3", Builtins.BuiltinSource);
      var n1_2 = new Name("n1", Builtins.BuiltinSource);

      var first = new[] { n1, n2 };
      var second = new[] { n2, n3, n1_2 };
      Assert.IsTrue(Matcher.NamesMatch(first, second));
      Assert.AreEqual(2, Matcher.NamesMatchCount(first, second), "Expected n1 (this is under two objects), and n2 (one object in both sets)");
    }
  }
}