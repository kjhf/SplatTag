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
  }
}