using GhostFields.Benchmarks.Contracts;
using System;

namespace GhostFields.Benchmarks.Implementations
{
    public class BenchmarksConsole : IConsole
    {
        public ConsoleColor BackgroundColor { get => Console.BackgroundColor; set => Console.BackgroundColor = value; }
        public ConsoleColor ForegroundColor { get => Console.ForegroundColor; set => Console.ForegroundColor = value; }

        public string ReadLine()
        {
            return Console.ReadLine();
        }

        public void WaitKey()
        {
            Console.ReadKey();
        }

        public IConsole WriteLine(string s)
        {
            Console.WriteLine(s);
            return this;
        }

        public IConsole WriteLine()
        {
            Console.WriteLine();
            return this;
        }
        public IConsole Write(string s)
        {
            Console.Write(s);
            return this;
        }
    }
}
