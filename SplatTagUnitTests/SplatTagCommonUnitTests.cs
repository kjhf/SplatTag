using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagCore;
using System.Collections.Generic;

namespace SplatTagUnitTests
{
  [TestClass]
  public class SplatTagCommonUnitTests
  {
    /// <summary>
    /// Test match a player by (no match).
    /// </summary>
    [TestMethod]
    public void AddFriendCodes()
    {
      FriendCode fc1 = new FriendCode(new[] { (short)1234, (short)5678, (short)9012 });
      FriendCode fc2 = new FriendCode(new[] { (short)1234, (short)5678, (short)9012 });
      FriendCode fc3 = new FriendCode(new[] { (short)9999, (short)8888, (short)7777 });

      List<FriendCode> fcs = new List<FriendCode>();
      SplatTagCommon.InsertFrontUnique(fc1, fcs);
      Assert.AreEqual(1, fcs.Count);

      SplatTagCommon.InsertFrontUnique(fc2, fcs);
      Assert.AreEqual(1, fcs.Count, "FC should match so should not be added again.");

      SplatTagCommon.InsertFrontUnique(fc3, fcs);
      Assert.AreEqual(2, fcs.Count, "FC was not added.");
      Assert.AreEqual(fc3, fcs[0], "FC was not added to the front");

      SplatTagCommon.InsertFrontUnique(fc2, fcs);
      Assert.AreEqual(2, fcs.Count, "Should be 2 codes.");
      Assert.AreEqual(fc2, fcs[0], "FC was not moved to the front");
    }
  }
}