using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagCore;
using System;

namespace SplatTagUnitTests
{
  /// <summary>
  /// <see cref="Source"/> matching unit tests
  /// </summary>
  [TestClass]
  public class SourceUnitTests
  {
    [TestMethod]
    public void TestStartWithChronology()
    {
      Source s1 = new Source("s1", DateTime.UtcNow.AddDays(1));
      Source s2 = new Source("s2", DateTime.UtcNow);

      Assert.IsTrue(s1.Start > s2.Start);
    }

    [TestMethod]
    public void TestInferredStart()
    {
      Source s1 = new Source("2000-01-01-s1");
      Source s2 = new Source("2002-01-01-s2");

      Assert.IsTrue(s1.Start < s2.Start);
    }
  }
}