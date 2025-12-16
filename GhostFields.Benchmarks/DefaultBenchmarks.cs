namespace GhostBodyObject.BenchmarkRunner
{
    // -------------------------------------------------------------
    // SAMPLE BENCHMARK CLASS
    // -------------------------------------------------------------
    public class DefaultBenchmarks : BenchmarkBase
    {
        [BruteForceBenchmark("GC", "Run GC Collect", "Z-SYS")]
        public void SequentialTest()
        {
            RunMonitoredAction(() =>
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
                GC.WaitForPendingFinalizers();
                GC.WaitForFullGCComplete();
            })
            .PrintToConsole($"GC.Collect")
            .PrintSpace();
        }
    }
}