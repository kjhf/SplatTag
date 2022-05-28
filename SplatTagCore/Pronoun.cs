using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace SplatTagCore
{
  [Flags]
  public enum PronounFlags : byte
  {
    NONE = 0,
    HE = 1 << 0,
    SHE = 1 << 1,
    THEY = 1 << 2,
    IT = 1 << 3,
    NEO = 1 << 4,
    ASK = 1 << 5,

    ALL = HE | SHE | THEY | IT | NEO | ASK,

    /// <summary>If set, the pronoun flags should be read in order right to left, e.g. they/she instead of she/they</summary>
    ORDER_RTL = 1 << 7
  }

  [Serializable]
  public class Pronoun : ISerializable, IReadonlySourceable
  {
    public const string NEO_PLACEHOLDER = "(neo)";
    private static readonly Regex heRegex = new(@"(^|\W)(he)(\W|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex sheRegex = new(@"(^|\W)(she)(\W|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex theyRegex = new(@"(^|\W)(they)(\W|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex itRegex = new(@"(^|\W)(it)(\W|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex neoRegex = new(@"(^|\W)(em|([censvxz])([iey])+r?m?|xyr)s?(elf)?(\W|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex allRegex = new(@"^all$|(^|\W)((pronouns? ?([ :]) ?(all|any))|((all|any) pronouns?))(\W|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex askRegex = new(@"^ask$|(^|\W)((pronouns? ?([ :]) ?(ask))|(ask (for )?pronouns?))(\W|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    public readonly PronounFlags value;
    public readonly Source source;

    /// <summary>
    /// Default construct a pronoun with NONE set.
    /// </summary>
    public Pronoun()
      : this(PronounFlags.NONE, Builtins.BuiltinSource)
    {
    }

    /// <summary>
    /// Construct a pronoun object with the user's pronouns set and the source.
    /// </summary>
    /// <param name="pronoun"></param>
    public Pronoun(PronounFlags pronoun, Source source)
    {
      this.value = pronoun;
      this.source = source;
    }

    /// <summary>
    /// Construct a Pronoun object with the given pronoun string.
    /// The constructor will parse the entire message and will positively match contained pronouns.
    /// </summary>
    /// <remarks>
    /// e.g. "she/her" will be SHE
    /// e.g. "he him they them" will be HE | THEY
    /// e.g. "pronouns: they / she" will be SHE | THEY | ORDER_RTL
    /// </remarks>
    public Pronoun(string pronouns, Source source)
    {
      this.source = source;
      this.value = new PronounFlags();
      if (!string.IsNullOrWhiteSpace(pronouns))
      {
        var heMatch = heRegex.Match(pronouns);
        if (heMatch.Success)
        {
          value |= PronounFlags.HE;
        }
        var sheMatch = sheRegex.Match(pronouns);
        if (sheMatch.Success)
        {
          value |= PronounFlags.SHE;

          if (heMatch.Success && heMatch.Index > sheMatch.Index)
          {
            value |= PronounFlags.ORDER_RTL;
          }
        }
        var theyMatch = theyRegex.Match(pronouns);
        if (theyMatch.Success)
        {
          value |= PronounFlags.THEY;

          if (heMatch.Success && heMatch.Index > theyMatch.Index)
          {
            value |= PronounFlags.ORDER_RTL;
          }
          if (sheMatch.Success && sheMatch.Index > theyMatch.Index)
          {
            value |= PronounFlags.ORDER_RTL;
          }
        }
        var itMatch = itRegex.Match(pronouns);
        if (itMatch.Success)
        {
          value |= PronounFlags.IT;

          if (heMatch.Success && heMatch.Index > itMatch.Index)
          {
            value |= PronounFlags.ORDER_RTL;
          }
          if (sheMatch.Success && sheMatch.Index > itMatch.Index)
          {
            value |= PronounFlags.ORDER_RTL;
          }
          if (theyMatch.Success && theyMatch.Index > itMatch.Index)
          {
            value |= PronounFlags.ORDER_RTL;
          }
        }
        var neoMatch = neoRegex.Match(pronouns);
        if (neoMatch.Success)
        {
          value |= PronounFlags.NEO;

          if (heMatch.Success && heMatch.Index > neoMatch.Index)
          {
            value |= PronounFlags.ORDER_RTL;
          }
          if (sheMatch.Success && sheMatch.Index > neoMatch.Index)
          {
            value |= PronounFlags.ORDER_RTL;
          }
          if (theyMatch.Success && theyMatch.Index > neoMatch.Index)
          {
            value |= PronounFlags.ORDER_RTL;
          }
          if (itMatch.Success && itMatch.Index > neoMatch.Index)
          {
            value |= PronounFlags.ORDER_RTL;
          }
        }
        var allMatch = allRegex.Match(pronouns);
        if (allMatch.Success)
        {
          value |= PronounFlags.ALL;
        }
        var askMatch = askRegex.Match(pronouns);
        if (askMatch.Success)
        {
          value |= PronounFlags.ASK;
        }
      }
    }

    public IReadOnlyList<Source> Sources => new Source[] { source };

    /// <summary>
    /// Overridden ToString, returns the pronoun representation in a standard form, e.g. she/her, they/she, etc.
    /// </summary>
    public override string ToString()
    {
      switch (this.value)
      {
        case PronounFlags.NONE: return "none";
        case PronounFlags.ALL: return "any/all";
        case PronounFlags.HE: return "he/him";
        case PronounFlags.SHE: return "she/her";
        case PronounFlags.THEY: return "they/them";
        case PronounFlags.IT: return "it/it";
        case PronounFlags.NEO: return NEO_PLACEHOLDER;
        case PronounFlags.ASK: return "ask";
        default:
        {
          var sb = new StringBuilder();
          var reversed = (this.value & PronounFlags.ORDER_RTL) != 0;
          if ((this.value & PronounFlags.HE) != 0)
          {
            if (reversed)
            {
              sb.Insert(0, "/he");
            }
            else
            {
              sb.Append("he/");
            }
          }
          if ((this.value & PronounFlags.SHE) != 0)
          {
            if (reversed)
            {
              sb.Insert(0, "/she");
            }
            else
            {
              sb.Append("she/");
            }
          }
          if ((this.value & PronounFlags.THEY) != 0)
          {
            if (reversed)
            {
              sb.Insert(0, "/they");
            }
            else
            {
              sb.Append("they/");
            }
          }
          if ((this.value & PronounFlags.IT) != 0)
          {
            if (reversed)
            {
              sb.Insert(0, "/it");
            }
            else
            {
              sb.Append("it/");
            }
          }
          if ((this.value & PronounFlags.NEO) != 0)
          {
            if (reversed)
            {
              sb.Insert(0, "/" + NEO_PLACEHOLDER);
            }
            else
            {
              sb.Append(NEO_PLACEHOLDER + "/");
            }
          }
          if ((this.value & PronounFlags.ASK) != 0)
          {
            if (reversed)
            {
              sb.Insert(0, "/ask");
            }
            else
            {
              sb.Append("ask/");
            }
          }

          if (sb[0] == '/')
          {
            sb.Remove(0, 1);
          }
          if (sb[^1] == '/')
          {
            sb.Remove(sb.Length - 1, 1);
          }
          return sb.ToString();
        }
      }
    }

    #region Serialization

    protected Pronoun(SerializationInfo info, StreamingContext context)
    {
      this.value = (PronounFlags)info.GetByte("P");
      var source = (string)info.GetValue("S", typeof(string));
      var converter = context.Context as Source.SourceStringConverter ?? new Source.SourceStringConverter();
      this.source = converter.Convert(source, Source.SourceStringConverter.ConstructSource);
    }

    // Serialize as dict
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("P", (byte)this.value);
      info.AddValue("S", this.source.Id);
    }

    #endregion Serialization
  }
}