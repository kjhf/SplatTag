using SplatTagCore.Social;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class Battlefy : ISerializable
  {
    /// <summary>
    /// Back-store for the Battlefy slugs
    /// </summary>
    private readonly List<BattlefyUserSocial> slugs = new List<BattlefyUserSocial>();

    /// <summary>
    /// Back-store for the Battlefy usernames
    /// </summary>
    private readonly List<Name> usernames = new List<Name>();

    /// <summary>
    /// Back-store for the Battlefy persistent ids
    /// </summary>
    private readonly List<Name> persistentIds = new List<Name>();

    public Battlefy()
    {
    }

    /// <summary>
    /// The persistent Battlefy slugs
    /// </summary>
    public IReadOnlyList<BattlefyUserSocial> Slugs => slugs;

    /// <summary>
    /// The Battlefy usernames
    /// </summary>
    public IReadOnlyList<Name> Usernames => usernames;

    /// <summary>
    /// The persistent Battlefy ids
    /// </summary>
    public IReadOnlyList<Name> PersistentIds => persistentIds;

    /// <summary>
    /// Add a new Battlefy slug to the front of this Battlefy profile
    /// </summary>
    /// <param name="ids"></param>
    public void AddSlug(string slug, Source source)
    {
      SplatTagCommon.InsertFrontUniqueSourced(new BattlefyUserSocial(slug, source), slugs);
    }

    /// <summary>
    /// Add new Battlefy slugs to the front of this Battlefy profile
    /// </summary>
    /// <param name="ids"></param>
    public void AddSlugs(IEnumerable<BattlefyUserSocial> slugs)
    {
      SplatTagCommon.AddNames(slugs, this.slugs);
    }

    /// <summary>
    /// Add a new Battlefy username to the front of this Battlefy profile
    /// </summary>
    public void AddUsername(string username, Source source)
    {
      SplatTagCommon.AddName(new Name(username, source), usernames);
    }

    /// <summary>
    /// Add new Battlefy usernames to the front of this Battlefy profile
    /// </summary>
    public void AddUsernames(IEnumerable<Name> usernames)
    {
      SplatTagCommon.AddNames(usernames, this.usernames);
    }

    /// <summary>
    /// Add a new Battlefy persistent id to the front of this Battlefy profile
    /// </summary>
    public void AddPersistentId(string persistentId, Source source)
    {
      SplatTagCommon.AddName(new Name(persistentId, source), persistentIds);
    }

    /// <summary>
    /// Add new Battlefy persistent ids to the front of this Battlefy profile
    /// </summary>
    public void AddPersistentIds(IEnumerable<Name> persistentIds)
    {
      SplatTagCommon.AddNames(persistentIds, this.persistentIds);
    }

    /// <summary>
    /// Return if this Battlefy matches another in any regard
    /// </summary>
    public bool MatchAny(Battlefy other)
    {
      return MatchPersistent(other) || Matcher.NamesMatch(usernames, other.usernames);
    }

    /// <summary>
    /// Return if this Battlefy matches another by persistent data (ids).
    /// </summary>
    public bool MatchPersistent(Battlefy other)
    {
      return Matcher.NamesMatch(slugs, other.slugs) || Matcher.NamesMatch(persistentIds, other.persistentIds);
    }

    public override string ToString()
    {
      return $"Slugs: [{string.Join(", ", slugs)}], Usernames: [{string.Join(", ", usernames)}], Ids: [{string.Join(", ", persistentIds)}]";
    }

    #region Serialization

    // Deserialize
    protected Battlefy(SerializationInfo info, StreamingContext context)
    {
      AddSlugs(info.GetValueOrDefault("Slugs", Array.Empty<BattlefyUserSocial>()));
      AddUsernames(info.GetValueOrDefault("Usernames", Array.Empty<Name>()));
      AddPersistentIds(info.GetValueOrDefault("PersistentIds", Array.Empty<Name>()));
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (Slugs.Count > 0)
        info.AddValue("Slugs", slugs);

      if (Usernames.Count > 0)
        info.AddValue("Usernames", usernames);

      if (PersistentIds.Count > 0)
        info.AddValue("PersistentIds", persistentIds);
    }

    #endregion Serialization
  }
}