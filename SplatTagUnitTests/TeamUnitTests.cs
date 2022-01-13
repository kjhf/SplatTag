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
    public void TestCompareToBySourceAscending_Simple()
    {
      Source s1 = new Source("s1", DateTime.UtcNow.AddDays(1));
      Source s2 = new Source("s2", DateTime.UtcNow);

      Team t1 = new Team("team 1", s1);
      Team t2 = new Team("team 2", s2);

      Assert.AreEqual(1, t1.CompareToBySourceAscending(t2));
      Assert.AreEqual(-1, t2.CompareToBySourceAscending(t1));
      Assert.AreEqual(0, t2.CompareToBySourceAscending(t2));
    }

    [TestMethod]
    public void TestCompareToBySourceAscending_Mixed()
    {
      Source s1 = new Source("s1", DateTime.UtcNow.AddDays(1));
      Source s2 = new Source("s2", DateTime.UtcNow.AddHours(1));
      Source s3 = new Source("s3", DateTime.UtcNow);
      Source s4 = new Source("s4", DateTime.UtcNow.AddDays(-1));

      Team t1 = new Team("team 1", s1);
      Team t2 = new Team("team 2", s2);

      Assert.AreEqual(1, t1.CompareToBySourceAscending(t2));
      Assert.AreEqual(-1, t2.CompareToBySourceAscending(t1));

      t1.AddTwitter("twit1", s3);
      t2.AddTwitter("twit2", s4);

      Assert.AreEqual(1, t1.CompareToBySourceAscending(t2));
      Assert.AreEqual(-1, t2.CompareToBySourceAscending(t1));
    }

    [TestMethod]
    public void TestCompareToBySourceAscending_Changed()
    {
      Source s1 = new Source("s1", DateTime.UtcNow.AddDays(1));
      Source s2 = new Source("s2", DateTime.UtcNow.AddHours(1));
      Source s3 = new Source("s3", DateTime.UtcNow);
      Source s4 = new Source("s4", DateTime.UtcNow.AddDays(-1));

      // i.e. team 1 has the older information.
      Team t1 = new Team("team 1", s2);
      Team t2 = new Team("team 2", s1);
      Assert.AreEqual(-1, t1.CompareToBySourceAscending(t2));
      Assert.AreEqual(1, t2.CompareToBySourceAscending(t1));

      // add twitter handles so that team 2 now has the oldest information
      t1.AddTwitter("twit1", s3);
      t2.AddTwitter("twit2", s4);

      // team 2 should now be older
      Assert.AreEqual(-1, t2.CompareToBySourceAscending(t1));
      Assert.AreEqual(1, t1.CompareToBySourceAscending(t2));
    }

    [TestMethod]
    public void TestCompareToBySourceDescending_Changed()
    {
      Source s1 = new Source("s1", DateTime.UtcNow.AddDays(1));
      Source s2 = new Source("s2", DateTime.UtcNow.AddHours(1));
      Source s3 = new Source("s3", DateTime.UtcNow);
      Source s4 = new Source("s4", DateTime.UtcNow.AddDays(-1));

      // i.e. team 2 has the newer information.
      Team t1 = new Team("team 1", s4);
      Team t2 = new Team("team 2", s3);
      Assert.AreEqual(-1, t2.CompareToBySourceDescending(t1));
      Assert.AreEqual(1, t1.CompareToBySourceDescending(t2));

      // add twitter handles so that team 1 now has the newest information
      t1.AddTwitter("twit1", s1);
      t2.AddTwitter("twit2", s2);

      // team 1 should now be newer
      Assert.AreEqual(-1, t1.CompareToBySourceDescending(t2));
      Assert.AreEqual(1, t2.CompareToBySourceDescending(t1));
    }

    [TestMethod]
    public void TestCompareToBySourceAscending_NoChange()
    {
      Source s1 = new Source("s1", DateTime.UtcNow.AddDays(1));
      Source s2 = new Source("s2", DateTime.UtcNow.AddHours(1));
      Source s3 = new Source("s3", DateTime.UtcNow);
      Source s4 = new Source("s4", DateTime.UtcNow.AddDays(-1));

      // i.e. team 1 has the older information.
      Team t1 = new Team("team 1", s4);
      Team t2 = new Team("team 2", s2);
      Assert.AreEqual(-1, t1.CompareToBySourceAscending(t2));
      Assert.AreEqual(1, t2.CompareToBySourceAscending(t1));

      // add twitter handles so that both teams have older information,
      // and team 2 now has some information older than team 1,
      // but is still not as old as team 1's oldest information.
      t1.AddTwitter("twit1", s1);
      t2.AddTwitter("twit2", s3);

      // team 1 should still be older
      Assert.AreEqual(-1, t1.CompareToBySourceAscending(t2));
      Assert.AreEqual(1, t2.CompareToBySourceAscending(t1));
    }

    [TestMethod]
    public void TestCompareToBySourceDescending_NoChange()
    {
      Source s1 = new Source("s1", DateTime.UtcNow.AddDays(1));
      Source s2 = new Source("s2", DateTime.UtcNow.AddHours(1));
      Source s3 = new Source("s3", DateTime.UtcNow);
      Source s4 = new Source("s4", DateTime.UtcNow.AddDays(-1));

      // i.e. team 1 has the most recent information.
      Team t1 = new Team("team 1", s1);
      Team t2 = new Team("team 2", s3);
      Assert.AreEqual(-1, t1.CompareToBySourceDescending(t2));
      Assert.AreEqual(1, t2.CompareToBySourceDescending(t1));

      // add twitter handles so that both teams have older information,
      // and team 2 now has some information more recent than team 1,
      // but is still not as new as team 1's most recent information.
      t1.AddTwitter("twit1", s4);
      t2.AddTwitter("twit2", s2);

      // team 1 should still be newer
      Assert.AreEqual(-1, t1.CompareToBySourceDescending(t2));
      Assert.AreEqual(1, t2.CompareToBySourceDescending(t1));
    }
  }
}