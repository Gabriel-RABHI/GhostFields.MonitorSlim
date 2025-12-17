using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using GhostBodyObject.BenchmarkRunner;
using MonitorSlim.Collections;
using MonitorSlim.Monitors;
using System.Collections.Concurrent;

namespace MonitorSlim
{
    public class MonitorBenchmark : BenchmarkBase
    {
        private const string text = "Bonjôrnõ. Hello world of î rollos...";
        private readonly int COUNT = 10_000_000;
        private readonly int MAX_THREADS = 5;

        [BruteForceBenchmark("M1", "Run benchmark : lock() vs MonitorSlim.", "Monitor")]
        public void RunBenchMonitor()
        {
            BenchmarkRunner.Run<BenchMonitor>();
        }

        [BruteForceBenchmark("M2", "Run everage class using lock() vs using MonitorSlim.", "Monitor")]
        public void RunBenchAverage()
        {
            BenchmarkRunner.Run<BenchAverage>();
        }

        [BruteForceBenchmark("M3", "Correctness test.", "Monitor")]
        public void CorrectnessTest()
        {
            int n = 0;
            var monitor = new ShortMonitor();
            RunParallelAction(Environment.ProcessorCount, (thid) =>
            {
                for (int i = 0; i < COUNT * 20; i++)
                {
                    monitor.Enter();
                    var v = Interlocked.Increment(ref n);
                    if (v > 1)
                        throw new Exception("Bad value :" + v);
                    v = Interlocked.Decrement(ref n);
                    if (v < 0)
                        throw new Exception("Bad value :" + v);
                    monitor.Exit();
                    if (i % COUNT == 0)
                        Console.Write(".");
                }
                Console.WriteLine();
            }).PrintToConsole("No incoherence found.");
        }

        [BruteForceBenchmark("Q1", "SpinWait based 'slim' concurrent queue vs .Net ConcurrentQueue.", "Concurrent queue")]
        public void ConcurrentQueueBenchmark()
        {
            for (int j = 1, th = 1; j < MAX_THREADS; j++, th *= 2)
            {
                var q = new ConcurrentQueueSlim<int>();
                RunParallelAction(th, (thid) =>
                {
                    if (thid % 2 == 0)
                    {
                        for (int i = 0; i < COUNT; i++)
                            q.Enqueue(i);
                    }
                    else
                    {
                        int n = 0;
                        do
                        {
                            if (q.TryDequeue(out var o)) n++;
                        } while (n < COUNT);
                    }
                }).PrintToConsole("Slim Queue - Thread Count = " + th).PrintDelayPerOp(COUNT * th);
            }

            for (int j = 1, th = 1; j < MAX_THREADS; j++, th *= 2)
            {
                var q = new ConcurrentQueue<int>();
                RunParallelAction(th, (thid) =>
                {
                    if (thid % 2 == 0)
                    {
                        for (int i = 0; i < COUNT; i++)
                            q.Enqueue(i);
                    }
                    else
                    {
                        int n = 0;
                        do
                        {
                            if (q.TryDequeue(out var o)) n++;
                        } while (n < COUNT);
                    }
                }).PrintToConsole(".Net Concurrent Queue - Thread Count = " + th).PrintDelayPerOp(COUNT * th);
            }
        }

        [BruteForceBenchmark("Q2", "SpinWait based 'slim' balance queue, to see how are distributed items betwen threads.", "Concurrent queue")]
        public void ConcurrentQueueBalance()
        {
            for (int j = 1, th = 1; j < MAX_THREADS; j++, th *= 2)
            {
                var q = new ConcurrentQueueSlim<int>();
                var receved = new int[th];
                int n = 0;
                RunParallelAction(th, (thid) =>
                {
                    if (thid == 0)
                    {
                        for (int i = 0; i < COUNT; i++)
                            q.Enqueue(i);
                    }
                    else
                    {
                        do
                        {
                            if (q.TryDequeue(out var o))
                            {
                                receved[thid]++;
                                Interlocked.Increment(ref n);
                            }
                        } while (n < COUNT);
                    }
                }).PrintToConsole("Slim Queue - Thread Count = " + th).PrintDelayPerOp(COUNT * th);
                Console.WriteLine("Balance = " + String.Join(" | ", receved));
            }

            for (int j = 1, th = 1; j < MAX_THREADS; j++, th *= 2)
            {
                var q = new ConcurrentQueue<int>();
                var receved = new int[th];
                int n = 0;
                RunParallelAction(th, (thid) =>
                {
                    if (thid == 0)
                    {
                        for (int i = 0; i < COUNT; i++)
                            q.Enqueue(i);
                    }
                    else
                    {
                        do
                        {
                            if (q.TryDequeue(out var o))
                            {
                                receved[thid]++;
                                Interlocked.Increment(ref n);
                            }
                        } while (n < COUNT);
                    }
                }).PrintToConsole(".Net Concurrent Queue - Thread Count = " + th).PrintDelayPerOp(COUNT * th);
                Console.WriteLine("Balance = " + String.Join(" | ", receved));
            }
        }
    }

    public class BenchMonitor
    {
        private ShortMonitor _monitor;
        private SpinLock sl = new SpinLock(false);

        [Benchmark]
        public void LockCriticalSection()
        {
            lock (this) { }
        }

        [Benchmark]
        public void MonitorSlimCriticalSection() { _monitor.Enter(); _monitor.Exit(); }

        [Benchmark]
        public void SpinLockCriticalSection()
        {
            bool taken = false;
            sl.Enter(ref taken);
            sl.Exit();
        }
    }

    public class BenchAverage
    {
        AverageAccumulatorSlim _avMs = new AverageAccumulatorSlim();
        AverageAccumulatorLock _avLk = new AverageAccumulatorLock();
        AverageAccumulatorSpin _avSp = new AverageAccumulatorSpin();

        [Benchmark]
        public void LockBasedAverage() => _avLk.Add(15);

        [Benchmark]
        public void SlimBasedAverage() => _avMs.Add(15);

        [Benchmark]
        public void SpinBasedAverage() => _avSp.Add(15);
    }

    public class AverageAccumulatorSlim
    {
        private ShortMonitor _monitor;
        private int _count, _sum;

        public void Add(int v)
        {
            _monitor.Enter();
            try
            {
                _sum += v;
                _count++;
            }
            finally
            {
                _monitor.Exit();
            }
        }

        public int Result
        {
            get
            {
                _monitor.Enter();
                try
                {
                    return _count == 0 ? 0 : _sum / _count;
                }
                finally
                {
                    _monitor.Exit();
                }
            }
        }
    }

    public class AverageAccumulatorSpin
    {
        private SpinLock sl = new SpinLock(false);
        private int _count, _sum;

        public void Add(int v)
        {
            bool taken = false;
            sl.Enter(ref taken);
            try
            {
                _sum += v;
                _count++;
            }
            finally
            {
                sl.Exit();
            }
        }

        public int Result
        {
            get
            {
                bool taken = false;
                sl.Enter(ref taken);
                try
                {
                    return _count == 0 ? 0 : _sum / _count;
                }
                finally
                {
                    sl.Exit();
                }
            }
        }
    }

    public class AverageAccumulatorLock
    {
        private int _count, _sum;

        public void Add(int v)
        {
            lock (this)
            {
                _sum += v;
                _count++;
            }
        }

        public int Result
        {
            get
            {
                lock (this)
                    return _count == 0 ? 0 : _sum / _count;
            }
        }
    }
}
