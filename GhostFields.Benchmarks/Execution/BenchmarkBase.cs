//#define CLEANUP_GC

using GhostFields.Benchmarks.Contracts;
using System;
using System.Diagnostics;
using System.Threading;

namespace GhostFields.Benchmarks.Execution
{
    public abstract class BenchmarkBase
    {
        protected void WriteLine(string prompt)
        {
            BenchmarkConsoleRunner.Console.ForegroundColor = ConsoleColor.White;
            BenchmarkConsoleRunner.Console.WriteLine(prompt);
        }

        protected IConsole Console => BenchmarkConsoleRunner.Console;

        protected void WriteCommentLine(string prompt)
        {
            BenchmarkConsoleRunner.WriteCommentLine(prompt);
        }

        protected void CleanUpGarbage()
        {
            GC.Collect();
            GC.WaitForFullGCComplete();
        }

        protected BenchmarkResult RunMonitoredAction(Action f)
        {
            GC.Collect();
            var sw = new Stopwatch();
            var swGC = new Stopwatch();
            var gen0 = GC.CollectionCount(0);
            var gen1 = GC.CollectionCount(1);
            var gen2 = GC.CollectionCount(2);
            sw.Start();
            f();
            sw.Stop();
#if CLEANUP_GC
            swGC.Start();
            CleanUpGarbage();
            swGC.Stop();
#endif
            return new BenchmarkResult()
            {
                Milliseconds = sw.ElapsedMilliseconds,
                GarbageCollectionMilliseconds = swGC.ElapsedMilliseconds,
                GCGen0 = GC.CollectionCount(0) - gen0,
                GCGen1 = GC.CollectionCount(1) - gen1,
                GCGen2 = GC.CollectionCount(2) - gen2,
            };
        }

        protected BenchmarkResult RepeatMonitoredAction(int seconds, Action f)
        {
            GC.Collect();
            var sw = new Stopwatch();
            var swGC = new Stopwatch();
            var gen0 = GC.CollectionCount(0);
            var gen1 = GC.CollectionCount(1);
            var gen2 = GC.CollectionCount(2);
            long count = 0;
            sw.Start();
            do
            {
                f();
                count++;
            } while (sw.ElapsedMilliseconds < seconds * 1000);
            sw.Stop();
#if CLEANUP_GC
            swGC.Start();
            CleanUpGarbage();
            swGC.Stop();
#endif
            return new BenchmarkResult()
            {
                Milliseconds = sw.ElapsedMilliseconds,
                GarbageCollectionMilliseconds = swGC.ElapsedMilliseconds,
                GCGen0 = GC.CollectionCount(0) - gen0,
                GCGen1 = GC.CollectionCount(1) - gen1,
                GCGen2 = GC.CollectionCount(2) - gen2,
                Count = count,
                Seconds = seconds
            };
        }

        protected BenchmarkResult RunParalellAction(int threadCount, Action<int> f)
        {
            return RunMonitoredAction(() =>
            {
                var _started = 0;
                var _ended = 0;
                Exception _inThreadEx = null;
                for (var i = 0; i < threadCount; i++)
                {
                    var th = new Thread(n =>
                    {
                        Interlocked.Increment(ref _started);
                        do
                        {
                            Thread.Yield();
                        } while (_started < threadCount);
                        try
                        {
                            f((int)n);
                        }
                        catch (Exception ex)
                        {
                            _inThreadEx = ex;
                        }
                        Interlocked.Increment(ref _ended);
                    });
                    th.Start(i);
                }
                do
                {
                    Thread.Yield();
                } while (_ended < threadCount);
                if (_inThreadEx != null)
                    throw _inThreadEx;
            });
        }
    }
}
