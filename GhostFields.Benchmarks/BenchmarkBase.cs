using System.Diagnostics;

namespace GhostBodyObject.BenchmarkRunner
{
    public abstract class BenchmarkBase
    {
        /// <summary>
        /// Run a synchronous action, monitor it, and return results.
        /// </summary>
        protected BenchmarkResult RunMonitoredAction(Action action)
        {
            return RunMonitoredActionAsync(() => { action(); return Task.CompletedTask; }).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Run an asynchronous action, monitor it, and return results.
        /// </summary>
        protected async Task<BenchmarkResult> RunMonitoredActionAsync(Func<Task> action)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var startAlloc = GC.GetTotalAllocatedBytes(true);
            var startG0 = GC.CollectionCount(0);
            var startG1 = GC.CollectionCount(1);
            var startG2 = GC.CollectionCount(2);

            var sw = Stopwatch.StartNew();
            await action();
            sw.Stop();

            var endAlloc = GC.GetTotalAllocatedBytes(true);

            return new BenchmarkResult
            {
                Duration = sw.Elapsed,
                BytesAllocated = endAlloc - startAlloc,
                Gen0 = GC.CollectionCount(0) - startG0,
                Gen1 = GC.CollectionCount(1) - startG1,
                Gen2 = GC.CollectionCount(2) - startG2
            };
        }

        /// <summary>
        /// Runs an action in parallel threads with tight-loop synchronization.
        /// </summary>
        protected BenchmarkResult RunParallelAction(int threadCount, Action<int> action)
        {
            return RunParallelActionAsync(threadCount, (id) => { action(id); return Task.CompletedTask; }).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Runs an async action in parallel tasks with tight-loop synchronization.
        /// </summary>
        protected async Task<BenchmarkResult> RunParallelActionAsync(int threadCount, Func<int, Task> action)
        {
            return await RunMonitoredActionAsync(async () =>
            {
                using var startSignal = new ManualResetEventSlim(false);
                using var readySignal = new CountdownEvent(threadCount);

                var tasks = new Task[threadCount];

                for (int i = 0; i < threadCount; i++)
                {
                    int localId = i;
                    tasks[i] = Task.Run(async () =>
                    {
                        readySignal.Signal();
                        var spin = new SpinWait();
                        while (!startSignal.IsSet)
                            spin.SpinOnce();
                        await action(localId);
                    });
                }
                readySignal.Wait();
                startSignal.Set();
                await Task.WhenAll(tasks);
            });
        }
    }
}