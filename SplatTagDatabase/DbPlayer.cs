using System;

namespace SplatTagDatabase
{
  [Serializable]
  internal class DbPlayer
  {
    public uint id;
    public string[] names;
    public long[] teams;
  }
}