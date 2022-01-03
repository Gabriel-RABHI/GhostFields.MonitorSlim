using System;
using System.Collections.Generic;
using System.Linq;

namespace GhostFields.Benchmarks.Execution
{
    public class BenchmarkRecord
    {
        private readonly BenchmarkBase _instance = null;
        private readonly List<BenchmarkMethod> _bhenchmarkMethods = new List<BenchmarkMethod>();

        public BenchmarkBase Instance => _instance;

        public IEnumerable<BenchmarkMethod> BhenchmarkMethods => _bhenchmarkMethods;

        public BenchmarkRecord(Type t)
        {
            _instance = (BenchmarkBase)Activator.CreateInstance(t);
            var methods = from m in t.GetMethods()
                          where m.GetCustomAttributes(typeof(BenchmarkMethodAttribute), false).Length > 0
                          select new BenchmarkMethod(m, _instance);
            _bhenchmarkMethods.AddRange(methods);
        }
    }
}
