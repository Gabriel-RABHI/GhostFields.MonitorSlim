using System.Linq;
using System.Reflection;

namespace GhostFields.Benchmarks.Execution
{
    public class BenchmarkMethod
    {
        private readonly BenchmarkBase _instance = null;
        private readonly BenchmarkMethodAttribute _attribute = null;
        private readonly MethodInfo _info = null;

        public string Category => _attribute.Category;

        public string Name => _attribute.Name;

        public string Code => _attribute.Code;

        public BenchmarkMethod(MethodInfo mi, BenchmarkBase instance)
        {
            _info = mi;
            _instance = instance;
            _attribute = (BenchmarkMethodAttribute)mi.GetCustomAttributes(typeof(BenchmarkMethodAttribute), false).First();
        }

        public void Run()
        {
            _info.Invoke(_instance, null);
        }
    }
}
