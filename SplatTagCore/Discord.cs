using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace SplatTagCore
{
  [Serializable]
  public class Discord : ISerializable
  {
    public static readonly Regex DISCORD_NAME_REGEX = new Regex(@"\(?.*#[0-9]{4}\)?", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

    /// <summary>
    /// Back-store for the Discord ids
    /// </summary>
    private readonly List<Name> ids = new List<Name>();

    /// <summary>
    /// Back-store for the Discord usernames
    /// </summary>
    private readonly List<Name> usernames = new List<Name>();

    public Discord()
    {
    }

    /// <summary>
    /// The persistent Discord ids
    /// </summary>
    public IReadOnlyList<Name> Ids => ids;

    /// <summary>
    /// The Discord usernames
    /// </summary>
    public IReadOnlyList<Name> Usernames => usernames;

    /// <summary>
    /// Add a new Discord id to the front of this profile
    /// </summary>
    /// <param name="ids"></param>
    public void AddId(string slug, Source source)
    {
      SplatTagCommon.AddName(new Name(slug, source), ids);
    }

    /// <summary>
    /// Add Discord ids to this Discord profile
    /// </summary>
    /// <param name="ids"></param>
    public void AddIds(IEnumerable<Name> ids)
    {
      SplatTagCommon.AddNames(ids, this.ids);
    }

    /// <summary>
    /// Add a new Discord name to the front of this profile
    /// </summary>
    /// <param name="ids"></param>
    public void AddUsername(string username, Source source)
    {
      SplatTagCommon.AddName(new Name(username, source), usernames);
    }

    /// <summary>
    /// Add Discord usernames to this Discord profile
    /// </summary>
    public void AddUsernames(IEnumerable<Name> usernames)
    {
      SplatTagCommon.AddNames(usernames, this.usernames);
    }

    /// <summary>
    /// Return if this Discord matches another in any regard
    /// </summary>
    public bool MatchAny(Discord other)
    {
      return MatchPersistent(other) || Matcher.NamesMatch(usernames, other.usernames) > 0;
    }

    /// <summary>
    /// Return if this Discord matches another by persistent data (ids).
    /// </summary>
    public bool MatchPersistent(Discord other)
    {
      return Matcher.NamesMatch(ids, other.ids) > 0;
    }

    public override string ToString()
    {
      return $"Ids: [{string.Join(", ", ids)}], Usernames: [{string.Join(", ", usernames)}]";
    }

    #region Serialization

    // Deserialize
    protected Discord(SerializationInfo info, StreamingContext context)
    {
      AddIds(info.GetValueOrDefault("Ids", Array.Empty<Name>()));
      AddUsernames(info.GetValueOrDefault("Usernames", Array.Empty<Name>()));
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (Ids.Count > 0)
        info.AddValue("Ids", ids);

      if (Usernames.Count > 0)
        info.AddValue("Usernames", usernames);
    }

    #endregion Serialization
  }
}