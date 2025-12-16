using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GhostBodyObject.BenchmarkRunner
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class BruteForceBenchmarkAttribute : Attribute
    {
        public string Code { get; }
        public string Name { get; }
        public string Category { get; }

        public BruteForceBenchmarkAttribute(string code, string name, string category = "General")
        {
            Code = code;
            Name = name;
            Category = category;
        }
    }
}