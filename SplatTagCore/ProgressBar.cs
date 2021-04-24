using System;
using System.Text;

namespace SplatTagCore
{
  /// <summary>
  /// <see cref="ProgressBar"/> functions
  /// </summary>
  public static class ProgressBar
  {
    /// <summary>
    /// Calculate the number of filled bars from a value, capacity, and width.
    /// </summary>
    public static int CalculateProgressBars(int value, int capacity, int width = 10) =>
      Math.Min(width - 1, (int)(((value + 1) * width) / (double)capacity));

    /// <summary>
    /// Make an ASCII progress bar from the current value, total capacity, and how big it can be.
    /// </summary>
    public static string GetProgressBar(int value, int capacity, int width = 10, bool rightToLeft = false)
    {
      int bars = CalculateProgressBars(value, capacity, width);
      return GetProgressBar(bars, width, rightToLeft);
    }

    /// <summary>
    /// Make an ASCII progress bar from the calculated bars and how big it can be.
    /// </summary>
    public static string GetProgressBar(int bars, int width, bool rightToLeft)
    {
      return new StringBuilder()
      .Append('[')
      .Append(rightToLeft ? "<" : new string('=', Math.Max(0, bars)))
      .Append(rightToLeft ? new string('=', Math.Max(0, bars)) : ">")
      .Append(new string(' ', Math.Max(0, width - 1 - bars)))
      .Append(']')
      .ToString();
    }
  }
}