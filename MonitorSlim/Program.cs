// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using GhostFields.Benchmarks.Execution;
using MonitorSlim;

BenchmarkConsoleRunner.PrintAllBenchmarksAndExecuteUserChoice();

BenchmarkRunner.Run<BenchMonitor>();

BenchmarkRunner.Run<BenchAverage>();