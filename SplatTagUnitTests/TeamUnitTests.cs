using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagCore;
using System;

namespace SplatTagUnitTests
{
  /// <summary>
  /// <see cref="Team"/> unit tests
  /// </summary>
  [TestClass]
  public class TeamUnitTests
  {
    [TestMethod]
    public void TestCompareToBySourceChronology_Simple()
    {
      Source s1 = new Source("s1", DateTime.UtcNow.AddDays(1));
      Source s2 = new Source("s2", DateTime.UtcNow);

      Team t1 = new Team("team 1", s1);
      Team t2 = new Team("team 2", s2);

      Assert.AreEqual(1, t1.CompareToBySourceChronology(t2));
      Assert.AreEqual(-1, t2.CompareToBySourceChronology(t1));
      Assert.AreEqual(0, t2.CompareToBySourceChronology(t2));
    }

    [TestMethod]
    public void TestCompareToBySourceChronology_Mixed()
    {
      Source s1 = new Source("s1", DateTime.UtcNow.AddDays(1));
      Source s2 = new Source("s2", DateTime.UtcNow.AddHours(1));
      Source s3 = new Source("s3", DateTime.UtcNow);
      Source s4 = new Source("s4", DateTime.UtcNow.AddDays(-1));

      Team t1 = new Team("team 1", s1);
      Team t2 = new Team("team 2", s2);

      Assert.AreEqual(1, t1.CompareToBySourceChronology(t2));
      Assert.AreEqual(-1, t2.CompareToBySourceChronology(t1));

      t1.AddTwitter("twit1", s3);
      t2.AddTwitter("twit2", s4);

      Assert.AreEqual(1, t1.CompareToBySourceChronology(t2));
      Assert.AreEqual(-1, t2.CompareToBySourceChronology(t1));
    }

    [TestMethod]
    public void TestCompareToBySourceChronology_Changed()
    {
      Source s1 = new Source("s1", DateTime.UtcNow.AddDays(1));
      Source s2 = new Source("s2", DateTime.UtcNow.AddHours(1));
      Source s3 = new Source("s3", DateTime.UtcNow);
      Source s4 = new Source("s4", DateTime.UtcNow.AddDays(-1));

      Team t1 = new Team("team 1", s4);
      Team t2 = new Team("team 2", s3);

      Assert.AreEqual(-1, t1.CompareToBySourceChronology(t2));
      Assert.AreEqual(1, t2.CompareToBySourceChronology(t1));

      t1.AddTwitter("twit1", s1);
      t2.AddTwitter("twit2", s2);

      Assert.AreEqual(1, t1.CompareToBySourceChronology(t2));
      Assert.AreEqual(-1, t2.CompareToBySourceChronology(t1));
    }
  }
}