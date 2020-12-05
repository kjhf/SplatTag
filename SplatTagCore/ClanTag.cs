﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SplatTagCore
{
  public class ClanTag : Name
  {
    /// <summary>
    /// Tag layout option
    /// </summary>
    public TagOption LayoutOption { get; set; }

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
    /// Get the Tag
    /// </summary>
    public virtual string Tag => Value;

    /// <summary>
    /// Constructor for ClanTag
    /// </summary>
    public ClanTag(string tag, TagOption tagOption, IEnumerable<Source> sources)
      : base(tag, sources)
    {
      LayoutOption = tagOption;
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
          else if (playerName.StartsWith(TransformedName, StringComparison.OrdinalIgnoreCase))
          {
            playerName = playerName.Substring(TransformedName.Length).Trim();
          }
          break;

        case TagOption.Back:
          if (playerName.EndsWith(Value, StringComparison.OrdinalIgnoreCase))
          {
            playerName = playerName.Substring(0, playerName.Length - Value.Length).Trim();
          }
          else if (playerName.EndsWith(TransformedName, StringComparison.OrdinalIgnoreCase))
          {
            playerName = playerName.Substring(0, playerName.Length - TransformedName.Length).Trim();
          }
          break;

        case TagOption.Surrounding:
          if (playerName.StartsWith(Value[0].ToString(), StringComparison.OrdinalIgnoreCase) && playerName.EndsWith(Value[1].ToString(), StringComparison.OrdinalIgnoreCase))
          {
            playerName = playerName.Substring(1, playerName.Length - 2).Trim();
          }
          else if (playerName.StartsWith(TransformedName[0].ToString(), StringComparison.OrdinalIgnoreCase) && playerName.EndsWith(TransformedName[1].ToString(), StringComparison.OrdinalIgnoreCase))
          {
            playerName = playerName.Substring(1, playerName.Length - 2).Trim();
          }
          break;
      }
      return playerName;
    }

    /// <summary>
    /// Get the player name with the tag added
    /// </summary>
    public string CombineToPlayer(string playerName)
    {
      return LayoutOption switch
      {
        TagOption.Back => $"{playerName}{Value}",
        TagOption.Surrounding => $"{Value}{playerName}{Value}",
        _ => $"{Value}{playerName}",
      };
    }

    /// <summary>
    /// Update the <see cref="LayoutOption"/> based on the current tag and the example player name.
    /// </summary>
    public void CalculateTagOption(string examplePlayerName)
    {
      this.LayoutOption = CalculateTagOption(Value, examplePlayerName);
    }

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
        if (examplePlayerName.StartsWith(tag, StringComparison.OrdinalIgnoreCase) || examplePlayerName.StartsWith(transformedTag, StringComparison.OrdinalIgnoreCase))
        {
          return TagOption.Front;
        }
        else if (examplePlayerName.EndsWith(tag, StringComparison.OrdinalIgnoreCase) || examplePlayerName.EndsWith(transformedTag, StringComparison.OrdinalIgnoreCase))
        {
          return TagOption.Back;
        }

        if (transformedTag.Length == 2)
        {
          char first = transformedTag[0];
          char second = transformedTag[1];
          // If the tag has 2 characters, check 'surrounding' criteria which is take the
          // first character of the tag and check if the captain's name begins with this character,
          // then take the last character of the tag and check if the captain's name ends with this character.
          // e.g. Tag: //, Captain's name: /captain/
          if (examplePlayerName.StartsWith(first.ToString(), StringComparison.OrdinalIgnoreCase)
          && examplePlayerName.EndsWith(second.ToString(), StringComparison.OrdinalIgnoreCase))
          {
            return TagOption.Surrounding;
          }
        }

        return TagOption.Unknown;
      }
    }

    /// <summary>
    /// Get the tag's value.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return Value;
    }
  }
}