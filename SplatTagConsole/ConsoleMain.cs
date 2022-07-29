using Newtonsoft.Json;
using NLog;
using SplatTagCore;
using SplatTagDatabase;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplatTagConsole
{
  public static class ConsoleMain
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static ConsoleMain()
    {
      // Set Console to UTF-8
      Console.OutputEncoding = Encoding.UTF8;

      // Invoked from command line
      JsonConvert.DefaultSettings ??= SplatTagJsonSnapshotDatabase.JsonConvertDefaultSettings;
    }

    public static async Task Main(string[] args)
    {
      AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
      {
        if (e.ExceptionObject is Exception exception)
        {
          logger.Error(exception, "Unhandled exception: " + exception);
          PathUtils.DumpLogSafely("crash", () => new[] { exception.ToString() });
        }
        else
        {
          logger.Error("Unhandled exception: " + e.ExceptionObject);
          PathUtils.DumpLogSafely("crash", () => new[] { e.ExceptionObject?.ToString() ?? "null" });
        }
      };

      ConsoleController controller = new();
      await controller.Handle(args);
    }
  }
}