using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagCore;

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
      bool success = FriendCode.TryParse("SW: 3456.7654.9876", out FriendCode friendCode);
      Assert.IsNotNull(friendCode);
      Assert.IsTrue(success);
      Assert.AreEqual("3456-7654-9876", friendCode.ToString());
    }

    [TestMethod]
    public void MatchFCStringWithLabel2()
    {
      bool success = FriendCode.TryParse("SW-1234-5678-4321", out FriendCode friendCode);
      Assert.IsNotNull(friendCode);
      Assert.IsTrue(success);
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

    [TestMethod]
    public void StripFCInsideName()
    {
      var (friendCode, stripped) = FriendCode.ParseAndStripFriendCode(":) Some pleb (SW: 0123-4567-8912)");
      Assert.IsNotNull(friendCode);
      Assert.IsNotNull(stripped);
      Assert.AreEqual("0123 4567 8912", friendCode.ToString(" "));
      Assert.AreEqual(":) Some pleb", stripped);
    }

    [TestMethod]
    public void StripFCInsideName2()
    {
      var (friendCode, stripped) = FriendCode.ParseAndStripFriendCode("My name 1234 - 5678 - 9012");
      Assert.IsNotNull(friendCode);
      Assert.IsNotNull(stripped);
      Assert.AreEqual("1234-5678-9012", friendCode.ToString());
      Assert.AreEqual("My name", stripped);
    }
  }
}