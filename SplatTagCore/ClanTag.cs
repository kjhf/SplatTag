using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SplatTagCore
{
  [Serializable]
  public class ClanTag : Name
  {
    private static readonly char[] tagDelimiters = new[] { ' ', '•', '_', '.', '⭐', '~', ']', '}', ')', '>' };

    /// <summary>
    /// Construct a ClanTag based on the tag and the source
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="source"></param>
    public ClanTag(string tag, Source source)
      : this(tag, TagOption.Unknown, source.AsEnumerable())
    {
    }

    /// <summary>
    /// Construct a ClanTag based on the tag and the source
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="sources"></param>
    public ClanTag(string tag, IEnumerable<Source> sources)
      : this(tag, TagOption.Unknown, sources)
    {
    }

    /// <summary>
    /// Constructor for ClanTag
    /// </summary>
    public ClanTag(string tag, TagOption tagOption, IEnumerable<Source> sources)
      : base(tag, sources)
    {
      LayoutOption = tagOption;
      if (tagOption == TagOption.Surrounding && tag.Length == 2 && tag[0] == tag[1])
      {
        Value = tag[0].ToString();
      }
    }

    /// <summary>
    /// Tag layout option
    /// </summary>
    public TagOption LayoutOption { get; set; }

    /// <summary>
    /// Get the Tag
    /// </summary>
    public virtual string Tag => Value;

    /// <summary>
    /// Calculates the <see cref="ClanTagOption"/> based on the tag and the example player name.
    /// </summary>
    public static TagOption CalculateTagOption(string tag, string examplePlayerName)
    {
      string transformedTag = tag.TransformString();
      if (string.IsNullOrWhiteSpace(transformedTag))
      {
        return TagOption.Unknown;
      }
      else
      {
        if (examplePlayerName.StartsWith(tag, StringComparison.OrdinalIgnoreCase)
          && examplePlayerName.EndsWith(tag, StringComparison.OrdinalIgnoreCase))
        {
          return TagOption.Surrounding;
        }
        else if (examplePlayerName.StartsWith(tag, StringComparison.OrdinalIgnoreCase) || examplePlayerName.StartsWith(transformedTag, StringComparison.OrdinalIgnoreCase))
        {
          return TagOption.Front;
        }
        else if (examplePlayerName.EndsWith(tag, StringComparison.OrdinalIgnoreCase) || examplePlayerName.EndsWith(transformedTag, StringComparison.OrdinalIgnoreCase))
        {
          return TagOption.Back;
        }

        return TagOption.Unknown;
      }
    }

    /// <summary>
    /// Calculate best guess at the tag for this team based on the player names of the team.
    /// This attempts front tag and symbols and surrounding symbols.
    /// </summary>
    /// <param name="playerNames"></param>
    /// <param name="source"></param>
    public static ClanTag? CalculateTagFromNames(IList<string> playerNames, Source source)
    {
      if (playerNames.Count <= 1) return null;

      var tagCandidates = new ConcurrentDictionary<(string, TagOption), int>();
      Parallel.ForEach(playerNames, (name) =>
      {
        // Starts with symbol(s)
        var frontMatch = Regex.Match(name, @"^[^\w\s]+");

        // Ends with symbol(s)
        var backMatch = Regex.Match(name, @"[^\w\s]+$");

        // Check if the tag is surrounding
        if (frontMatch.Success && backMatch.Success && frontMatch.Value == backMatch.Value)
        {
          tagCandidates.AddOrUpdate((frontMatch.Value, TagOption.Surrounding), 1, (_, count) => count + 1);
        }
        else
        {
          // Otherwise, check front symbols, then front & back with delimiters
          if (frontMatch.Success)
          {
            tagCandidates.AddOrUpdate((frontMatch.Value, TagOption.Front), 1, (_, count) => count + 1);
          }

          // Has tag then a delimiter
          foreach (char delimiter in tagDelimiters)
          {
            string[] split = name.Split(delimiter);
            if (split.Length > 1)
            {
              tagCandidates.AddOrUpdate((split[0] + delimiter, TagOption.Front), 1, (_, count) => count + 1);
              tagCandidates.AddOrUpdate((delimiter + split[split.Length - 1], TagOption.Back), 1, (_, count) => count + 1);
            }
          }
        }
      });
      var bestCandidate = tagCandidates.OrderByDescending(x => x.Value).FirstOrDefault();
      if (bestCandidate.Value >= (playerNames.Count / 2))
      {
        if (SplatTagController.Verbose)
        {
          Console.WriteLine($"Tag deduced! tag={bestCandidate.Key.Item1} option={bestCandidate.Key.Item2} counted {bestCandidate.Value} times!");
        }
        return new ClanTag(tag: bestCandidate.Key.Item1, tagOption: bestCandidate.Key.Item2, sources: new[] { source });
      }
      else
      {
        return null;
      }
    }

    /// <summary>
    /// Update the <see cref="LayoutOption"/> based on the current tag and the example player name.
    /// </summary>
    public void CalculateTagOption(string examplePlayerName)
    {
      this.LayoutOption = CalculateTagOption(Value, examplePlayerName);
    }

    /// <summary>
    /// Get the player name with the tag added
    /// </summary>
    public string CombineToPlayer(string playerName)
    {
      return LayoutOption switch
      {
        TagOption.Back => playerName + Value,
        TagOption.Surrounding => Value + playerName + Value,
        _ => Value + playerName,
      };
    }

    /// <summary>
    /// Get the player name with the tag stripped.
    /// </summary>
    public string StripFromPlayer(string playerName)
    {
      switch (LayoutOption)
      {
        default:
          if (playerName.StartsWith(Value, StringComparison.OrdinalIgnoreCase))
          {
            playerName = playerName.Substring(Value.Length).Trim();
          }
          else if (playerName.StartsWith(Transformed, StringComparison.OrdinalIgnoreCase))
          {
            playerName = playerName.Substring(Transformed.Length).Trim();
          }
          break;

        case TagOption.Back:
          if (playerName.EndsWith(Value, StringComparison.OrdinalIgnoreCase))
          {
            playerName = playerName.Substring(0, playerName.Length - Value.Length).Trim();
          }
          else if (playerName.EndsWith(Transformed, StringComparison.OrdinalIgnoreCase))
          {
            playerName = playerName.Substring(0, playerName.Length - Transformed.Length).Trim();
          }
          break;

        case TagOption.Surrounding:
          if (playerName.StartsWith(Value, StringComparison.OrdinalIgnoreCase) && playerName.EndsWith(Value, StringComparison.OrdinalIgnoreCase))
          {
            playerName = playerName.Substring(1, playerName.Length - 2).Trim();
          }
          break;
      }
      return playerName;
    }

    /// <summary>
    /// Get the tag's value.
    /// </summary>
    public override string ToString()
    {
      return Value ?? base.ToString();
    }

    #region Serialization

    // Deserialize
    protected ClanTag(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      this.LayoutOption = info.GetEnumOrDefault("LayoutOption", TagOption.Unknown);
    }

    // Serialize
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("LayoutOption", this.LayoutOption.ToString());
    }

    #endregion Serialization
  }
}