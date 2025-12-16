using Spectre.Console;

namespace GhostBodyObject.BenchmarkRunner
{
    public class BenchmarkResult
    {
        public const int LABEL_PADDING = 50;
        public const int VALUE_PADDING = 20;

        public TimeSpan Duration { get; init; }
        public long BytesAllocated { get; init; }
        public int Gen0 { get; init; }
        public int Gen1 { get; init; }
        public int Gen2 { get; init; }

        /// <summary>
        /// Displays the execution summary (Time, Memory, GC) in a vertical list.
        /// </summary>
        public BenchmarkResult PrintToConsole(string label)
        {
            var table = new Table();
            table.Border(TableBorder.None);
            table.HideHeaders();
            table.AddColumn(new TableColumn("Label").PadRight(2));
            table.AddColumn(new TableColumn("Value").RightAligned());

            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule($"[cyan]{label}[/]").Centered().RuleStyle("blue dim"));
            AnsiConsole.WriteLine();

            if (Gen0 + Gen1 + Gen2 == 0)
                table.AddRow(
                    "[Gray]Garbage Collector (gen 0, 1, 2)[/]".PadRight(LABEL_PADDING),
                    $"[Green]None[/]".PadLeft(VALUE_PADDING));
            else
                table.AddRow(
                    "[red]Garbage Collector (gen 0, 1, 2)[/]".PadRight(LABEL_PADDING),
                    $"[red]{Gen0} / {Gen1} / {Gen2}[/]".PadLeft(VALUE_PADDING));

            if (BytesAllocated == 0)
                table.AddRow(
                    "[Gray]Memory Used[/]".PadRight(LABEL_PADDING),
                    $"[Green]None[/]".PadLeft(VALUE_PADDING));
            else
                table.AddRow(
                    "[red]Memory Used[/]".PadRight(LABEL_PADDING),
                    $"[red]{FormatBytes(BytesAllocated)}[/]".PadLeft(VALUE_PADDING));

            table.AddRow(
                "[yellow]Duration[/]".PadRight(LABEL_PADDING),
                $"[yellow]{Duration.TotalMilliseconds:N0} ms[/]".PadLeft(VALUE_PADDING));

            AnsiConsole.Write(table);
            return this;
        }

        /// <summary>
        /// Computes and displays throughput (Ops/Sec) and Latency underneath the main results.
        /// </summary>
        public BenchmarkResult PrintDelayPerOp(long totalOperations)
        {
            if (totalOperations <= 0) return this;

            double ms = Duration.TotalMilliseconds;
            double opsPerSec = ms > 0 ? (totalOperations / ms) * 1000.0 : 0;
            double nsPerOp = ms > 0 ? (ms * 1_000_000.0) / totalOperations : 0;

            var table = new Table();
            table.Border(TableBorder.None);
            table.HideHeaders();
            table.AddColumn(new TableColumn("Label").PadRight(2));
            table.AddColumn(new TableColumn("Value").RightAligned());

            table.AddRow(
                "[Gray]Operation cost (ns)[/]".PadRight(LABEL_PADDING),
                $"[White]{nsPerOp:N2}[/]".PadLeft(VALUE_PADDING));

            table.AddRow(
                "[Gray]Operations per second[/]".PadRight(LABEL_PADDING),
                $"[White]{opsPerSec:N0}[/]".PadLeft(VALUE_PADDING));

            AnsiConsole.Write(table);
            return this;
        }

        public BenchmarkResult PrintSpace()
        {
            AnsiConsole.WriteLine();
            return this;
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes == 0) return "0 B";
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}"; // e.g. "1.2 MB"
        }
    }
    }