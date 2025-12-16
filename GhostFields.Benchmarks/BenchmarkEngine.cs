using Spectre.Console;
using System.Reflection;

namespace GhostBodyObject.BenchmarkRunner
{
    public static class BenchmarkEngine
    {
        public static void DiscoverAndShow()
        {
            DiscoverAndShowAsync().GetAwaiter().GetResult();
        }

        public static async Task DiscoverAndShowAsync()
        {
            var benchmarks = FindBenchmarks();
            if (!benchmarks.Any())
            {
                AnsiConsole.MarkupLine("[red]No benchmarks found. Ensure your classes inherit BenchmarkBase and methods have [BenchmarkAttribute].[/]");
                return;
            }
            AnsiConsole.Write(new FigletText("GBO-Benchmark").Color(Color.Cyan1));
            AnsiConsole.MarkupLine("[grey]Simple Brut Force Benchmarks Runner[/]");
            AnsiConsole.WriteLine();

            while (true)
            {
                var padding = benchmarks.Max(b => b.Attribute.Category.Length);
                var selection = AnsiConsole.Prompt(
                    new SelectionPrompt<BenchmarkMetadata>()
                        .Title("Select a benchmark to run:")
                        .PageSize(15)
                        .MoreChoicesText("[grey](Move up and down for more)[/]")
                        .AddChoices(benchmarks)
                        .UseConverter(b => $"[grey][[ {b.Attribute.Category.PadRight(padding)} ]][/] [white]{b.Attribute.Name}[/] [grey]({b.Attribute.Code})[/]")
                    );
                try
                {
                    var instance = (BenchmarkBase)Activator.CreateInstance(selection.Type);

                    if (selection.Method.ReturnType == typeof(Task))
                        await (Task)selection.Method.Invoke(instance, null);
                    else
                        selection.Method.Invoke(instance, null);
                }
                catch (Exception ex)
                {
                    AnsiConsole.WriteException(ex);
                }
                AnsiConsole.WriteLine();
            }
        }

        private static List<BenchmarkMetadata> FindBenchmarks()
        {
            var results = new List<BenchmarkMetadata>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsSubclassOf(typeof(BenchmarkBase)) || type.IsAbstract)
                        continue;

                    foreach (var method in type.GetMethods())
                    {
                        var attr = method.GetCustomAttribute<BruteForceBenchmarkAttribute>();
                        if (attr != null)
                            results.Add(new BenchmarkMetadata(type, method, attr));
                    }
                }
            }
            return results
                .OrderBy(x => x.Attribute.Category)
                .ThenBy(x => x.Attribute.Code)
                .ToList();
        }

        private record BenchmarkMetadata(Type Type, MethodInfo Method, BruteForceBenchmarkAttribute Attribute);
    }
}