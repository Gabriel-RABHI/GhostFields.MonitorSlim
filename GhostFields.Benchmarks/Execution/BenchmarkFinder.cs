using System;
using System.Collections.Generic;
using System.Linq;

namespace GhostFields.Benchmarks.Execution
{
    public class BenchmarkFinder
    {
        private readonly List<BenchmarkRecord> _benchmarks = new List<BenchmarkRecord>();
        private readonly List<BenchmarkMethod> _methods = new List<BenchmarkMethod>();

        public IEnumerable<BenchmarkRecord> Benchmarks => _benchmarks;

        public IEnumerable<BenchmarkMethod> BenchmarkMethods => _methods;

        public BenchmarkFinder()
        {
            var foundTypes = new List<Type>();
            var codes = new HashSet<string>();
            // -------- Extract all bechmark inherited classes and create an instance by reflexion
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var derivedTypes = from t in a.GetTypes()
                                       where t.IsSubclassOf(typeof(BenchmarkBase)) && !t.IsAbstract
                                       select new BenchmarkRecord(t);
                    _benchmarks.AddRange(derivedTypes);
                }
                catch (Exception ex)
                {
                    BenchmarkConsoleRunner.Console.WriteLine("Unable to load " + a + " : " + ex.Message);
                }
            }
            foreach (var b in _benchmarks)
                _methods.AddRange(b.BhenchmarkMethods);
            _methods.Sort((a, b) => (a.Category + a.Code).CompareTo(b.Category + b.Code));
            foreach (var m in _methods)
            {
                if (string.IsNullOrEmpty(m.Code))
                    throw new Exception("The given method have empty code : " + m.Name);
                if (codes.Contains(m.Code))
                    throw new Exception("A method with the same code already exist : " + m.Code);
                else codes.Add(m.Code);
            }
        }
    }
}
