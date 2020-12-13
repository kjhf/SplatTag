﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore.Social
{
  [Serializable]
  public class Battlefy : ISerializable
  {
    /// <summary>
    /// Back-store for the Battlefy slugs
    /// </summary>
    private readonly List<BattlefySocial> slugs = new List<BattlefySocial>();

    /// <summary>
    /// Back-store for the Battlefy usernames
    /// </summary>
    private readonly List<Name> usernames = new List<Name>();

    public Battlefy()
    {
    }

    /// <summary>
    /// The persistent Battlefy slugs
    /// </summary>
    public IReadOnlyList<BattlefySocial> Slugs => slugs;

    /// <summary>
    /// The Battlefy usernames
    /// </summary>
    public IReadOnlyList<Name> Usernames => usernames;

    /// <summary>
    /// Add a new Battlefy slug to the front of this Battlefy profile
    /// </summary>
    /// <param name="ids"></param>
    public void AddSlug(string slug, Source source)
    {
      SplatTagCommon.InsertFrontUniqueSourced(new BattlefySocial(slug, source), this.slugs);
    }

    /// <summary>
    /// Add new Battlefy slugs to the front of this Battlefy profile
    /// </summary>
    /// <param name="ids"></param>
    public void AddSlugs(IEnumerable<BattlefySocial> slugs)
    {
      SplatTagCommon.AddNames(slugs, this.slugs);
    }

    /// <summary>
    /// Add a new Battlefy username to the front of this Battlefy profile
    /// </summary>
    public void AddUsername(string username, Source source)
    {
      SplatTagCommon.AddName(new Name(username, source), this.usernames);
    }

    /// <summary>
    /// Add new Battlefy usernames to the front of this Battlefy profile
    /// </summary>
    public void AddUsernames(IEnumerable<Name> usernames)
    {
      SplatTagCommon.AddNames(usernames, this.usernames);
    }

    /// <summary>
    /// Return if this Battlefy matches another in any regard
    /// </summary>
    public bool MatchAny(Battlefy other)
    {
      return MatchPersistent(other) || Matcher.NamesMatch(this.usernames, other.usernames) > 0;
    }

    /// <summary>
    /// Return if this Battlefy matches another by persistent data (ids).
    /// </summary>
    public bool MatchPersistent(Battlefy other)
    {
      return Matcher.NamesMatch(this.slugs, other.slugs) > 0;
    }

    public override string ToString()
    {
      return $"Slugs: [{string.Join(", ", slugs)}], Usernames: [{string.Join(", ", usernames)}]";
    }

    #region Serialization

    // Deserialize
    protected Battlefy(SerializationInfo info, StreamingContext context)
    {
      AddSlugs(info.GetValueOrDefault("Slugs", Array.Empty<BattlefySocial>()));
      AddUsernames(info.GetValueOrDefault("Usernames", Array.Empty<Name>()));
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (this.Slugs.Any())
        info.AddValue("Slugs", this.slugs);

      if (this.Usernames.Any())
        info.AddValue("Usernames", this.usernames);
    }

    #endregion Serialization
  }
}