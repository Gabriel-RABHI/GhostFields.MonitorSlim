using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using GhostFields.Benchmarks.Execution;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorSlim
{
    public class MonitorBenchmark : BenchmarkBase
    {
        private const string text = "Bonjôrnõ. Hello world of î rollos...";
        private readonly int COUNT = 10_000_000;
        private readonly int MAX_THREADS = 5;

        [BenchmarkMethod(Code = "M1", Category = "Monitor", Name = "Run benchmark : lock() vs MonitorSlim.")]
        public void RunBenchMonitor()
        {
            BenchmarkRunner.Run<BenchMonitor>();
        }

        [BenchmarkMethod(Code = "M2", Category = "Monitor", Name = "Run everage class using lock() vs using MonitorSlim.")]
        public void RunBenchAverage()
        {
            BenchmarkRunner.Run<BenchAverage>();
        }

        [BenchmarkMethod(Code = "Q1", Category = "Concurrent queue", Name = "SpinWait based 'slim' concurrent queue vs .Net ConcurrentQueue.")]
        public void ConcurrentQueueBenchmark()
        {
            for (int j = 1, th = 1; j < MAX_THREADS; j++, th *= 2)
            {
                var q = new ConcurrentQueueSlim<int>();
                RunParalellAction(th, (thid) =>
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
                RunParalellAction(th, (thid) =>
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

        [BenchmarkMethod(Code = "Q2", Category = "Concurrent queue", Name = "SpinWait based 'slim' balance queue, to see how are distributed items betwen threads.")]
        public void ConcurrentQueueBalance()
        {
            for (int j = 1, th = 1; j < MAX_THREADS; j++, th *= 2)
            {
                var q = new ConcurrentQueueSlim<int>();
                var receved = new int[th];
                int n = 0;
                RunParalellAction(th, (thid) =>
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
                            if (q.TryDequeue(out var o)) {
                                receved[thid]++;
                                Interlocked.Increment(ref n);
                            }
                        } while (n < COUNT);
                    }
                }).PrintToConsole("Slim Queue - Thread Count = " + th).PrintDelayPerOp(COUNT * th);
                WriteCommentLine("Balance = " + String.Join(" | ", receved));
            }

            for (int j = 1, th = 1; j < MAX_THREADS; j++, th *= 2)
            {
                var q = new ConcurrentQueue<int>();
                var receved = new int[th];
                int n = 0;
                RunParalellAction(th, (thid) =>
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
                WriteCommentLine("Balance = " + String.Join(" | ", receved));
            }
        }
    }

    public class BenchMonitor
    {
        private MonitorSlim _monitor;

        [Benchmark]
        public void LockCriticalSection()
        {
            lock (this) { }
        }

        [Benchmark]
        public void MonitorSlimCriticalSection() { _monitor.Enter(); _monitor.Exit(); }
    }

    public class BenchAverage
    {
        AverageAccumulatorSlim _avMs = new AverageAccumulatorSlim();
        AverageAccumulatorLock _avLk = new AverageAccumulatorLock();

        [Benchmark]
        public void LockBasedAverage() => _avLk.Add(15);

        [Benchmark]
        public void SlimBasedAverage() => _avMs.Add(15);
    }

    public class AverageAccumulatorSlim
    {
        private MonitorSlim _monitor;
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
