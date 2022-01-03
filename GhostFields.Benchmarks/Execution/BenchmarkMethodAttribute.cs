using System;

namespace GhostFields.Benchmarks.Execution
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class BenchmarkMethodAttribute : Attribute
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }
}
