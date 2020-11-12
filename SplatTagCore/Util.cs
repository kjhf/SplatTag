using System;
using System.Text;

namespace SplatTagCore
{
  public static class Util
  {
    /// <summary>
    /// Make an ASCII progress bar from the current value, total capacity, and how big it can be.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="capacity"></param>
    /// <param name="width"></param>
    public static string GetProgressBar(int value, int capacity, int width = 10)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("[");
      int bars = Math.Min(width - 1, (int)(((value + 1) * width) / (double)capacity));
      for (int i = 0; i < bars; i++)
      {
        sb.Append("=");
      }
      sb.Append(">");
      for (int i = bars; i < (width - 1); i++)
      {
        sb.Append(" ");
      }
      sb.Append("]");
      return sb.ToString();
    }
  }
}