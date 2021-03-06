﻿using System;
using System.Runtime.InteropServices;
using System.Security;

namespace SplatTagDatabase
{
  /// <summary>
  /// Class needed to set the Windows timer resolution to 1ms.
  /// Needed for the full CPU usage in the parallel parts.
  /// It significantly increases energy usage (but we're gamers so...) so we should put it back when done.
  /// This really should not be a thing but thanks Microsoft.
  /// Stolen from https://stackoverflow.com/questions/16612236/why-is-my-c-sharp-program-faster-in-a-profiler/38404066#38404066
  /// </summary>
  public static class WinApi
  {
    [SuppressUnmanagedCodeSecurity]
    [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
    private static extern uint TimeBeginPeriod(uint uMilliseconds);

    public static void TryTimeBeginPeriod(uint uMilliseconds)
    {
      Console.WriteLine("Engaging TURBO");
      try
      {
        TimeBeginPeriod(uMilliseconds);
      }
      catch (DllNotFoundException ex)
      {
        Console.Error.WriteLine("Cannot invoke TimeBeginPeriod. SystemException: " + ex.Message);
      }
    }

    [SuppressUnmanagedCodeSecurity]
    [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
    private static extern uint TimeEndPeriod(uint uMilliseconds);

    public static void TryTimeEndPeriod(uint uMilliseconds)
    {
      try
      {
        TimeEndPeriod(uMilliseconds);
      }
      catch (DllNotFoundException ex)
      {
        Console.Error.WriteLine("Cannot invoke TimeEndPeriod. SystemException: " + ex.Message);
      }
      Console.WriteLine("TURBO disengaged");
    }
  }
}