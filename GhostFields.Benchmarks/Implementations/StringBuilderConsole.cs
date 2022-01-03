using GhostFields.Benchmarks.Contracts;
using System;
using System.Text;

namespace GhostFields.Benchmarks.Implementations
{
    public class StringBuilderConsole : IConsole
    {
        private readonly StringBuilder _sb = new StringBuilder();

        public ConsoleColor BackgroundColor { get; set; }

        public ConsoleColor ForegroundColor { get; set; }

        public string ReadLine()
        {
            return "";
        }

        public void WaitKey()
        {
        }

        public IConsole Write(string s)
        {
            _sb.Append(s);
            return this;
        }

        public IConsole WriteLine(string s)
        {
            _sb.Append(s + "\r\n");
            return this;
        }

        public IConsole WriteLine()
        {
            _sb.Append("\r\n");
            return this;
        }

        public override string ToString()
        {
            return _sb.ToString();
        }
    }
}
