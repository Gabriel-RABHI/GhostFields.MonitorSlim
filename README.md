# ShortMonitor
More than 2x faster Monitor classes for .Net, useful for really short and hot critical code sections like inner fields protection. They are simply revisited Spinlock implementations: sometimes, old fashion simple primitives are quite efficient. Typical use case is done in the sample with the AverageAccumulator class implementations. This Monitors are structures without any reference. They can be integrated in unmanaged structure while the .Net Monitor class need an object reference to call Enter or Exit methods.

- **ShortMonitor** : Standalone lock primitive for narrow critical sections. This synchronysation primitive is not recursive : the same thread cannot enter multiple times.
- **ShortRecursiveMonitor** : Standalone recursive lock primitive for narrow critical sections.
- **ShortCountMonitor** : Standalone lock primitive that accept a limited entrancy count.
- **ShortNonSpinnedMonitor** : Standalone lock primitive for narrow critical sections using a busy-wait loop.
- **ShortReadWriteMonitor** : Standalone reader-writer lock primitive for narrow critical sections.

Results for the simple, ShortMonitor lock :

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.4291/22H2/2022Update)
Intel Core i9-9900K CPU 3.60GHz (Coffee Lake), 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3


| Method                     | Mean      | Error     | StdDev    |
|--------------------------- |----------:|----------:|----------:|
| LockCriticalSection        | 13.471 ns | 0.0424 ns | 0.0376 ns |
| MonitorSlimCriticalSection |  4.683 ns | 0.1202 ns | 0.3289 ns |

## Usage

That kind of Monitor is used to protect really short code sections that are called millions times, typically to protect inner fields from incoherences. You have to copy the ShortMonitor in your source code to permit code inlining.

```
    public class AverageAccumulator
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
```

Performance gain against lock() is significant :

| Method           |      Mean |     Error |    StdDev |
| ---------------- | --------: | --------: | --------: |
| LockBasedAverage | 14.102 ns | 0.0159 ns | 0.0148 ns |
| SlimBasedAverage |  6.676 ns | 0.0111 ns | 0.0086 ns |

## ConcurrentQueueSlim<T>

This class is written to test a naive implementation of a SpinWait based concurrent queue. **This is a toy !**

We can see it is up to 5x or faster than .Net lock-free implementation when the number of concurrent thread to enqueue / dequeue is high. It mean that SpinWait is stopping few threads by calling Sleep(). **The side effect** is that the distribution of items to the threads is not as uniform as the one of the .Net ConcurrentQueue.

![](https://raw.githubusercontent.com/Gabriel-RABHI/GhostFields.MonitorSlim/master/Pictures/queue-results.jpg)

But as a job queue, you can see that even with 4 threads to enqueue and 4 threads to dequeue, the total item count is stable : it means that few thread are receiving large number of items like a batched list of items. This property is a good one for a job publisher / consumer model while process many items on a single thread is faster than perfect distribution - the goal is not to distribute the job between Thread, but do the job as fast as possible !

**Be careful**, that kind of benchmark often lie. Do your own production code based benchmark to see the potential benefits.

## ConcurrentList<T>

This class is written as a response to David Fowler question : **I need a data structure that has lock free enumeration, fast adding and removing. Order doesn’t matter, I keep coming back to a doubly linked list.**

So, I implemented a simple "naive" version using my ShortMonitor class :
- Enumerations are lock-free.
- Insert and remove is quite fast but serialized using MonitorLock.

Performance may be "honorable" compared to a well written "doubly linked list", it depend of the usage. I'm curious to compare !

On one side, the writers are serialized by a short lock, but on the enumerators side there is a lock free yield return loop on the entries. It make it thread safe.

**Drawback** : the memory used by the structure is the one used at the peak count item. In other words, if, during the lifetime of the instance of this class, you insert 1 Million items, the memory used by this instance will never be shrink to use less memory.

**Caution** : this can be a advantage for all subsequent variation of item count, because the memory is preallocated. But if you have large variations of item count, then it can use large memory space unnecessary. So, this class fit for use case where you have average max item count relatively stable with large variations : in that case, performance will be good.

## License
GhostFields.MonitorSlim is licensed under MIT License, which means it is free for all of your products, including commercial software.
