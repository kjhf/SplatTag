using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SplatTagUnitTests
{
  [TestClass]
  public class PerformanceTests
  {
    // [TestMethod]
    public void TimeAnyPerformance()
    {
      const int TIMES = 20;
      Random rand = new Random();

      for (int cap = 0; cap < 50; cap++)
      {
        long[] spans = new long[TIMES];

        for (int times = 0; times < TIMES; times++)
        {
          var list1 = new List<string>(cap);
          var list2 = new List<string>(cap);

          for (int i = 0; i < list1.Capacity; i++)
          {
            list1.Add(rand.Next().ToString());
            list2.Add(rand.Next().ToString());
          }

          var stopWatch = new Stopwatch();
          stopWatch.Start();
          var smallListAny = list1.Any(x => list2.Any(y => y == x));
          stopWatch.Stop();
          var elapsed = stopWatch.Elapsed;
          //Console.WriteLine("SMALL ANY      " + elapsed);
          spans[times] = elapsed.Ticks;
          if (smallListAny)
          {
            Console.Write("!");
          }
          else
          {
            Console.Write(".");
          }

          Console.WriteLine();
        }

        Console.WriteLine($"AVERAGE for cap {cap}: " + TimeSpan.FromTicks((long)spans.Average()));
      }
    }

    // [TestMethod]
    public void TimeIntersectPerformance()
    {
      const int TIMES = 20;
      Random rand = new Random();

      for (int cap = 0; cap < 50; cap++)
      {
        long[] spans = new long[TIMES];

        for (int times = 0; times < TIMES; times++)
        {
          var list1 = new List<string>(cap);
          var list2 = new List<string>(cap);

          for (int i = 0; i < list1.Capacity; i++)
          {
            list1.Add(rand.Next().ToString());
            list2.Add(rand.Next().ToString());
          }

          var stopWatch = new Stopwatch();
          stopWatch.Start();
          var smallListAny = list1.Intersect(list2).Any();
          stopWatch.Stop();
          var elapsed = stopWatch.Elapsed;
          //Console.WriteLine("SMALL ANY      " + elapsed);
          spans[times] = elapsed.Ticks;
          if (smallListAny)
          {
            Console.Write("!");
          }
          else
          {
            Console.Write(".");
          }

          Console.WriteLine();
        }

        Console.WriteLine($"AVERAGE for cap {cap}: " + TimeSpan.FromTicks((long)spans.Average()));
      }
    }

    public static void TimeIntersectVsAny()
    {
      Random rand = new Random();

      for (int outer = 0; outer < 10; outer++)
      {
        var bigList1 = new List<string>(10000);
        var bigList2 = new List<string>(10000);

        var smallList1 = new List<string>(5);
        var smallList2 = new List<string>(5);

        for (int i = 0; i < bigList1.Capacity; i++)
        {
          bigList1.Add(rand.Next().ToString());
          bigList2.Add(rand.Next().ToString());
        }

        for (int i = 0; i < smallList1.Capacity; i++)
        {
          smallList1.Add(rand.Next().ToString());
          smallList2.Add(rand.Next().ToString());
        }

        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var bigListIntersects = bigList1.Intersect(bigList2).Any();
        stopWatch.Stop();
        Console.WriteLine("BIG IN         " + stopWatch.Elapsed);

        stopWatch = new Stopwatch();
        stopWatch.Start();
        var bigListAny = bigList1.Any(x => bigList2.Any(y => y == x));
        stopWatch.Stop();
        Console.WriteLine("BIG ANY        " + stopWatch.Elapsed);

        stopWatch = new Stopwatch();
        stopWatch.Start();
        var smallListIntersects = smallList1.Intersect(smallList2).Any();
        stopWatch.Stop();
        Console.WriteLine("SMALL IN       " + stopWatch.Elapsed);

        stopWatch = new Stopwatch();
        stopWatch.Start();
        var smallListAny = smallList1.Any(x => smallList2.Any(y => y == x));
        stopWatch.Stop();
        Console.WriteLine("SMALL ANY      " + stopWatch.Elapsed);

        Console.WriteLine();
      }
    }
  }
}