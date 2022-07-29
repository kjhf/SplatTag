using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

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

    /// <summary>
    /// Write a log to file with the current date, and with the given contents, and synchronously wait for the file to be written.
    /// Also catches errors.
    /// </summary>
    public static void DumpLogSafely(string logTitle, Func<IEnumerable<string>> contentsFn)
    {
      try
      {
        string filePath = Path.Combine(SplatTagControllerFactory.GetDumpPath(), logTitle + DateTime.Now.ToString("-yyyy-MM-dd-HH-mm-ss") + ".log.br");
        logger.Trace("Saving log to " + filePath);

        // Wait until the log is written before continuing so the program has finished writing before exiting.
        var creationWaitTask = Task.Run(() => WaitForFileCreatedAndReady(filePath));
        var savingTask = Task.Run(() => StreamLogWithCompress(filePath));

        Task.WaitAll(creationWaitTask, savingTask);
        logger.Trace($"{logTitle}: creationWaitTask: {creationWaitTask.Status}, savingTask: {savingTask.Status}");
      }
      catch (Exception ex)
      {
        string error = $"Unable to save the {logTitle} log because of an exception: {ex}";
        logger.Error(ex, error);
      }

      void StreamLogWithCompress(string filePath)
      {
        // Stream the merge logs' StringBuilders to file
        using FileStream writer = new(filePath, FileMode.Create);
        using var compressor = new BrotliStream(writer, CompressionLevel.Optimal);
        foreach (var line in contentsFn())
        {
          var bytes = Encoding.UTF8.GetBytes(line);
          compressor.Write(bytes);
        }
      }
    }
  }
}