using GhostBodyObject.BenchmarkRunner;
using MonitorSlim.Collections;

namespace MonitorSlim
{
    public class ConcurrentListBenchmark : BenchmarkBase
    {
        private readonly int COUNT = 10_000_000;
        private readonly int LIST_LENGHT = 10_000;

        [BruteForceBenchmark("L1", "Add, remove, enumerate concurrently randomly around 10k entries.", "Concurrent List")]
        public void RunListRandomBench()
        {
            var list = new ConcurrentList<int>();
            var ended = false;
            long cycles = 0;
            RunParallelAction(Environment.ProcessorCount, (thid) =>
            {
                var l = list;
                var rnd = new Random();
                if (thid == 0)
                {
                    var _added = new Stack<int>();
                    try
                    {
                        for (int i = 0; i < COUNT; i++)
                        {
                            if (l.Count < LIST_LENGHT)
                                _added.Push(l.Add(rnd.Next()));
                            else
                            {
                                if (rnd.Next(100) < 49)
                                    _added.Push(l.Add(rnd.Next()));
                                else
                                    if (_added.TryPop(out int idx))
                                    l.RemoveIndex(idx);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    ended = true;
                }
                else
                {
                    while (!ended)
                    {
                        Interlocked.Increment(ref cycles);
                        long sum;
                        foreach (var v in list.Values)
                            sum = v;
                    }
                }
            }).PrintToConsole($"{COUNT} add / removes done").PrintDelayPerOp(COUNT);
            Console.WriteLine($"There is {cycles} enumerations done concurrently bye {Environment.ProcessorCount - 1} threads ont a {list.Count} length list.");
        }

        [BruteForceBenchmark("L2", "Add 10k items, remove all, add 10k, remove... and enumerate concurrently.", "Concurrent List")]
        public void RunVaryingListMonitor()
        {
            var list = new ConcurrentList<int>();
            var ended = false;
            long cycles = 0;
            RunParallelAction(Environment.ProcessorCount, (thid) =>
            {
                var l = list;
                var rnd = new Random();
                if (thid == 0)
                {
                    var _added = new Stack<int>();
                    try
                    {
                        _added.Push(l.Add(rnd.Next()));
                        for (int i = 0; i < COUNT; i++)
                        {
                            if ((i / 10_000) % 2 == 0)
                                _added.Push(l.Add(rnd.Next()));
                            else
                            {
                                if (_added.TryPop(out int idx))
                                    l.RemoveIndex(idx);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    ended = true;
                }
                else
                {
                    while (!ended)
                    {
                        Interlocked.Increment(ref cycles);
                        long sum;
                        foreach (var v in list.Values)
                            sum = v;
                    }
                }
            }).PrintToConsole($"{COUNT} add / removes done").PrintDelayPerOp(COUNT);
            Console.WriteLine($"There is {cycles} enumerations done concurrently bye {Environment.ProcessorCount - 1} threads ont a {list.Count} length list.");
        }
    }
}
