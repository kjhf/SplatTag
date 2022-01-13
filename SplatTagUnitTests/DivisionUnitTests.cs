using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagCore;

namespace SplatTagUnitTests
{
  /// <summary>
  /// Division unit tests
  /// </summary>
  [TestClass]
  public class DivisionUnitTests
  {
    [TestMethod]
    public void UnknownIsUnknown()
    {
      Assert.IsTrue(Division.Unknown.IsUnknown);
    }

    [TestMethod]
    public void KnownIsNotUnknown()
    {
      Assert.IsFalse(new Division(1, DivType.LUTI).IsUnknown);
    }

    [TestMethod]
    public void TestComparison()
    {
      var div1 = new Division(1, DivType.LUTI);
      var div2 = new Division(2, DivType.LUTI);
      Assert.IsTrue(div1 < div2);
      Assert.IsTrue(div2 >= div1);
    }

    [TestMethod]
    public void Division_Constructor()
    {
      var division = new Division(1, DivType.EBTV);
      Assert.AreEqual("EBTV  Div 1", division.ToString());
      Assert.AreEqual(string.Empty, division.Season);
      Assert.AreEqual(DivType.EBTV, division.DivType);
      Assert.AreEqual(1, division.Value);
    }

    [TestMethod]
    public void Division_Constructor_with_Season()
    {
      const string season = "Season2";
      var division = new Division(3, DivType.DSB, season);
      Assert.AreEqual($"DSB {season} Div 3", division.ToString());
      Assert.AreEqual(season, division.Season);
      Assert.AreEqual(DivType.DSB, division.DivType);
      Assert.AreEqual(3, division.Value);
    }

    [TestMethod]
    public void SerializeDivision_1()
    {
      const string test = "LUTI SX Div X+";
      var division = new Division(test);
      Assert.AreEqual("SX", division.Season);
      Assert.AreEqual(DivType.LUTI, division.DivType);
      Assert.AreEqual(Division.X_PLUS, division.Value);
    }

    [TestMethod]
    public void SerializeDivision_RoundTrip()
    {
      const string season = "S10";
      var division = new Division(Division.X, DivType.DSB, season);
      Assert.AreEqual($"DSB {season} Div X", division.ToString());
      Assert.AreEqual(season, division.Season);
      Assert.AreEqual(DivType.DSB, division.DivType);
      Assert.AreEqual(Division.X, division.Value);
      division = new Division(division.ToString());
      Assert.AreEqual($"DSB {season} Div X", division.ToString());
      Assert.AreEqual(season, division.Season);
      Assert.AreEqual(DivType.DSB, division.DivType);
      Assert.AreEqual(Division.X, division.Value);
    }
  }
}