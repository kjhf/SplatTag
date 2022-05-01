using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SplatTagDatabase
{
  public static class PathUtils
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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