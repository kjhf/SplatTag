using System;

namespace SplatTagDatabase
{
  [Serializable]
  internal class DbTeam
  {
    public uint id;
    public string name;
    public string[] clanTags;
    public int clanTagOption;
  }
}