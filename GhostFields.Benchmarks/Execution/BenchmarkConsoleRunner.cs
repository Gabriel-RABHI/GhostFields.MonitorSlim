using GhostFields.Benchmarks.Contracts;
using GhostFields.Benchmarks.Implementations;
using System;
using System.Diagnostics;
using System.Linq;

namespace GhostFields.Benchmarks.Execution
{
    public static class BenchmarkConsoleRunner
    {
        public const int CHR_WIDTH = 79;
        private static IConsole _console = new BenchmarksConsole();

        public static IConsole Console => _console;

        public static void SetConsole(IConsole c)
        {
            _console = c;
        }

        public static string CompleteString(string s, int lenght, char r)
        {
            if (s.Length < lenght)
                for (var i = s.Length; i < lenght; i++)
                    s += r;
            return s;
        }

        public static void WriteInstructionLine(string content)
        {
            _console.ForegroundColor = ConsoleColor.Green;
            _console.WriteLine(content);
            _console.ForegroundColor = ConsoleColor.White;
        }

        public static void WriteCommentLine(string content)
        {
            _console.ForegroundColor = ConsoleColor.Gray;
            _console.WriteLine(content);
            _console.ForegroundColor = ConsoleColor.White;
        }

        public static void WriteWarningLine(string content)
        {
            _console.BackgroundColor = ConsoleColor.Red;
            _console.ForegroundColor = ConsoleColor.White;
            _console.WriteLine(content);
            _console.ForegroundColor = ConsoleColor.White;
            _console.BackgroundColor = ConsoleColor.Black;
        }

        public static void WriteStepLine(string content)
        {
            _console.ForegroundColor = ConsoleColor.Cyan;
            _console.WriteLine(content);
            _console.ForegroundColor = ConsoleColor.White;
        }

        public static void WriteMajorStepLine(string content)
        {
            if (content.Length > CHR_WIDTH - 4)
            {
                WriteStepLine(content);
            }
            else
            {
                _console.ForegroundColor = ConsoleColor.DarkCyan;
                for (var i = 0; i < (CHR_WIDTH - (content.Length + 2)) / 2; i++)
                    _console.Write("+");
                _console.ForegroundColor = ConsoleColor.Cyan;
                _console.Write(" " + content + " ");
                _console.ForegroundColor = ConsoleColor.DarkCyan;
                for (var i = 0; i < (CHR_WIDTH - (content.Length + 2)) / 2; i++)
                    _console.Write("+");
                _console.ForegroundColor = ConsoleColor.White;
                _console.Write("\r\n");
            }
        }

        public static void WriteMinorStepLine(string content)
        {
            if (content.Length > CHR_WIDTH - 4)
            {
                WriteStepLine(content);
            }
            else
            {
                _console.ForegroundColor = ConsoleColor.Gray;
                for (var i = 0; i < (CHR_WIDTH - (content.Length + 2)) / 2; i++)
                    _console.Write("+");
                _console.ForegroundColor = ConsoleColor.White;
                _console.Write(" " + content + " ");
                _console.ForegroundColor = ConsoleColor.Gray;
                for (var i = 0; i < (CHR_WIDTH - (content.Length + 2)) / 2; i++)
                    _console.Write("+");
                _console.ForegroundColor = ConsoleColor.White;
                _console.Write("\r\n");
            }
        }

        public static void WriteResultLine(string margin, string content, string result)
        {
            _console.ForegroundColor = ConsoleColor.DarkYellow;
            _console.Write(margin);
            _console.ForegroundColor = ConsoleColor.Yellow;
            _console.Write(content);
            _console.ForegroundColor = ConsoleColor.DarkGray;
            for (var i = 0; i < (CHR_WIDTH - (margin.Length + content.Length + result.Length)); i++)
                _console.Write(".");
            _console.ForegroundColor = ConsoleColor.White;
            _console.Write(result);
            _console.Write("\r\n");
        }

        public static void WriteResultLine(string margin, string label, string count, string content, string result)
        {
            _console.ForegroundColor = ConsoleColor.DarkYellow;
            _console.Write(margin);
            _console.ForegroundColor = ConsoleColor.Yellow;
            _console.Write(label);
            _console.ForegroundColor = ConsoleColor.White;
            _console.Write(count);
            _console.ForegroundColor = ConsoleColor.Yellow;
            _console.Write(content);
            _console.ForegroundColor = ConsoleColor.DarkGray;
            for (var i = 0; i < (CHR_WIDTH - (margin.Length + label.Length + count.Length + content.Length + result.Length)); i++)
                _console.Write(".");
            _console.ForegroundColor = ConsoleColor.White;
            _console.Write(result);
            _console.Write("\r\n");
        }

        public static void WriteCommand(string lineStart, string keyCode, string description)
        {
            _console.ForegroundColor = ConsoleColor.DarkGray;
            _console.Write(lineStart);
            _console.ForegroundColor = ConsoleColor.Blue;
            _console.Write(keyCode);
            _console.ForegroundColor = ConsoleColor.White;
            _console.Write(" : ");
            _console.ForegroundColor = ConsoleColor.Green;
            _console.Write(description);
            _console.Write("\r\n");
        }

        public static void WriteError(string introduction, string description)
        {
            _console.ForegroundColor = ConsoleColor.DarkRed;
            _console.Write(introduction);
            _console.ForegroundColor = ConsoleColor.White;
            _console.Write(" : ");
            _console.ForegroundColor = ConsoleColor.Red;
            _console.Write(description);
            _console.ForegroundColor = ConsoleColor.White;
            _console.Write("\r\n");
        }

        public static void WriteException(Exception ex)
        {
            _console.ForegroundColor = ConsoleColor.DarkRed;
            _console.WriteLine("-------------------------------- EXCEPTION --------------------------------");
            _console.ForegroundColor = ConsoleColor.Red;
            _console.WriteLine(ex.Message);
            _console.WriteLine(ex.StackTrace);
            if (ex.InnerException != null)
                WriteException(ex.InnerException);
            _console.ForegroundColor = ConsoleColor.White;
            _console.Write("\r\n");
        }

        public static void WriteReturn()
        {
            _console.WriteLine();
        }

        private static void DisplayContextWarnings()
        {
            if (Debugger.IsAttached)
            {
                WriteWarningLine("Debugger is attached : this may slowdown execution.");
            }
#if DEBUG
            WriteWarningLine("DEBUG directive defined : this will slow down the code execution (code may bot been optimized) !");
#endif
        }

        /// <summary>
        /// Search by reflexion, the various benchmarks and tests. For Console App only.
        /// </summary>
        public static void PrintAllBenchmarksAndExecuteUserChoice()
        {
            try
            {
                var finder = new BenchmarkFinder();
                // -------- Display the menu
                WriteCommentLine("///////////////////////////////////////////////////////////////////////////////");
                WriteCommentLine("////////////////////  BENCHMARK / SAMPLE CODE CONSOLE APP  ////////////////////");
                WriteCommentLine("///////////////////////////////////////////////////////////////////////////////");

                DisplayContextWarnings();

                while (true)
                {
                    WriteReturn();
                    if (finder.BenchmarkMethods.Count() == 0)
                        WriteError("Strange", "No code to run found !");
                    var cat = "";
                    foreach (var b in finder.BenchmarkMethods)
                    {
                        if (b.Category != cat)
                        {
                            cat = b.Category;
                            WriteInstructionLine(CompleteString(cat + " ", 79, '-'));
                        }
                        WriteCommand("    ", b.Code, b.Name);
                    }
                    WriteCommand("", "gc", "execute a GC collect.");
                    WriteCommand("", "all", "execute all.");
                    WriteCommand("", "exit", "quit.");
                    WriteInstructionLine("Enter a key code (in blue color) to execute a sample / bench ('EQ*' work, multiple key too) :");
                    _console.ForegroundColor = ConsoleColor.White;
                    var input = _console.ReadLine();
                    if (input.ToLower() == "exit")
                        break;
                    if (input.ToLower() == "gc")
                    {
                        GC.Collect();
                        WriteStepLine("GC collect done.");
                    }
                    else if (input.ToLower() == "all")
                    {
                        foreach (var e in finder.BenchmarkMethods)
                            ExecuteBenchmark(e);
                    }
                    else
                    {
                        foreach (var cmd in input.Split())
                        {
                            if (!string.IsNullOrWhiteSpace(cmd))
                            {
                                var en = cmd.Contains("*") ?
                                    finder.BenchmarkMethods.Where(m => m.Code.ToLower().StartsWith(cmd.ToLower().Replace("*", ""))) :
                                    finder.BenchmarkMethods.Where(m => m.Code.ToLower() == cmd.ToLower());
                                foreach (var e in en)
                                    ExecuteBenchmark(e);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteException(ex);
                WriteInstructionLine("Press return key to continue.");
                _console.ReadLine();
            }
        }

        private static void ExecuteBenchmark(BenchmarkMethod e)
        {
            try
            {
                DisplayContextWarnings();
                WriteStepLine("===============================================================================");
                WriteStepLine("====> " + (string.IsNullOrEmpty(e.Category) ? e.Category + " / " : "") + e.Name);
                WriteStepLine("===============================================================================");
                WriteReturn();
                e.Run();
                WriteReturn();
                WriteStepLine("************************** Execution successfull ! ***************************");
            }
            catch (Exception ex)
            {
                WriteException(ex);
            }
        }
    }
}
