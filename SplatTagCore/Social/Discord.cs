using System.Collections.Generic;

namespace SplatTagCore.Social
{
  public class Discord
  {
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
    /// Add Discord ids to this Discord profile
    /// </summary>
    /// <param name="ids"></param>
    public void AddIds(IEnumerable<Name> ids)
    {
      SplatTagCommon.AddNames(ids, this.ids);
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
      return MatchPersistent(other) || Matcher.NamesMatch(this.usernames, other.usernames) > 0;
    }

    /// <summary>
    /// Return if this Discord matches another by persistent data (ids).
    /// </summary>
    public bool MatchPersistent(Discord other)
    {
      return Matcher.NamesMatch(this.ids, other.ids) > 0;
    }
  }
}