using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorSlim
{
    public class BenchMonitor
    {
        private ShortMonitor _monitor;

        [Benchmark]
        public void LockBasedFoo()
        {
            lock (this) { }
        }

        [Benchmark]
        public void SlimBasedFoo() { _monitor.Enter(); _monitor.Exit(); }
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
