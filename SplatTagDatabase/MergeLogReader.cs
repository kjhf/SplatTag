using NLog;
using SplatTagDatabase.Merging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SplatTagDatabase
{
  public static class MergeLogReader
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Gets the latest MergeLog by write time.
    /// Uses the default folder if <paramref name="dir"/> is not specified.
    /// </summary>
    public static FileInfo? GetLatestMergeLog(string? dir = null)
    {
      dir ??= SplatTagControllerFactory.GetDefaultPath();

      // Check in the save directory for the latest snapshot.
      return new DirectoryInfo(dir).GetFiles("MergeLog-*.log").OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
    }

    /// <summary>
    /// Reads the specified MergeLog.
    /// Uses the latest MergeLog if <paramref name="fileName"/> is not specified.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="fileName"/> is null and <see cref="GetLatestMergeLog"/> failed to find the latest file.</exception>
    public static List<MergeLogEntry> Read(string? fileName = null)
    {
      fileName ??= GetLatestMergeLog()?.FullName;
      var result = new List<MergeLogEntry>();
      foreach (var line in File.ReadAllLines(fileName))
      {
        try
        {
          var entry = new MergeLogEntry(line);
          result.Add(entry);
        }
        catch (Exception ex)
        {
          logger.Error(ex, ex.Message);
        }
      }
      return result;
    }
  }
}