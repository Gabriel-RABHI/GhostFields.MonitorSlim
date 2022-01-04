# MonitorSlim
Up to 2x faster Monitor class for .Net, useful for small and hot critical code sections like inner fields protection. Typical use case is done in the sample with the AverageAccumulator class implementations. This Monitor is a structure without any reference. It can be integrated in unmanaged structure while the .Net Monitor class need an object reference to call Enter or Exit methods.

Results :

|       Method |      Mean |     Error |    StdDev |
|------------- |----------:|----------:|----------:|
| lock() | 13.315 ns | 0.0245 ns | 0.0229 ns |
| MonitorSlim.Enter / Exit |  6.114 ns | 0.0858 ns | 0.0803 ns |

*B*enchmarkDotNet=v0.13.1, OS=Windows 10.0.19042.1110 (20H2/October2020Update)*
*Intel Core i9-9900K CPU 3.60GHz (Coffee Lake), 1 CPU, 16 logical and 8 physical cores*
*.NET SDK=6.0.100*
  *[Host]     : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT  [AttachedDebugger]*
  DefaultJob : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT*

## Usage

That kind of Monitor is used to protect really short code sections that are called millions times, typically to protect inner fields from incoherences. You have to copy the MonitorSlim in your source code to permit code inlining.

```
	public class AverageAccumulator
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
```

Performance gain against lock() is significant :

| Method           |      Mean |     Error |    StdDev |
| ---------------- | --------: | --------: | --------: |
| LockBasedAverage | 14.102 ns | 0.0159 ns | 0.0148 ns |
| SlimBasedAverage |  6.676 ns | 0.0111 ns | 0.0086 ns |

## ConcurrentQueueSlim

This class is written to test a naive implementation of a SpinWait based concurrent queue. **This is a toy !**

We can see it is up to 5x or faster than .Net lock-free implementation when the number of concurrent thread to enqueue / dequeue is high. It mean that SpinWait is stopping few threads by calling Sleep(). **The side effect** is that the distribution of items to the threads is not as uniform as the one of the .Net ConcurrentQueue.

![](https://raw.githubusercontent.com/Gabriel-RABHI/GhostFields.MonitorSlim/master/Pictures/queue-results.jpg)

But as a job queue, you can see that even with 4 threads to enqueue and 4 threads to dequeue, the total item count is stable : it means that few thread are receiving large number of items like a batched list of items. This property is a good one for a job publisher / consumer model while process many items on a single thread is faster than perfect distribution.

**Be careful**, that kind of benchmark often lie. Do your own production code based benchmark to see the potential benefits.

