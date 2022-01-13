using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplatTagCore;

namespace SplatTagUnitTests
{
  /// <summary>
  /// Pronoun matching unit tests
  /// </summary>
  [TestClass]
  public class PronounUnitTests
  {
    private readonly Source s = Builtins.ManualSource;

    [TestMethod]
    public void NoMatchNullString()
    {
      var result = new Pronoun(null!, s);
      Assert.AreEqual(result.value, PronounFlags.NONE);
      Assert.AreEqual("none", result.ToString());
    }

    [TestMethod]
    public void NoMatchEmptyString()
    {
      var result = new Pronoun("", s);
      Assert.AreEqual(result.value, PronounFlags.NONE);
      Assert.AreEqual("none", result.ToString());
    }

    [TestMethod]
    public void NoMatchJunkString()
    {
      var result = new Pronoun("aaa", s);
      Assert.AreEqual(result.value, PronounFlags.NONE);
      Assert.AreEqual("none", result.ToString());
    }

    [TestMethod]
    public void NoMatchJunkDigitsString()
    {
      var result = new Pronoun("1111", s);
      Assert.AreEqual(result.value, PronounFlags.NONE);
      Assert.AreEqual("none", result.ToString());
    }

    [TestMethod]
    public void MatchSingleString_He()
    {
      var result = new Pronoun("he", s);
      Assert.AreEqual(result.value, PronounFlags.HE);
      Assert.AreEqual("he/him", result.ToString());
    }

    [TestMethod]
    public void MatchSingleString_He2()
    {
      const string test = "he/him";
      var result = new Pronoun(test, s);
      Assert.AreEqual(result.value, PronounFlags.HE);
      Assert.AreEqual(test, result.ToString());
    }

    [TestMethod]
    public void MatchSingleString_She()
    {
      var result = new Pronoun("She", s);
      Assert.AreEqual(result.value, PronounFlags.SHE);
      Assert.AreEqual("she/her", result.ToString());
    }

    [TestMethod]
    public void MatchSingleString_She2()
    {
      var result = new Pronoun("She/her", s);
      Assert.AreEqual(result.value, PronounFlags.SHE);
      Assert.AreEqual("she/her", result.ToString());
    }

    [TestMethod]
    public void MatchSingleString_They()
    {
      var result = new Pronoun("they", s);
      Assert.AreEqual(result.value, PronounFlags.THEY);
      Assert.AreEqual("they/them", result.ToString());
    }

    [TestMethod]
    public void MatchSingleString_They2()
    {
      const string test = "they/them";
      var result = new Pronoun(test, s);
      Assert.AreEqual(result.value, PronounFlags.THEY);
      Assert.AreEqual(test, result.ToString());
    }

    [TestMethod]
    public void MatchSingleString_It()
    {
      var result = new Pronoun("it", s);
      Assert.AreEqual(result.value, PronounFlags.IT);
      Assert.AreEqual("it/it", result.ToString());
    }

    [TestMethod]
    public void MatchSingleString_It2()
    {
      const string test = "it/it";
      var result = new Pronoun(test, s);
      Assert.AreEqual(result.value, PronounFlags.IT);
      Assert.AreEqual(test, result.ToString());
    }

    [TestMethod]
    public void MatchSingleString_All()
    {
      var result = new Pronoun("all pronouns", s);
      Assert.AreEqual(result.value, PronounFlags.ALL);
      Assert.AreEqual("any/all", result.ToString());
    }

    [TestMethod]
    public void MatchSingleString_All2()
    {
      var result = new Pronoun("pronouns: all", s);
      Assert.AreEqual(result.value, PronounFlags.ALL);
      Assert.AreEqual("any/all", result.ToString());
    }

    [TestMethod]
    public void MatchSingleString_Any()
    {
      var result = new Pronoun("any pronouns", s);
      Assert.AreEqual(result.value, PronounFlags.ALL);
      Assert.AreEqual("any/all", result.ToString());
    }

    [TestMethod]
    public void MatchSingleString_Any2()
    {
      var result = new Pronoun("pronouns: any", s);
      Assert.AreEqual(result.value, PronounFlags.ALL);
      Assert.AreEqual("any/all", result.ToString());
    }

    [TestMethod]
    public void MatchSingleString_Ask()
    {
      var result = new Pronoun("ask for pronouns", s);
      Assert.AreEqual(result.value, PronounFlags.ASK);
      Assert.AreEqual("ask", result.ToString());
    }

    [TestMethod]
    public void MatchSingleString_Ask2()
    {
      var result = new Pronoun("pronouns: ask", s);
      Assert.AreEqual(result.value, PronounFlags.ASK);
      Assert.AreEqual("ask", result.ToString());
    }

    [TestMethod]
    public void MatchMultipleString_HeShe()
    {
      const string test = "he/she";
      var result = new Pronoun(test, s);
      Assert.AreEqual(result.value, PronounFlags.HE | PronounFlags.SHE);
      Assert.AreEqual(test, result.ToString());
    }

    [TestMethod]
    public void MatchMultipleString_HeShe_Reverse()
    {
      const string test = "she/he";
      var result = new Pronoun(test, s);
      Assert.AreEqual(result.value, PronounFlags.HE | PronounFlags.SHE | PronounFlags.ORDER_RTL);
      Assert.AreEqual(test, result.ToString());
    }

    [TestMethod]
    public void MatchMultipleString_HeThey()
    {
      const string test = "he/they";
      var result = new Pronoun(test, s);
      Assert.AreEqual(result.value, PronounFlags.HE | PronounFlags.THEY);
      Assert.AreEqual(test, result.ToString());
    }

    [TestMethod]
    public void MatchMultipleString_HeThey_Reverse()
    {
      const string test = "they/he";
      var result = new Pronoun(test, s);
      Assert.AreEqual(result.value, PronounFlags.HE | PronounFlags.THEY | PronounFlags.ORDER_RTL);
      Assert.AreEqual(test, result.ToString());
    }

    [TestMethod]
    public void MatchMultipleString_SheThey()
    {
      const string test = "she/they";
      var result = new Pronoun(test, s);
      Assert.AreEqual(result.value, PronounFlags.SHE | PronounFlags.THEY);
      Assert.AreEqual(test, result.ToString());
    }

    [TestMethod]
    public void MatchMultipleString_SheThey_Reverse()
    {
      const string test = "they/she";
      var result = new Pronoun(test, s);
      Assert.AreEqual(result.value, PronounFlags.SHE | PronounFlags.THEY | PronounFlags.ORDER_RTL);
      Assert.AreEqual(test, result.ToString());
    }

    [TestMethod]
    public void MatchMultipleString_HeIt()
    {
      const string test = "he/it";
      var result = new Pronoun(test, s);
      Assert.AreEqual(result.value, PronounFlags.HE | PronounFlags.IT);
      Assert.AreEqual(test, result.ToString());
    }

    [TestMethod]
    public void MatchMultipleString_HeIt_Reverse()
    {
      const string test = "it/he";
      var result = new Pronoun(test, s);
      Assert.AreEqual(result.value, PronounFlags.HE | PronounFlags.IT | PronounFlags.ORDER_RTL);
      Assert.AreEqual(test, result.ToString());
    }

    [TestMethod]
    public void MatchMultipleString_SheNeo()
    {
      const string test = "she/ve";
      var result = new Pronoun(test, s);
      Assert.AreEqual(result.value, PronounFlags.SHE | PronounFlags.NEO);
      Assert.AreEqual("she/" + Pronoun.NEO_PLACEHOLDER, result.ToString());
    }

    [TestMethod]
    public void MatchMultipleString_SheNeo_Reverse()
    {
      const string test = "ve/she";
      var result = new Pronoun(test, s);
      Assert.AreEqual(result.value, PronounFlags.SHE | PronounFlags.NEO | PronounFlags.ORDER_RTL);
      Assert.AreEqual(Pronoun.NEO_PLACEHOLDER + "/she", result.ToString());
    }

    [TestMethod]
    public void MatchNeoBranches()
    {
      foreach (var test in new string[] { "ce", "cer", "eir", "eirself", "em", "emself", "ey", "ne", "sie", "ve", "ver", "vis", "xe", "xem", "xemself", "xyr", "ze", "zie", "zirs" })
      {
        var result = new Pronoun(test, s);
        Assert.AreEqual(PronounFlags.NEO, result.value, $"Expected Neo parse for pronoun {test}");
        Assert.AreEqual(Pronoun.NEO_PLACEHOLDER, result.ToString(), $"Expected Neo parse for pronoun {test}");
      }
    }

    [TestMethod]
    public void NoMatchNeoBranches()
    {
      foreach (var test in new string[] { "care", "air", "airs", "cat", "dog", "them", "themself", "ye", "yes", "sit", "vet", "visit", "ex", "zero" })
      {
        var result = new Pronoun(test, s);
        Assert.AreNotEqual(result.value, PronounFlags.NEO, $"Unexpected Neo parse for pronoun {test}");
        Assert.AreNotEqual(Pronoun.NEO_PLACEHOLDER, result.ToString(), $"Unexpected Neo parse for pronoun {test}");
      }
    }

    [TestMethod]
    public void PronounsInsideTextShouldBeParsed()
    {
      var result = new Pronoun("This is a player description with some pronouns he/she/they with an fc 0123-4567-8912", s);
      Assert.AreEqual(result.value, PronounFlags.HE | PronounFlags.SHE | PronounFlags.THEY);
      Assert.AreEqual("he/she/they", result.ToString());
    }

    [TestMethod]
    public void PronounsInsideTextShouldBeParsed_Reverse()
    {
      var result = new Pronoun("This is a player description with some pronouns they/she with an fc 0123-4567-8912", s);
      Assert.AreEqual(result.value, PronounFlags.SHE | PronounFlags.THEY | PronounFlags.ORDER_RTL);
      Assert.AreEqual("they/she", result.ToString());
    }

    [TestMethod]
    public void PronounsFromProfile_1()
    {
      var result = new Pronoun("**she/they** • chaotic kazoo bard •", s);
      Assert.AreEqual(result.value, PronounFlags.SHE | PronounFlags.THEY);
      Assert.AreEqual("she/they", result.ToString());
    }

    [TestMethod]
    public void PronounsFromProfile_2()
    {
      var result = new Pronoun("he/him or they/them pronouns please", s);
      Assert.AreEqual(result.value, PronounFlags.HE | PronounFlags.THEY);
      Assert.AreEqual("he/they", result.ToString());
    }

    [TestMethod]
    public void PronounsFromProfile_3()
    {
      var result = new Pronoun("They/Them - 99 - UTC", s);
      Assert.AreEqual(result.value, PronounFlags.THEY);
      Assert.AreEqual("they/them", result.ToString());
    }

    [TestMethod]
    public void PronounsFromProfile_4()
    {
      var result = new Pronoun("Name/Nick - they/she", s);
      Assert.AreEqual(result.value, PronounFlags.SHE | PronounFlags.THEY | PronounFlags.ORDER_RTL);
      Assert.AreEqual("they/she", result.ToString());
    }

    [TestMethod]
    public void PronounsFromProfile_5()
    {
      var result = new Pronoun("I'm a streamer, 99 years old, ⛳. She/Her. I enjoy Splatoon.", s);
      Assert.AreEqual(result.value, PronounFlags.SHE);
      Assert.AreEqual("she/her", result.ToString());
    }

    [TestMethod]
    public void PronounsFromProfile_6()
    {
      var result = new Pronoun("I like Splatoon `he/him/ne`.", s);
      Assert.AreEqual(result.value, PronounFlags.HE | PronounFlags.NEO);
      Assert.AreEqual("he/" + Pronoun.NEO_PLACEHOLDER, result.ToString());
    }
  }
}