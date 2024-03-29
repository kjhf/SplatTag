﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public void NoCodeProperty()
    {
      var fc = new FriendCode("1111-2222-3333");
      Assert.IsFalse(fc.NoCode);
      fc = FriendCode.NO_FRIEND_CODE;
      Assert.IsTrue(fc.NoCode);
    }

    [TestMethod]
    public void NoMatchNullString()
    {
      bool result = FriendCode.TryParse(null!, out FriendCode friendCode);
      Assert.AreEqual(FriendCode.NO_FRIEND_CODE, friendCode);
      Assert.IsFalse(result);
    }

    [TestMethod]
    public void NoMatchEmptyString()
    {
      bool result = FriendCode.TryParse("", out FriendCode friendCode);
      Assert.AreEqual(FriendCode.NO_FRIEND_CODE, friendCode);
      Assert.IsFalse(result);
    }

    [TestMethod]
    public void NoMatchJunkString()
    {
      bool result = FriendCode.TryParse("aaa", out FriendCode friendCode);
      Assert.AreEqual(FriendCode.NO_FRIEND_CODE, friendCode);
      Assert.IsFalse(result);
    }

    [TestMethod]
    public void NoMatchZeros()
    {
      bool result = FriendCode.TryParse("0000-0000-0000", out FriendCode friendCode);
      Assert.AreEqual(FriendCode.NO_FRIEND_CODE, friendCode);
      Assert.IsFalse(result);
    }

    [TestMethod]
    public void NoMatchBadFC()
    {
      bool result = FriendCode.TryParse("0000-0001-0002", out FriendCode friendCode);
      Assert.AreEqual(FriendCode.NO_FRIEND_CODE, friendCode);
      Assert.IsFalse(result);
    }

    [TestMethod]
    public void NoMatchJunkDigitsString()
    {
      bool result = FriendCode.TryParse("0123456789101112", out FriendCode friendCode);
      Assert.AreEqual(FriendCode.NO_FRIEND_CODE, friendCode);
      Assert.IsFalse(result);
    }

    [TestMethod]
    public void MatchDigitsString()
    {
      bool result = FriendCode.TryParse("111122223333", out FriendCode friendCode);
      Assert.AreNotEqual(FriendCode.NO_FRIEND_CODE, friendCode);
      Assert.IsTrue(result);
      Assert.AreEqual("1111-2222-3333", friendCode.ToString());
    }

    [TestMethod]
    public void MatchFCString()
    {
      bool result = FriendCode.TryParse("3333-4444-5555", out FriendCode friendCode);
      Assert.AreNotEqual(FriendCode.NO_FRIEND_CODE, friendCode);
      Assert.IsTrue(result);
      Assert.AreEqual("3333-4444-5555", friendCode.ToString());
    }

    [TestMethod]
    public void MatchFCStringWithLabel()
    {
      bool success = FriendCode.TryParse("SW: 3456.7654.9876", out FriendCode friendCode);
      Assert.AreNotEqual(FriendCode.NO_FRIEND_CODE, friendCode);
      Assert.IsTrue(success);
      Assert.AreEqual("3456-7654-9876", friendCode.ToString());
    }

    [TestMethod]
    public void MatchFCStringWithLabel2()
    {
      bool success = FriendCode.TryParse("SW-1234-5678-4321", out FriendCode friendCode);
      Assert.AreNotEqual(FriendCode.NO_FRIEND_CODE, friendCode);
      Assert.IsTrue(success);
      Assert.AreEqual("1234-5678-4321", friendCode.ToString());
    }

    [TestMethod]
    public void SeparatorTest()
    {
      bool success = FriendCode.TryParse("5555.6789 7890", out FriendCode friendCode);
      Assert.AreNotEqual(FriendCode.NO_FRIEND_CODE, friendCode);
      Assert.IsTrue(success);
      Assert.AreEqual("5555/6789/7890", friendCode.ToString("/"));
    }

    [TestMethod]
    public void JoinSeparatorTest()
    {
      bool success = FriendCode.TryParse("123456789", out FriendCode friendCode);
      Assert.AreNotEqual(FriendCode.NO_FRIEND_CODE, friendCode);
      Assert.IsTrue(success);
      Assert.AreEqual("000123456789", friendCode.ToString(""));
    }

    [TestMethod]
    public void FCInsideOtherTextShouldParse()
    {
      bool success = FriendCode.TryParse("This is a player name believe it or not with the fc 0123-4567-8912?", out FriendCode friendCode);
      Assert.AreNotEqual(FriendCode.NO_FRIEND_CODE, friendCode);
      Assert.IsTrue(success);
      Assert.AreEqual("0123.4567.8912", friendCode.ToString("."));
    }

    [TestMethod]
    public void StripFCInsideName()
    {
      var (friendCode, stripped) = FriendCode.ParseAndStripFriendCode(":) Some pleb (SW: 0123-4567-8912)");
      Assert.AreNotEqual(FriendCode.NO_FRIEND_CODE, friendCode);
      Assert.IsNotNull(stripped);
      Assert.AreEqual("0123 4567 8912", friendCode.ToString(" "));
      Assert.AreEqual(":) Some pleb", stripped);
    }

    [TestMethod]
    public void StripFCInsideName2()
    {
      var (friendCode, stripped) = FriendCode.ParseAndStripFriendCode("My name 1234 - 5678 - 9012");
      Assert.AreNotEqual(FriendCode.NO_FRIEND_CODE, friendCode);
      Assert.IsNotNull(stripped);
      Assert.AreEqual("1234-5678-9012", friendCode.ToString());
      Assert.AreEqual("My name", stripped);
    }

    [TestMethod]
    public void NegativeTestStripFCFromName()
    {
      var (friendCode, stripped) = FriendCode.ParseAndStripFriendCode("SomeSquid0123");
      Assert.AreEqual(FriendCode.NO_FRIEND_CODE, friendCode);
      Assert.IsNotNull(stripped);
      Assert.AreEqual("SomeSquid0123", stripped);
    }

    [TestMethod]
    public void NegativeTestStripFCFromName2()
    {
      var (friendCode, stripped) = FriendCode.ParseAndStripFriendCode("0123SomeSquid4567");
      Assert.AreEqual(FriendCode.NO_FRIEND_CODE, friendCode);
      Assert.IsNotNull(stripped);
      Assert.AreEqual("0123SomeSquid4567", stripped);
    }

    [TestMethod]
    public void NegativeTestStripFCFromName3()
    {
      var (friendCode, stripped) = FriendCode.ParseAndStripFriendCode("Ludic--<");
      Assert.AreEqual(FriendCode.NO_FRIEND_CODE, friendCode);
      Assert.IsNotNull(stripped);
      Assert.AreEqual("Ludic--<", stripped);
    }

    [TestMethod]
    public void ToULongRoundTrip()
    {
      bool result = FriendCode.TryParse("0001-2345-6789", out FriendCode friendCode);
      Assert.AreNotEqual(FriendCode.NO_FRIEND_CODE, friendCode);
      Assert.IsTrue(result);
      Assert.AreEqual("0001-2345-6789", friendCode.ToString());
      Assert.AreEqual((ulong)123456789, friendCode.ToULong());

      var fc2 = new FriendCode(123456789);
      Assert.AreEqual(friendCode, fc2);
    }

    [TestMethod]
    public void ToULongRoundTrip_2()
    {
      bool result = FriendCode.TryParse("9999-9999-9998", out FriendCode friendCode);
      Assert.AreNotEqual(FriendCode.NO_FRIEND_CODE, friendCode);
      Assert.IsTrue(result);
      Assert.AreEqual("9999-9999-9998", friendCode.ToString());
      Assert.AreEqual((ulong)999999999998, friendCode.ToULong());

      var fc2 = new FriendCode(999999999998);
      Assert.AreEqual(fc2, friendCode);
    }
  }
}