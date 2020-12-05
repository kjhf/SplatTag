using System;
using System.Collections.Generic;
using System.Linq;

namespace SplatTagCore.Social
{
  public class Battlefy : Social
  {
    protected override string SocialBaseAddress => "battlefy.com/users/";
    private readonly List<string> usernames = new List<string>();
    public IReadOnlyList<string> Usernames => usernames;

    public Battlefy(string slug, Source source)
      : this(slug, new string[0], source.AsEnumerable())
    {
    }

    public Battlefy(string slug, IEnumerable<Source> sources)
      : this(slug, new string[0], sources)
    {
    }

    public Battlefy(string slug, string username, Source source)
      : this(slug, username.AsEnumerable(), source.AsEnumerable())
    {
    }

    public Battlefy(string slug, IEnumerable<string> usernames, Source source)
      : this(slug, usernames, source.AsEnumerable())
    {
    }

    public Battlefy(string slug, IEnumerable<string> usernames, IEnumerable<Source> sources)
      : base(slug, sources)
    {
      AddUsernames(usernames);
    }

    public void AddUsernames(IEnumerable<string> usernames)
    {
      SplatTagCommon.AddStrings(usernames, this.usernames);
    }

    public bool Match(Battlefy other, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
    {
      return this.Value.Equals(other.Value, stringComparison)
        || this.usernames.Intersect(other.usernames).Any();
    }
  }
}