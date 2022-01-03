using System;

namespace GhostFields.Benchmarks.Contracts
{
    public interface IConsole
    {
        IConsole WriteLine(string s);

        IConsole WriteLine();

        IConsole Write(string s);

        ConsoleColor BackgroundColor { get; set; }

        ConsoleColor ForegroundColor { get; set; }

        string ReadLine();

        void WaitKey();
    }
}
