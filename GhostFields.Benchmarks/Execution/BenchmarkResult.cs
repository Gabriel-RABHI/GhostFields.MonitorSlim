using System;

namespace GhostFields.Benchmarks.Execution
{
    /// <summary>
    /// Bench result data set.
    /// </summary>
    public class BenchmarkResult
    {
        private static readonly string _OUTPUT_PREFIX = "   ->   ";
        private static readonly object _locker = new object();

        private long _milliseconds = 0;
        private long _millisecondsGC = 0;
        private int _gcGen0;
        private int _gcGen1;
        private int _gcGen2;
        private int memoryBlocks = 0;
        private long _count = -1;
        private long _seconds = -1;
        private string _lastOpName = "";

        /// <summary>
        /// Ellapsed time to complete the bench.
        /// </summary>
        public long Milliseconds { get => _milliseconds; set => _milliseconds = value; }

        /// <summary>
        /// Ellapsed time to complete the garbage collection.
        /// </summary>
        public long GarbageCollectionMilliseconds { get => _millisecondsGC; set => _millisecondsGC = value; }

        /// <summary>
        /// Generation 0 Garbage Collector collect count
        /// </summary>
        public int GCGen0 { get => _gcGen0; set => _gcGen0 = value; }

        /// <summary>
        /// Generation 1 Garbage Collector collect count
        /// </summary>
        public int GCGen1 { get => _gcGen1; set => _gcGen1 = value; }

        /// <summary>
        /// Generation 2 Garbage Collector collect count
        /// </summary>
        public int GCGen2 { get => _gcGen2; set => _gcGen2 = value; }

        /// <summary>
        /// Memory blocks allocated from the start of the bench
        /// </summary>
        public int MemoryBlocks { get => memoryBlocks; set => memoryBlocks = value; }

        /// <summary>
        /// Iteration count
        /// </summary>
        public long Count { get => _count; set => _count = value; }

        /// <summary>
        /// Bech loop duration
        /// </summary>
        public long Seconds { get => _seconds; set => _seconds = value; }

        /// <summary>
        /// Print main data set.
        /// </summary>
        /// <param name="opName">Prompt to include in the repport output</param>
        /// <returns></returns>
        public BenchmarkResult PrintToConsole(string opName = null)
        {
            lock (_locker)
            {
                // -------- //
                BenchmarkConsoleRunner.Console.ForegroundColor = ConsoleColor.Yellow;
                if (!string.IsNullOrWhiteSpace(opName) && _lastOpName != opName)
                {
                    BenchmarkConsoleRunner.WriteMajorStepLine(opName);
                    _lastOpName = opName;
                }
                var s = "";
                // -------- //
                BenchmarkConsoleRunner.Console.ForegroundColor = ConsoleColor.Red;
                if (_gcGen0 != 0)
                    s += (" / " + _gcGen0 + " gen0");
                if (_gcGen1 != 0)
                    s += (" / " + _gcGen1 + " gen1");
                if (_gcGen2 != 0)
                    s += (" / " + _gcGen2 + " gen2");
                if (memoryBlocks != 0)
                    s += (" / " + memoryBlocks + " mallocs");
                if (GarbageCollectionMilliseconds > 0)
                    s += (" / GC : " + GarbageCollectionMilliseconds + " ms");
                if (!string.IsNullOrWhiteSpace(s))
                    BenchmarkConsoleRunner.Console.WriteLine(_OUTPUT_PREFIX + "Garbage Collector " + s);
                // -------- //
                BenchmarkConsoleRunner.Console.ForegroundColor = ConsoleColor.Magenta;
                // -------- //
                BenchmarkConsoleRunner.WriteResultLine(_OUTPUT_PREFIX, "Duration", _milliseconds + " ms");
                if (_count > 0 && _seconds > 0)
                    BenchmarkConsoleRunner.WriteResultLine(_OUTPUT_PREFIX, "Count", _count.ToString(), "Iterations per second", _count / _seconds + " ms");
                BenchmarkConsoleRunner.Console.ForegroundColor = ConsoleColor.White;
            }
            return this;
        }

        /// <summary>
        /// Display
        /// </summary>
        /// <param name="count">Number of iteration / operation done during this bench</param>
        /// <param name="opName">Prompt to include in the repport output</param>
        /// <returns></returns>
        public BenchmarkResult PrintDelayPerOp(long count, string opName = null)
        {
            lock (_locker)
            {
                BenchmarkConsoleRunner.Console.ForegroundColor = ConsoleColor.Yellow;
                if (!string.IsNullOrWhiteSpace(opName) && _lastOpName != opName)
                {
                    BenchmarkConsoleRunner.WriteMinorStepLine(opName);
                    _lastOpName = opName;
                }
                var timePerComputation = (Milliseconds / (double)count) / 1000d;
                if (timePerComputation > 5)
                    BenchmarkConsoleRunner.WriteResultLine(_OUTPUT_PREFIX, "Operation cost (s)", timePerComputation.ToString("n"));
                else
                if (timePerComputation * 1000 > 5)
                    BenchmarkConsoleRunner.WriteResultLine(_OUTPUT_PREFIX, "Operation cost (ms)", (timePerComputation * 1_000).ToString("n"));
                else
                if (timePerComputation * 1000000 > 5)
                    BenchmarkConsoleRunner.WriteResultLine(_OUTPUT_PREFIX, "Operation cost (us - picoseconds)", (timePerComputation * 1_000_000).ToString("n"));
                else
                    BenchmarkConsoleRunner.WriteResultLine(_OUTPUT_PREFIX, "Operation cost (ns - nanoseconds)", (timePerComputation * 1_000_000_000).ToString("n"));
                BenchmarkConsoleRunner.WriteResultLine(_OUTPUT_PREFIX, "Operations per second", ((long)(count / (double)Milliseconds * 1_000d)).ToString("n"));
                BenchmarkConsoleRunner.Console.ForegroundColor = ConsoleColor.White;
            }
            return this;
        }

        public BenchmarkResult PrintComparison(long count, long givenResultCount, BenchmarkResult result, string givenResultOpName = null)
        {
            lock (_locker)
            {
                if (givenResultOpName == null)
                    givenResultOpName = "This operation";
                var opPerSecondA = (count / (double)Milliseconds * 1000d);
                var opPerSecondB = (givenResultCount / (double)result.Milliseconds * 1000d);
                if (opPerSecondA != 0)
                {
                    BenchmarkConsoleRunner.Console.ForegroundColor = ConsoleColor.Blue;
                    var ratio = opPerSecondB / opPerSecondA;
                    if (ratio >= 1)
                        BenchmarkConsoleRunner.Console.WriteLine(_OUTPUT_PREFIX + givenResultOpName + " : " + (ratio).ToString("n") + " faster");
                    else
                        BenchmarkConsoleRunner.Console.WriteLine(_OUTPUT_PREFIX + givenResultOpName + " : " + (1 / ratio).ToString("n") + " slower");
                    BenchmarkConsoleRunner.Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    BenchmarkConsoleRunner.WriteError("Computation error", "Unable to display the ratio (divid by zero).");
                }
            }
            return this;
        }

        public BenchmarkResult PrintSpace()
        {
            BenchmarkConsoleRunner.Console.WriteLine("");
            return this;
        }
    }
}
