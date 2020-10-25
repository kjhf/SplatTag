using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagCore;
using System.Collections.Generic;

namespace SplatTagUnitTests
{
  /// <summary>
  /// Friend Code matching unit tests
  /// </summary>
  [TestClass]
  public class FriendCodeUnitTests
  {
    [TestMethod]
    public void NoMatchNullString()
    {
      FriendCode.TryParse(null, out FriendCode friendCode);
      Assert.IsNull(friendCode);
    }

    [TestMethod]
    public void NoMatchEmptyString()
    {
      FriendCode.TryParse("", out FriendCode friendCode);
      Assert.IsNull(friendCode);
    }

    [TestMethod]
    public void NoMatchJunkString()
    {
      FriendCode.TryParse("aaa", out FriendCode friendCode);
      Assert.IsNull(friendCode);
    }

    [TestMethod]
    public void NoMatchJunkDigitsString()
    {
      FriendCode.TryParse("0123456789101112", out FriendCode friendCode);
      Assert.IsNull(friendCode);
    }

    [TestMethod]
    public void MatchDigitsString()
    {
      FriendCode.TryParse("111122223333", out FriendCode friendCode);
      Assert.IsNotNull(friendCode);
      Assert.AreEqual("1111-2222-3333", friendCode.ToString());
    }

    [TestMethod]
    public void MatchFCString()
    {
      FriendCode.TryParse("3333-4444-5555", out FriendCode friendCode);
      Assert.IsNotNull(friendCode);
      Assert.AreEqual("3333-4444-5555", friendCode.ToString());
    }

    [TestMethod]
    public void MatchFCStringWithLabel()
    {
      FriendCode.TryParse("SW: 3456.7654.9876", out FriendCode friendCode);
      Assert.IsNotNull(friendCode);
      Assert.AreEqual("3456-7654-9876", friendCode.ToString());
    }

    [TestMethod]
    public void MatchFCStringWithLabel2()
    {
      FriendCode.TryParse("SW-1234-5678-4321", out FriendCode friendCode);
      Assert.IsNotNull(friendCode);
      Assert.AreEqual("1234-5678-4321", friendCode.ToString());
    }
    
    [TestMethod]
    public void SeparatorTest()
    {
      FriendCode.TryParse("5555.6789 7890", out FriendCode friendCode);
      Assert.IsNotNull(friendCode);
      Assert.AreEqual("5555/6789/7890", friendCode.ToString("/"));
    }

    [TestMethod]
    public void JoinSeparatorTest()
    {
      FriendCode.TryParse("123456789", out FriendCode friendCode);
      Assert.IsNotNull(friendCode);
      Assert.AreEqual("000123456789", friendCode.ToString(""));
    }

    [TestMethod]
    public void FCInsideOtherTextShouldParse()
    {
      FriendCode.TryParse("This is a player name believe it or not with the fc 0123-4567-8912?", out FriendCode friendCode);
      Assert.IsNotNull(friendCode);
      Assert.AreEqual("0123.4567.8912", friendCode.ToString("."));
    }
  }
}