using NLog;
using System;
using System.IO;

namespace SplatTagDatabase
{
  public static class PathUtils
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Synchronously wait for a file to become ready. Does not check file's existence.
    /// Returns true if file is now ready or false for timeout.
    /// </summary>
    public static bool WaitForFileCreatedAndReady(string filePath, int timeoutMillis = 30000, int checkIntervalMillis = 100)
    {
      var start = DateTime.Now;
      while (true)
      {
        try
        {
          using var stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
          Console.WriteLine($"{nameof(WaitForFileCreatedAndReady)}: {filePath} is ready with {stream.Length} bytes to read.");
          return true;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)  // DirectoryNotFoundException and FileNotFoundException are IOException
        {
          var now = DateTime.Now;
          if (now - start > TimeSpan.FromMilliseconds(timeoutMillis))
          {
            return false;
          }
          System.Threading.Thread.Sleep(checkIntervalMillis);
        }
      }
    }

    public static string? FindFileUpToRoot(string fileName)
    {
      var dir = Directory.GetCurrentDirectory();
      string? testPath = Path.Combine(dir, fileName);
      if (testPath != null && File.Exists(testPath))
      {
        return testPath;
      }

      do
      {
        dir = Directory.GetParent(dir)?.FullName;
        testPath = Path.Combine(dir ?? "", fileName);
      }
      while (dir != null && !File.Exists(testPath));
      return File.Exists(testPath) ? testPath : null;
    }

    public static void LoadDotEnv(string fileName = ".env")
    {
      string? dotenvPath = FindFileUpToRoot(fileName);
      LoadDotEnvFromPath(dotenvPath);
    }

    public static void LoadDotEnvFromPath(string? filePath)
    {
      if (filePath == null || !File.Exists(filePath))
      {
        logger.Warn(".env file not found.");
      }
      else
      {
        logger.Debug(".env file: " + filePath);
        foreach (var line in File.ReadAllLines(filePath))
        {
          var parts = line.Split(
              '=',
              StringSplitOptions.RemoveEmptyEntries);

          if (parts.Length != 2)
            continue;

          logger.Trace("Setting env key " + parts[0]);
          Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }
      }
    }
  }
}